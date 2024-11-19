using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ReJo.UI;
using ReJo.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using RUI = Autodesk.Revit.UI;

namespace ReJo
{
    public class FiltersPaneExternalEventHandler : RUI.IExternalEventHandler
    {
        public void Execute(RUI.UIApplication app)
        {

            try
            {
                if (!CmdInit.IsInitialized)
                {
                    var dockablePane = app.GetDockablePane(FiltersPane.This.DockablePaneId);
                    if (dockablePane != null)
                        dockablePane.Hide();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("ReJo", ex.Message);
            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "Filters Pane External Event";
        }
    }


    public class RulesWndExternalEventHandler : RUI.IExternalEventHandler
    {
        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }
        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }
        public void Execute(RUI.UIApplication app)
        {

            try
            {
                RulesWnd.This.Show();
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "RulesWnd External Event";
        }
    }

    public class OpenDocumentExternalEventHandler : RUI.IExternalEventHandler
    {
        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }
        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }

        public void Execute(RUI.UIApplication app)
        {
            try
            {
                HashSet<string> filesPathOpened = new HashSet<string>();
                foreach (Document doc in app.Application.Documents)
                {
                    filesPathOpened.Add(doc.PathName);
                }

                bool anyOpen = false;
                foreach (var file in CmdInit.This.JoinService.Model3dFilesLoaded.Values)
                {
                    if (!filesPathOpened.Contains(file.FileFullPath))
                    {

                        //if (file.FileFullPath.StartsWith(JoinService.This.AppSettingsPath))
                        //{
                        //    LockFile(file.FileFullPath);
                        //}


                        var uiDoc = app.OpenAndActivateDocument(file.FileFullPath);
                        anyOpen = true;
                    }
                }

                if (anyOpen)
                    CmdInit.This.JoinService.UpdateModel3dValues();
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "Open Document External Event";
        }

        private void LockFile(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                // Blocca l'intero file
                fileStream.Lock(0, fileStream.Length);
                //Console.WriteLine("File locked. Press Enter to release the lock.");
                //Console.ReadLine();
                // Rilascia il blocco
                //fileStream.Unlock(0, fileStream.Length);
            }
        }
    }

    public class ShowRuleExternalEventHandler : RUI.IExternalEventHandler
    {
        
        public Guid RuleId { get; set; } = Guid.Empty;


        
        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }

        public void Execute(RUI.UIApplication app)
        {

            try
            {
                FiltersData filtersData = FiltersPane.This.View.GetFiltersData();
                if (filtersData == null)
                    return;

                FiltersDataItem filtersDataItem = filtersData.Items.FirstOrDefault(f => f.RulesIO.FirstOrDefault(r => r.RuleId == RuleId) != null);

                if (filtersDataItem == null)
                    return;

                FilterItemView? filterItem = FiltersPane.This.View.FilterItems.FirstOrDefault(item => item.UniqueId == filtersDataItem.RvtFilter?.RvtFilterUniqueId);
                if (filterItem != null)
                {
                    var filterUniqueId = filterItem.UniqueId;



                    RulesWnd.Create();

                    var ruleErrorsConflict = filterItem.RuleErrors.Where(item =>
                    {
                        if (item is RuleErrorConflict ruleErrConf)
                        {
                            if (item.FilterUniqueId == filterUniqueId)
                                return true;

                        }
                        return false;
                    }).ToList();

                    RulesWnd.This?.View.SetRuleErrorsConflict(ruleErrorsConflict);

                    RulesWnd.This?.View.Load(filterUniqueId, filtersData, RuleId);
                    RulesWnd.This?.Show();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return " Show Rule External Event";
        }
    }


    //public class GetElementsByFilterExternalEventHandler : RUI.IExternalEventHandler
    //{
    //    public Document Document { get; set; }
    //    public string RvtFilterId { get; set; }


    //    List<Model3dObjectKey> _Model3dObjectsKey;
    //    public List<Model3dObjectKey> Model3dObjectsKey
    //    {
    //        get => _Model3dObjectsKey;
    //        private set
    //        {
    //            _Model3dObjectsKey = value;
    //            _tcs.TrySetResult(true); // Segnala che la proprietà è stata settata
    //        }
    //    }


    //    private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

    //    ExternalEvent ExternalEvent { get; set; } = null;
    //    public void CreateExternalEvent()
    //    {
    //        ExternalEvent = ExternalEvent.Create(this);
    //    }

    //    public async Task<List<Model3dObjectKey>> RaiseExternalEvent()
    //    {
    //        ExternalEvent?.Raise();
    //        await _tcs.Task;

    //        return Model3dObjectsKey;
    //    }

    //    public void Execute(RUI.UIApplication app)
    //    {

    //        try
    //        {
    //            Model3dObjectsKey = Utils.GetModel3dObjectsKey(Document, RvtFilterId);
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }

    //    public string GetName()
    //    {
    //        // not sure where this will appear or if need to be unique
    //        return "GetElementsByFilter External Event";
    //    }
    //}

    public class GetElementsByFilterExternalEventHandler : RUI.IExternalEventHandler
    {
        public Document Document { get; set; }
        public string RvtFilterId { get; set; }
        
        public List<Model3dObjectKey> Model3dObjectsKey { get; private set; }
        

        private Queue<TaskCompletionSource<bool>> _eventQueue = new Queue<TaskCompletionSource<bool>>();

        public ExternalEvent ExternalEvent { get; set; } = null;

        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public async Task<List<Model3dObjectKey>> RaiseExternalEvent()
        {
            var tcs = new TaskCompletionSource<bool>();
            _eventQueue.Enqueue(tcs);

            if (_eventQueue.Count == 1)
            {
                ExternalEvent?.Raise();
            }

            await tcs.Task;
            return Model3dObjectsKey;
        }

        public void Execute(RUI.UIApplication app)
        {
            try
            {
                Model3dObjectsKey = Utils.GetModel3dObjectsKey(Document, RvtFilterId);
            }
            catch (Exception ex)
            {
                _eventQueue.Peek().TrySetException(ex);
            }
            finally
            {
                _eventQueue.Dequeue().TrySetResult(true);

                if (_eventQueue.Count > 0)
                {
                    ExternalEvent?.Raise();
                }
            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "GetElementsByFilter External Event";
        }
    }

    public class SelectElementsExternalEventHandler : RUI.IExternalEventHandler
    {
        public Document Document { get; set; }
        public List<Model3dObjectKey> Elements = new List<Model3dObjectKey>();

        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }

        public void Execute(RUI.UIApplication app)
        {

            try
            {
                Utils.SelectElements(Document, Elements);
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "SelectElements External Event";
        }
    }

    public class UpdateInitButtonIconExternalEventHandler : RUI.IExternalEventHandler
    {

        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }

        public void Execute(RUI.UIApplication app)
        {

            try
            {
                List<RibbonPanel> panels = CmdInit.This.UIApplication.GetRibbonPanels(Application.TabReJoName); // Assumi che il pulsante sia nel primo pannello

                RibbonPanel panel = panels[0];

                // Trova il pulsante specifico
                PushButton? pushButton = panel.GetItems()
                    .OfType<PushButton>()
                    .FirstOrDefault(b => b.Name == "BtnInit");


                string? baseDirectory = Path.GetDirectoryName(Application.ExecutingAssemblyPath);

                if (pushButton != null && !string.IsNullOrEmpty(baseDirectory))
                {
                    if (!CmdInit.IsInitialized)
                    {
                        pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_L.png")));
                        pushButton.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoin_S.png")));
                    }
                    else
                    {
                        pushButton.LargeImage = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoinOn_L.png")));
                        pushButton.Image = new BitmapImage(new Uri(Path.Combine(baseDirectory, "Resources\\ReJo\\CollegaJoinOn_S.png")));
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "UpdateInitButtonIcon External Event";
        }
    }

    public class CloseModelessDialogExternalEventHandler : RUI.IExternalEventHandler
    {

        ExternalEvent ExternalEvent { get; set; } = null;
        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public void RaiseExternalEvent()
        {
            ExternalEvent?.Raise();
        }

        public void Execute(RUI.UIApplication app)
        {

            try
            {
                WindowManager.CloseModelessWindows();
            }
            catch (Exception ex)
            {

            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "CloseModelessDialog External Event";
        }
    }

    public class AddFilterExternalEventHandler : RUI.IExternalEventHandler
    {
        public Document Document { get; set; } = null;
        public RvtFilterForIO FilterData { get; set; } = null;

        public string ResultMsg { get; private set; } = string.Empty;
        public string FilterElUniqueId { get; private set;} = string.Empty;


        private Queue<TaskCompletionSource<bool>> _eventQueue = new Queue<TaskCompletionSource<bool>>();

        public ExternalEvent ExternalEvent { get; set; } = null;

        public void CreateExternalEvent()
        {
            ExternalEvent = ExternalEvent.Create(this);
        }

        public async Task<string> RaiseExternalEvent()
        {
            var tcs = new TaskCompletionSource<bool>();
            _eventQueue.Enqueue(tcs);

            if (_eventQueue.Count == 1)
            {
                ExternalEvent?.Raise();
            }

            await tcs.Task;
            return FilterElUniqueId;
        }

        public void Execute(RUI.UIApplication app)
        {
            try
            {
                using (Transaction t = new Transaction(Document, "Add FilterElement"))
                {
                    t.Start();
                    if (!FilterElement.IsNameUnique(Document, FilterData.RvtFilterName))
                    {
                        //filtro già presente per nome

                        ParameterFilterElement parFilterEl = Utils.GetFilterElementByName(Document, FilterData.RvtFilterName);

                        Document.Delete(parFilterEl.Id);
                    }


                    ParameterFilterElement parameterFilterElement = FilterElementConverter.Convert(FilterData.RvtFilterName, FilterData.RvtParameterFilterData, Document);


                    Document.ActiveView.AddFilter(parameterFilterElement.Id);
                    t.Commit();

                    FilterElUniqueId = parameterFilterElement.UniqueId;
                }
            }
            catch (Exception ex)
            {
                _eventQueue.Peek().TrySetException(ex);
            }
            finally
            {
                _eventQueue.Dequeue().TrySetResult(true);

                if (_eventQueue.Count > 0)
                {
                    ExternalEvent?.Raise();
                }
            }
        }

        public string GetName()
        {
            // not sure where this will appear or if need to be unique
            return "AddFilter External Event";
        }
    }

}

