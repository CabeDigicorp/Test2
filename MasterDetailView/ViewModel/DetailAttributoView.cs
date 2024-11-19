

using CommonResources;
using Commons;
using DevExpress.Data.Extensions;
using DevExpress.Xpf.Core.Native;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MasterDetailView
{

    /// <summary>
    /// Rappresenta gli attributi di più entità
    /// </summary>
    public class MultiDetailAttributiView : NotificationBase
    {


        //List<Entity> _entities = new List<Entity>(); //entità da modificare correntemente visualizzate
        Entity _entity = null; //sigle entity selection
        List<Guid> _entitiesId = new List<Guid>();
        public List<Guid> EntitiesId { get => _entitiesId; }

        protected EntitiesListMasterDetailView _master;
        EntitiesListMasterDetailView Master { get => _master; }


        public bool IsModelToViewLoading { get; private set; }

        public MultiDetailAttributiView(EntitiesListMasterDetailView entitiesListMasterDetailView)
        {
            _master = entitiesListMasterDetailView;
        }


        ObservableCollection<DetailAttributoView> _attributiValoriComuniView = new ObservableCollection<DetailAttributoView>();
        public ObservableCollection<DetailAttributoView> AttributiValoriComuniView
        {
            get
            {
                return _attributiValoriComuniView;
            }
            set
            {
                if (SetProperty(ref _attributiValoriComuniView, value))
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(_attributiValoriComuniView);
                    view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                }

            }
        }



        public void Load(EntityView entityView, bool force = false)//Load Selected entity
        {
            IsModelToViewLoading = true;

            //System.Diagnostics.Debug.WriteLine("MultiDetailAttributiView.Load");

            if (entityView == null)
                return;

            SelectedItem = null;

            _entitiesId.Clear();
            _entitiesId.Add(entityView.Id);
            _entity = entityView.Entity;
            
            IOrderedEnumerable<DetailAttributoView> orderedAtt = entityView.DetailAttributiView.Values.OrderBy(item => item.Attributo.DetailViewOrder);

            if (AttributiValoriComuniView.Count == 0)
            {
                AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(orderedAtt);
            }
            else
            {
                if (force)
                    AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(orderedAtt);

                EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                HashSet<string> codiciAtt = new HashSet<string>(orderedAtt.Select(item => item.CodiceAttributo));

                int i = 0;
                for (i = 0; i < AttributiValoriComuniView.Count; i++)
                {

                    //Attributo att = AttributiValoriComuniView[i].EntityAttributo.Attributo;
                    Attributo att = AttributiValoriComuniView[i].Attributo;
                    //if (codiciAtt.Contains(att.Codice))
                    //    AttributiValoriComuniView[i].IsValoreViewVisible = true;
                    //else
                    //    AttributiValoriComuniView[i].IsValoreViewVisible = false;

                    if (entsHelper.GetAttributo(_entity, att.Codice) != null)
                        AttributiValoriComuniView[i].IsValoreViewVisible = true;
                    else
                        AttributiValoriComuniView[i].IsValoreViewVisible = false;


                    Valore val = entsHelper.GetValoreAttributo(_entity, att.Codice, false, false);

                    ValoreView valoreView = AttributiValoriComuniView[i].CreateValoreView(att, val);
                    AttributiValoriComuniView[i].ValoreView = valoreView;
                }
            }
            
            UpdateUI();

            //            AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(orderedAtt);

            IsModelToViewLoading = false;
        }


        public void Load(HashSet<Guid> ids) //Load Checked entities in detail
        {

            IsModelToViewLoading = true;

            if (Master.EntityType == null)
                return;

            SelectedItem = null;

            List<DetailAttributoView> attributiValoriComuniView = new List<DetailAttributoView>();
            _entitiesId = new List<Guid>(Master.CheckedEntitiesId);
            _entity = null;

            if (!ids.Any())//se nessuna entity coinvolto
            {



                int tabIndex = 0;
                foreach (Attributo att in Master.EntityType.Attributi.Values.OrderBy(item => item.DetailViewOrder))
                {
                    if (!att.IsVisible)
                        continue;

                    //EntityAttributo attributo = new EntityAttributo(null, att);
                    //DetailAttributoView attView = new DetailAttributoView(Master, attributo.Valore, attributo) { TabIndex = tabIndex++ };

                    //EntityAttributo attributo = new EntityAttributo(null, att);
                    DetailAttributoView attView = new DetailAttributoView(Master, att.ValoreDefault.Clone(), att) { TabIndex = tabIndex++ };

                    attView.IsValoreViewVisible = false;
                    attributiValoriComuniView.Add(attView);
                }


                //AttributiValoriComuniView.Clear(); //08/02/24 a cosa serve? sembra che faccia partire l'evento VirtualEntities_LoadingStateChanged
                AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(attributiValoriComuniView.OrderBy(item => item.Attributo.DetailViewOrder));


            }
            else //modifica multipla
            {
                List<EntityAttributo> entityAttributi = Master.DataService.GetAttributiValoriComuni(Master.EntityType.GetKey(), ids.ToList());

                int tabIndex = 0;
                foreach (EntityAttributo entAtt in entityAttributi)
                {

                    if (!entAtt.Attributo.IsVisible)
                        continue;

                    DetailAttributoView detailAttributoView = null;
                    if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                    {
                        if (entAtt.Valore.IsMultiValore(true))
                        {
                            ValoreTesto val = new ValoreTesto() { V = LocalizationProvider.GetString(ValoreHelper.Multi) };
                            detailAttributoView = new DetailAttributoView(Master, val, entAtt.Attributo) { TabIndex = tabIndex++ };
                        }
                        else
                        {
                            detailAttributoView = new DetailAttributoView(Master, entAtt.Valore, entAtt.Attributo) { TabIndex = tabIndex++ };
                        }
                    }
                    else
                    {
                        detailAttributoView = new DetailAttributoView(Master, entAtt.Valore, entAtt.Attributo) { TabIndex = tabIndex++, };
                    }
                    attributiValoriComuniView.Add(detailAttributoView);
                }

                //AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(attributiValoriComuniView.OrderBy(item => item.EntityAttributo.Attributo.DetailViewOrder));

                if (AttributiValoriComuniView.Count == 0)
                {
                    AttributiValoriComuniView = new ObservableCollection<DetailAttributoView>(attributiValoriComuniView.OrderBy(item => item.Attributo.DetailViewOrder));
                }
                else
                {
                    Dictionary<string, ValoreView> attributiValoriComuniViewByCodice = attributiValoriComuniView.ToDictionary(item => item.CodiceAttributo, item => item.ValoreView);

                    int i = 0;
                    for (i = 0; i < AttributiValoriComuniView.Count; i++)
                    {
                        //Attributo att = AttributiValoriComuniView[i].EntityAttributo.Attributo;
                        Attributo att = AttributiValoriComuniView[i].Attributo;

                        ValoreView valView = null;
                        if (attributiValoriComuniViewByCodice.TryGetValue(att.Codice, out valView))
                        {
                            AttributiValoriComuniView[i].IsValoreViewVisible = true;
                            AttributiValoriComuniView[i].ValoreView = valView;
                        }
                    }
                }
                
                UpdateUI();
            }


            IsModelToViewLoading = false;

        }



        public void UpdateUI()
        {
            foreach (DetailAttributoView entAttView in AttributiValoriComuniView)
            {
                entAttView.UpdateUI();
            }
            RaisePropertyChanged(GetPropertyName(() => AttributiValoriComuniView));

        }

        /// <summary>
        /// Per tutte le entità in _entities sostituisce il valore dell'attributo con "valore"
        /// </summary>
        /// <param name="codiceAttributo"></param>
        /// <param name="valore"></param>
        public bool SetValoreAttributo(string codiceAttributo, Valore valore, Valore oldValore = null)
        {
            bool res = false;

            try
            {

                ModelAction action = null;

                if (Master.IsMultipleModify)
                {

                    ModelAction actionMod = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        AttributoCode = codiceAttributo,
                        EntityTypeKey = Master.EntityType.GetKey(),
                        NewValore = valore.Clone(),
                        OldValore = oldValore
                    };
                    actionMod.EntitiesId = new HashSet<Guid>(Master.ReadyToModifyEntitiesId);

                    action = new ModelAction()
                    {
                        ActionName = ActionName.MULTI_AND_CALCOLA,
                        EntityTypeKey = Master.EntityType.GetKey(),
                    };

                    action.NestedActions.Add(actionMod);
                }
                else
                {
                    action = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        AttributoCode = codiceAttributo,
                        EntityTypeKey = Master.EntityType.GetKey(),
                        NewValore = valore.Clone(),
                        OldValore = oldValore
                    };

                    action.EntitiesId.Add(_entitiesId[0]);
                }


                ModelActionResponse mar = Master.CommitAction(action);

                if (mar.ActionResponse == ActionResponse.OK)
                {
                    bool applyFilterAndSort = false;
                    int filterAttIndex = -1;

                    if (Master.IsAlertVisible)
                        applyFilterAndSort = true;

                    AttributoFilterData attFilterData = Master.RightPanesView.FilterView.Data.Items.FirstOrDefault(item => item.CodiceAttributo == codiceAttributo);
                    if (attFilterData != null)
                    {
                        filterAttIndex = Master.RightPanesView.FilterView.Data.Items.IndexOf(attFilterData);
                        applyFilterAndSort = true;
                    }
                    else
                    {

                        //Controllo se il valore cambiato incide sui gruppi di apparteneza
                        if (Master.RightPanesView.GroupView.Data != null)
                        {
                            foreach (AttributoGroupData attGroupData in Master.RightPanesView.GroupView.Data.Items)
                            {
                                if (Master.EntityType.Attributi[attGroupData.CodiceAttributo] is AttributoRiferimento)
                                {
                                    AttributoRiferimento attRif = Master.EntityType.Attributi[attGroupData.CodiceAttributo] as AttributoRiferimento;
                                    if (attRif.ReferenceCodiceGuid == codiceAttributo)
                                        applyFilterAndSort = true;
                                }
                                else
                                {
                                    if (codiceAttributo == attGroupData.CodiceAttributo)
                                        applyFilterAndSort = true;
                                }

                            }
                        }
                    }
                    if (Master.IsMultipleModify)
                    {
                        //Master.PendingCommand |= EntitiesListMasterDetailViewCommands.AfterMultipleModify;
                        //Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandNewEntitiesGroup;
                        Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ExpandCheckedEntityGroups;
                        Master.PendingCommand |= EntitiesListMasterDetailViewCommands.SelectRows;
                    }

                    Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;

                    if (applyFilterAndSort)//Il valore cambiato incide sui filtri attivi o sui raggruppamenti
                    {
                        //Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;
                        //Master.PendingCommand |= EntitiesListMasterDetailViewCommands.LoadRequest;

                        if (filterAttIndex >= 0)
                            Master.RightPanesView.FilterView.ClearNextAttributiFilter(filterAttIndex);

                        Master.ApplyFilterAndSort(Master.SelectedEntityId);
                    }

                    res = true;

                }

                    
                Master.LoadAlertByAction(mar);

                Master.IsMultipleModify = false;
                Master.UpdateUI();
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
            return res;
        }

        public bool ContainsEntity(Guid id)
        {
            //Entity entity = _entities.FirstOrDefault(item => itemId == id);
            //return entity != null;
            return _entitiesId.Contains(id);
        }

        public async void AddItemInValoreCollection(string codiceAttributo, Valore valItem)
        {

            ModelAction action = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_ADD, AttributoCode = codiceAttributo, EntityTypeKey = Master.EntityType.GetKey(), NewValore = valItem.Clone() };

            if (Master.IsMultipleModify)
                action.EntitiesId = new HashSet<Guid>(Master.CheckedEntitiesId.Intersect(Master.FilteredEntitiesId));
            else
                action.EntitiesId.Add(_entitiesId[0]);

            //Master.ModelActionsStack.CommitAction(action, Master);
            var mar = Master.CommitAction(action);


            if (valItem is ValoreGuidCollectionItem)
                Master.LoadAlertByAction(mar);

            //await Master.UpdateCache(true);

            //Master.RightPanesView.FilterView.Update(codiceAttributo);
            //Master.RightPanesView.SortView.Update(codiceAttributo);

        }

        public async void AddItemsInValoreCollection(string codiceAttributo, List<Valore> valItems)
        {
            bool isValoreGuidCollection = valItems.FirstOrDefault() is ValoreGuidCollectionItem;

            ModelAction action = new ModelAction() { AttributoCode = codiceAttributo, EntityTypeKey = Master.EntityType.GetKey() };

            if (isValoreGuidCollection)
                action.ActionName = ActionName.MULTI_AND_CALCOLA;
            else
                action.ActionName = ActionName.MULTI;


            foreach (Valore valItem in valItems)
            {
                ModelAction itemAction = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_ADD, AttributoCode = codiceAttributo, EntityTypeKey = Master.EntityType.GetKey(), NewValore = valItem.Clone() };

                if (Master.IsMultipleModify)
                    itemAction.EntitiesId = new HashSet<Guid>(Master.CheckedEntitiesId.Intersect(Master.FilteredEntitiesId));
                else
                    itemAction.EntitiesId.Add(_entitiesId[0]);


                action.NestedActions.Add(itemAction);
            }

            var mar = Master.CommitAction(action);

            if (valItems.FirstOrDefault() is ValoreGuidCollectionItem)
                Master.LoadAlertByAction(mar);

            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;


        }

        public async void RemoveItemInValoreCollection(string codiceAttributo, Valore valItem)
        {
            ModelAction action = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_REMOVE, AttributoCode = codiceAttributo, EntityTypeKey = Master.EntityType.GetKey(), NewValore = valItem.Clone() };

            if (Master.IsMultipleModify)
                action.EntitiesId = new HashSet<Guid>(Master.CheckedEntitiesId.Intersect(Master.FilteredEntitiesId));
            else
                action.EntitiesId.Add(_entitiesId[0]);

            //Master.ModelActionsStack.CommitAction(action, Master);
            var mar = Master.CommitAction(action);

            if (valItem is ValoreGuidCollectionItem)
                Master.LoadAlertByAction(mar);

            Master.PendingCommand |= EntitiesListMasterDetailViewCommands.ApplyGroups;

            await Master.UpdateCache(true);

            //Master.RightPanesView.FilterView.Update(codiceAttributo);
            //Master.RightPanesView.SortView.Update(codiceAttributo);


        }

        public void ReplaceItemInValoreCollection(string codiceAttributo, Valore valItem)
        {
            ModelAction action = new ModelAction() { ActionName = ActionName.ATTRIBUTO_VALORECOLLECTION_REPLACE, AttributoCode = codiceAttributo, EntityTypeKey = Master.EntityType.GetKey(), NewValore = valItem.Clone() };

            if (Master.IsMultipleModify)
                action.EntitiesId = new HashSet<Guid>(Master.CheckedEntitiesId.Intersect(Master.FilteredEntitiesId));
            else
                action.EntitiesId.Add(_entitiesId[0]);

            //Master.ModelActionsStack.CommitAction(action, Master);
            var mar = Master.CommitAction(action);

            if (valItem is ValoreGuidCollectionItem)
                Master.LoadAlertByAction(mar);

            Master.UpdateCache();

            //Master.RightPanesView.FilterView.Update(codiceAttributo);
            //Master.RightPanesView.SortView.Update(codiceAttributo);


        }

        public Valore GetValoreAttributo(string codiceAttributo, bool deep, bool brief)
        {
            if (_entitiesId.Count != 1)
                return null;

            return Master.EntitiesHelper.GetValoreAttributo(_entity, codiceAttributo, deep, brief);
        }

        internal void UpdateValues(/*Valore val*/)
        {
            ////ricalcolo gli attributi dell'entità
            //for (int i=0; i< AttributiValoriComuniView.Count; i++)
            //{
            //    DetailAttributoView attView = AttributiValoriComuniView[i];
            //    attView.UpdateValue(/*val*/);
            //}

            EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
            int i = 0;
            for (i = 0; i < AttributiValoriComuniView.Count; i++)
            {

                //Attributo att = AttributiValoriComuniView[i].EntityAttributo.Attributo;
                Attributo att = AttributiValoriComuniView[i].Attributo;

                Valore val = entsHelper.GetValoreAttributo(_entity, att.Codice, false, false);
                if (val != null)
                {
                    ValoreView valoreView = AttributiValoriComuniView[i].CreateValoreView(att, val);
                    AttributiValoriComuniView[i].ValoreView = valoreView;
                }
            }
        }

        internal string GetValoreAttributoFormat(string codiceAttributo)
        {
            //Se in modifica multipla prendo il formato dell'Attributo altrimenti prendo il formato dal EntityAttributo
            if (_entitiesId.Count != 1)
            {

                //trovo l'Attributo sorgente
                EntityType sourceEntType = Master.EntityType;
                Attributo sourceAtt = Master.EntityType.Attributi[codiceAttributo];
                while (sourceAtt is AttributoRiferimento)//per andare in profondità riferimento di riferimento
                {
                    AttributoRiferimento attRif = sourceAtt as AttributoRiferimento;

                    sourceEntType = Master.DataService.GetEntityTypes()[attRif.ReferenceEntityTypeKey];

                    if (sourceEntType == null)
                        break;

                    sourceAtt = sourceEntType.Attributi[attRif.ReferenceCodiceAttributo];
                }

                return Master.AttributoFormatHelper.GetValorePaddedFormat(sourceAtt);
            }
            else if (Master.EntitiesHelper.GetAttributi(_entity).ContainsKey(codiceAttributo))
            {
                Attributo sourceAtt = Master.EntitiesHelper.GetSourceAttributo(_entity.EntityTypeCodice, codiceAttributo);
                string format = Master.AttributoFormatHelper.GetValorePaddedFormat(sourceAtt);

                return format;
                //trovo l'EntityAttributo sorgente
                //EntityAttributo sourceEntAtt = Master.EntitiesHelper.GetSourceEntityAttributo(_entity, codiceAttributo);

                //if (sourceEntAtt == null)
                //    return Master.AttributoFormatHelper.GetValorePaddedFormat(_entity, codiceAttributo);
                //else
                //    return Master.AttributoFormatHelper.GetValorePaddedFormat(_entity, codiceAttributo);


            }
            else return string.Empty;
        }

        DetailAttributoView _selectedItem = null;
        public DetailAttributoView SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    HashSet<string> etichetteAtts = new HashSet<string>();

                    if (_selectedItem != null)
                    {

                        string formula = string.Empty;
                        if (_selectedItem.ValoreView is ValoreContabilitaView)
                            formula = (_selectedItem.ValoreView as ValoreContabilitaView).Formula;
                        else if (_selectedItem.ValoreView is ValoreRealeView)
                            formula = (_selectedItem.ValoreView as ValoreRealeView).Formula;
                        //else if (_selectedItem.ValoreView is ValoreTestoView)
                        //    formula = (_selectedItem.ValoreView as ValoreTestoView).Testo;


                        //ricavo gli attributi richiamati nella formula
                        string sFuncs = ValoreCalculatorFunction.FunctionName;

                        string pattern = string.Format("[^a-zA-Z0-9]" + //non precedute da lettera o numero
                            "({0})" + //precedute da una delle funzioni
                            "\\{1}(.*?)\\{2}", sFuncs, "{", "}");//elementi tra le parentesi graffe

                        string[] splittedStr = Regex.Split(" " + formula, pattern); //aggiunto lo spazio iniziale altrimenti non riconosce le funzioni se sono all'inizio della stringa


                        for (int i = 0; i < splittedStr.Length; i++)
                        {
                            if (splittedStr[i] == sFuncs && splittedStr.Length > i + 1)
                            {
                                string etichettaAtt = splittedStr[i + 1];
                                etichetteAtts.Add(etichettaAtt);
                            }
                        }
                    }
                    
                    HilightAttributi(etichetteAtts);
                    //UpdateUI();
                }
            }
        }

        void HilightAttributi(HashSet<string> etichettaAtts)
        {
            foreach (DetailAttributoView att in AttributiValoriComuniView)
            {
                if (etichettaAtts.Contains(att.Etichetta))
                    att.IsHilighted = true;
                else
                    att.IsHilighted = false;
            }

        }


    }




    ///// <summary>
    ///// Rappresenta la vista di un attributo di un'entità nei dettagli
    ///// </summary>
    //public class DetailAttributoView_Old : NotificationBase<EntityAttributo>
    //{
    //    EntitiesListMasterDetailView _master = null;
    //    EntitiesListMasterDetailView Master { get => _master; }

    //    EntitiesHelper _entitiesHelper = null;

    //    public DetailAttributoView_Old(EntitiesListMasterDetailView master, Valore valore, EntityAttributo entAtt = null) : base(entAtt)
    //    {
    //        _master = master;

    //        _entitiesHelper = new EntitiesHelper(_master.DataService);

    //        _valoreView = CreateValoreView(entAtt.Attributo, valore);

    //    }

    //    public EntityAttributo EntityAttributo { get { return This; } }

    //    public string Etichetta
    //    {
    //        get { return This.Attributo.Etichetta; }
    //        set { SetProperty(This.Attributo.Etichetta, value, () => This.Attributo.Etichetta = value); }
    //    }


    //    public double EtichettaWidth
    //    {
    //        get { return Master.EntityType.DetailAttributoEtichettaWidth; }
    //    }

    //    ValoreView _valoreView;
    //    public ValoreView ValoreView
    //    {
    //        get
    //        {
    //            return _valoreView;
    //        }
    //        set
    //        {
    //            SetProperty(ref _valoreView, value);
    //        }
    //    }

    //    public ValoreView CreateValoreView(Attributo att, Valore valore)
    //    {
    //        ValoreView valoreView = null;

    //        if (valore == null)
    //        {
    //            valoreView = new ValoreTestoView(Master, new ValoreTesto()) { Tag = att.Codice };
    //        }
    //        else if (valore.GetType() == typeof(ValoreTesto))
    //        {
    //            if (att.Codice == BuiltInCodes.Attributo.Link)
    //                valoreView = new ValoreLinkView(Master, valore as ValoreTesto) { Tag = att.Codice };
    //            else
    //                valoreView = new ValoreTestoView(Master, valore as ValoreTesto) { Tag = att.Codice };
    //        }
    //        else if (valore.GetType() == typeof(ValoreData))
    //            valoreView = new ValoreDataView(Master, valore as ValoreData) { Tag = att.Codice };
    //        else if (valore.GetType() == typeof(ValoreTestoCollection))
    //        {
    //            ValoreTestoCollectionView valCollectionView = new ValoreTestoCollectionView(Master, valore as ValoreTestoCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
    //            valoreView = valCollectionView;
    //        }
    //        else if (valore.GetType() == typeof(ValoreGuidCollection))
    //        {
    //            if (att.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Allegati)
    //            {
    //                ValoreLinkCollectionView valCollectionView = new ValoreLinkCollectionView(Master, valore as ValoreGuidCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
    //                valCollectionView.Init();
    //                valoreView = valCollectionView;
    //            }
    //            else
    //            {
    //                ValoreGuidCollectionView valCollectionView = new ValoreGuidCollectionView(Master, valore as ValoreGuidCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
    //                valCollectionView.Init();
    //                valoreView = valCollectionView;
    //            }
    //        }
    //        else if (valore.GetType() == typeof(ValoreTestoRtf))
    //        {
    //            ValoreTestoRtf valRtf = valore as ValoreTestoRtf;
    //            ValoreTestoRtfView valoreTestoRtfView = new ValoreTestoRtfView(Master, valRtf);
    //            valoreTestoRtfView.Tag = att.Codice;

    //            if (IsSearched)
    //                valoreTestoRtfView.HighlightedText = string.Join(" ", TextsSearched);
    //            valoreView = valoreTestoRtfView;

    //        }
    //        else if (valore.GetType() == typeof(ValoreContabilita))
    //            valoreView = new ValoreContabilitaView(Master, valore as ValoreContabilita) { Tag = att.Codice };
    //        else if (valore.GetType() == typeof(ValoreReale))
    //            valoreView = new ValoreRealeView(Master, valore as ValoreReale) { Tag = att.Codice };
    //        else if (valore.GetType() == typeof(ValoreGuid))
    //            valoreView = new ValoreGuidView(Master, valore as ValoreGuid) { Tag = att.Codice };
    //        else if (valore.GetType() == typeof(ValoreElenco))
    //            valoreView = new ValoreElencoView(Master, att.Codice, valore as ValoreElenco);
    //        else if (valore.GetType() == typeof(ValoreColore))
    //            valoreView = new ValoreColoreView(Master, att.Codice, valore as ValoreColore);
    //        else if (valore.GetType() == typeof(ValoreBooleano))
    //            valoreView = new ValoreBooleanoView(Master, valore as ValoreBooleano) { Tag = att.Codice };
    //        else if (valore.GetType() == typeof(ValoreFormatoNumero))
    //            valoreView = new ValoreFormatoNumeroView(Master, att.Codice, valore as ValoreFormatoNumero);
    //        //else if (valore.GetType() == typeof(ValoreVariabile))
    //        //    valoreView = new ValoreVariabileView(Master, att.Codice, valore as ValoreVariabile);

    //        valoreView.UpdateValore(valore);
    //        //bool isreadonly = valoreView.IsReadOnly;
    //        return valoreView;
    //    }


    //    public string CodiceAttributo
    //    {
    //        get { return This.Attributo.Codice; }
    //    }

    //    public string SuggestionCodeAttributo
    //    {
    //        get { return This.Attributo.SuggestionCode; }
    //    }


    //    bool _isValoreViewVisible = true;
    //    public bool IsValoreViewVisible
    //    {
    //        get { return _isValoreViewVisible; }
    //        set { SetProperty(ref _isValoreViewVisible, value); }
    //    }

    //    public bool IsExpandable
    //    {
    //        get
    //        {
    //            if (EntityAttributo.Attributo is AttributoRiferimento)
    //            {
    //                bool isExpandable = false;
    //                try
    //                {
    //                    AttributoRiferimento attRif = EntityAttributo.Attributo as AttributoRiferimento;
    //                    Dictionary<string, EntityType> entityTypes = Master.DataService.GetEntityTypes();

    //                    if (entityTypes.ContainsKey(attRif.ReferenceEntityTypeKey))
    //                    {
    //                        EntityType entType = entityTypes[attRif.ReferenceEntityTypeKey];
    //                        if (entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
    //                            isExpandable = entType.Attributi[attRif.ReferenceCodiceAttributo].IsExpandable;
    //                        else
    //                        {
    //                            MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "!entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo)");
    //                        }
    //                    }
    //                    else
    //                    {
    //                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "!entityTypes.ContainsKey(attRif.ReferenceEntityTypeKey)");
    //                    }
    //                }
    //                catch (KeyNotFoundException e)
    //                {
    //                    //MessageBox.Show(e.Message);
    //                    MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
    //                }
    //                return isExpandable;
    //            }
    //            else if (EntityAttributo.Attributo.ValoreAttributo is ValoreAttributoGuidCollection)
    //            {
    //                var attSettings = EntityAttributo.Attributo.ValoreAttributo as ValoreAttributoGuidCollection;
    //                if (attSettings != null)
    //                {
    //                    if (attSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
    //                        return true;
    //                }
    //                return false;
    //            }
    //            else
    //            {
    //                return EntityAttributo.Attributo.IsExpandable;
    //            }
    //        }
    //    }

    //    //public bool IsPreviewable
    //    //{
    //    //    get
    //    //    {
    //    //        if (EntityAttributo.Attributo is AttributoRiferimento)
    //    //        {
    //    //            return true;
    //    //            //AttributoRiferimento attRif = EntityAttributo.Attributo as AttributoRiferimento;
    //    //            //return Master.DataService.GetEntityTypes()[attRif.ReferenceEntityTypeCodice].Attributi[attRif.ReferenceCodiceAttributo].IsPreviewable;
    //    //        }
    //    //        else return EntityAttributo.Attributo.IsPreviewable;
    //    //    }
    //    //}

    //    public ICommand ExpandCommand
    //    {
    //        get
    //        {
    //            return new CommandHandler(() => IsExpanded=!IsExpanded);
    //        }
    //    }

    //    public bool IsExpanded
    //    {
    //        get { return EntityAttributo.Attributo.IsExpanded; }
    //        set
    //        {
    //            EntityAttributo.Attributo.IsExpanded = value;
    //            UpdateUI();
    //            //RaisePropertyChanged(GetPropertyName(() => IsExpanded));
    //            //RaisePropertyChanged(GetPropertyName(() => Height));
    //        }
    //    }

    //    public bool IsGroupExpanded
    //    {
    //        get { return Master.IsAttributoViewGroupExpanded(EntityAttributo.Attributo.GroupName); }
    //        set { Master.SetAttributoViewGroupExpanded(EntityAttributo.Attributo.GroupName, value); }
    //    }

    //    public bool AllowMasterGrouping { get => EntityAttributo.Attributo.AllowMasterGrouping; }


    //    public double Height
    //    {
    //        get { return IsExpanded ? Double.NaN : EntityAttributo.Attributo.Height; }
    //    }

    //    public bool IsFiltered
    //    {
    //        get
    //        {
    //            List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => ( item.GetCodice() == This.Attributo.Codice && item.IsValid() && item.IsFiltroAttivato)).ToList();
                
    //            if (attFilterView.Count > 0)
    //            {
    //                return true;
    //            }

    //            return false;
    //        }
    //    }

    //    public bool IsSorted
    //    {
    //        get
    //        {
    //            AttributoSortView attSortView = Master.RightPanesView.SortView.Items.FirstOrDefault(item => item.Attributo.Codice == This.Attributo.Codice);
    //            if (attSortView != null)
    //            {
    //               return true;
    //            }

    //            return false;
    //        }
    //    }

    //    public bool IsSearched
    //    {
    //        get
    //        {
    //            List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => (item.GetCodice() == This.Attributo.Codice && item.IsValid())).ToList();

    //            if (attFilterView.Count > 0)
    //            {
    //                return true;
    //            }

    //            return false;
    //        }
    //    }

    //    public List<string> TextsSearched
    //    {
    //        get
    //        {
    //            List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => (item.GetCodice() == This.Attributo.Codice && item.IsValid())).ToList();
    //            return attFilterView.Select(item => item.TextSearched).ToList();
    //        }
    //    }

    //    public int TabIndex { get; set; } = 0;


    //    public void UpdateUI()
    //    {
    //        if (ValoreView != null)
    //            ValoreView.UpdateUI();

    //        RaisePropertyChanged(GetPropertyName(() => IsExpanded));
    //        RaisePropertyChanged(GetPropertyName(() => EtichettaWidth));
    //        RaisePropertyChanged(GetPropertyName(() => Height));
    //        RaisePropertyChanged(GetPropertyName(() => ValoreView));
    //        RaisePropertyChanged(GetPropertyName(() => IsHilighted));



    //    }

    //    //internal void UpdateValue(/*Valore val*/)
    //    //{
    //    //    //Master.Calculator.Calculate(EntityAttributo.Entity, EntityAttributo.Attributo, val);

    //    //    if (EntityAttributo.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo ||
    //    //        EntityAttributo.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
    //    //        EntityAttributo.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
    //    //        EntityAttributo.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
    //    //        _valoreView.UpdateValore(EntityAttributo.Valore);

    //    //}

    //    public string GroupName
    //    {
    //        get
    //        {
    //            return EntityAttributo.Attributo.GroupName;
    //        }
    //    }

    //    public ICommand LeftDoubleClickCommand
    //    {
    //        get
    //        {
    //            return new CommandHandler(() => this.LeftDoubleClick());
    //        }
    //    }

    //    void LeftDoubleClick()
    //    {

    //    }

    //    bool _isHilighted = false;
    //    public bool IsHilighted
    //    {
    //        get
    //        {
    //            return _isHilighted;
    //        }
    //        set
    //        {
    //            if (SetProperty(ref _isHilighted, value))
    //            {
    //            }
    //        }
    //    }




    //    public bool IsSourceGuid
    //    {
    //        get
    //        {
    //            Attributo sourceAttributo = _entitiesHelper.GetSourceAttributo(This.Attributo);
    //            if (sourceAttributo == null)
    //                return false;

    //            return sourceAttributo.Definizione.Codice == BuiltInCodes.DefinizioneAttributo.Guid;
    //        }
    //    }

    //    public bool IsAdvanced
    //    {
    //        get =>  EntityAttributo.Attributo.IsAdvanced; 
    //    }

    //    public ICommand CopyRtfFieldCommand { get => new CommandHandler(() => this.CopyRtfField()); } 
    //    void CopyRtfField()
    //    {
    //        //RtfDataService rtfdataService = new RtfDataService(Master.DataService);
    //        RtfEntityDataService rtfdataService = new RtfEntityDataService(Master.MainOperation);

    //        IDataObject dataObject = Clipboard.GetDataObject();

    //        List<string> fieldsPath = null;
    //        if (dataObject.GetDataPresent(RtfEntityDataService.RtfFieldClipboardFormat))
    //            fieldsPath = dataObject.GetData(RtfEntityDataService.RtfFieldClipboardFormat) as List<string>;
    //        else
    //        {
    //            fieldsPath = new List<string>();
    //        }

    //        dataObject = new DataObject();
    //        Clipboard.Clear();
    //        string displayName = rtfdataService.GetRtfFieldDisplayName(Master.EntityType.Codice, EntityAttributo.Attributo.Codice);

    //        fieldsPath.Insert(0, displayName);

    //        //max dieci copiati
    //        fieldsPath = fieldsPath.Take(10).ToList();

    //        dataObject.SetData(RtfEntityDataService.RtfFieldClipboardFormat, fieldsPath);
    //        Clipboard.SetDataObject(dataObject);

    //    }

    //    private string AttributoSyntax
    //    {
    //        get => string.Format("att{{{0}}}", Etichetta);
    //    }

    //    public string CopyAttributoSyntaxMenuItemHeader
    //    {
    //        get
    //        {
    //            string headerCopySyntaxAttributo = string.Format("{0} \"{1}\"", LocalizationProvider.GetString("Copia2"), AttributoSyntax);
    //            return headerCopySyntaxAttributo;
    //        }
    //    }

    //    public ICommand CopyAttributoSyntaxCommand { get { return new CommandHandler(() => this.CopyAttributoSyntax()); } }
    //    public void CopyAttributoSyntax()
    //    {
    //        Clipboard.Clear();
    //        DataObject dataObject = new DataObject();
    //        dataObject.SetData(DataFormats.Text, AttributoSyntax);
    //        Clipboard.SetDataObject(dataObject);
    //    }


    //}

    /// <summary>
    /// Rappresenta la vista di un attributo di un'entità nei dettagli
    /// </summary>
    public class DetailAttributoView : NotificationBase<Attributo>
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        EntitiesHelper _entitiesHelper = null;

        public DetailAttributoView(EntitiesListMasterDetailView master, Valore valore, Attributo att = null) : base(att)
        {
            _master = master;

            _entitiesHelper = new EntitiesHelper(_master.DataService);

            _valoreView = CreateValoreView(Attributo, valore);

        }

        public Attributo Attributo { get { return This; } }

        //Valore _valore = null;
        //public Valore Valore { get => _valore; }

        public string Etichetta
        {
            get { return Attributo.Etichetta; }
            set { SetProperty(Attributo.Etichetta, value, () => Attributo.Etichetta = value); }
        }


        public double EtichettaWidth
        {
            get { return Master.EntityType.DetailAttributoEtichettaWidth; }
        }

        ValoreView _valoreView;
        public ValoreView ValoreView
        {
            get
            {
                return _valoreView;
            }
            set
            {
                SetProperty(ref _valoreView, value);
            }
        }

        public ValoreView CreateValoreView(Attributo att, Valore valore)
        {
            ValoreView valoreView = null;

            if (valore == null)
            {
                valoreView = new ValoreTestoView(Master, new ValoreTesto()) { Tag = att.Codice };
            }
            else if (valore.GetType() == typeof(ValoreTesto))
            {
                if (att.Codice == BuiltInCodes.Attributo.Link)
                    valoreView = new ValoreLinkView(Master, valore as ValoreTesto) { Tag = att.Codice };
                else
                    valoreView = new ValoreTestoView(Master, valore as ValoreTesto) { Tag = att.Codice };
            }
            else if (valore.GetType() == typeof(ValoreData))
                valoreView = new ValoreDataView(Master, valore as ValoreData) { Tag = att.Codice };
            else if (valore.GetType() == typeof(ValoreTestoCollection))
            {
                ValoreTestoCollectionView valCollectionView = new ValoreTestoCollectionView(Master, valore as ValoreTestoCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
                valoreView = valCollectionView;
            }
            else if (valore.GetType() == typeof(ValoreGuidCollection))
            {
                if (att.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Allegati)
                {
                    ValoreLinkCollectionView valCollectionView = new ValoreLinkCollectionView(Master, valore as ValoreGuidCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
                    valCollectionView.Init();
                    valoreView = valCollectionView;
                }
                else
                {
                    ValoreGuidCollectionView valCollectionView = new ValoreGuidCollectionView(Master, valore as ValoreGuidCollection) { Tag = att.Codice, Etichetta = att.Etichetta };
                    valCollectionView.Init();
                    valoreView = valCollectionView;
                }
            }
            else if (valore.GetType() == typeof(ValoreTestoRtf))
            {
                ValoreTestoRtf valRtf = valore as ValoreTestoRtf;
                ValoreTestoRtfView valoreTestoRtfView = new ValoreTestoRtfView(Master, valRtf);
                valoreTestoRtfView.Tag = att.Codice;

                if (IsSearched)
                    valoreTestoRtfView.HighlightedText = string.Join(" ", TextsSearched);
                valoreView = valoreTestoRtfView;

            }
            else if (valore.GetType() == typeof(ValoreContabilita))
                valoreView = new ValoreContabilitaView(Master, valore as ValoreContabilita) { Tag = att.Codice };
            else if (valore.GetType() == typeof(ValoreReale))
                valoreView = new ValoreRealeView(Master, valore as ValoreReale) { Tag = att.Codice };
            else if (valore.GetType() == typeof(ValoreGuid))
                valoreView = new ValoreGuidView(Master, valore as ValoreGuid) { Tag = att.Codice };
            else if (valore.GetType() == typeof(ValoreElenco))
                valoreView = new ValoreElencoView(Master, att.Codice, valore as ValoreElenco);
            else if (valore.GetType() == typeof(ValoreColore))
                valoreView = new ValoreColoreView(Master, att.Codice, valore as ValoreColore);
            else if (valore.GetType() == typeof(ValoreBooleano))
                valoreView = new ValoreBooleanoView(Master, valore as ValoreBooleano) { Tag = att.Codice };
            else if (valore.GetType() == typeof(ValoreFormatoNumero))
                valoreView = new ValoreFormatoNumeroView(Master, att.Codice, valore as ValoreFormatoNumero);
            //else if (valore.GetType() == typeof(ValoreVariabile))
            //    valoreView = new ValoreVariabileView(Master, att.Codice, valore as ValoreVariabile);

            valoreView.UpdateValore(valore);
            //bool isreadonly = valoreView.IsReadOnly;
            return valoreView;
        }


        public string CodiceAttributo
        {
            get { return Attributo.Codice; }
        }

        public string SuggestionCodeAttributo
        {
            get { return Attributo.SuggestionCode; }
        }


        bool _isValoreViewVisible = true;
        public bool IsValoreViewVisible
        {
            get { return _isValoreViewVisible; }
            set { SetProperty(ref _isValoreViewVisible, value); }
        }

        public bool IsExpandable
        {
            get
            {
                if (Attributo is AttributoRiferimento)
                {
                    bool isExpandable = false;
                    try
                    {
                        AttributoRiferimento attRif = Attributo as AttributoRiferimento;
                        Dictionary<string, EntityType> entityTypes = Master.DataService.GetEntityTypes();

                        if (entityTypes.ContainsKey(attRif.ReferenceEntityTypeKey))
                        {
                            EntityType entType = entityTypes[attRif.ReferenceEntityTypeKey];
                            if (entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo))
                                isExpandable = entType.Attributi[attRif.ReferenceCodiceAttributo].IsExpandable;
                            else
                            {
                                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "!entType.Attributi.ContainsKey(attRif.ReferenceCodiceAttributo)");
                            }
                        }
                        else
                        {
                            MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "!entityTypes.ContainsKey(attRif.ReferenceEntityTypeKey)");
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
                    }
                    return isExpandable;
                }
                else if (Attributo.ValoreAttributo is ValoreAttributoGuidCollection)
                {
                    var attSettings = Attributo.ValoreAttributo as ValoreAttributoGuidCollection;
                    if (attSettings != null)
                    {
                        if (attSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
                            return true;
                    }
                    return false;
                }
                else
                {
                    return Attributo.IsExpandable;
                }
            }
        }


        public ICommand ExpandCommand
        {
            get
            {
                return new CommandHandler(() => IsExpanded = !IsExpanded);
            }
        }

        public bool IsExpanded
        {
            get { return Attributo.IsExpanded; }
            set
            {
                Attributo.IsExpanded = value;
                UpdateUI();
            }
        }

        public bool IsGroupExpanded
        {
            get { return Master.IsAttributoViewGroupExpanded(Attributo.GroupName); }
            set { Master.SetAttributoViewGroupExpanded(Attributo.GroupName, value); }
        }

        public bool AllowMasterGrouping { get => Attributo.AllowMasterGrouping; }


        public double Height
        {
            get { return IsExpanded ? Double.NaN : Attributo.Height; }
            //get { return IsExpanded ? 200 : Attributo.Height; }
        }

        public bool IsFiltered
        {
            get
            {
                List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => (item.GetCodice() == Attributo.Codice && item.IsValid() && item.IsFiltroAttivato)).ToList();

                if (attFilterView.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsSorted
        {
            get
            {
                AttributoSortView attSortView = Master.RightPanesView.SortView.Items.FirstOrDefault(item => item.Attributo.Codice == Attributo.Codice);
                if (attSortView != null)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsSearched
        {
            get
            {
                List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => (item.GetCodice() == Attributo.Codice && item.IsValid())).ToList();

                if (attFilterView.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public List<string> TextsSearched
        {
            get
            {
                List<AttributoFilterView> attFilterView = Master.RightPanesView.FilterView.Items.Where(item => (item.GetCodice() == Attributo.Codice && item.IsValid())).ToList();
                return attFilterView.Select(item => item.TextSearched).ToList();
            }
        }

        public int TabIndex { get; set; } = 0;


        public void UpdateUI()
        {
            if (ValoreView != null)
                ValoreView.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => IsExpanded));
            RaisePropertyChanged(GetPropertyName(() => EtichettaWidth));
            RaisePropertyChanged(GetPropertyName(() => Height));
            RaisePropertyChanged(GetPropertyName(() => ValoreView));
            RaisePropertyChanged(GetPropertyName(() => IsHilighted));



        }

        public string GroupName
        {
            get
            {
                return Attributo.GroupName;
            }
        }

        public ICommand LeftDoubleClickCommand
        {
            get
            {
                return new CommandHandler(() => this.LeftDoubleClick());
            }
        }

        void LeftDoubleClick()
        {

        }

        bool _isHilighted = false;
        public bool IsHilighted
        {
            get
            {
                return _isHilighted;
            }
            set
            {
                if (SetProperty(ref _isHilighted, value))
                {
                }
            }
        }




        public bool IsSourceGuid
        {
            get
            {
                Attributo sourceAttributo = _entitiesHelper.GetSourceAttributo(Attributo);
                if (sourceAttributo == null)
                    return false;

                return sourceAttributo.Definizione.Codice == BuiltInCodes.DefinizioneAttributo.Guid;
            }
        }

        public bool IsAdvanced
        {
            get => Attributo.IsAdvanced;
        }

        public ICommand CopyRtfFieldCommand { get => new CommandHandler(() => this.CopyRtfField()); }
        void CopyRtfField()
        {
            //RtfDataService rtfdataService = new RtfDataService(Master.DataService);
            RtfEntityDataService rtfdataService = new RtfEntityDataService(Master.MainOperation);

            IDataObject dataObject = Clipboard.GetDataObject();

            List<string> fieldsPath = null;
            if (dataObject.GetDataPresent(RtfEntityDataService.RtfFieldClipboardFormat))
                fieldsPath = dataObject.GetData(RtfEntityDataService.RtfFieldClipboardFormat) as List<string>;
            else
            {
                fieldsPath = new List<string>();
            }

            dataObject = new DataObject();
            Clipboard.Clear();
            string displayName = rtfdataService.GetRtfFieldDisplayName(Master.EntityType.Codice, Attributo.Codice);

            fieldsPath.Insert(0, displayName);

            //max dieci copiati
            fieldsPath = fieldsPath.Take(10).ToList();

            dataObject.SetData(RtfEntityDataService.RtfFieldClipboardFormat, fieldsPath);
            Clipboard.SetDataObject(dataObject);

        }

        private string AttributoSyntax
        {
            get => string.Format("att{{{0}}}", Etichetta);
        }

        public string CopyAttributoSyntaxMenuItemHeader
        {
            get
            {
                string headerCopySyntaxAttributo = string.Format("{0} \"{1}\"", LocalizationProvider.GetString("Copia2"), AttributoSyntax);
                return headerCopySyntaxAttributo;
            }
        }

        public ICommand CopyAttributoSyntaxCommand { get { return new CommandHandler(() => this.CopyAttributoSyntax()); } }
        public void CopyAttributoSyntax()
        {
            Clipboard.Clear();
            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.Text, AttributoSyntax);
            Clipboard.SetDataObject(dataObject);
        }


    }

}