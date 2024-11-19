using _3DModelExchange;
using AttivitaWpf.View;
using CommonResources;
using Commons;
using ComputoWpf;
using DevExpress.CodeParser;
using DevExpress.Mvvm.Native;
using ElementiWpf;
using log4net;
using MasterDetailModel;
using MasterDetailView;
using Model;
using NPOI.HPSF;
using ProtoBuf;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WebServiceClient;
using WebServiceClient.Clients;
using LicenseHelper = Commons.LicenseHelper;

namespace MainApp
{
    /// <summary>
    /// Classe per gestire le operazioni sui dati del modello (anche) che riguardano più sezioni
    /// </summary>
    public class MainOperation : IMainOperation
    {
        MainMenuView _mainMenuView = null;
        MainMenuView MainMenuView { get => _mainMenuView; }

        Model3dFiltersData model3dFilters;
        
        public event EventHandler ProgressChanged;
        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private readonly BackgroundWorker Worker;
        private readonly ICommand InstigateWorkCommand;

        public MainOperation(MainMenuView mainMenuView)
        {
            _mainMenuView = mainMenuView;

            this.InstigateWorkCommand =
            new DelegateCommand(o => this.Worker.RunWorkerAsync(),
                o => !this.Worker.IsBusy);

            this.Worker = new BackgroundWorker();
            this.Worker.WorkerReportsProgress = true;
            this.Worker.ProgressChanged += this.MainMenuViewProgressChanged;
        }

        

        private void MainMenuViewProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainMenuView.MessageBarView.ProgressValue = e.ProgressPercentage;
        }

        //private void ApplyComputoRulesDoWork(object sender, DoWorkEventArgs e)
        //{
        //    this.ApplyComputoRulesDoWork();
        //}


        /// <summary>
        /// Aggiunge elementi alla sezione Elementi
        /// </summary>
        /// <param name="elmsKey"></param>
        public void AddElementi(List<Model3dObjectKey> elmsKey, bool addToGroup)
        {  
            
            try
            {

                List<Model3dElementRelation> rel = new List<Model3dElementRelation>();

                EntitiesHelper entsHelper = new EntitiesHelper(MainMenuView.ClientDataService);
                EntityType elmType = entsHelper.GetEntityType(BuiltInCodes.EntityType.Elementi);


                elmsKey.ForEach(item =>
                {
                    Dictionary<string, HashSet<DivisioneItemId>> divisioniKey = new Dictionary<string, HashSet<DivisioneItemId>>();
                    CreateDivisioniItemIdMap(elmType, divisioniKey);


                    rel.Add(new Model3dElementRelation()
                    {
                        Model3dObject = item,
                        Divisioni = divisioniKey,
                    });
                });

                

                Guid groupId = Guid.Empty;
                if (addToGroup)
                {
                    DivisioneItemType divItemType = entsHelper.GetDivisioneTypeByCodice(BuiltInCodes.Attributo.IfcGroup);
                    if (divItemType == null)
                    {
                        MainMenuView.MainOperation.ShowMessageBarView(string.Format("{0}: {1}", LocalizationProvider.GetString("Suddivisione non trovata"), BuiltInCodes.Attributo.IfcGroup));
                        return;
                    }

                    List<Guid> selectedGroup = new List<Guid>();
                    MainMenuView.WindowService.SelectDivisioneIdsWindow(divItemType.DivisioneId, ref selectedGroup, LocalizationProvider.GetString("Seleziona un gruppo"), SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.AllowNoSelection, null);

                    groupId = selectedGroup.FirstOrDefault();

                    if (groupId != Guid.Empty)
                    {
                        Entity group = MainMenuView.ClientDataService.GetEntityById(divItemType.GetKey(), groupId);
                        string projectGlobalId = entsHelper.GetValorePlainText(group, BuiltInCodes.Attributo.ProjectGlobalId, false, false);

                        if (!string.IsNullOrEmpty(projectGlobalId))
                        {
                            MainMenuView.MainOperation.ShowMessageBarView("Non è possibile aggiungere un elemento ad un gruppo del modello 3d");
                            return;
                        }
                    }

                    

                }
                
                MainMenuView.WindowService.ShowWaitCursor(true);
                
                //if (DeveloperVariables.IsTesting)
                MainMenuView.DivisioniView.AddDivisioniItemsByModel3d(rel);
                //else
                //MainMenuView.DivisioniView.AddDivisioniItemsByModel3d(rel);

                ElementiView elmView = MainMenuView.ElementiView as ElementiView;
                elmView.AddElementiItemsByModel3d(rel, groupId);
            }
            finally
            {
                MainMenuView.WindowService.ShowWaitCursor(false);
            }
        }

        private static void CreateDivisioniItemIdMap(EntityType elmType, Dictionary<string, HashSet<DivisioneItemId>> divisioniKey)
        {
            ////////////////////////////////////
            //costruisco la lista delle divisioni collegate agli elementi

            IEnumerable<Attributo> guidAtts = elmType.Attributi.Values
                .Where(item => (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid || item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                                            && item.GuidReferenceEntityTypeKey.StartsWith(BuiltInCodes.EntityType.Divisione));

            foreach (var guidAtt in guidAtts)
            {
                var divItemId = new DivisioneItemId();
                divItemId.EntityTypeKey = guidAtt.GuidReferenceEntityTypeKey;

                if (guidAtt.ValoreAttributo is ValoreAttributoGuid valAttGuid)
                {
                    divItemId.ItemPath = valAttGuid.ItemPath;
                }

                divisioniKey.TryAdd(guidAtt.GuidReferenceEntityTypeKey, new HashSet<DivisioneItemId>());
                divisioniKey[guidAtt.GuidReferenceEntityTypeKey].Add(divItemId);
            }

            ////////////////////////////////////
        }

        public void UpgradeProjectToLastVersion(int projectVersion)
        {
            try
            {
                ClientDataService dataService = MainMenuView.ClientDataService;

                Dictionary<string, DefinizioneAttributo> defAtts = dataService.GetDefinizioniAttributo();
                Dictionary<string, EntityType> entTypes = MainMenuView.ClientDataService.GetEntityTypes();
                EntitiesHelper entitiesHelper = new EntitiesHelper(dataService);

                List<string> entTypesKey = entTypes.Keys.ToList();

                List<string> entTypesKeyOrdered = entitiesHelper.GetEntityTypeDependencyOrder();

                bool forceUpdateEntities = false;
                if (projectVersion < (int) FileVersion.v103) 
                    forceUpdateEntities = true;

                if (projectVersion < (int)FileVersion.v104)
                {
                    foreach (string key in entTypesKeyOrdered)
                    {
                        EntityType entType = entTypes[key];
                        foreach (Attributo att in entType.Attributi.Values)
                        {
                            if (att is AttributoRiferimento attRif)
                            {
                                Attributo attSource = entitiesHelper.GetSourceAttributo(attRif);

                                if (attSource != null && attSource.AllowMasterGrouping == false)
                                    attRif.AllowMasterGrouping = attSource.AllowMasterGrouping;
                            }
                        }
                    }
                }

                //////////////////////////////////////////////////
                //aggiornamento di EntityTypes e Entities
                IEnumerable<string> entTypesParentKeyOrdered = entTypesKey.Where(item =>
                {
                    TreeEntityType treeEntType = entTypes[item] as TreeEntityType;
                    if (treeEntType != null)
                        return treeEntType.IsParentType();
                    return false;
                });

                //aggiorno gli attributi dei parent di tutti gli entity type
                foreach (string key in entTypesParentKeyOrdered)
                {
                    EntityType entType = entTypes[key];

                    EntityType clone = entType.Clone();
                    clone.CreaAttributi(defAtts, entTypes);
                    dataService.SetEntityType(clone, false, forceUpdateEntities);
                }

                //aggiorno tutti gli altri attributi di ent foglie
                foreach (string key in entTypesKeyOrdered)
                {
                    EntityType entType = entTypes[key];

                    EntityType clone = entType.Clone();
                    clone.CreaAttributi(defAtts, entTypes);
                    dataService.SetEntityType(clone, false, forceUpdateEntities);//operazione lenta...
                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

        }



        //internal async Task ApplyComputoRulesAsync()
        internal void ApplyComputoRules()
        {

            ApplyComputoRulesWindow applyComputoRulesWnd = new ApplyComputoRulesWindow();
            applyComputoRulesWnd.SourceInitialized += (x, y) => applyComputoRulesWnd.HideMinimizeAndMaximizeButtons();
            //applyComputoRulesWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            applyComputoRulesWnd.Owner = Application.Current.MainWindow;
            applyComputoRulesWnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            List<Guid> ids = null;
            MainMenuView.ClientDataService.GetFilteredEntities(BuiltInCodes.EntityType.Computo, null, null, null, out ids);

            if (ids.Count > 0)
            {
                //se il computo ha almeno una voce

                if (applyComputoRulesWnd.ShowDialog() == true)
                {
                    MainMenuView.TabControlSelectedIndex = (int)SectionEnum.Computo;//computo
                    MainMenuView.MainView.WindowActivate();

                    ApplyComputoRulesDoWork(applyComputoRulesWnd.View.Data);
                }
            }
            else
            {
                MainMenuView.TabControlSelectedIndex = (int)SectionEnum.Computo;//computo
                MainMenuView.MainView.WindowActivate();

                ApplyComputoRulesDoWork(applyComputoRulesWnd.View.Data);
            }

            var model3dFilesInfo = MainMenuView.ClientDataService.GetModel3dFilesInfo();





        }


        private void ApplyComputoRulesDoWork(ApplyComputoRulesData data)
        {
            if (MainMenuView.Model3dService.GetModel3dType() == Model3dType.Ifc)
            {
                if (MainMenuView.BIMViewerWindow != null)
                {
                    MainMenuView.BIMViewerWindow.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        ApplyComputoRulesDoWork2(data);

                    }), DispatcherPriority.Background);
                }

            }
            else if (MainMenuView.Model3dService.GetModel3dType() == Model3dType.Revit)
            {
                ApplyComputoRulesDoWork2(data);
            }



        }

        private async Task UpdateApplyComputoRulesMainMenuViewProgress(int progressValue, string msg)
        {
            int progressValuePrec = MainMenuView.MessageBarView.ProgressValue;

            for (int i = progressValuePrec + 1; i < progressValue; i++)
            {

                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    MainMenuView.MessageBarView.Show(msg, false, i);
                    
                }, DispatcherPriority.ApplicationIdle);

                Thread.Sleep(20);
            }
            
        }

        //private async void  ApplyComputoRulesDoWork2(ApplyComputoRulesData data)
        //{
        //    await UpdateMainMenuViewProgress(0);
        //    Dictionary<Model3dFilterData, List<Model3dObjectKey>> model3dObjectKeyByFilter;


        //    ClientDataService dataService = MainMenuView.ClientDataService;

        //    ////////////////////////
        //    ////Pulisce tutti i valori precedentemente calcolati derivati da ifc

        //    ////lato client
        //    //MainAppM3dCalculatorFunction mainAppM3dCalcFunc = MainMenuView.CalculatorFunctions[MainAppM3dCalculatorFunction.Name] as MainAppM3dCalculatorFunction;
        //    //mainAppM3dCalcFunc.Clear();

        //    ////lato service
        //    //dataService.ClearValuesFromModel3d();
        //    ///////////////////////

        //    model3dFilters = dataService.GetModel3dFiltersData();

        //    //mappa degli oggetti del modello 3d divisi per filtro 
        //    model3dObjectKeyByFilter = new Dictionary<Model3dFilterData, List<Model3dObjectKey>>();
        //    foreach (Model3dFilterData filter in model3dFilters.Items) //per ogni filtro
        //    {
        //        //bypasso i filtri senza regole
        //        if (!filter.RulesComputo.Any())
        //            continue;

        //        List<Model3dObjectKey> elemsFilter = MainMenuView.Model3dService.GetElementsByFilter(filter.Id);
        //        if (elemsFilter != null)
        //            model3dObjectKeyByFilter[filter] = elemsFilter;
        //        else
        //            model3dObjectKeyByFilter[filter] = new List<Model3dObjectKey>();
        //    }
        //    MainMenuView.WindowService.ShowWaitCursor(true);
        //    await UpdateMainMenuViewProgress(20);

        //    ////Lista degli oggetti 3d di tutti i filtri            
        //    //List<Model3dObjectKey> model3dObjectsKey = new HashSet<Model3dObjectKey>(model3dObjectKeyByFilter.SelectMany(item => item.Value), new Model3dObjectKeyEqualityComparer()).ToList();

        //    ////inserisco i nuovi elementi e le nuove divisioni
        //    //List <Model3dElementRelation> rel = new List<Model3dElementRelation>();
        //    //model3dObjectsKey.ForEach(item => rel.Add(new Model3dElementRelation() { Model3dObject = item }));

        //    //Lista degli oggetti 3d di tutti i filtri    
        //    List<string> model3dObjectsKey = new HashSet<string>(model3dObjectKeyByFilter.Values.SelectMany(item => item.Select(item1 => item1.GetKey()))).ToList();

        //    //inserisco i nuovi elementi e le nuove divisioni
        //    List<Model3dElementRelation> rel = new List<Model3dElementRelation>();
        //    model3dObjectsKey.ForEach(item => rel.Add(new Model3dElementRelation() { Model3dObject = new Model3dObjectKey(item) }));


        //    //Aggiungo/aggiorno le divisioni
        //    MainMenuView.DivisioniView.AddDivisioniItemsByModel3d(rel);

        //    await UpdateMainMenuViewProgress(40);

        //    //Aggiungo/aggiorno gli elementi
        //    ElementiView elmView = MainMenuView.ElementiView as ElementiView;
        //    Dictionary<Model3dObjectKey, Guid> elementiItemsId = elmView.AddElementiItemsByModel3d(rel);

        //    await UpdateMainMenuViewProgress(60);

        //    //costruisco una nuova mappa dei computoItems già presenti per controllare le voci da non duplicare
        //    string separator = "|";
        //    Dictionary<string, Guid> existingIds = MainMenuView.ClientDataService.CreateKey(MainMenuView.ComputoView.ComputoItemsView.EntityType.GetKey(), separator, new List<string>() { BuiltInCodes.Attributo.Model3dRuleId, BuiltInCodes.Attributo.Model3dRule, BuiltInCodes.Attributo.ElementiItem_Guid });//234234|Walls|5677545

        //    //mappa inversa (id di computoItem => key)
        //    Dictionary<Guid, string> existingKeys = existingIds.ToDictionary(item => item.Value, item => item.Key);


        //    //Casi possibili per una voce di computo al momento dell'applicazione delle regole
        //    //1.Precedentemente aggiunta dall'utente
        //    //2.Precedentemente aggiunta da una regola e non modificata dall'utente (qta o articolo)
        //    //3.Precedentemente aggiunta da una regola e modificata dall'utente (qta o articolo)
        //    //4.Precedentemente assente
        //    //5.Precedentemente aggiunta da una regola che non è più presente 

        //    //costruisco la struttura degli elementi da aggiungere o modificare o eliminare nel computo
        //    List<ComputoItemByRule> computoItems = new List<ComputoItemByRule>();
        //    foreach (Model3dFilterData filter in model3dFilters.Items) //per ogni filtro
        //    {
        //        foreach (Model3dRuleComputo rule in filter.RulesComputo) //per ogni regola
        //        {
        //            foreach (Model3dObjectKey m3dObjKey in model3dObjectKeyByFilter[filter])//per ogni elemento nel filtro
        //            {
        //                string keyOriginalRule = string.Join(separator, rule.Id, filter.Descri, elementiItemsId[m3dObjKey]);
        //                string keyModifiedRule = string.Join(separator, rule.Id, string.Format("{0}{1}", filter.Descri, "*"), elementiItemsId[m3dObjKey]);

        //                Guid existingComputoItemId = Guid.Empty;
        //                if (existingIds.ContainsKey(keyOriginalRule)) //voce precedentemente introdotta dalla stessa regola e non modificata dall'utente
        //                {
        //                    existingComputoItemId = existingIds[keyOriginalRule];
        //                    existingIds.Remove(keyOriginalRule);
        //                    existingKeys.Remove(existingComputoItemId);
        //                }
        //                else if (existingIds.ContainsKey(keyModifiedRule)) //voce precedentemente introdotta dalla stessa regola e modificata dall'utente
        //                {
        //                    existingComputoItemId = existingIds[keyModifiedRule];
        //                    existingIds.Remove(keyModifiedRule);
        //                    existingKeys.Remove(existingComputoItemId);
        //                    continue;
        //                }

        //                //item da aggiungere o modificare
        //                computoItems.Add(new ComputoItemByRule()
        //                {
        //                    ElementiItemId = elementiItemsId[m3dObjKey],
        //                    Filter = filter,
        //                    Rule = rule,
        //                    PrezzarioItemId = Guid.Empty,
        //                    ExistingComputoItemId = existingComputoItemId,
        //                    ToRemove = false,
        //                });


        //            }
        //        }
        //    }

        //    //tolgo dagli existing gli items che non derivano da una regola
        //    foreach (string key in existingIds.Keys)
        //    {
        //        //rem Temp
        //        //n.b. E`stata commentata per non togliere dagli existing oltre a quelli senza regola 
        //        //anche gli elementi precedentemente computati.
        //        //Precedentemente computati vuol dire computati (applica regola) prima che venisse 
        //        //aperto il modello con peso leggero.
        //        //if (key.StartsWith(Guid.Empty.ToString()))//items che non derivano da una regola
        //            //existingKeys.Remove(existingIds[key]);
        //        //end Rem

        //        if (data.SelectedItem == ApplyComputoRulesEnum.Ignore)
        //            existingKeys.Remove(existingIds[key]);
        //        else if (data.SelectedItem == ApplyComputoRulesEnum.Remove)
        //        {
        //            if (key.StartsWith(Guid.Empty.ToString()))//items che non derivano da una regola
        //                existingKeys.Remove(existingIds[key]);
        //        }

        //    }


        //    //items rimasti provenienti da regole non più esistenti (da eliminare)
        //    foreach (Guid id in existingKeys.Keys)
        //    {
        //        computoItems.Add(new ComputoItemByRule()
        //        {
        //            ToRemove = true,
        //            ExistingComputoItemId = id,
        //        });

        //        existingIds.Remove(existingKeys[id]);
        //    }

        //    await UpdateMainMenuViewProgress(80);


        //    MainMenuView.ComputoView.ApplyRules(computoItems);

        //    await UpdateMainMenuViewProgress(100);

        //    MainMenuView.MessageBarView.Ok();
        //    MainMenuView.WindowService.ShowWaitCursor(false);
        //}

        private async void ApplyComputoRulesDoWork2(ApplyComputoRulesData data)
        {

            UndoGroupBegin(UndoGroupsName.ApplyComputoRules, null);

            string msgBase = LocalizationProvider.GetString("ApplicazioneRegoleInCorso");
            string msg = string.Format("{0} {1}", msgBase, LocalizationProvider.GetString("Filtri2"));


            Model3dType model3dServiceType = MainMenuView.Model3dService.GetModel3dType();

            await UpdateApplyComputoRulesMainMenuViewProgress(0, msgBase);
            ClientDataService dataService = MainMenuView.ClientDataService;

            model3dFilters = dataService.GetModel3dFiltersData();

            //mappa degli oggetti del modello 3d divisi per filtro 
            Dictionary<Guid, List<string>> model3dObjectKeyByFilter = new Dictionary<Guid, List<string>>(); //Key Model3dFilterData.Id
            foreach (Model3dFilterData filter in model3dFilters.Items) //per ogni filtro
            {
                //bypasso i filtri senza regole
                if (!filter.RulesComputo.Any())
                    continue;

                List<Model3dObjectKey> elemsFilter = MainMenuView.Model3dService.GetElementsByFilter(filter.Id);


                if (elemsFilter != null)
                    model3dObjectKeyByFilter[filter.Id] = elemsFilter.Select(item => item.GetKey()).ToList();
                else
                    model3dObjectKeyByFilter[filter.Id] = new List<string>();
            }


            MainMenuView.WindowService.ShowWaitCursor(true);
            msg = string.Format("{0} {1}", msgBase, LocalizationProvider.GetString("Divisioni"));
            await UpdateApplyComputoRulesMainMenuViewProgress(20, msg);

            //Lista degli oggetti 3d di tutti i filtri    
            List<string> model3dObjectsKey = new HashSet<string>(model3dObjectKeyByFilter.Values.SelectMany(item => item)).ToList();


            //costruisco una nuova mappa dei computoItems già presenti per controllare le voci da non duplicare
            string separator = "|";
            Dictionary<Guid, string> keysById = null;

            Dictionary<string, Guid> existingIds = MainMenuView.ClientDataService.CreateKey(MainMenuView.ComputoView.ComputoItemsView.EntityType.GetKey(), separator,
                new List<string>() { BuiltInCodes.Attributo.Model3dRuleId, BuiltInCodes.Attributo.Model3dRule, BuiltInCodes.Attributo.ElementiItem_Guid },//234234|Walls|5677545
                out keysById);

            //mappa inversa (id di computoItem => key)
            Dictionary<Guid, string> existingKeys = existingIds.ToDictionary(item => item.Value, item => item.Key);


            List<Model3dElementRelation> rel = new List<Model3dElementRelation>();

            EntitiesHelper entsHelper = new EntitiesHelper(MainMenuView.ClientDataService);
            EntityType elmType = entsHelper.GetEntityType(BuiltInCodes.EntityType.Elementi);

            //inserisco i nuovi elementi e le nuove divisioni

            model3dObjectsKey.ForEach(item =>
            {
                Dictionary<string, HashSet<DivisioneItemId>> divisioniKey = new Dictionary<string, HashSet<DivisioneItemId>>();
                CreateDivisioniItemIdMap(elmType, divisioniKey);

                rel.Add(new Model3dElementRelation()
                {
                    Model3dObject = new Model3dObjectKey(item) { Model3DType = model3dServiceType },
                    Divisioni = divisioniKey,
                });
            });


            //Aggiungo/aggiorno le divisioni
            //if (DeveloperVariables.IsTesting)
                MainMenuView.DivisioniView.AddDivisioniItemsByModel3d(rel);
            //else
            //    MainMenuView.DivisioniView.AddDivisioniItemsByModel3d_old(rel);

            msg = string.Format("{0} {1}", msgBase, LocalizationProvider.GetString("Elementi2"));
            await UpdateApplyComputoRulesMainMenuViewProgress(40, msg);

            //Aggiungo/aggiorno gli elementi
            ElementiView elmView = MainMenuView.ElementiView as ElementiView;
            Dictionary<string, Guid> elementiItemsId = elmView.AddElementiItemsByModel3d(rel, Guid.Empty);

            msg = string.Format("{0} {1}", msgBase, LocalizationProvider.GetString("Computo"));
            await UpdateApplyComputoRulesMainMenuViewProgress(60, msg);




            //Casi possibili per una voce di computo al momento dell'applicazione delle regole
            //1.Precedentemente aggiunta dall'utente
            //2.Precedentemente aggiunta da una regola e non modificata dall'utente (qta o articolo)
            //3.Precedentemente aggiunta da una regola e modificata dall'utente (qta o articolo)
            //4.Precedentemente assente
            //5.Precedentemente aggiunta da una regola che non è più presente 

            //costruisco la struttura degli elementi da aggiungere o modificare o eliminare nel computo
            List<ComputoItemByRule> computoItems = new List<ComputoItemByRule>();
            foreach (Model3dFilterData filter in model3dFilters.Items) //per ogni filtro
            {
                foreach (Model3dRuleComputo rule in filter.RulesComputo) //per ogni regola
                {
                    foreach (string m3dObjKey in model3dObjectKeyByFilter[filter.Id])//per ogni elemento nel filtro
                    {
                        string keyOriginalRule = string.Join(separator, rule.Id, filter.Descri, elementiItemsId[m3dObjKey]);
                        string keyModifiedRule = string.Join(separator, rule.Id, string.Format("{0}{1}", filter.Descri, "*"), elementiItemsId[m3dObjKey]);

                        Guid existingComputoItemId = Guid.Empty;
                        if (existingIds.ContainsKey(keyOriginalRule)) //voce precedentemente introdotta dalla stessa regola e non modificata dall'utente
                        {
                            existingComputoItemId = existingIds[keyOriginalRule];
                            existingIds.Remove(keyOriginalRule);
                            existingKeys.Remove(existingComputoItemId);
                        }
                        else if (existingIds.ContainsKey(keyModifiedRule)) //voce precedentemente introdotta dalla stessa regola e modificata dall'utente
                        {
                            existingComputoItemId = existingIds[keyModifiedRule];
                            existingIds.Remove(keyModifiedRule);
                            existingKeys.Remove(existingComputoItemId);
                            continue;
                        }

                        //item da aggiungere o modificare
                        computoItems.Add(new ComputoItemByRule()
                        {
                            ElementiItemId = elementiItemsId[m3dObjKey],
                            Filter = filter,
                            Rule = rule,
                            PrezzarioItemId = Guid.Empty,
                            ExistingComputoItemId = existingComputoItemId,
                            ToRemove = false,
                        });


                    }
                }
            }

            //tolgo dagli existing gli items che non derivano da una regola
            foreach (string key in existingIds.Keys)
            {
                //rem Temp
                //n.b. E`stata commentata per non togliere dagli existing oltre a quelli senza regola 
                //anche gli elementi precedentemente computati.
                //Precedentemente computati vuol dire computati (applica regola) prima che venisse 
                //aperto il modello con peso leggero.
                //if (key.StartsWith(Guid.Empty.ToString()))//items che non derivano da una regola
                //existingKeys.Remove(existingIds[key]);
                //end Rem

                if (data.SelectedItem == ApplyComputoRulesEnum.Ignore)
                    existingKeys.Remove(existingIds[key]);
                else if (data.SelectedItem == ApplyComputoRulesEnum.Remove)
                {
                    if (key.StartsWith(Guid.Empty.ToString()))//items che non derivano da una regola
                        existingKeys.Remove(existingIds[key]);
                }

            }



            foreach (string key in existingKeys.Values)
            {
                IEnumerable<Guid> ids = keysById.Where(item => item.Value == key).Select(item => item.Key);
                foreach (Guid id in ids)
                {
                    computoItems.Add(new ComputoItemByRule()
                    {
                        ToRemove = true,
                        ExistingComputoItemId = id,
                    });
                }
            }


            msg = string.Format("{0} {1}", msgBase, LocalizationProvider.GetString("Regole"));
            await UpdateApplyComputoRulesMainMenuViewProgress(80, msg);

            MainMenuView.ComputoView.ApplyRules(computoItems);



            await UpdateApplyComputoRulesMainMenuViewProgress(100, msgBase);

            MainMenuView.MessageBarView.Ok();
            MainMenuView.WindowService.ShowWaitCursor(false);

            UndoGroupEnd();
        }

        internal void SelectItemsByModel3d(List<Model3dObjectKey> elementsId)
        {
            EntitiesListMasterDetailView currentMaster = null;

            switch (MainMenuView.CurrentSection)
            {
                case SectionEnum.Elementi:
                    MainMenuView.ElementiView.SelectItemsByModel3d(elementsId);
                    break;
                case SectionEnum.Computo:
                    MainMenuView.ComputoView.SelectItemsByModel3d(elementsId);
                    break;
                case SectionEnum.Attivita:
                    SectionItemTemplateView currentView = MainMenuView.AttivitaView.CurrentTemplateView;
                    if (currentView != null && currentView is WBSView)
                        MainMenuView.AttivitaView.WBSView.SelectItemsByModel3d(elementsId);
                    break;
                default:
                    {
                        MainMenuView.MessageBarView.Show(LocalizationProvider.GetString("NessunElementoTrovatoNellaSezioneCorrente"));
                        break;
                    }
            }
        }

        /// <summary>
        /// Aggiorna gli altri EntityTypes dipendenti a seguito di modifica di un EntityType
        /// </summary>
        /// <param name="updatedEntityTypeCodice"></param>
        public void UpdateEntityTypesView(List<string> entityTypesKey)
        {
            

            foreach (string entityTypeKey in entityTypesKey)
            {
                Dictionary<string, EntityType> entTypes = MainMenuView.ClientDataService.GetEntityTypes();
                if (!entTypes.ContainsKey(entityTypeKey))
                    continue;

                EntityType entType = entTypes[entityTypeKey];

                if (entType is DivisioneItemType)
                {
                    MainMenuView.DivisioniView.CurrentDivisioneView.UpdateEntityType();
                }
                else
                {
                    if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
                        MainMenuView.DatiGeneraliView.ContattiView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
                        MainMenuView.ElencoPrezziView.CapitoliView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.InfoProgetto)
                        MainMenuView.DatiGeneraliView.InfoProgettoView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
                        MainMenuView.ElencoPrezziView.PrezzarioView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
                        MainMenuView.ElementiView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Computo)
                        MainMenuView.ComputoView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Report)
                        MainMenuView.StampeView.ReportView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Documenti)
                        MainMenuView.StampeView.DocumentiView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Stili)
                        MainMenuView.DatiGeneraliView.StiliProgettoView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.WBS)
                        MainMenuView.AttivitaView.WBSView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Calendari)
                        MainMenuView.AttivitaView.CalendariView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Variabili)
                        MainMenuView.DatiGeneraliView.VariabiliView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Allegati)
                        MainMenuView.DatiGeneraliView.AllegatiView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.Tag)
                        MainMenuView.DatiGeneraliView.TagView.UpdateEntityType();

                    if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
                        MainMenuView.AttivitaView.ElencoAttivitaView.UpdateEntityType();
                }
            }

            
        }

        public void ShowMessageBarView(string msg, bool isOkButtonVisible = true, int progressValue = -1, bool close = false)
        {
            if (close)
                MainMenuView.MessageBarView.Ok();
            else
                MainMenuView.MessageBarView.Show(msg, isOkButtonVisible, progressValue);
        }

        public void ShowQuestionMessageBarView(string msg)
        {
            MainMenuView.MessageBarView.ShowQuestion(msg);
        }

        public Dictionary<string, IDataService> GetPrezzariCache()
        {
            return MainMenuView.PrezzariCache;
        }

        public string GetPrezzariFolder()
        {
            string path = null;

            if (!Path.IsPathRooted(MainMenuView.AppSettings.PrezzariPath))
                path = string.Format("{0}{1}", MainMenuView.AppSettingsPath, MainMenuView.AppSettings.PrezzariPath);
            else
                path = MainMenuView.AppSettings.PrezzariPath;

            return path;
        }
        
        public string GetModelliFolder()
        {
            string path = null;

            if (!Path.IsPathRooted(MainMenuView.AppSettings.ModelliPath))
                path = string.Format("{0}{1}", MainMenuView.AppSettingsPath, MainMenuView.AppSettings.ModelliPath);
            else
                path = MainMenuView.AppSettings.ModelliPath;

            return path;
        }

        public string GetAppSettingsPath()
        {
            return MainMenuView.AppSettingsPath;
        }

        public IDataService GetDataServiceByFile(string fullFileName, out Int32 projectFileVersion)
        {
            projectFileVersion = 0;

            if (!File.Exists(fullFileName))
                return null;

            using (var file = File.OpenRead(fullFileName))
            {
                var pStream = new ProgressStream(file);
                pStream.BytesRead +=
                    new ProgressStreamReportDelegate(pStream_BytesRead);


                //BinaryReader reader = new BinaryReader(file);
                //projectFileVersion = reader.ReadInt32();


                //Project project = null;
                //if (projectFileVersion <= MainMenuView.CurrentFileVersion)
                //{
                //    project = Serializer.Deserialize<Project>(pStream);
                //}
                //else
                //{
                //    //oss: non può stare qua perchè potrebbe essere in corso la progressbar
                //    //ShowMessageBarView(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                //}

                Project project = null;
                project = ModelSerializer.Deserialize(pStream, out _);



                if (project == null)
                    return null;

                //spedisco il project al server
                ProjectService projectService = new ProjectService();
                projectService.Init(project/*, new FileRepository()*/);

                return projectService;
            }
        }

        void pStream_BytesRead(object sender,
                                      ProgressStreamReportEventArgs args)
        {
            int perc = (int) ((100.0 * args.StreamPosition) / args.StreamLength);

            OnProgressChanged(new ProgressChangedEventArgs(perc, null));

            //Console.WriteLine(
            //    string.Format("{2} bytes moved | {0} of {1} total",
            //        args.StreamPosition,
            //        args.StreamLength,
            //        args.BytesMoved
            //    )
           //);
        }

        public bool IsProjectClosing()
        {
            return _mainMenuView.IsProjectClosing;
        }
        public bool IsAdvancedMode()
        {
            return MainViewStatus.IsAdvancedMode;
        }

        public IWindowService GetWindowService()
        {
            return _mainMenuView.WindowService;
        }


        public void ImportModel(IDataService sourceDataService)
        {
            try
            {
                if (sourceDataService == null)
                    return;


                IDataService targetDataService = MainMenuView.ClientDataService;


                Dictionary<string, EntityType> targetEntTypes = targetDataService.GetEntityTypes();
                Dictionary<string, EntityType> sourceEntTypes = sourceDataService.GetEntityTypes();

                EntitiesHelper targetEntsHelper = new EntitiesHelper(targetDataService);
                EntitiesHelper sourceEntsHelper = new EntitiesHelper(sourceDataService);

                //keys delle divisoni da sostituire nei riferimenti
                //key: old, value: new
                Dictionary<string, string> replaceDivKeys = new Dictionary<string, string>();

                //codici di attributo da sostituire nei riferimenti
                //key: source "entityTypeKey"
                Dictionary<string, AttributiImportRevision> replaceAttributiCodice = new Dictionary<string, AttributiImportRevision>();

                //codici delle divisoni modificati
                //key: old, value: new
                //Dictionary<string, string> replaceDivsCodice = new Dictionary<string, string>();
                List<EntityTypeCodiceRevision> replaceDivsCodice = new List<EntityTypeCodiceRevision>();

                HashSet<string> divCodes = new HashSet<string>();
                HashSet<string> divParentCodes = new HashSet<string>();



                //ordino per dipendenza

                //IOrderedEnumerable<EntityType> sourceOrderedEntTypes = sourceEntTypes.Values.OrderBy(item => item.DependentTypesEnum, new DependentEntityTypesComparer());
                var sourceOrderedEntTypesCodice = sourceEntsHelper.GetEntityTypeDependencyOrder();
                IEnumerable<EntityType> sourceOrderedEntTypes = sourceOrderedEntTypesCodice.Select(item => sourceEntTypes[item]);

                foreach (EntityType sourceEntType in sourceOrderedEntTypes)
                {
                    //1.creare le EntityType mancanti (tipicamente divisioni) con un codice univoco


                    EntityType targetEntType = null;
                    if (EntitiesHelper.IsCodiceBuiltIn(sourceEntType.Codice))
                        targetEntType = targetEntTypes.Values.FirstOrDefault(item => item.Codice == sourceEntType.Codice && sourceEntType.IsParentType() == item.IsParentType());
                    else//divisioni create dal cliente
                        targetEntType = targetEntTypes.Values.FirstOrDefault(item => item.Name == sourceEntType.Name && sourceEntType.IsParentType() == item.IsParentType());



                    EntityType newTargetEntType = null;

                    if (targetEntType != null)
                        newTargetEntType = targetEntType.Clone();


                    if (sourceEntType is DivisioneItemType)
                    {
                        if (!divCodes.Contains(sourceEntType.Codice))
                            divCodes.Add(sourceEntType.Codice);
                        else
                        {
                            ShowMessageBarView(string.Format("{0}: {1}", LocalizationProvider.GetString("DueDivisioniConStessoCodice"), sourceEntType.Codice));
                            return;
                        }

                        if (targetEntType == null)//divisione non presente nel target
                        {
                            DivisioneItemType sourceDivType = sourceEntType as DivisioneItemType;

                            string cod = sourceDivType.Codice;
                            if (!EntitiesHelper.IsCodiceBuiltIn(sourceDivType.Codice))
                            {
                                cod = targetEntsHelper.GetNewCodiceDivisione();
                                //replaceDivsCodice.Add(sourceDivType.Codice, cod);
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = sourceDivType.Codice, NewEntityTypeCodice = cod });
                            }
                            else
                            {
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = string.Empty, NewEntityTypeCodice = cod });
                            }

                            Guid newDivId = targetDataService.AddDivisione(cod, sourceDivType.Name, sourceDivType.Model3dClassName);
                            if (replaceDivKeys.ContainsKey(sourceEntType.GetKey()))
                            {
                            }
                            replaceDivKeys.Add(sourceEntType.GetKey(), DivisioneItemType.CreateKey(newDivId));
                            newTargetEntType = targetEntsHelper.GetDivisioneTypeById(newDivId);
                        }
                        else //divisione presente nel target
                        {
                            

                            DivisioneItemType targetDivType = targetEntType as DivisioneItemType;
                            if (replaceDivKeys.ContainsKey(sourceEntType.GetKey()))
                            {
                            }
                            replaceDivKeys.Add(sourceEntType.GetKey(), targetDivType.GetKey());

                            DivisioneItemType sourceDivType = sourceEntType as DivisioneItemType;
                            if (sourceDivType.Codice != targetDivType.Codice)
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = sourceDivType.Codice, NewEntityTypeCodice = targetDivType.Codice });
                        }

                    }
                    else if (sourceEntType is DivisioneItemParentType)
                    {
                        if (!divParentCodes.Contains(sourceEntType.Codice))
                            divParentCodes.Add(sourceEntType.Codice);
                        else
                        {
                            ShowMessageBarView(string.Format("{0}: {1}", LocalizationProvider.GetString("DueDivisioniConStessoCodice"), sourceEntType.Codice));
                            return;
                        }

                        if (targetEntType == null)//divisione parent non presente nel target
                        {
                            DivisioneItemParentType sourceDivType = sourceEntType as DivisioneItemParentType;

                            string cod = sourceDivType.Codice;
                            if (!EntitiesHelper.IsCodiceBuiltIn(sourceDivType.Codice))
                            {
                                cod = targetEntsHelper.GetNewCodiceDivisione();
                                //replaceDivsCodice.Add(sourceDivType.Codice, cod);
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = sourceDivType.Codice, NewEntityTypeCodice = cod });
                            }
                            else
                            {
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = string.Empty, NewEntityTypeCodice = cod });
                            }

                            Guid newDivId = targetDataService.AddDivisione(cod, sourceDivType.Name, sourceDivType.Model3dClassName);
                            if (replaceDivKeys.ContainsKey(sourceEntType.GetKey()))
                            {
                            }
                            replaceDivKeys.Add(sourceEntType.GetKey(), DivisioneItemParentType.CreateKey(newDivId));
                            newTargetEntType = targetEntsHelper.GetDivisioneTypeById(newDivId);
                        }
                        else
                        {
                            DivisioneItemParentType targetDivType = targetEntType as DivisioneItemParentType;
                            if (replaceDivKeys.ContainsKey(sourceEntType.GetKey()))
                            {
                            }
                            replaceDivKeys.Add(sourceEntType.GetKey(), targetDivType.GetKey());

                            DivisioneItemParentType sourceDivType = sourceEntType as DivisioneItemParentType;
                            if (sourceDivType.Codice != targetDivType.Codice)
                                replaceDivsCodice.Add(new EntityTypeCodiceRevision() { OldEntityTypeCodice = sourceDivType.Codice, NewEntityTypeCodice = targetDivType.Codice });
                        }
                    }
                    else
                    {

                    }


                    if (newTargetEntType == null)
                    {
                    }

                    //Update codici divisioni negli attributi
                    string sourceEntityTypeKey = sourceEntType.GetKey();


                    if (sourceEntType.IsCustomizable())//Se è una EntityType nella quale l'utente può aggiungere/togliere/modificare attributi
                    {
                        EntityType sourceEntTypeClone = sourceEntType.Clone();

                        foreach (Attributo att in sourceEntTypeClone.Attributi.Values)
                        {

                            ////////////////////////////////////////////////////////////
                            //Aggiornamento dei riferimenti alle divisioni negli attributi
                            if (replaceDivKeys.ContainsKey(att.GuidReferenceEntityTypeKey))
                                att.GuidReferenceEntityTypeKey = replaceDivKeys[att.GuidReferenceEntityTypeKey];

                            if (replaceDivKeys.ContainsKey(att.EntityTypeKey))
                                att.EntityTypeKey = replaceDivKeys[att.EntityTypeKey];

                            foreach (string key in replaceDivKeys.Keys)
                            {
                                att.Codice = att.Codice.Replace(key, replaceDivKeys[key]);
                                AttributoRiferimento newAttRif = att as AttributoRiferimento;
                                if (newAttRif != null)
                                {
                                    if (replaceDivKeys.ContainsKey(newAttRif.ReferenceEntityTypeKey))
                                        newAttRif.ReferenceEntityTypeKey = replaceDivKeys[newAttRif.ReferenceEntityTypeKey];

                                    newAttRif.ReferenceCodiceAttributo = newAttRif.ReferenceCodiceAttributo.Replace(key, replaceDivKeys[key]);
                                    newAttRif.ReferenceCodiceGuid = newAttRif.ReferenceCodiceGuid.Replace(key, replaceDivKeys[key]);

                                }
                            }

                        }//fine attributi 



                        HashSet<string> codiciAtt = new HashSet<string>(newTargetEntType.Attributi.Select(item => item.Value.Codice));

                        //Update Attributi
                        foreach (Attributo att in sourceEntTypeClone.Attributi.Values)
                        {

                            ////////////////////////////////////////////////////////////
                            //Aggiungo l'attributo al target

                            Attributo targetAtt = null;
                            if (EntitiesHelper.IsCodiceBuiltIn(att.Codice))
                                targetAtt = newTargetEntType.Attributi.Values.FirstOrDefault(item => item.Codice == att.Codice);
                            else//divisioni create dal cliente
                                targetAtt = newTargetEntType.Attributi.Values.FirstOrDefault(item => item.Etichetta == att.Etichetta && item.DefinizioneAttributoCodice == att.DefinizioneAttributoCodice);

                            Attributo newAtt = null;
                            if (targetAtt == null) //attributo non presente (nè per codice che per "etichetta e definizione")
                            {
                                newAtt = att.Clone();
                                if (!EntitiesHelper.IsCodiceBuiltIn(att.Codice))
                                {
                                    newAtt.Codice = EntitiesHelper.GetNewCodiceAttributo(codiciAtt);

                                    //memorizzazione in struttiura temporanea dei codici modificati
                                    if (!replaceAttributiCodice.ContainsKey(sourceEntityTypeKey))
                                        replaceAttributiCodice.Add(sourceEntityTypeKey, new AttributiImportRevision());

                                    if (!replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.ContainsKey(att.Codice))
                                        replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.Add(att.Codice, newAtt.Codice);
                                }

                                if (newTargetEntType.Attributi.ContainsKey(newAtt.Codice))
                                {
                                }
                                newTargetEntType.Attributi.Add(newAtt.Codice, newAtt);
                            }
                            else if (att.DefinizioneAttributoCodice == targetAtt.DefinizioneAttributoCodice)
                            {
                                //trovato attributo target (o per codice o per etichetta)
                                string codiceOld = att.Codice;

                                att.Codice = targetAtt.Codice;
                                newTargetEntType.Attributi[targetAtt.Codice] = att;
                                newAtt = att;

                                if (codiceOld != newAtt.Codice)
                                {
                                    if (!replaceAttributiCodice.ContainsKey(sourceEntityTypeKey))
                                        replaceAttributiCodice.Add(sourceEntityTypeKey, new AttributiImportRevision());

                                    if (!replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.ContainsKey(codiceOld))
                                        replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.Add(codiceOld, newAtt.Codice);
                                }
                            }
                        }//fine attributi 

                        newTargetEntType.Attributi = newTargetEntType.Attributi.Values.ToDictionary(item => item.Codice, item => item);

                        ////////////////////////////////////////////////////////////////////////////
                        //aggiornamento dei reference degli attributi riferimento per i nuovi codici attributo 

                        if (replaceAttributiCodice.GetValueOrNull(sourceEntityTypeKey) != null)
                        {
                            IEnumerable<string> sourceAttCodici = replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.Select(item => item.Key);
                            foreach (string sourceAttCodice in sourceAttCodici)
                            {
                                string newAttCodice = replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision[sourceAttCodice];

                                AttributoRiferimento targetAttRif = newTargetEntType.Attributi[newAttCodice] as AttributoRiferimento;
                                if (targetAttRif != null)
                                {
                                    //AttributoRiferimento sourceAttRif = sourceEntTypeClone.Attributi[sourceAttCodice] as AttributoRiferimento;
                                    AttributoRiferimento sourceAttRif = sourceEntType.Attributi[sourceAttCodice] as AttributoRiferimento;

                                    if (!EntitiesHelper.IsCodiceBuiltIn(targetAttRif.ReferenceCodiceAttributo))
                                        targetAttRif.ReferenceCodiceAttributo = replaceAttributiCodice[sourceAttRif.ReferenceEntityTypeKey].CodiciRevision[sourceAttRif.ReferenceCodiceAttributo];

                                    if (!EntitiesHelper.IsCodiceBuiltIn(targetAttRif.ReferenceCodiceGuid))
                                        targetAttRif.ReferenceCodiceGuid = replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision[sourceAttRif.ReferenceCodiceGuid];

                                }
                            }
                        }

                        //////////////////////////////////////////////////////////////////////////////////
                        //Update AttributiMasterCodes
                        HashSet<string> masterCodes = newTargetEntType.AttributiMasterCodes.ToHashSet();

                        List<string> sourceMasterCodes = sourceEntTypeClone.AttributiMasterCodes.ToList();

                        if (replaceAttributiCodice.ContainsKey(sourceEntityTypeKey))
                        {
                            replaceAttributiCodice[sourceEntityTypeKey].CodiciRevision.ForEach(item =>
                            {
                                int index = sourceMasterCodes.IndexOf(item.Key);
                                if (index >= 0)
                                    sourceMasterCodes[index] = item.Value;
                            });
                        }

                        masterCodes.UnionWith(sourceMasterCodes);
                        newTargetEntType.AttributiMasterCodes = masterCodes.ToList();

                        for (int i = 0; i < newTargetEntType.AttributiMasterCodes.Count; i++)
                        {
                            foreach (string key in replaceDivKeys.Keys)
                            {
                                newTargetEntType.AttributiMasterCodes[i] = newTargetEntType.AttributiMasterCodes[i].Replace(key, replaceDivKeys[key]);
                            }
                        }


                        targetDataService.SetEntityType(newTargetEntType, false);

                    }

                    ////////////////////////////////////////////////////////////////
                    //importazione delle voci
                    //importo le voci se sourceEntType non è parentatype, se ha un master e se esiste un criterio di confronto che non includa l'Id
                    if (!sourceEntType.IsParentType() && sourceEntType.MasterType != MasterType.NoMaster && !sourceEntType.EntityComparer.AttributiCode.Contains(BuiltInCodes.Attributo.Id))
                    {

                        List<Guid> selectedItems = new List<Guid>();

                        if (sourceEntType.IsTreeMaster)
                        {
                            //Rimuovo i parent dalle selezionate
                            List<TreeEntityMasterInfo> treeEntsInfo = sourceDataService.GetFilteredTreeEntities(sourceEntType.GetKey(), null, null, out selectedItems);
                            HashSet<Guid> parentsId = new HashSet<Guid>(treeEntsInfo.Select(item => item.ParentId));
                            selectedItems.RemoveAll(item => parentsId.Contains(item));
                        }
                        else
                        {
                            sourceDataService.GetFilteredEntities(sourceEntType.GetKey(), null, null, null, out selectedItems);
                        }


                        if (selectedItems.Count > 0)
                        {
                            EntitiesImportStatus importStatus = new EntitiesImportStatus();
                            importStatus.TargetPosition = TargetPosition.Bottom;
                            importStatus.ConflictAction = EntityImportConflictAction.Overwrite;
                            importStatus.Source = sourceDataService;
                            importStatus.SourceName = "Source";
                            selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = sourceEntType.GetKey() }));

                            while (importStatus.Status != EntityImportStatusEnum.Completed)
                            {
                                targetDataService.ImportEntities(importStatus);
                                if (importStatus.Status == EntityImportStatusEnum.Waiting)
                                {
                                    if (!MainMenuView.WindowService.EntitiesImportWindow(importStatus))
                                        break;
                                }
                            }

                            if (sourceEntType.GetKey() == BuiltInCodes.EntityType.Report)
                            {
                                if (!replaceAttributiCodice.ContainsKey(sourceEntityTypeKey))
                                    replaceAttributiCodice.Add(sourceEntityTypeKey, new AttributiImportRevision());

                                //trovo le voci di report che sono state aggiunte 
                               replaceAttributiCodice[sourceEntityTypeKey].ImportedEntitiesId = importStatus.PerformedEntitiesId.Where(item => item.SourceId != item.TargetId || item.ConflictAction == EntityImportConflictAction.Overwrite)
                                    .Select(item => item.TargetId).ToList();

                            }
                        }
                    }


                }//fine EntityType

                ///////////////////////////////////////////////////////////////////////
                //Altri aggiornamenti specifici per Entity type

                //List<EntityTypeCodiceRevision> entTypesCodiceRevision = replaceDivsCodice.Select(item =>
                //    new EntityTypeCodiceRevision() { OldEntityTypeCodice = item.Value, NewEntityTypeCodice = item.Key })
                //    .Where(item => item.NewEntityTypeCodice != item.OldEntityTypeCodice)
                //    .ToList();

                List<EntityTypeCodiceRevision> entTypesCodiceRevision = new List<EntityTypeCodiceRevision>();

                Dictionary<string, string> newDivsCodiceByOld = new Dictionary<string, string>();//key: old codice, value: new codice
                foreach (EntityTypeCodiceRevision item in replaceDivsCodice)
                {
                    if (item.OldEntityTypeCodice != item.NewEntityTypeCodice)
                    {
                        entTypesCodiceRevision.Add(item);

                        if (!newDivsCodiceByOld.ContainsKey(item.OldEntityTypeCodice) && item.OldEntityTypeCodice != null && item.OldEntityTypeCodice.Any())
                            newDivsCodiceByOld.Add(item.OldEntityTypeCodice, item.NewEntityTypeCodice);
                    }
                }

                

                List<AttributoCodiceRevision> attsCodiceRevision = new List<AttributoCodiceRevision>();
                foreach (string sourceEntTypeKey in replaceAttributiCodice.Keys)
                {
                    var attsCodice = replaceAttributiCodice.GetValueOrNull(sourceEntTypeKey);
                    if (attsCodice == null)
                        continue;


                    foreach (string sourceAttCodice in attsCodice.CodiciRevision.Keys)
                    {
                        AttributoCodiceRevision attCodiceRev = new AttributoCodiceRevision();
                        attCodiceRev.OldEntityTypeCodice = sourceEntTypes[sourceEntTypeKey].Codice;
                        if (newDivsCodiceByOld.ContainsKey(attCodiceRev.OldEntityTypeCodice))
                            attCodiceRev.NewEntityTypeCodice = newDivsCodiceByOld[attCodiceRev.OldEntityTypeCodice];
                        else
                            attCodiceRev.NewEntityTypeCodice = sourceEntTypes[sourceEntTypeKey].Codice;

                        attCodiceRev.OldAttributoCodice = sourceAttCodice;
                        attCodiceRev.NewAttributoCodice = attsCodice.CodiciRevision[sourceAttCodice];

                        if (attCodiceRev.NewAttributoCodice != attCodiceRev.OldAttributoCodice || attCodiceRev.NewEntityTypeCodice != attCodiceRev.OldEntityTypeCodice)
                        {
                            attsCodiceRevision.Add(attCodiceRev);
                        }
                    }
                }

                List<Guid> importedEntitiesId = new List<Guid>(replaceAttributiCodice[BuiltInCodes.EntityType.Report].ImportedEntitiesId);

                MainMenuView.StampeView.ReportView.UpdateCodici(importedEntitiesId, entTypesCodiceRevision, attsCodiceRevision);


            }
            catch (Exception exc)   
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
        }

        public Project CreateProjectByModel(string modelFullFileName, out Int32 modelFileVersion)
        {
            modelFileVersion = 0;

            if (!File.Exists(modelFullFileName))
            {

                return null;
            }


            using (var file = File.OpenRead(modelFullFileName))
            {
                var pStream = new ProgressStream(file);

                Project project = null;
                //BinaryReader reader = new BinaryReader(file);
                //modelFileVersion = reader.ReadInt32();


                //if (modelFileVersion <= MainMenuView.CurrentFileVersion)
                //{
                //    project = Serializer.Deserialize<Project>(pStream);
                //}

                project = ModelSerializer.Deserialize(pStream, out _);

                if (project == null)
                {
                    ShowMessageBarView(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                }

                return project;
            }

        }

        public string GetProjectFileExtension()
        {
            return MainMenuView.ProjectFileExtension;
        }

        public string GetProjectFullFileName()
        {
            if (MainMenuView.CurrentProjectSource != null)
                return MainMenuView.CurrentProjectSource.FullName;

            return null;
        }

        public IDataService GetCurrentProjectDataService()
        {
            return MainMenuView.ClientDataService.Service;
        }

        public List<Guid> GetSelectedEntitiesId(string entityTypeKey)
        {
            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                return MainMenuView.ComputoView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
            {
                return MainMenuView.ElencoPrezziView.CapitoliView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            {
                return MainMenuView.DatiGeneraliView.ContattiView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            {
                return MainMenuView.ElementiView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            {
                return MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Documenti)
            {
                return MainMenuView.StampeView.DocumentiView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Report)
            {
                return MainMenuView.StampeView.ReportView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Stili)
            {
                return MainMenuView.DatiGeneraliView.StiliProgettoView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
            {
                return MainMenuView.AttivitaView.WBSView.ItemsView.CheckedEntitiesId.ToList();
            }
            else if (MainMenuView.DivisioniView.CurrentDivisioneView != null)//si suppone ci sia una divisione corrente
            {
                return MainMenuView.DivisioniView.CurrentDivisioneView.ItemsView.CheckedEntitiesId.ToList();
            }
            return null;
        }

        public List<Guid> GetFilteredEntitiesId(string entityTypeKey)
        {
            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                return MainMenuView.ComputoView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
            {
                return MainMenuView.ElencoPrezziView.CapitoliView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
            {
                return MainMenuView.DatiGeneraliView.ContattiView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Elementi)
            {
                return MainMenuView.ElementiView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
            {
                return MainMenuView.ElencoPrezziView.PrezzarioView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Documenti)
            {
                return MainMenuView.StampeView.DocumentiView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Report)
            {
                return MainMenuView.StampeView.ReportView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.Stili)
            {
                return MainMenuView.DatiGeneraliView.StiliProgettoView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
            {
                return MainMenuView.AttivitaView.WBSView.ItemsView.FilteredEntitiesId.ToList();
            }
            else if (MainMenuView.DivisioniView.CurrentDivisioneView != null)//si suppone ci sia una divisione corrente
            {
                return MainMenuView.DivisioniView.CurrentDivisioneView.ItemsView.FilteredEntitiesId.ToList();
            }
            return null;
        }

        public bool IsRecalculateItemsNeeded(string entityTypeKey, bool? valueToSet = null)
        {
            bool current = false;

            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
            {
                current = MainMenuView.ComputoView.ItemsView.IsRecalculateItemsNeeded;
            }

            if (valueToSet.HasValue)
            {
                MainMenuView.ComputoView.ItemsView.IsRecalculateItemsNeeded = valueToSet.Value;
            }


            return current;

        }

        /// <summary>
        /// Scopo: Aggiornare il Project appena prima di salvarlo
        /// </summary>
        public void UpdateProjectViewSettings()
        {

            ViewSettings viewSettings = MainMenuView.ClientDataService.GetViewSettings();

            MainMenuView.DatiGeneraliView.ContattiView.UpdateViewSettings(viewSettings.EntityTypes[ContattiItemType.CreateKey()]);
            MainMenuView.DatiGeneraliView.InfoProgettoView.UpdateViewSettings(viewSettings.EntityTypes[ContattiItemType.CreateKey()]);
            MainMenuView.ElementiView.UpdateViewSettings(viewSettings.EntityTypes[ElementiItemType.CreateKey()]);
            MainMenuView.ElencoPrezziView.PrezzarioView.UpdateViewSettings(viewSettings.EntityTypes[PrezzarioItemType.CreateKey()]);
            MainMenuView.ElencoPrezziView.CapitoliView.UpdateViewSettings(viewSettings.EntityTypes[CapitoliItemType.CreateKey()]);
            MainMenuView.ComputoView.UpdateViewSettings(viewSettings.EntityTypes[ComputoItemType.CreateKey()]);
            MainMenuView.StampeView.DocumentiView.UpdateViewSettings(viewSettings.EntityTypes[DocumentiItemType.CreateKey()]);
            MainMenuView.StampeView.ReportView.UpdateViewSettings(viewSettings.EntityTypes[ReportItemType.CreateKey()]);
            MainMenuView.AttivitaView.WBSView.UpdateViewSettings(viewSettings.EntityTypes[WBSItemType.CreateKey()]);
            MainMenuView.AttivitaView.ElencoAttivitaView.UpdateViewSettings(viewSettings.EntityTypes[ElencoAttivitaItemType.CreateKey()]);
            MainMenuView.AttivitaView.CalendariView.UpdateViewSettings(viewSettings.EntityTypes[CalendariItemType.CreateKey()]);
            MainMenuView.StampeView.ReportView.UpdateViewSettings(viewSettings.EntityTypes[ReportItemType.CreateKey()]);
            MainMenuView.StampeView.DocumentiView.UpdateViewSettings(viewSettings.EntityTypes[DocumentiItemType.CreateKey()]);

            MainMenuView.ClientDataService.SetViewSettings(viewSettings);

        }

        public async Task<AddResponse> SaveWebProject(Project project, Guid operaId, string nomeProgetto)
        {

            AddResponse res = new AddResponse(false);

            EntitiesHelper entsHelper = new EntitiesHelper(MainMenuView.ClientDataService);

            List<string> fullFilesName = new List<string>();

            var gr = await UtentiWebClient.RefreshToken();
            if (!gr.Success)
            {
                res.Message = gr.Message;
                return res;
            }

            ///////////////////////////////////
            //Caricamento nel web dei file Ifc locali

            var model3DFilesInfo = MainMenuView.ClientDataService.GetModel3dFilesInfo();
            
            List<Model3dFileInfo> model3DFilesValidi = new List<Model3dFileInfo>();
            foreach (var modelFile in model3DFilesInfo.Items)
            {
                string model3dFullFilePath = MainMenuView.Model3dFilesInfoView.CreateFullFilePath(modelFile.FilePath);
                FileInfo fileInfo = new FileInfo(model3dFullFilePath);
                if (fileInfo.Exists)
                {
                    fullFilesName.Add(model3dFullFilePath);
                    model3DFilesValidi.Add(modelFile);
                }
            }

            ////////////////////////////////////
            //Caricamento nel web degli allegati locali

            //key: EntityId, value: web link
            Dictionary<Guid, string> links = new Dictionary<Guid, string>();

            //key: EntityId, value: file id
            Dictionary<Guid, Guid> filesUploadId = new Dictionary<Guid, Guid>();

            //Upload allegati
            List<Guid> allegatiItemsId = null;
            MainMenuView.ClientDataService.GetFilteredEntities(BuiltInCodes.EntityType.Allegati, null, null, null, out allegatiItemsId);
            List<Entity> allegatiItems = MainMenuView.ClientDataService.GetEntitiesById(BuiltInCodes.EntityType.Allegati, allegatiItemsId);

            List<Entity> allegatiValidi = new List<Entity>();
            foreach (AllegatiItem allItem in allegatiItems)
            {
                string localFullFileName = entsHelper.GetValorePlainText(allItem, BuiltInCodes.Attributo.Link, false, true);
                if (!string.IsNullOrEmpty(localFullFileName))
                {
                    FileInfo fileInfo = new FileInfo(localFullFileName);
                    if (fileInfo.Exists)
                    {
                        allegatiValidi.Add(allItem);
                        fullFilesName.Add(localFullFileName);
                    }
                }
            }


            if (fullFilesName.Any())
            {


                //////////////////////////////////////////////////////////
                //upload files
                AllegatiWebClient.ProgressChanged += AllegatiWebClient_ProgressChanged;

                AddResponse resp = await AllegatiWebClient.UploadFiles(operaId, fullFilesName);

                AllegatiWebClient.ProgressChanged -= AllegatiWebClient_ProgressChanged;

                if (resp.Success)
                {
                    var allegatiDto = await AllegatiWebClient.GetAllegati(operaId);


                    ///////////////////////////////
                    //cambio link nei model3d
                    foreach (var modelFile in model3DFilesValidi)
                    {
                        string model3dFullFilePath = MainMenuView.Model3dFilesInfoView.CreateFullFilePath(modelFile.FilePath);

                        string fileName = Path.GetFileName(model3dFullFilePath);

                        ModelData.Dto.AllegatoDto allegatoDto = allegatiDto.FirstOrDefault(item => item.FileName == fileName);

                        //modifica percorso locale in web link
                        //string webLink = string.Format("{0}{1}operaId={2}&fileName={3}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath, operaId.ToString(), fileName);
                        string webLink = string.Format("{0}{1}operaId={2}&uploadGuid={3}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath, operaId.ToString(), allegatoDto.Id);
                        modelFile.FilePath = webLink;
                        modelFile.FileName = fileName;
                        //modelFile.FileId = Guid.NewGuid();
                    }

                    MainMenuView.ClientDataService.SetModel3dFilesInfo(model3DFilesInfo);



                    ////////////////////////////////
                    //cambio link negli allegati

                    foreach (AllegatiItem allItem in allegatiValidi)
                    {
                        string localFullFileName = entsHelper.GetValorePlainText(allItem, BuiltInCodes.Attributo.Link, false, true);

                        string fileName = Path.GetFileName(localFullFileName);

                        ModelData.Dto.AllegatoDto allegatoDto = allegatiDto.FirstOrDefault(item => item.FileName == fileName);

                        //links.Add(allItem.EntityId, string.Format("{0}{1}operaId={2}&fileName={3}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath, operaId.ToString(), fileName));
                        links.Add(allItem.EntityId, string.Format("{0}{1}operaId={2}&uploadGuid={3}", ServerAddress.ApiCurrent, CacheManager.ApiFileRepositoryPath, operaId.ToString(), allegatoDto.Id));
                        allItem.FileName = fileName;

                        filesUploadId.Add(allItem.EntityId, Guid.NewGuid());
                    }



                    //Modifica dei link locali con quelli web
                    ModelAction action = new ModelAction() { EntityTypeKey = BuiltInCodes.EntityType.Allegati, ActionName = ActionName.MULTI };
                    foreach (var id in links.Keys)
                    {
                        ModelAction itemLinkAction = new ModelAction()
                        {
                            EntityTypeKey = BuiltInCodes.EntityType.Allegati,
                            AttributoCode = BuiltInCodes.Attributo.Link,
                            ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                            EntitiesId = new HashSet<Guid>() { id },
                            NewValore = new ValoreTesto() { V = links[id] },
                        };

                        action.NestedActions.Add(itemLinkAction);

                        //ModelAction itemCacheFileNameAction = new ModelAction()
                        //{
                        //    EntityTypeKey = BuiltInCodes.EntityType.Allegati,
                        //    AttributoCode = BuiltInCodes.Attributo.FileUploadId,
                        //    ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        //    EntitiesId = new HashSet<Guid>() { id },
                        //    NewValore = new ValoreGuid() { V = filesUploadId[id] },
                        //};

                        //action.NestedActions.Add(itemCacheFileNameAction);
                    }


                    ModelActionResponse mar = MainMenuView.ClientDataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.Allegati });
                    }
                }//fine success
                else
                {
                    return resp;
                }
            }

            //Aggiunta del progetto nel db
            res = await ProgettiWebClient.SaveProject(project, operaId, nomeProgetto, MainMenuView.CurrentFileVersion);

            return res;
        }

        private void AllegatiWebClient_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 100)
                ShowMessageBarView(e.UserState.ToString(), false, e.ProgressPercentage);
            else
                ShowMessageBarView(e.UserState.ToString(), false, 100, true);
        }

        public string GetDeploymentVersion()
        {
            return MainMenuView.ShortDeploymentVersion;
        }

        public bool IsValidVersion(string minAppVersion)
        {
            NumberFormatInfo formatProvider = new NumberFormatInfo();
            formatProvider.NumberDecimalSeparator = ".";
            formatProvider.NumberGroupSeparator = "";

            double minAppVersionInt = 0;
            double.TryParse(minAppVersion, NumberStyles.AllowDecimalPoint, formatProvider, out minAppVersionInt);

            double deploymentVersionDouble = 999;
            string deploymentVersion = GetDeploymentVersion();
            if (deploymentVersion != null && deploymentVersion.Any())
                double.TryParse(deploymentVersion, NumberStyles.AllowDecimalPoint, formatProvider, out deploymentVersionDouble);

            if (deploymentVersionDouble >= minAppVersionInt)
                return true;

            return false;
        }

        public SectionItemTemplateView GetWBSView()
        {
            return MainMenuView.AttivitaView.WBSView;
        }

        public SectionItemTemplateView GetFogliDiCalcoloView()
        {
            return MainMenuView.FogliDiCalcoloView;
        }

        public void AddToModelActionStack(ModelAction modelAction)
        {
            MainMenuView.ModelActionsStack.CommitAction(modelAction);
        }
        public void UndoGroupBegin(string undoGroupName, string entityTypeKey)
        {
            MainMenuView.ModelActionsStack.UndoGroupBegin(undoGroupName, entityTypeKey);
        }
        public void UndoGroupEnd()
        {
            MainMenuView.ModelActionsStack.UndoGroupEnd();
        }
        public void UndoGroupCancel()
        {
            MainMenuView.ModelActionsStack.UndoGroupCancel();
        }

        public bool ImportItems(string fullFileName, string entityTypeKey)
        {
            bool res = false;

            if (!File.Exists(fullFileName))
                return false;

            if (Path.GetExtension(fullFileName) == string.Format(".{0}", XlsxImport.FileExtension))
            {
                var xlsxImport = new XlsxImport();
                xlsxImport.DataService = MainMenuView.ClientDataService;
                xlsxImport.EntityTypeKey = entityTypeKey;
                xlsxImport.MainOperation = this;

                res = xlsxImport.Run(fullFileName);

            }


            return res;
        }


        public Model3dType GetModel3dType()
        {
            if (MainMenuView.IsReJoInitialized)
                return Model3dType.Revit;

            if (MainMenuView.IsIfcViewerInitialized)
                return Model3dType.Ifc;

            return Model3dType.Unknown;
        }
    }

    public class AttributiImportRevision
    {
        /// <summary>
        /// Codici revisionati in importazione
        /// key = source codice, value: target codice
        /// </summary>
        public Dictionary<string, string> CodiciRevision = new Dictionary<string, string>();

        /// <summary>
        /// id degli item importati (aggiunti o sovrascritti)
        /// </summary>
        public List<Guid> ImportedEntitiesId = new List<Guid>();

    }
}
