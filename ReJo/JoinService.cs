using _3DModelExchange;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using CommonResources;
using Newtonsoft.Json.Serialization;
using ReJo.UI;
using ReJo.Utility;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace ReJo
{
    /// <summary>
    /// JoinService dovrebbe avere quasi tutte le funzioni di IMainAppService ma non essere derivato
    /// </summary>
    public class JoinService// : IMainAppService
    {
        ClientServerPipeHelper ClientServerPipeHelper = ClientServerPipeHelper.Instance;
        bool _isBusy = false;
        public Dictionary<string, Model3dFile> Model3dFilesLoaded { get; private set; } = new Dictionary<string, Model3dFile>();//key: FullFilePath
        public string AppSettingsPath { get; private set; }

        private object? _returnValue = null;
        private bool _returnOk = false;

        public static JoinService This { get; private set; } = null;

        public JoinService()
        {
            ClientServerPipeHelper.StartServer(JoinRevitConstant.JoinToRevitPipeName);
            ClientServerPipeHelper.TriggerReceivedMessage += ClientServerCommonHelper_TriggerReceivedMessage;

            This = this;
        }

        /// <summary>
        /// Oggetti temporanei
        /// key: FilterId di FiltersData, value: Rvt filter id
        /// Questa mappa deve essere consistente per la funzione GetElementsByFilter
        /// Mappa di appoggio per ApplyComputoRules
        /// </summary>
        public Dictionary<Guid, RvtFilterForIO> RvtFilterIdMap { get; set; } = new Dictionary<Guid, RvtFilterForIO>();

        #region JoinToRevit

        private void ClientServerCommonHelper_TriggerReceivedMessage(object? sender, MessageReceived e)
        {
            //MessageBox.Show("JoinService Received in ReJo");

            if (_isBusy)
                return;

            _isBusy = true;

            
            try
            {
                //check se i dati ricevuti sono completi
                string receivedData = e.ReceivedData.TrimEnd('\0');
                int byteCount = Encoding.UTF8.GetByteCount(receivedData);

                if (byteCount >= ClientServerPipeHelper.BytesCount)
                {
                    MessageBox.Show("ClientServerPipeHelper.BytesCount overflow");
                    _isBusy = false;
                    return;
                }

                RevitAction revAction = null;
                bool res = RevitSerializer.JsonDeserialize(receivedData, out revAction, typeof(RevitAction));

                switch (revAction.Name)
                {
                    case RevitActionName.ReJoInit_return:
                        {

                            Model3dFiles? model3dFiles = revAction.Parameters[0] as Model3dFiles;
                            string appSettingsPath = (string) revAction.Parameters[1];

                            if (model3dFiles != null)
                                Model3dFilesLoaded = model3dFiles.Items.ToDictionary(item => item.FileFullPath, item => item);

                            AppSettingsPath = appSettingsPath;


                            CmdInit.This.SetInitialized(true);

                        }
                        break;
                    case RevitActionName.SelectElements:
                        {
                            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
                            var elements = RevitSerializer.ToObject<List<Model3dObjectKey>>(revAction.Parameters[0]);
                            RevitSelectElements(doc, elements);
                        }
                        break;
                    case RevitActionName.LoadModels:
                        {
                            var model3dFiles = RevitSerializer.ToObject<Model3dFiles>(revAction.Parameters[0]);
                            foreach (var model3dFile in model3dFiles.Items)
                                Model3dFilesLoaded.TryAdd(model3dFile.FileFullPath, model3dFile);

                            CmdInit.This.OpenDocumentHandler.RaiseExternalEvent();

                            WindowHelper.BringWindowToFront((CmdInit.This.UIApplication.MainWindowHandle));
                        }
                        break;
                    case RevitActionName.UpdateModel3dValues:
                        {
                            var model3DValues = RevitSerializer.ToObject<Model3dValues>(revAction.Parameters[0]);
                            RevitUpdateModel3dValues(model3DValues);
                        }
                        break;
                    case RevitActionName.GetCurrentProjectModel3dTags_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.SelectPrezzarioItems_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.GetCurrentProjectModel3dFilters_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.GetUpdatedPrezzarioItems_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.GetCurrentProjectModel3dFiles_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.GetElementsByFilter:
                        {
                            try
                            {
                                Guid filterID = new Guid(RevitSerializer.ToObject<string>(revAction.Parameters[0]));
                                List<Model3dObjectKey> model3dObjs = GetElementsByFilter(filterID);

                                //send to Join
                                var joinAction = new JoinAction()
                                {
                                    Name = JoinActionName.GetElementsByFilter_return,
                                    Parameters = new List<object> { model3dObjs },
                                };

                                string json = string.Empty;
                                if (RevitSerializer.JsonSerialize(joinAction, out json))
                                {
                                    var ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                                    OnTransmit(ok);
                                }
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message);
                            }
                        }
                        break;
                    case RevitActionName.Error_return:
                        {
                            _returnValue = null; 
                            _returnOk = true;
                        }
                        break;
                    case RevitActionName.ProjectClose:
                        {
                            WindowManager.CloseModalWindows();
                            CmdInit.This.CloseModelessDialogHandler.RaiseExternalEvent();
                            CmdInit.This.SetInitialized(false);
                        }
                        break;
                    case RevitActionName.ShowRule:
                        {
                            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
                            Guid ruleId = new Guid(RevitSerializer.ToObject<string>(revAction.Parameters[0]));
                            RevitShowRule(ruleId);
                        }
                        break;
                    case RevitActionName.SetCurrentProjectModel3dFilters_return:
                        {
                            _returnValue = revAction.Parameters[0];
                            _returnOk = true;
                        }
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception ex)
            {
            
            }

            _isBusy = false;
        }

        private void RevitSelectElements(Document doc, List<Model3dObjectKey> elements)
        {
            try
            {
                CmdInit.This.SelectElementsHandler.Document = doc;
                CmdInit.This.SelectElementsHandler.Elements = elements;
                CmdInit.This.SelectElementsHandler.RaiseExternalEvent();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private List<Model3dObjectKey> GetElementsByFilter(Guid filterID)
        {
            List < Model3dObjectKey > m3dObjs = new List < Model3dObjectKey >();

            try
            {
                
                RvtFilterIdMap.TryGetValue(filterID, out var rvtFilter);
                if (rvtFilter != null)
                {
                    Document doc = CmdInit.This.GetDocument(rvtFilter.ProjectIfcGuid);
                    if (doc != null &&  !string.IsNullOrEmpty(rvtFilter.RvtFilterUniqueId))
                    {
                        CmdInit.This.GetElementsByFilterHandler.Document = doc;
                        CmdInit.This.GetElementsByFilterHandler.RvtFilterId = rvtFilter.RvtFilterUniqueId;

                        var task = Task.Run(async () => await CmdInit.This.GetElementsByFilterHandler.RaiseExternalEvent());
                        m3dObjs = task.Result;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

            return m3dObjs;
        }



        private void RevitUpdateModel3dValues(Model3dValues model3DValues)
        {
            try
            {
                Utils.UpdateModel3dValues(model3DValues);
            }
            catch (Exception exc)
            { }

            try
            { 
                //send to Join
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.UpdateModel3dValues_return,
                    Parameters = new List<object> { model3DValues },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    var ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                }

            }
            catch (Exception exc)
            {
                
            }
        }



        private void RevitShowRule(Guid ruleId)
        {
            if (RulesWnd.This != null)
            {
                RulesWnd.This.Activate();
            }
            else 
            {

                if (FiltersPane.This == null)
                    return;

                CmdInit.This.ShowRuleHandler.RuleId = ruleId;
                CmdInit.This.ShowRuleHandler.RaiseExternalEvent();
            }
        }


        #endregion

        #region RevitToJoin

        public bool Init()
        {

            

            Task<Model3dFiles> task = null;

            bool ret = false;
            try
            {
                //Check initialization
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.ReJoInit,
                    Parameters = new List<object>(),
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ret = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);

                    OnTransmit(ret);
                    if (!ret)
                    {
                        MessageBox.Show(LocalizationProvider.GetString("Inizializzare Rejo in Join"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



            return false;
        }

        public void AddElementi(List<Model3dObjectKey> elementsId, bool createGroup)
        {
            if (!CanTransmit())
                return;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.AddElementi,
                    Parameters = new List<object> { elementsId, createGroup },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    var ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ApplyComputoRules()
        {
            if (!CanTransmit())
                return;

            bool ret = false;
            try
            {
                //Check initialization
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.ApplyComputoRules,
                    Parameters = new List<object>(),
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ret = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ret);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string GetAppSettingsPath()
        {
            throw new NotImplementedException();
        }

        public List<Model3dObjectKey> GetComputoModel3dObjectsKey()
        {
            throw new NotImplementedException();
        }



        public FiltersData GetCurrentProjectModel3dFilters()
        {
            if (!CanTransmit())
                return null;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;

            Task<FiltersData> task = null;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.GetCurrentProjectModel3dFilters,
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);

                    OnTransmit(ok);
                    if (!ok)
                        return null;

                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    FiltersData ret = RevitSerializer.ToObject<FiltersData>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        private void OnTransmit(bool isOk)
        {
            if (!isOk)
            {
                _returnValue = null;
                _returnOk = false;
                CmdInit.This.SetInitialized(false);
            }

            
        }

        private bool CanTransmit()
        {
            if (!CmdInit.IsInitialized)
                return false;

            return true;
        }

        public PreferencesData GetCurrentProjectModel3dPreferences()
        {
            throw new NotImplementedException();
        }

        public TagsData GetCurrentProjectModel3dTags()
        {
            if (!CanTransmit())
                return null;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;
            Task<TagsData> task = null;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.GetCurrentProjectModel3dTags,
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                    if (!ok)
                        return null;

                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    TagsData ret = RevitSerializer.ToObject<TagsData>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        public UserViewList GetCurrentProjectModel3dUserViews()
        {
            throw new NotImplementedException();
        }

        public IMainAppProjectService GetProjectService(string fullFileName)
        {
            throw new NotImplementedException();
        }

        public List<Model3dPrezzarioItem> GetUpdatedPrezzarioItems(List<Model3dPrezzarioItem> prezzarioItems)
        {
            if (!CanTransmit())
                return null;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;
            Task<List<Model3dPrezzarioItem>> task = null;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.GetUpdatedPrezzarioItems,
                    Parameters = new List<object> { prezzarioItems },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                    if (!ok)
                        return null;
                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    List<Model3dPrezzarioItem> ret = RevitSerializer.ToObject<List<Model3dPrezzarioItem>>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        public Dictionary<UserTranslKey, UserTranslData> GetUserRotoTranslation()
        {
            throw new NotImplementedException();
        }

        public bool ImportPrezzarioItems(IMainAppProjectService prj, List<Guid> filtersDataItemsSelected, ref Dictionary<Guid, Guid> prezzarioItemIdsMap)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Select Join elements
        /// </summary>
        /// <param name="elementsId"></param>
        public void SelectElements(List<Model3dObjectKey> elementsId)
        {
            if (!CanTransmit())
                return;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.SelectElementi,
                    Parameters = new List<object> { elementsId },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    var ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public List<Model3dPrezzarioItem> SelectPrezzarioItems(string windowTitle, Guid selectedPrezzarioItemId, bool isSingleSelection)
        {
            if (!CanTransmit())
                return null;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;
            Task<List<Model3dPrezzarioItem>> task = null;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.SelectPrezzarioItems,
                    Parameters = new List<object> { windowTitle, selectedPrezzarioItemId, isSingleSelection },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                    if (!ok)
                        return null;
                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    List<Model3dPrezzarioItem> ret = RevitSerializer.ToObject<List<Model3dPrezzarioItem>>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        public bool SetCurrentProjectModel3dFilters(FiltersData filtersData)
        {
            if (!CanTransmit())
                return false;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;
            Task<bool> task = null;


            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.SetCurrentProjectModel3dFilters,
                    Parameters = new List<object> { filtersData },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);

                    OnTransmit(ok);
                    if (!ok)
                        return false;
                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    bool ret = RevitSerializer.ToObject<bool>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        public bool SetCurrentProjectModel3dPreferences(PreferencesData excludedData)
        {
            throw new NotImplementedException();
        }

        public bool SetCurrentProjectModel3dTags(TagsData tagsData)
        {
            if (!CanTransmit())
                return false;

            bool ok = false;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.SetCurrentProjectModel3dTags,
                    Parameters = new List<object> { tagsData },
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);

                    OnTransmit(ok);
                    if (!ok)
                        return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return ok;
        }

        public bool SetCurrentProjectModel3dUserViews(UserViewList userViews)
        {
            throw new NotImplementedException();
        }

        public void SetUserRotoTranslation(Dictionary<UserTranslKey, UserTranslData> transl)
        {
            throw new NotImplementedException();
        }

        public void UpdateModel3dValues()
        {
            if (!CanTransmit())
                return;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.UpdateModel3dValues,
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    var ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);
                    OnTransmit(ok);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public Model3dFiles GetCurrentProjectModel3dFiles()
        {
            if (!CanTransmit())
                return null;

            bool ok = false;
            _returnValue = null;
            _returnOk = false;

            Task<Model3dFiles> task = null;

            try
            {
                var joinAction = new JoinAction()
                {
                    Name = JoinActionName.GetCurrentProjectModel3dFiles,
                };

                string json = string.Empty;
                if (RevitSerializer.JsonSerialize(joinAction, out json))
                {
                    ok = ClientServerPipeHelper.TransmitDataToIPCServerExecutable(JoinRevitConstant.RevitToJoinPipeName, json);

                    OnTransmit(ok);
                    if (!ok)
                        return null;
                }

                task = Task.Run(() =>
                {
                    System.Threading.SpinWait.SpinUntil(() => _returnOk == true);
                    Model3dFiles ret = RevitSerializer.ToObject<Model3dFiles>(_returnValue);
                    _returnValue = null;
                    _returnOk = false;
                    return ret;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return task.Result;
        }

        public void WindowActivate()
        {
            throw new NotImplementedException();
        }

        #endregion



        internal void UpdateModel3dFiles()
        {
            var model3dFiles = GetCurrentProjectModel3dFiles();
            if (model3dFiles != null)
            {
                Model3dFilesLoaded = model3dFiles.Items.ToDictionary(item => item.FileFullPath, item => item);
            }
            
        }

        public Model3dType GetModel3DType()
        {
            return Model3dType.Revit;
        }
    }
}
