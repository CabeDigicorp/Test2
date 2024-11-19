using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Globalization;
using Commons;
using MasterDetailModel;
using CommonResources;
using Model;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.IO;
using DevExpress.Xpf.Core.Native;
using _3DModelExchange;
using static System.Data.Odbc.ODBC32;
using System.Security.RightsManagement;
using Net.Sgoliver.NRtfTree.Core;
//using ColorConverter = Commons.ColorConverter;

namespace MasterDetailView
{
    /// <summary>
    /// Valore class
    /// </summary>
    public interface ValoreView
    {
        object Tag { get; set; }
        string Testo { get; }
        bool IsReadOnly { get; }
        void UpdateUI();
        void UpdateValore(Valore val);


    }


    public class ValoreTestoView : NotificationBase<ValoreTesto>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreTestoView(EntitiesListMasterDetailView master, ValoreTesto v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                return GetDeepText();//This.View;
            }
            set
            {
                if (SetProperty(This.V, value, () => This.V = value))
                {
                    if (!This.IsMultiValore())
                    {

                        if (Master.IsMultipleModify)
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            Master.UpdateCache(true);
                        }
                        else
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            //Master.UpdateCache(false);
                            Master.UpdateCache(true);

                            //ricalcolo tutti gli attributi dell'entità correntemente visualizzata
                            //Master.AttributiEntities.UpdateValues(/*This*/);
                            //Master.AttributiEntities.UpdateUI();
                        }

                    }
                }

            }
        }

        public string Result
        {
            get
            {
                return GetResultDeepText();
            }
        }

        public bool IsResultVisible
        {
            get
            {
                bool res = false;
                string codiceAttributo = Tag as string;

                if (!Master.EntityType.Attributi.ContainsKey(codiceAttributo))
                    return false;

                Attributo att = Master.EntityType.Attributi[codiceAttributo];
                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                {
                    if (This.IsMultiValore(true))
                        res = true;
                    else
                        res = Testo != Result;
                }
                else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                {
                    res = true;
                }

                return res;
            }
        }

        public bool IsSourceVisible
        {
            get
            {
                bool res = true;
                string codiceAttributo = Tag as string;
                if (!Master.EntityType.Attributi.ContainsKey(codiceAttributo))
                    return false;

                Attributo att = Master.EntityType.Attributi[codiceAttributo];
                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                {
                    res = false;
                }

                return res;
            }
        }


        string GetDeepText()
        {
            if (IsPreviewMode && !Master.IsMultipleModify)
            {
                Valore val = Master.AttributiEntities.GetValoreAttributo(Tag as string, true, false);
                if (val is ValoreTesto)
                {
                    ValoreTesto valTesto = val as ValoreTesto;
                    return valTesto.V != null ? valTesto.V : "";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return This.V;
            }

        }

        string GetResultDeepText()
        {
            if (IsPreviewMode && !Master.IsMultipleModify)
            {
                Valore val = Master.AttributiEntities.GetValoreAttributo(Tag as string, true, false);
                if (val is ValoreTesto)
                {
                    ValoreTesto valTesto = val as ValoreTesto;
                    return valTesto.Result != null ? valTesto.Result : "";
                }
                else
                {
                    return "";
                }
            }
            else if (Master.IsMultipleModify)
            {
                if (This.IsMultiValore(true))
                    return LocalizationProvider.GetString(ValoreHelper.Multi);
                else
                    return This.Result;
            }
            else
            {
                return This.Result;
            }

        }

        public bool IsReadOnly
        {
            get
            {

                bool ret = false;
                string attCode = Tag as string;

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    if (att.IsValoreReadOnly ||
                        att.IsValoreLockedByDefault ||
                        att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                        att.IsPreviewMode)
                        return true;

                }

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }


                return ret;
            }
        }
        public bool IsPreviewable
        {
            get
            {
                if (!Master.EntityType.Attributi.ContainsKey(Tag as string))
                    return false;

                if (Master.IsMultipleModify)
                    return false;

                AttributoRiferimento attRif = Master.EntityType.Attributi[Tag as string] as AttributoRiferimento;
                if (attRif != null)
                {
                    Attributo attPrimary = Master.EntitiesHelper.GetSourceAttributo(attRif);
                    if (attPrimary == null)
                        return false;

                    return Master.DataService.GetEntityTypes()[attPrimary.EntityType.GetKey()].AttributoIsPreviewable(attPrimary.Codice);
                }
                else
                    return Master.EntityType.AttributoIsPreviewable(Tag as string);
            }
        }

        public ICommand PreviewCommand
        {
            get
            {
                return new CommandHandler(() => IsPreviewMode = !IsPreviewMode);
            }
        }
        public bool IsPreviewMode
        {
            get
            {
                if (!Master.EntityType.Attributi.ContainsKey(Tag as string))
                    return false;

                if (Master.EntityType.Attributi[Tag as string] is AttributoRiferimento && Master.IsMultipleModify)
                    return true;
                else
                    return Master.EntityType.Attributi[Tag as string].IsPreviewMode;
            }
            set
            {
                Master.EntityType.Attributi[Tag as string].IsPreviewMode = value;
                RaisePropertyChanged(GetPropertyName(() => IsPreviewMode));
                RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
                Master.UpdateCache(true);
            }
        }

        public string Background
        {
            get
            {
                //if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                //{
                //    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                //    if (IsReadOnly || isAttRif)
                //        return ColorConverter.ColorsEnum.WhiteSmoke.ToString();// "WhiteSmoke";
                //}

                //return ColorConverter.ColorsEnum.White.ToString();// "White";

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();
                }

                return Colors.White.ToString();
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
            RaisePropertyChanged(GetPropertyName(() => IsResultVisible));
            RaisePropertyChanged(GetPropertyName(() => IsSourceVisible));
            RaisePropertyChanged(GetPropertyName(() => Result));
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public ICommand ResultMouseDoubleClickCommand { get => new CommandHandler(() => this.ResultMouseDoubleClick()); }
        void ResultMouseDoubleClick()
        {
            string codiceAttributo = Tag as string;
            Attributo att = Master.EntityType.Attributi[Tag as string];
            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(att))
                {
                }
                else
                {
                    Master.ReplaceValore(this);
                }
            }
            else
            {
                DetailAttributoView attView = Master.AttributiEntities.AttributiValoriComuniView.FirstOrDefault(item => item.CodiceAttributo == Tag as string);
                attView.IsExpanded = true;
            }
        }

        public void UpdateValore(Valore val)
        {
            This.Update(val);
        }

        public string PreviewButtonToolTip { get => LocalizationProvider.GetString("VisualizzaStruttura"); }

    }

    public class ValoreDataView : NotificationBase<ValoreData>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }
        public ValoreDataView(EntitiesListMasterDetailView master, ValoreData v = null) : base(v)
        {
            _master = master;
            TimePickerVisible = false;
        }

        public object Tag { get; set; }

        public DateTime? Data
        {
            get
            {
                Attributo att = Master.EntityType.Attributi[Tag as string];
                FormatString = att.ValoreFormat;
                if (FormatString.Contains("H") || FormatString.Contains("m"))
                    TimePickerVisible = true;
                else
                    TimePickerVisible = false;

                //if (This.V == null)
                //    return (att.ValoreDefault as ValoreData).V;

                 return This.V;
            }
            set
            {
                Valore oldValore = new ValoreData() { V = This.V };
                if (SetProperty(This.V, value, () => This.V = value))
                {
                    if (!This.IsMultiValore())
                    {

                        if (Master.IsMultipleModify)
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            Master.UpdateCache(true);
                        }
                        else
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This, oldValore);
                            Master.UpdateCache(true);
                        }
                    }
                }
            }
        }
        
        //public object? EditValue
        //{
        //    get
        //    {
        //        return Data;
        //    }
        //    set
        //    {
        //        if (value is DateTime?)
        //            Data =  value as DateTime?;

        //    }

        //}

        private string _FormatString;
        public string FormatString
        {
            get
            {
                return _FormatString;
            }
            set
            {
                if (SetProperty(ref _FormatString, value))
                {
                    _FormatString = value;
                }
            }
        }

        private bool _TimePickerVisible;
        public bool TimePickerVisible
        {
            get
            {
                return _TimePickerVisible;
            }
            set
            {
                if (SetProperty(ref _TimePickerVisible, value))
                {
                    UpdateUI();
                }
            }
        }

        public string Mask { get => TimePickerVisible? "g" : "d"; } //for devexpress

        public string Testo
        {
            get { return This.ToPlainText(); }
        }

        public bool IsReadOnly
        {
            get
            {
                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                string attCode = Tag.ToString();

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }

                //EntityType entType = null;
                //if (Master.SelectedEntityView != null)
                //    entType = Master.EntitiesHelper.GetTreeEntityType(Master.SelectedEntityView.Entity);

                //if (entType.Attributi.ContainsKey(attCode))
                //{
                //    Attributo att = entType.Attributi[attCode];
                //    if (att.IsValoreReadOnly || att.IsValoreLockedByDefault || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                //        return true;
                //}

                return false;
            }
        }
        public bool IsPreviewable { get => false; }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
            RaisePropertyChanged(GetPropertyName(() => Data));
            RaisePropertyChanged(GetPropertyName(() => Foreground));
            RaisePropertyChanged(GetPropertyName(() => IsMultiValore));
            RaisePropertyChanged(GetPropertyName(() => MaxWidth));
            RaisePropertyChanged(GetPropertyName(() => Mask));

        }

        public void UpdateValore(Valore val)
        {
            This.Update(val);
        }

        public string Background
        {
            get
            {
                //bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                //if (IsReadOnly || isAttRif)
                //    return ColorConverter.ColorsEnum.WhiteSmoke.ToString();
                //else return ColorConverter.ColorsEnum.White.ToString();

                bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                if (IsReadOnly || isAttRif)
                    return Colors.WhiteSmoke.ToString();
                else return Colors.White.ToString();
            }
        }

        public string Foreground
        {
            get
            {
                if (This.V == null)
                    return Colors.Gray.ToString();
                return Colors.Black.ToString();

            }
        }

        public DateTime DefaultValue
        {
            get
            {
                return new DateTime(2021, 12, 25);

            }
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public bool IsMultiValore { get => This.IsMultiValore(); }
        public string MultiText { get => LocalizationProvider.GetString(ValoreHelper.Multi); }
        public double MaxWidth
        {
            get
            {
                if (This.IsMultiValore())
                    return 20;
                else
                    return 600;
            }
        }


    }

    #region ValoreCollection

    public class ValoreCollectionView : NotificationBase<ValoreCollection>, ValoreView
    {
        public object Tag { get; set; }//tipicamente codice attributo nel quale è contenuto

        protected EntitiesListMasterDetailView _master = null;
        public EntitiesListMasterDetailView Master { get => _master; }

        public ValoreCollectionView(EntitiesListMasterDetailView master, ValoreCollection v = null) : base(v)
        {
            _master = master;
        }


        public string Etichetta { get; set; }

        protected ObservableCollection<ValoreCollectionItemView> _valori = new ObservableCollection<ValoreCollectionItemView>();
        public ObservableCollection<ValoreCollectionItemView> Valori
        {
            get { return _valori; }
        }
        public string Testo
        {
            get
            {
                if (This.Items != null)
                    return This.ToPlainText();
                else
                    return "";
            }
        }



        int _selectedIndex = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    //Master.OnAttributoValueSelected(this);
                }

            }
        }

        ValoreCollectionItemView _current = null;
        public ValoreCollectionItemView Current
        {
            get { return _current; }
            set
            {
                SetProperty(ref _current, value);
            }
        }
        public ICommand AddCommand
        {
            get
            {
                return new CommandHandler(() => this.Add());
            }
        }
        public virtual void Add()
        {
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
            RaisePropertyChanged(GetPropertyName(() => ItemsCount));
            RaisePropertyChanged(GetPropertyName(() => IsItemsSelectionByHand));
            RaisePropertyChanged(GetPropertyName(() => IsItemsSelectionByFilter));
            RaisePropertyChanged(GetPropertyName(() => AllowAddItems));
        }

        public bool IsMultiValore
        {
            get { return This.IsMultiValore(); }
        }


        //public bool IsReadOnly { get => Master.EntityType.Attributi[Tag as string].IsValoreReadOnly; }
        public bool IsReadOnly
        {
            get
            {
                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    bool isReadOnly = (att.IsValoreReadOnly || att.IsValoreLockedByDefault || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento);
                    return isReadOnly;
                }
                else return false;
            }
        }

        public bool AllowAddItems
        {
            get => !IsReadOnly && IsItemsSelectionByHand;
        }

        public Commons.ColorConverter.ColorsEnum Background1
        {
            get
            {
                if (IsReadOnly)
                    return Commons.ColorConverter.ColorsEnum.WhiteSmoke;
                else
                    return Commons.ColorConverter.ColorsEnum.Transparent;
            }
        }
        public SolidColorBrush Background
        {
            get
            {
                if (IsReadOnly)
                    return new SolidColorBrush(Colors.WhiteSmoke);
                else
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public bool IsPreviewable { get => false; }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public void UpdateValore(Valore val)
        {

        }

        public string ItemsCount
        {
            get => string.Format("({0})", _valori.Count);
        }

        public bool IsItemsSelectionByHand { get => GetItemsSelectionType() == ItemsSelectionTypeEnum.ByHand; }
        public bool IsItemsSelectionByFilter { get => GetItemsSelectionType() == ItemsSelectionTypeEnum.ByFilter; }
        

        private ItemsSelectionTypeEnum GetItemsSelectionType()
        {
            string codiceAttributo = Tag as string;

            Attributo att = Master.EntitiesHelper.GetSourceAttributo(Master.EntityType.GetKey(), codiceAttributo);

            //if (Master.EntityType.Attributi.TryGetValue(codiceAttributo, out att))
            //{
            if (att != null)
            { 
                ValoreAttributoGuidCollection valAttGuidColl = att.ValoreAttributo as ValoreAttributoGuidCollection;
                if (valAttGuidColl != null)
                {
                    return valAttGuidColl.ItemsSelectionType;
                        
                }
            }
            return ItemsSelectionTypeEnum.Nothing;
        }
    }

    public class ValoreCollectionItemView : NotificationBase<ValoreCollectionItem>
    {
        protected ValoreCollectionView _owner;

        protected EntitiesListMasterDetailView _master = null;
        public EntitiesListMasterDetailView Master { get => _master; }

        public ValoreCollectionItemView(EntitiesListMasterDetailView master, ValoreCollectionView owner, ValoreCollectionItem v = null) : base(v)
        {
            _master = master;
            _owner = owner;
        }

        Valore AsValore() { return This as Valore; }

        public Guid Id
        {
            get { return This.Id; }
        }

        public virtual string Testo1 { get; set; }
        public virtual string Testo2 { get; set; }
        public virtual string Testo3 { get; set; }
        public virtual string ToolTip
        {
            get
            {
                string str = string.Format("{0}\n{1}", Testo1, Testo2);
                return str;
            }
        }

        public bool Removed
        {
            get { return This.Removed; }
        }

        public ICommand RemoveCommand
        {
            get
            {
                return new CommandHandler(() => this.Remove());
            }
        }
        public void Remove()
        {
            Master.AttributiEntities.RemoveItemInValoreCollection(_owner.Tag as string, AsValore());

            _owner.Valori.Remove(this);

            
        }

        public ICommand EditCommand
        {
            get
            {
                return new CommandHandler(() => this.Edit());
            }
        }
        public virtual void Edit()
        {
        }

        public string Etichetta
        {
            get { return _owner.Etichetta; }
        }

        public bool IsVisible { get { return true; } }

        bool _isEditing = false;
        public bool IsEditing { get => _isEditing; }

        public bool IsReadOnly { get => _owner.IsReadOnly; }

    }

    public class ValoreTestoCollectionView : ValoreCollectionView
    {
        public ValoreTestoCollectionView(EntitiesListMasterDetailView master, ValoreTestoCollection v = null) : base(master, v)
        {
            if (v.Items != null)
            {
                v.Items.RemoveAll(item => item.Removed == true);
                _valori.Clear();
                foreach (ValoreTestoCollectionItem item in v.Items)
                {
                    if (!item.Removed)
                        _valori.Add(new ValoreTestoCollectionItemView(Master, this, item));
                }
            }
        }

        public override void Add()
        {
            ValoreTestoCollectionItem newValore = new ValoreTestoCollectionItem() { Id = Guid.NewGuid(), Testo1 = "", Testo2 = "", Testo3 = "" };   //aggiunge newValore a tutte le entità interessate (modifica multipla)

            Master.AttributiEntities.AddItemInValoreCollection(Tag as string, newValore);

            Valori.Add(new ValoreTestoCollectionItemView(Master, this, newValore));
            Current = Valori[Valori.Count - 1];

            Master.WindowService.EditAttributoMultiValoreItemWindow(Master, Current as ValoreTestoCollectionItemView);

            Master.ReplaceValore(this);
            //Master.UpdateCache(true);

            UpdateUI();
        }
    }

    public class ValoreTestoCollectionItemView : ValoreCollectionItemView
    {
        public ValoreTestoCollectionItemView(EntitiesListMasterDetailView master, ValoreCollectionView owner, ValoreTestoCollectionItem v = null) : base(master, owner, v)
        {
        }

        new ValoreTestoCollectionItem This { get => base.This as ValoreTestoCollectionItem; }

        public override void Edit()
        {
            Attributo att = Master.EntityType.Attributi[_owner.Tag as string];
            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                return;
            }
            else
            {

                _owner.Current = this;
                Master.WindowService.EditAttributoMultiValoreItemWindow(Master, this);
            }
        }

        public override string Testo1
        {
            get { return This.Testo1; }
            set
            {
                if (SetProperty(This.Testo1, value, () => This.Testo1 = value))
                {
                    Master.AttributiEntities.ReplaceItemInValoreCollection(_owner.Tag as string, This);
                    //RaisePropertyChanged(GetPropertyName(() => this.Testo1Height));
                }
            }
        }
        public override string Testo2
        {
            get { return This.Testo2; }
            set
            {
                if (SetProperty(This.Testo2, value, () => This.Testo2 = value))
                {
                    Master.AttributiEntities.ReplaceItemInValoreCollection(_owner.Tag as string, This);
                    //RaisePropertyChanged(GetPropertyName(() => this.Testo2Height));
                }
            }
        }
        public override string Testo3
        {
            get { return This.Testo3; }
            set
            {
                if (SetProperty(This.Testo3, value, () => This.Testo3 = value))
                {
                    Master.AttributiEntities.ReplaceItemInValoreCollection(_owner.Tag as string, This);
                    //RaisePropertyChanged(GetPropertyName(() => this.Testo3Height));
                }
            }
        }
    }

    public class ValoreGuidCollectionView : ValoreCollectionView
    {
        public ValoreGuidCollectionView(EntitiesListMasterDetailView master, ValoreGuidCollection v = null) : base(master, v)
        {
        }

        protected virtual ValoreGuidCollectionItemView NewItem(EntitiesListMasterDetailView master, ValoreCollectionView collectionView, ValoreGuidCollectionItem item)
        {
            return new ValoreGuidCollectionItemView(master, collectionView, item);
        }

        public void Init()
        {

            List<ValoreCollectionItem> items = This.Items;
            //List<ValoreCollectionItem> items = This.V;

            if (items != null)
            {
                items.RemoveAll(item => item.Removed == true);
                _valori.Clear();
                foreach (ValoreGuidCollectionItem item in items)
                {
                    if (GetEntityById(item.EntityId) == null)
                        item.Removed = true;

                    if (!item.Removed)
                        _valori.Add(NewItem(Master, this, item));
                }
            }


            //if (This.V != null)
            //{
            //    This.V.RemoveAll(item => item.Removed == true);
            //    _valori.Clear();
            //    foreach (ValoreGuidCollectionItem item in This.V)
            //    {
            //        if (GetEntityById(item.EntityId) == null)
            //            item.Removed = true;

            //        if (!item.Removed)
            //            _valori.Add(NewItem(Master, this, item));
            //    }
            //}
        }



        public Entity GetEntityById(Guid id)
        {
            string codiceAttributo = Tag as string;
            if (!Master.EntityType.Attributi.ContainsKey(codiceAttributo))
                return null;

            Attributo att = Master.EntityType.Attributi[codiceAttributo];
            if (att == null)
                return null;

            if (att is AttributoRiferimento)
                att = Master.GetSourceAttributoOf(att);

            Entity refEnt = Master.EntitiesHelper.GetDataServiceEntityById(att.GuidReferenceEntityTypeKey, id);
            return refEnt;
        }

        public override void Add()
        {
            bool res = false;
            Attributo att = Master.EntityType.Attributi[Tag as string];

            if (!Master.DataService.GetEntityTypes().ContainsKey(att.GuidReferenceEntityTypeKey))
                return;

            EntityType refEntityType = Master.DataService.GetEntityTypes()[att.GuidReferenceEntityTypeKey];

            List<Guid> selectedItems = new List<Guid>();

            if (refEntityType is DivisioneItemType)
            {
                DivisioneItemType divType = refEntityType as DivisioneItemType;

                res = Master.WindowService.SelectDivisioneIdsWindow(divType.DivisioneId, ref selectedItems,
                    LocalizationProvider.GetString("AggiungiUnaPiuVoci"),
                    SelectIdsWindowOptions.Nothing, null);
            }
            else if (refEntityType is PrezzarioItemType)
            {
                EntityTypeViewSettings viewSettings = null;
                string externalPrezzarioFileName = string.Empty;

                if (Master.WindowService.SelectPrezzarioIdsWindow(ref selectedItems, ref externalPrezzarioFileName,
                    LocalizationProvider.GetString("AggiungiUnaPiuVoci"), 0, true, true, true, ref viewSettings))
                {

                    IEnumerable<Guid> prezzarioInternoIds = null;

                    Dictionary<string, IDataService> prezzariCache = Master.MainOperation.GetPrezzariCache();
                    if (prezzariCache.ContainsKey(externalPrezzarioFileName))
                    {
                        //Importazione nel prezzario interno degli articoli 

                        EntitiesImportStatus importStatus = new EntitiesImportStatus();
                        importStatus.TargetPosition = TargetPosition.Bottom;
                        importStatus.ConflictAction = EntityImportConflictAction.Undefined;
                        importStatus.Source = prezzariCache[externalPrezzarioFileName];
                        importStatus.SourceName = externalPrezzarioFileName;
                        selectedItems.ForEach(item => importStatus.StartingEntitiesId.Add(new EntityImportId() { SourceId = item, SourceEntityTypeKey = PrezzarioItemType.CreateKey() }));

                        //Master.DataService.ImportEntities(importStatus);
                        while (importStatus.Status != EntityImportStatusEnum.Completed)
                        {
                            Master.DataService.ImportEntities(importStatus);
                            if (importStatus.Status == EntityImportStatusEnum.Waiting)
                            {
                                if (!Master.WindowService.EntitiesImportWindow(importStatus))
                                    break;
                            }
                        }

                        prezzarioInternoIds = importStatus.StartingEntitiesId.Select(item => item.TargetId).ToList();
                        selectedItems = prezzarioInternoIds.ToList();

                        Master.MainOperation.UpdateEntityTypesView(new List<string>(importStatus.EntityTypes.Keys));
                    }
                    res = true;
                }
            }
            else
            {
                res = Master.WindowService.SelectEntityIdsWindow(att.GuidReferenceEntityTypeKey, ref selectedItems,
                    LocalizationProvider.GetString("AggiungiUnaPiuVoci"),
                    SelectIdsWindowOptions.Nothing, null, null);
            }

            if (res)
            {
                HashSet<Guid> entityIds = GetEntityIds();

                List<Valore> valItems = new List<Valore>();
                    
                foreach (Guid id in selectedItems)
                {
                    if (entityIds.Contains(id))
                        continue;

                    ValoreGuidCollectionItem newValore = new ValoreGuidCollectionItem()
                    { Id = Guid.NewGuid(), EntityId = id };

                    valItems.Add(newValore);
                    //Master.AttributiEntities.AddItemInValoreCollection(att.Codice, newValore);

                    Valori.Add(NewItem(Master, this, newValore));
                }

                Master.AttributiEntities.AddItemsInValoreCollection(att.Codice, valItems);
                Master.ReplaceValore(this);

                
                Master.UpdateCache(true);

            }
            Current = null;
            UpdateUI();
        }

        public HashSet<Guid> GetEntityIds()
        {
            HashSet<Guid> entityIds = new HashSet<Guid>(This.Items.Select(item => (item as ValoreGuidCollectionItem).EntityId));
            return entityIds;
        }

        //public string FilterAsString
        //{
        //    get
        //    {
        //        string str = string.Empty;
        //        Attributo att = null;
        //        Master.EntityType.Attributi.TryGetValue(Tag as string, out att);
        //        if (att != null)
        //        {
        //            var attSettings = att.ValoreAttributo as ValoreAttributoGuidCollection;
        //            if (attSettings != null)
        //            {
        //                if (attSettings.ItemsSelectionType == ItemsSelectionTypeEnum.ByFilter)
        //                {
        //                    FilterData filterData = (This as ValoreGuidCollection)?.Filter;
        //                    if (filterData != null)
        //                    {
        //                        AttributoFilterData attFilterData = filterData.Items.FirstOrDefault();
        //                        if (attFilterData != null)
        //                        {
        //                            str = Master.EntitiesHelper.GetAttributoFilterTextDescription(attFilterData);
        //                            //str = string.Join("\n", attFilterData.CheckedValori);
        //                        }
                                

        //                    }
        //                }
        //            }
        //        }

        //        return str;
        //    }
        //}


    }




    public class ValoreGuidCollectionItemView : ValoreCollectionItemView
    {
        public ValoreGuidCollectionItemView(EntitiesListMasterDetailView master, ValoreCollectionView owner, ValoreGuidCollectionItem v = null) : base(master, owner, v)
        {
        }

        new ValoreGuidCollectionItem This { get => base.This as ValoreGuidCollectionItem; }

        /// <summary>
        /// Attributi ordinati dell'entità riferita
        /// </summary>
        List<Attributo> _attributiRefEntity = null;

        protected ValoreGuidCollectionView Owner { get => _owner as ValoreGuidCollectionView; }

        public override void Edit()
        {
            Attributo att = Master.EntityType.Attributi[_owner.Tag as string];
            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                return;
            }
            else
            {


                bool res = false;
                _owner.Current = this;

                EntityType refEntityType = Master.DataService.GetEntityTypes()[att.GuidReferenceEntityTypeKey];

                List<Guid> selectedItems = new List<Guid>();

                if (This != null && This.EntityId != Guid.Empty)
                    selectedItems.Add(This.EntityId);

                //bool isSingleSelection = true;
                //bool allowNoSeleciton = false;

                var modelActionCount = Master.ModelActionsStack.GetCount();


                if (refEntityType is DivisioneItemType)
                {
                    DivisioneItemType divType = refEntityType as DivisioneItemType;

                    res = Master.WindowService.SelectDivisioneIdsWindow(divType.DivisioneId, ref selectedItems,
                        LocalizationProvider.GetString("SelezionaUnaVoce"),
                        SelectIdsWindowOptions.IsSingleSelection, null);
                }
                else
                {
                    res = Master.WindowService.SelectEntityIdsWindow(att.GuidReferenceEntityTypeKey, ref selectedItems,
                        LocalizationProvider.GetString("SelezionaUnaVoce"),
                        SelectIdsWindowOptions.IsSingleSelection | SelectIdsWindowOptions.NotAllowAcceptSelection, null, null);
                }

                if (res)
                {
                    HashSet<Guid> entityIds = (_owner as ValoreGuidCollectionView).GetEntityIds();

                    Guid selectedId = selectedItems.FirstOrDefault();
                    if (selectedId == Guid.Empty)
                        return;

                    if (!entityIds.Contains(selectedId))
                    {

                        if (SetProperty(This.EntityId, selectedId, () => This.EntityId = selectedId))
                        {
                            Master.AttributiEntities.ReplaceItemInValoreCollection(att.Codice, This);
                        }
                    }

                    UpdateUI();
                }

                var modelActionCount2 = Master.ModelActionsStack.GetCount();
                if (modelActionCount2 > modelActionCount)
                {
                    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                    var entTypesToUpdate = entsHelper.GetDependentEntityTypesKey(Master.EntityType.GetKey());
                    entTypesToUpdate.Insert(0, Master.EntityType.GetKey());
                    Master.MainOperation.UpdateEntityTypesView(entTypesToUpdate);

                    Master.UpdateCache(true);
                }
            }
        }

        public override string Testo1
        {
            get
            {
                //return GetTesto(0);
                Entity refEnt = Owner.GetEntityById(This.EntityId);
                string userIdentity = refEnt.ToUserIdentity(UserIdentityMode.SingleLine1);
                return userIdentity;
            }
        }

        public override string Testo2
        {
            get
            {
                //return GetTesto(1);
                Entity refEnt = Owner.GetEntityById(This.EntityId);
                string userIdentity = refEnt.ToUserIdentity(UserIdentityMode.SingleLine2);
                return userIdentity;
            }
        }

        public override string Testo3
        {
            get
            {
                return GetTesto(2);
            }
        }

        public virtual void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo1));
            RaisePropertyChanged(GetPropertyName(() => Testo2));
            RaisePropertyChanged(GetPropertyName(() => Testo3));
        }



        private string GetTesto(int index)
        {
            Entity refEnt = Owner.GetEntityById(This.EntityId);

            if (refEnt == null)
                return string.Empty;

            if (index == 0 || _attributiRefEntity == null)
                _attributiRefEntity = new List<Attributo>(refEnt.EntityType.Attributi.Values.Where(item => item.IsVisible).OrderBy(item => item.DetailViewOrder));

            if (index >= _attributiRefEntity.Count)
                return string.Empty;

            Attributo refEntAtt = _attributiRefEntity[index];

            if (refEntAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
            {
                return String.Empty;
            }
            else
            {
                Valore val = Master.EntitiesHelper.GetValoreAttributo(refEnt, refEntAtt.Codice, false, true);
                if (val == null)
                    return string.Empty;

                return val.PlainText;
            }

        }

        public override string ToolTip
        {
            get
            {
                Entity refEnt = Owner.GetEntityById(This.EntityId);
                return refEnt.ToUserIdentity(UserIdentityMode.Deep);
            }
        }



    }

    #endregion ValoreCollection


    public class ValoreTestoRtfView : NotificationBase<ValoreTestoRtf>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreTestoRtfView(EntitiesListMasterDetailView master, ValoreTestoRtf v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }


        public string Testo
        {
            get
            {
                return GetHighlightedText();
            }
            set
            {


                ////////////////////////////////
                //Non serve viene fatto su GetHighlightedText in EditRtf()
                //Aggiornamento dell'rtf che va a finire nel file join con stili aggiornati
                //string rtf = value;
                //EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                //entsHelper.UpdateRtfByStiliItems(ref rtf);
                ////////////////////////////////

                //                string rtf1 = rtf;
                //                string rtf2 = entsHelper.GetRtfPreview(rtf1);
                //                int p = 0;

                if (IsPreviewMode || IsReadOnly)
                    return;

                string rtf = value;
                if (SetProperty(This.V, rtf, () => This.V = rtf))
                {
                    if (!This.IsMultiValore())
                    {
                        Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                        Master.UpdateCache(false);
                    }

                }


            }
        }


        string GetHighlightedText()
        {
            string rtf = null;
            if (IsPreviewMode && !Master.IsMultipleModify)
            {
                Valore val = Master.AttributiEntities.GetValoreAttributo(Tag as string, true, false);
                if (val is ValoreTestoRtf)
                {
                    ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;
                    rtf = valTestoRtf.V != null ? valTestoRtf.V : "";

                    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);

                    entsHelper.UpdateRtfByStiliItems(ref rtf);
                }
                else
                {
                    return string.Empty;
                }

            }
            else
            {

                rtf = This.V;

                EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                entsHelper.UpdateRtfByStiliItems(ref rtf);


            }

            if (_highlightedText.Any())
                ValoreHelper.HighlightText(ref rtf, _highlightedText);

            return rtf;
        }


        public ICommand EditRtfCommand
        {
            get
            {
                return new CommandHandler(() => this.EditRtf());
            }
        }

        private void EditRtf()
        {
            string testoRtf;
            ValoreHelper.RtfFromPlainString("", out testoRtf);
            if (!This.IsMultiValore())
            {
                testoRtf = Testo;
            }

            Attributo att = Master.EntityType.Attributi[Tag as string];
            string plainText = "";

            Master.WindowService.ShowEditRtfWindow(ref testoRtf, att.Etichetta, out plainText);

            Testo = testoRtf;



        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsPreviewMode));
            RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
            RaisePropertyChanged(GetPropertyName(() => Testo));
            RaisePropertyChanged(GetPropertyName(() => Height));
            RaisePropertyChanged(GetPropertyName(() => Background));

        }

        public ICommand PreviewCommand
        {
            get
            {
                return new CommandHandler(() => IsPreviewMode = !IsPreviewMode);
            }
        }

        public bool IsPreviewMode
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    if (Master.EntityType.Attributi[Tag as string] is AttributoRiferimento && Master.IsMultipleModify)
                        return true;
                    else
                        return Master.EntityType.Attributi[Tag as string].IsPreviewMode;
                }
                return false;
            }
            set
            {
                Master.EntityType.Attributi[Tag as string].IsPreviewMode = value;

                if (Master.SelectedEntityView == null)
                    return;

                //Espando il campo se sono in un valore rtf e voglio vedere la struttura
                if (value && Master.SelectedEntityView.DetailAttributiView.ContainsKey(Tag as string))
                    Master.SelectedEntityView.DetailAttributiView[Tag as string].IsExpanded = true;


                RaisePropertyChanged(GetPropertyName(() => IsPreviewMode));
                RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
                RaisePropertyChanged(GetPropertyName(() => Testo));
                Master.UpdateCache(true);

            }
        }

        string _highlightedText = "";
        public string HighlightedText
        {
            get { return _highlightedText; }
            set { SetProperty(ref _highlightedText, value); }
        }

        public string Background
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    //if (IsReadOnly && !isAttRif)
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                    else return Colors.White.ToString();// "White";
                }
                return Colors.WhiteSmoke.ToString();
            }
        }

        //public bool IsReadOnly { get => Master.EntityType.Attributi[Tag as string].IsValoreReadOnly || Master.EntityType.Attributi[Tag as string].IsPreviewMode; }
        public bool IsReadOnly
        {
            get
            {
                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    bool res = (att.IsValoreReadOnly ||
                        att.IsValoreLockedByDefault ||
                        att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                        att.IsPreviewMode);
                    return res;
                }
                else return false;
            }
        }
        public bool IsPreviewable
        {
            get => !Master.IsMultipleModify;
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            try
            {
                Master.ReplaceValore(this);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public void UpdateValore(Valore val)
        {
            This.Update(val);
        }

        public string PreviewButtonToolTip { get => LocalizationProvider.GetString("VisualizzaStruttura"); }


        public double Height
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    var att = Master.EntityType.Attributi[Tag as string];
                    if (att.IsExpanded)
                        return 200;

                    return att.Height;
                }
                return 0;
            }
        }
    }



    public class ValoreRealeView : NotificationBase<ValoreReale>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreRealeView(EntitiesListMasterDetailView master, ValoreReale v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                return This.ToPlainText();
            }
        }


        public string Numero
        {
            get
            {
                if (/*Master.IsMultipleModify ||*/ This.V == null || This.V == string.Empty)
                    return string.Empty;
                else
                {
                    string num = This.FormatRealResult(Format);
                    if (num != null && num.Any())
                        return string.Format("{0} = ", num);

                    return string.Empty;
                }
            }
        }

        public string Format
        {
            get
            {
                return Master.AttributiEntities.GetValoreAttributoFormat(Tag as string);
                //return Master.EntityType.Attributi[Tag as string].ValoreFormat;
            }
        }

        public string Formula
        {
            get
            {
                //if (This.IsFormulaEqualResult(Format) && !This.IsMultiValore())
                //    return This.FormatRealResult(Format);
                //else
                    return This.V;
            }
            set
            {

                if (SetProperty(This.V, value, () => This.V = value))
                {
                    if (!This.IsMultiValore())
                    {

                        if (Master.IsMultipleModify)
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            Master.UpdateCache(true);
                        }
                        else
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            //Master.UpdateCache(false);
                            Master.UpdateCache(true);

                            //ricalcolo tutti gli attributi dell'entità correntemente visualizzata
                            //Master.Calculator.ResetExpressions();
                            //Master.AttributiEntities.UpdateValues(/*This*/);
                            //Master.AttributiEntities.UpdateUI();
                        }
                    }
                }



            }
        }

        public bool IsReadOnly
        {
            get
            {
                
                bool ret = false;
                string attCode = Tag as string;
                

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (!Master.IsMultipleModify && Master.SelectedEntityView != null)
                {
                    if (Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                    {
                        if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                            return true;
                    }
                }
                else if (Master.IsMultipleModify)
                {
                    if (Master.EntityType.Attributi[attCode].IsValoreReadOnly)
                        return true;
                }

                return ret;
            }
        }

        public bool IsPreviewable { get => false; }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Numero));
            RaisePropertyChanged(GetPropertyName(() => Formula));
            RaisePropertyChanged(GetPropertyName(() => ResultDescription));
            RaisePropertyChanged(GetPropertyName(() => ResultForeground));
        }

        public void UpdateValore(Valore val)
        {
            This.Update(val);
        }

        public string ResultDescription
        {
            get { return This.ResultDescription; }
        }

        public string Background
        {
            get
            {
                bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;

                if (IsReadOnly || isAttRif)
                    return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                else return Colors.White.ToString();// "White";
            }
        }

        public string Foreground
        {
            get
            {
                Attributo att = Master.EntityType.Attributi[Tag as string];
                Attributo attSource = Master.GetSourceAttributoOf(att);

                bool isAttRif = att is AttributoRiferimento;

                if (isAttRif && (attSource.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
                                 attSource.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale))
                    return Colors.Gray.ToString();
                else
                    return Colors.Black.ToString();

            }
        }

        public string ResultForeground
        {
            get
            {

                if (!Master.IsMultipleModify)
                {
                    if (This.RealResult == null || double.IsNaN(This.RealResult.Value))
                        return ColorsHelper.Convert(MyColorsEnum.ErrorColor).ToString();
                }

                return Colors.Black.ToString();

            }
        }

        //public bool IsResultVisible
        //{
        //    get
        //    {
        //        bool res = false;
        //        string codiceAttributo = Tag as string;

            //        if (!Master.EntityType.Attributi.ContainsKey(codiceAttributo))
            //            return false;

            //        Attributo att = Master.EntityType.Attributi[codiceAttributo];
            //        if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
            //        {
            //            res = !This.IsFormulaEqualResult(Format);
            //        }
            //        else if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            //        {
            //            res = true;
            //        }

            //        return res;
            //    }
            //}

            //public bool IsSourceVisible
            //{
            //    get
            //    {
            //        bool res = true;
            //        string codiceAttributo = Tag as string;
            //        if (!Master.EntityType.Attributi.ContainsKey(codiceAttributo))
            //            return false;

            //        Attributo att = Master.EntityType.Attributi[codiceAttributo];
            //        if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            //        {
            //            res = false;
            //        }

            //        return res;
            //    }
            //}

            //public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
            //void MouseDoubleClick()
            //{
            //    Master.ReplaceValore(this);
            //}

            //public ICommand ResultMouseDoubleClickCommand { get => new CommandHandler(() => this.ResultMouseDoubleClick()); }
            //void ResultMouseDoubleClick()
            //{
            //    string codiceAttributo = Tag as string;
            //    Attributo att = Master.EntityType.Attributi[Tag as string];
            //    if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            //    {
            //        if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(att))
            //        {
            //        }
            //        else
            //        {
            //            Master.ReplaceValore(this);
            //        }
            //    }
            //    else
            //    {
            //        DetailAttributoView attView = Master.AttributiEntities.AttributiValoriComuniView.FirstOrDefault(item => item.CodiceAttributo == Tag as string);
            //        attView.IsExpanded = true;
            //    }
            //}

        public ICommand HelpCommand { get => new CommandHandler(() => this.Help()); }
        void Help()
        {
            string codiceAttributo = Tag as string;

            if (codiceAttributo == BuiltInCodes.Attributo.Lavoro)
            {
                IsLavoroHelpPopupOpen = false;
                IsLavoroHelpPopupOpen = true;
            }
            else
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = @"https://join.digicorp.it/guide/#t=Funzioni_e_operatori.htm";
                process.Start();
                //System.Diagnostics.Process.Start(@"https://join.digicorp.it/guide/#t=Funzioni_e_operatori.htm");
            }
        }

        bool _isLavoroHelpPopupOpen = false;
        public bool IsLavoroHelpPopupOpen
        {
            get => _isLavoroHelpPopupOpen;
            set => SetProperty(ref _isLavoroHelpPopupOpen, value);
        }

        public ICommand ApplyRoundUpCommand { get => new CommandHandler(() => this.ApplyRoundUp()); }
        void ApplyRoundUp()
        {

            if (Formula == "[Multi]")
            {
                Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Questa operazione non è consentita per valore _Multi"));
                //string etAtt = Master.EntityType.Attributi[Tag as string].Etichetta;
                //string function = string.Format(ValoreHelper.ItselfFormula, etAtt);
                //Formula = string.Format("{0}({1};8)", RoundUpFunction.FunctionName, function);
            }
            else
            {
                Formula = string.Format("{0}({1};8)", RoundUpFunction.FunctionName, Formula);
            }
        }

        public ICommand ApplyRoundDownCommand { get => new CommandHandler(() => this.ApplyRoundDown()); }
        void ApplyRoundDown()
        {
            if (Formula == "[Multi]")
            {
                Master.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Questa operazione non è consentita per valore _Multi"));
                //string etAtt = Master.EntityType.Attributi[Tag as string].Etichetta;
                //string function = string.Format(ValoreHelper.ItselfFormula, etAtt);
                //Formula = string.Format("{0}({1};8)", RoundDownFunction.FunctionName, function);
            }
            else
            {
                Formula = string.Format("{0}({1};8)", RoundDownFunction.FunctionName, Formula);
            }
        }

    }

    public class ValoreContabilitaView : NotificationBase<ValoreContabilita>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreContabilitaView(EntitiesListMasterDetailView master, ValoreContabilita v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                return This.ToPlainText();
            }
        }

        public string Numero
        {
            get
            {
                if (/*This.IsFormulaEqualResult(Format) || Master.IsMultipleModify || */This.V == null || This.V == "")
                    return "";
                else
                {
                    string num = This.FormatRealResult(Format);
                    if (num != null && num.Any())
                        return string.Format("{0} = ", num);

                    return string.Empty;
                }
            }
        }

        public string Format
        {
            get
            {
                return Master.AttributiEntities.GetValoreAttributoFormat(Tag as string);
                //return Master.EntityType.Attributi[Tag as string].ValoreFormat;
            }
        }

        public string Formula
        {
            get
            {
                //if (This.IsFormulaEqualResult(Format) && !This.IsMultiValore())
                //    return This.FormatRealResult(Format);
                //else
                    return This.V;
            }
            set
            {

                //string v1 = This.RemoveSymbols(value, Format);

                if (SetProperty(This.V, value, () => This.V = value))
                {
                    if (!This.IsMultiValore())
                    {

                        if (Master.IsMultipleModify)
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            Master.UpdateCache(true);
                        }
                        else
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            //Master.UpdateCache(false);
                            Master.UpdateCache(true);

                            //ricalcolo tutti gli attributi dell'entità correntemente visualizzata
                            //Master.Calculator.ResetExpressions();
                            //Master.AttributiEntities.UpdateValues(/*This*/);
                            //Master.AttributiEntities.UpdateUI();
                        }

                    }
                }



            }
        }
        public bool IsReadOnly
        {
            get
            {  
                
                bool ret = false;
                string attCode = Tag as string;
                //if (Master.DataService == null || Master.DataService.IsReadOnly)
                //    return true;

                //if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                //{
                //    Attributo att = Master.EntityType.Attributi[Tag as string];
                //    return (att.IsValoreReadOnly || att.IsValoreLockedByDefault || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento);
                //}
                //else
                //    return false;

                //EntityType entType = Master.EntityType;
                //if (Master.SelectedEntityView != null)
                //    entType = Master.EntitiesHelper.GetTreeEntityType(Master.SelectedEntityView.Entity);

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                //if (Master.EntityType.Attributi.ContainsKey(attCode))
                //{
                //    Attributo att = Master.EntityType.Attributi[attCode];
                //    if (att.IsValoreReadOnly || att.IsValoreLockedByDefault || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                //        return true;
                //}

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }

                return ret;
            }
        }

        public bool IsPreviewable { get => false; }

        public void UpdateUI()
        {
            //if (Master.SelectedEntityView != null)
            //    This = Master.SelectedEntityView.Entity().Attributi[Tag as string].Valore as ValoreContabilita;

            RaisePropertyChanged(GetPropertyName(() => Numero));
            RaisePropertyChanged(GetPropertyName(() => Formula));
            RaisePropertyChanged(GetPropertyName(() => ResultDescription));
            RaisePropertyChanged(GetPropertyName(() => ResultForeground));

        }

        public void UpdateValore(Valore val)
        {
            This.Update(val);
        }

        public string ResultDescription
        {
            get { return This.ResultDescription; }
        }

        public string Background
        {
            get
            {
                bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                //if (IsReadOnly && !isAttRif)
                if (IsReadOnly || isAttRif)
                    return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                else return Colors.White.ToString();// "White";
            }
        }

        public string Foreground
        {
            get
            {
                Attributo att = Master.EntityType.Attributi[Tag as string];
                Attributo attSource = Master.GetSourceAttributoOf(att);

                bool isAttRif = att is AttributoRiferimento;

                if (isAttRif && (attSource.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
                                 attSource.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale))
                    return Colors.Gray.ToString();
                else
                    return Colors.Black.ToString();
            }
        }

        public string ResultForeground
        {
            get
            {
                if (!Master.IsMultipleModify)
                {
                    if (!This.RealResult.HasValue)
                        return ColorsHelper.Convert(MyColorsEnum.ErrorColor).ToString();

                }

                return Colors.Black.ToString();
            }
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public ICommand ResultMouseDoubleClickCommand { get => new CommandHandler(() => this.ResultMouseDoubleClick()); }
        void ResultMouseDoubleClick()
        {
            string codiceAttributo = Tag as string;
            Attributo att = Master.EntityType.Attributi[Tag as string];
            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
            {
                if (Master.EntitiesHelper.IsAttributoRiferimentoGuidCollection(att))
                {
                }
                else
                {
                    Master.ReplaceValore(this);
                }
            }
            else
            {
                DetailAttributoView attView = Master.AttributiEntities.AttributiValoriComuniView.FirstOrDefault(item => item.CodiceAttributo == Tag as string);
                attView.IsExpanded = true;
            }
        }

        public ICommand HelpCommand { get => new CommandHandler(() => this.Help()); }
        void Help()
        {
            //System.Diagnostics.Process.Start(@"https://join.digicorp.it/guide/#t=Funzioni_e_operatori.htm");
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = @"https://join.digicorp.it/guide/#t=Funzioni_e_operatori.htm";
            process.Start();

        }

    }

    public class ValoreGuidView : NotificationBase<ValoreGuid>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreGuidView(EntitiesListMasterDetailView master, ValoreGuid v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public bool IsReadOnly { get => false; }

        public bool IsPreviewable { get => false; }

        public string Background
        {
            get
            {
                bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                //if (IsReadOnly && !isAttRif)
                if (IsReadOnly || isAttRif)
                    return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                else return Colors.White.ToString();// "White";
            }
        }

        public string Testo { get; set; }

        public void UpdateUI() { }

        public void UpdateValore(Valore val)
        {
        }
    }

    public class ValoreElencoView : NotificationBase<ValoreElenco>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ObservableCollection<ValoreAttributoElencoItem> Items { get; set; } = new ObservableCollection<ValoreAttributoElencoItem>();

        public ValoreElencoView(EntitiesListMasterDetailView master, string tag, ValoreElenco v = null) : base(v)
        {
            _master = master;
            Tag = tag;
            Items = new ObservableCollection<ValoreAttributoElencoItem>(ValoreAttributoElenco.Items);

            IsMultiSelection = ValoreAttributoElenco.IsMultiSelection;
            if (SelectedItems == null)
                SelectedItems = new ObservableCollection<object>();

        }


        public ValoreAttributoElenco ValoreAttributoElenco
        {
            get
            {
                if (Tag != null && Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                    Attributo sourceAtt = entsHelper.GetSourceAttributo(att);
                    if (sourceAtt == null)
                        return null;

                    if (sourceAtt.ValoreAttributo == null)
                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "att.ValoreAttributo == null");

                    return sourceAtt.ValoreAttributo as ValoreAttributoElenco;
                }
                return null;
            }
        }

        public string Delimiter { get; set; } = ";";
        public object Tag { get; set; }
        ObservableCollection<object> _SelectedItems;
        public ObservableCollection<object> SelectedItems
        {
            get => _SelectedItems;
            set
            {
                //SetProperty(ref _SelectedItems, value); 
                _SelectedItems = value;
                RaisePropertyChanged(GetPropertyName(() => SelectedItems));
            }
        }
        public int SelectedIndex { get; set; }

        ValoreAttributoElencoItem _selectedItem = null;
        public ValoreAttributoElencoItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                int Ids = 0;
                if (IsMultiSelection)
                {
                    foreach (ValoreAttributoElencoItem Item in SelectedItems)
                    {
                        if (ValoreAttributoElenco.Items.FirstOrDefault(item => item.Text == Item.Text) != null)
                        {
                            Ids |= ValoreAttributoElenco.Items.FirstOrDefault(item => item.Text == Item.Text).Id;
                            ValoreElenco valoreElenco = new ValoreElenco();
                            valoreElenco.ValoreAttributoElencoId = Ids;
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, valoreElenco);
                            This.ValoreAttributoElencoId = Ids;
                        }
                    }
                }
                else
                {
                    if (value == null)
                        return;


                    if (SetProperty(ref _selectedItem, value))
                    {
                        if (_selectedItem.Id == -1)
                        {
                            //select by text
                            if (SetProperty(This.V, _selectedItem.Text, () => This.V = _selectedItem.Text))
                            {
                                ValoreElenco valoreElenco = new ValoreElenco();
                                valoreElenco.ValoreAttributoElencoId = -1;
                                valoreElenco.V = _selectedItem.Text;
                                Master.AttributiEntities.SetValoreAttributo(Tag as string, valoreElenco);
                                Master.UpdateCache(false);
                            }
                        }
                        else
                        {
                            //select by id
                            if (SetProperty(This.ValoreAttributoElencoId, _selectedItem.Id, () => This.ValoreAttributoElencoId = _selectedItem.Id))
                            {
                                Ids = ((ValoreAttributoElencoItem)_selectedItem).Id;
                                ValoreElenco valoreElenco = new ValoreElenco();
                                valoreElenco.ValoreAttributoElencoId = Ids;
                                Master.AttributiEntities.SetValoreAttributo(Tag as string, valoreElenco);
                                Master.UpdateCache(false);
                            }
                        }
                    }
                    //if (SetProperty(ref _selectedItem, value))
                    //{
                    //    _selectedItem = value;
                    //    Ids = ((ValoreAttributoElencoItem)_selectedItem).Id;
                    //    ValoreElenco valoreElenco = new ValoreElenco();
                    //    valoreElenco.ValoreAttributoElencoId = Ids;
                    //    Master.AttributiEntities.SetValoreAttributo(Tag as string, valoreElenco);
                    //    Master.UpdateCache(false);
                    //}
                }
            }
        }

        bool _IsMultiSelection;
        public bool IsMultiSelection
        {
            get => _IsMultiSelection;
            set
            {
                //SetProperty(ref _IsMultiSelection, value); 
                _IsMultiSelection = value;
                RaisePropertyChanged(GetPropertyName(() => IsMultiSelection));
            }
        }
        public string Testo
        {
            get
            {
                if (ValoreAttributoElenco != null)
                {
                    ValoreAttributoElencoItem elItem = null;
                    int Ids = This.ValoreAttributoElencoId;
                    if (Ids >= 0)
                    {
                        if (ValoreAttributoElenco.IsMultiSelection)
                        {
                            foreach (ValoreAttributoElencoItem Item in ValoreAttributoElenco.Items)
                            {
                                if ((Ids & Item.Id) == Item.Id)
                                    if (!SelectedItems.Contains(Item))
                                        SelectedItems.Add(Item);
                            }
                        }
                        else
                        {
                            if (ValoreAttributoElenco.Items.Any(d => d.Id != -1))
                                SelectedItem = ValoreAttributoElenco.Items.FirstOrDefault(item => item.Id == This.ValoreAttributoElencoId);
                            else
                                SelectedItem = ValoreAttributoElenco.Items.FirstOrDefault(item => item.Text == This.V);

                            if (SelectedItem != null)
                                return SelectedItem.Text;

                        }
                    }
                    else
                    {
                        if (ValoreAttributoElenco.IsMultiSelection)
                        {
                            SelectedItems.Add(ValoreAttributoElenco.Items.FirstOrDefault(item => item.Text == This.V));
                        }
                        else
                        {
                            SelectedItem = ValoreAttributoElenco.Items.FirstOrDefault(item => item.Text == This.V);
                            if (SelectedItem != null)
                                return SelectedItem.Text;
                            else
                            {
                                if (!Items.Any(x => x.Id == -1))
                                    Items.Insert(0, new ValoreAttributoElencoItem() { Id = -1, Text = LocalizationProvider.GetString(ValoreHelper.Multi) });

                                SelectedItem = Items.FirstOrDefault(item => item.Text == This.V);
                                return SelectedItem.Text;
                            }
                        }

                    }

                    if (elItem != null)
                        return elItem.Text;
                }
                return null;
            }
            set 
            { 

            }
        }


        public bool IsReadOnly
        {
            get
            {
                bool ret = false;
                string attCode = Tag as string;

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    if (att.IsValoreReadOnly ||
                        att.IsValoreLockedByDefault ||
                        att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                        att.IsPreviewMode)
                        return true;

                }

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }


                return ret;

            }
        }


        public string Background
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                }

                return Colors.White.ToString();// "White";
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
            RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
            RaisePropertyChanged(GetPropertyName(() => Background));
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public void UpdateValore(Valore val)
        {

        }
    }

    public class ValoreColoreView : NotificationBase<ValoreColore>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ObservableCollection<ValoreAttributoColoreItem> Items { get; set; } = new ObservableCollection<ValoreAttributoColoreItem>();

        public ValoreColoreView(EntitiesListMasterDetailView master, string tag, ValoreColore v = null) : base(v)
        {
            _master = master;
            Tag = tag;
            Items = new ObservableCollection<ValoreAttributoColoreItem>(ValoreAttributoColore.Items);
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                return ColorSelected.Text;
            }
        }

        private ValoreAttributoColoreItem _ColorSelected;
        public ValoreAttributoColoreItem ColorSelected
        {
            get
            {
                return Items.Where(d => d.HexValue == This.Hexadecimal).FirstOrDefault();
            }
            set
            {
                if (SetProperty(ref _ColorSelected, value))
                {
                    _ColorSelected = value;
                    Master.AttributiEntities.SetValoreAttributo(Tag as string, ConvertToValoreColore(_ColorSelected));
                    Master.UpdateCache(false);
                }
            }
        }

        ValoreAttributoColoreItem ConvertToValoreAtttribuToColoreItem(ValoreColore v)
        {
            ValoreAttributoColoreItem ObjectConverted = new ValoreAttributoColoreItem();

            if (!String.IsNullOrEmpty(v.Hexadecimal))
            {
                ObjectConverted.HexValue = v.Hexadecimal;
                ObjectConverted.Text = v.V;
                ObjectConverted.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(v.Hexadecimal);
            }

            return ObjectConverted;
        }

        ValoreColore ConvertToValoreColore(ValoreAttributoColoreItem v)
        {
            ValoreColore ObjectConverted = new ValoreColore();
            ObjectConverted.Hexadecimal = v.HexValue;
            ObjectConverted.V = v.Text;
            return ObjectConverted;
        }

        public bool IsReadOnly
        {
            get
            {
                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    return (att.IsValoreReadOnly ||
                            att.IsValoreLockedByDefault ||
                            att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                            att.IsPreviewMode);
                }
                else
                    return false;
            }
        }


        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
            RaisePropertyChanged(GetPropertyName(() => ColorSelected));
        }

        public void UpdateValore(Valore val)
        {

        }

        ValoreAttributoColore ValoreAttributoColore
        {
            get
            {
                if (Tag != null && Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                    Attributo sourceAtt = entsHelper.GetSourceAttributo(att);
                    if (sourceAtt == null)
                        return null;

                    //List<ValoreAttributoColoreItem> lista = new List<ValoreAttributoColoreItem>();

                    //foreach (var item in ColorInfo.ColoriInstallatiInMacchina)
                    //{
                    //    ValoreAttributoColoreItem attributo = new ValoreAttributoColoreItem();
                    //    attributo.HexValue = item.HexValue;
                    //    attributo.Text = item.Name;
                    //    attributo.Color = item.Color;
                    //    lista.Add(attributo);
                    //}

                    if (sourceAtt.ValoreAttributo != null)
                    {
                        //(sourceAtt.ValoreAttributo as ValoreAttributoColore).Items = lista;
                        (sourceAtt.ValoreAttributo as ValoreAttributoColore).Load();
                    }

                    return sourceAtt.ValoreAttributo as ValoreAttributoColore;

                }
                return null;
            }
        }

    }

    public class ValoreBooleanoView : NotificationBase<ValoreBooleano>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreBooleanoView(EntitiesListMasterDetailView master, ValoreBooleano v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public bool? Check
        {
            get { return This.V; }
            set
            {
                if (SetProperty(This.V, value, () => This.V = value))
                {
                    Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                    Master.UpdateCache(true);
                    //EntitiesMasterDetailView.This.UpdateMasterEntitiesChanged();
                    //FilterView.This.Update(Tag as string);
                }
            }
        }

        //string _text = string.Empty;
        //public string Text
        //{
        //    get { return "ciao"; }
        //    set
        //    {
        //        if (SetProperty(This.V, value, () => This.V = value))
        //    }
        //}

        public string Testo
        {
            get { return This.ToPlainText(); }
        }

        public bool IsReadOnly
        {
            get
            {
                bool ret = false;
                string attCode = Tag as string;

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    if (att.IsValoreReadOnly ||
                            att.IsValoreLockedByDefault ||
                            att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                            att.IsPreviewMode)
                            return true;
                }

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }


                return ret;
            }
        }
        public bool IsPreviewable
        {
            get
            {
                if (!Master.EntityType.Attributi.ContainsKey(Tag as string))
                    return false;

                if (Master.IsMultipleModify)
                    return false;

                AttributoRiferimento attRif = Master.EntityType.Attributi[Tag as string] as AttributoRiferimento;
                if (attRif != null)
                {
                    Attributo attPrimary = Master.EntitiesHelper.GetSourceAttributo(attRif);
                    if (attPrimary == null)
                        return false;

                    return Master.DataService.GetEntityTypes()[attPrimary.EntityType.GetKey()].AttributoIsPreviewable(attPrimary.Codice);
                }
                else
                    return Master.EntityType.AttributoIsPreviewable(Tag as string);
            }
        }

        public ICommand PreviewCommand
        {
            get
            {
                return new CommandHandler(() => IsPreviewMode = !IsPreviewMode);
            }
        }
        public bool IsPreviewMode
        {
            get
            {
                if (!Master.EntityType.Attributi.ContainsKey(Tag as string))
                    return false;

                if (Master.EntityType.Attributi[Tag as string] is AttributoRiferimento && Master.IsMultipleModify)
                    return true;
                else
                    return Master.EntityType.Attributi[Tag as string].IsPreviewMode;
            }
            set
            {
                Master.EntityType.Attributi[Tag as string].IsPreviewMode = value;
                RaisePropertyChanged(GetPropertyName(() => IsPreviewMode));
                RaisePropertyChanged(GetPropertyName(() => IsReadOnly));
                Master.UpdateCache(true);
            }
        }

        public string Background
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                }

                return Colors.White.ToString();// "White";
            }
        }

        public void UpdateUI()
        {
            //RaisePropertyChanged(GetPropertyName(() => Testo));
        }

        public void UpdateValore(Valore val)
        {

        }
    }

    public class ValoreFormatoNumeroView : NotificationBase<ValoreFormatoNumero>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ObservableCollection<ValoreAttributoFormatoNumeroItem> Items { get; set; } = new ObservableCollection<ValoreAttributoFormatoNumeroItem>();

        public ValoreFormatoNumeroView(EntitiesListMasterDetailView master, string tag, ValoreFormatoNumero v = null) : base(v)
        {
            _master = master;
            Tag = tag;
            Items = new ObservableCollection<ValoreAttributoFormatoNumeroItem>(ValoreAttributoFormatoNumero.Items);
        }



        ValoreAttributoFormatoNumero ValoreAttributoFormatoNumero
        {
            get
            {
                if (Tag != null && Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
                    Attributo sourceAtt = entsHelper.GetSourceAttributo(att);
                    if (sourceAtt == null)
                        return null;

                    if (sourceAtt.ValoreAttributo == null)
                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), "att.ValoreAttributo == null");

                    return sourceAtt.ValoreAttributo as ValoreAttributoFormatoNumero;
                }
                return null;
            }
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                if (ValoreAttributoFormatoNumero != null)
                {
                    ValoreAttributoFormatoNumeroItem elItem = ValoreAttributoFormatoNumero.Items.FirstOrDefault(item => item.Id == This.ValoreAttributoFormatoNumeroId);
                    if (elItem != null)
                        return elItem.Format;
                }
                return null;
            }
            set
            {
                if (IsReadOnly)
                    return;

                ValoreAttributoFormatoNumeroItem elItem = ValoreAttributoFormatoNumero.Items.FirstOrDefault(item => item.Format == value);
                if (elItem == null)
                    return;

                if (SetProperty(This.V, value, () => This.V = value))
                {
                    This.ValoreAttributoFormatoNumeroId = elItem.Id;

                    if (!This.IsMultiValore())
                    {
                        Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                        Master.UpdateCache(false);
                    }
                }

            }
        }


        public bool IsReadOnly
        {
            get
            {
                bool ret = false;
                string attCode = Tag as string;

                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    if (att.IsValoreReadOnly ||
                        att.IsValoreLockedByDefault ||
                        att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                        att.IsPreviewMode)
                        return true;

                }

                if (Master.SelectedEntityView != null && Master.SelectedEntityView.DetailAttributiView.ContainsKey(attCode))
                {
                    if (Master.SelectedEntityView.IsValoreAttributoReadOnly(attCode))
                        return true;
                }


                return ret;
            }
        }


        public string Background
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                }

                return Colors.White.ToString();// "White";
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
        }


        public ICommand AddFormatoNumeroCommand { get { return new CommandHandler(() => this.AddFormatoNumero()); } }
        void AddFormatoNumero()
        {
            List<string> formats = new List<string>();
            if (Master.WindowService.SelectNumberFormatsWnd(ref formats, true) == true)
            {
                string format = formats.FirstOrDefault();
                if (format != null && format.Any())
                {
                    if (ValoreAttributoFormatoNumero.Items.FirstOrDefault(item => item.Format == format) == null)
                    {
                        //NumericFormatHelper.UpdateCulture(false);
                        //NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);

                        ValoreAttributoFormatoNumeroItem newItem = new ValoreAttributoFormatoNumeroItem()
                        {
                            Id = Guid.NewGuid(),
                            Format = format,
                            //Text = nf.SymbolText,
                        };
                        ValoreAttributoFormatoNumero.Items.Add(newItem);

                        Master.DataService.SetEntityType(Master.EntityType, false);

                        Testo = newItem.Format;
                        Master.UpdateCache(true);
                    }
                }
            }
        }

        public ICommand MouseDoubleClickCommand { get => new CommandHandler(() => this.MouseDoubleClick()); }
        void MouseDoubleClick()
        {
            Master.ReplaceValore(this);
        }

        public void UpdateValore(Valore val)
        {
        }
    }

    public class ValoreLinkView : NotificationBase<ValoreTesto>, ValoreView
    {
        EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public ValoreLinkView(EntitiesListMasterDetailView master, ValoreTesto v = null) : base(v)
        {
            _master = master;
        }

        public object Tag { get; set; }

        public string Testo
        {
            get
            {
                return This.V;
            }
            set
            {
                if (SetProperty(This.V, value, () => This.V = value))
                {
                    if (!This.IsMultiValore())
                    {

                        if (Master.IsMultipleModify)
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            Master.UpdateCache(true);
                        }
                        else
                        {
                            Master.AttributiEntities.SetValoreAttributo(Tag as string, this.This);
                            //Master.UpdateCache(false);
                            Master.UpdateCache(true);
                        }

                    }
                }

            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (Master.DataService == null || Master.DataService.IsReadOnly)
                    return true;

                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    Attributo att = Master.EntityType.Attributi[Tag as string];
                    return (att.IsValoreReadOnly ||
                            att.IsValoreLockedByDefault ||
                            att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento ||
                            att.IsPreviewMode);
                }
                else
                    return true;
            }
        }

        public ICommand EditLinkCommand  { get => new CommandHandler(() => this.EditLink()); }

        private void EditLink()
        {
            string localLink = string.Empty;

            var openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (File.Exists(Testo))
                openFileDialog.InitialDirectory = Path.GetDirectoryName(Testo);

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                localLink = openFileDialog.FileName;
                Testo = localLink;
                UpdateUI();
            }
        }

        public bool IsPreviewable
        {
            get => false;
        }

        public string Background
        {
            get
            {
                if (Master.EntityType.Attributi.ContainsKey(Tag as string))
                {
                    bool isAttRif = Master.EntityType.Attributi[Tag as string] is AttributoRiferimento;
                    if (IsReadOnly || isAttRif)
                        return Colors.WhiteSmoke.ToString();// "WhiteSmoke";
                }

                return Colors.White.ToString();// "White";
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Testo));
        }

        public void UpdateValore(Valore val)
        {

        }
    }

    public class ValoreLinkCollectionView : ValoreGuidCollectionView
    {

        public ValoreLinkCollectionView(EntitiesListMasterDetailView master, ValoreGuidCollection v = null) : base(master, v)
        {

        }

        protected override ValoreGuidCollectionItemView NewItem(EntitiesListMasterDetailView master, ValoreCollectionView collectionView, ValoreGuidCollectionItem item)
        {
            return new ValoreLinkCollectionItemView(master, collectionView, item);
        }
    }

    public class ValoreLinkCollectionItemView : ValoreGuidCollectionItemView
    {
        public ValoreLinkCollectionItemView(EntitiesListMasterDetailView master, ValoreCollectionView owner, ValoreGuidCollectionItem v = null) : base(master, owner, v)
        {
        }
        new ValoreGuidCollectionItem This { get => base.This as ValoreGuidCollectionItem; }

        public bool IsLink
        {
            get
            {
                string codiceAttributo = Owner.Tag.ToString();
                Attributo att = Master.EntityType.Attributi[codiceAttributo];
                if (att == null)
                    return false;

                Attributo attSource = Master.GetSourceAttributoOf(att);

                if (attSource.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Allegati)
                    return true;

                return false;
            }
        }

        public string Link
        {
            get
            {
                if (IsLink)
                {
                    Entity refEnt = Owner.GetEntityById(This.EntityId);

                    if (refEnt == null)
                        return string.Empty;

                    Valore val = Master.EntitiesHelper.GetValoreAttributo(refEnt, BuiltInCodes.Attributo.Link, false, true);
                    if (val == null)
                        return string.Empty;

                    return val.PlainText;
                }
                return String.Empty;
            }
        }

        //public string FileId
        //{
        //    get
        //    {
        //        if (IsLink)
        //        {
        //            Entity refEnt = Owner.GetEntityById(This.EntityId);

        //            if (refEnt == null)
        //                return string.Empty;

        //            Valore val = Master.EntitiesHelper.GetValoreAttributo(refEnt, BuiltInCodes.Attributo.FileUploadId, false, true);
        //            if (val == null)
        //                return string.Empty;

        //            return val.PlainText;
        //        }
        //        return string.Empty;
        //    }
        //}

        public override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => IsLink));
        }

    }

    //public class ValoreVariabileView : NotificationBase<ValoreVariabile>, ValoreView
    //{
    //    EntitiesListMasterDetailView _master = null;
    //    EntitiesListMasterDetailView Master { get => _master; }

    //    public ValoreVariabileView(EntitiesListMasterDetailView master, string tag, ValoreVariabile v = null) : base(v)
    //    {
    //        _master = master;
    //        Tag = tag;
    //    }

    //    public object Tag { get; set; }

    //    public bool IsPreviewable { get => false; }

    //    public string Testo
    //    {
    //        get
    //        {
    //            string codiceAtt = Tag as string;

    //            string res = string.Empty;
    //            if (Master.EntityType.Attributi.ContainsKey(codiceAtt))
    //            {
    //                Attributo att = Master.EntityType.Attributi[codiceAtt];
    //                ValoreAttributoVariabili valAtt = att.ValoreAttributo as ValoreAttributoVariabili;
    //                if (valAtt != null)
    //                {
    //                    string codiceAttVar = valAtt.CodiceAttributo;

    //                    EntityType varType = Master.DataService.GetEntityType(BuiltInCodes.EntityType.Variabili);
    //                    Attributo attVar = null;
    //                    if (varType != null)
    //                    {
    //                        attVar = varType.Attributi[codiceAttVar];

    //                        EntitiesHelper entsHelper = new EntitiesHelper(Master.DataService);
    //                        Valore val = entsHelper.GetValoreAttributo(BuiltInCodes.EntityType.Variabili, codiceAttVar, false, true);
    //                        if (val is ValoreReale)
    //                        {
    //                            AttributoFormatHelper formatHelper = new AttributoFormatHelper(Master.DataService);
    //                            string format = formatHelper.GetValorePaddedFormat(attVar);
    //                            res = (val as ValoreReale).FormatRealResult(format);
    //                        }
    //                        else if (val is ValoreContabilita)
    //                        {
    //                            AttributoFormatHelper formatHelper = new AttributoFormatHelper(Master.DataService);
    //                            string format = formatHelper.GetValorePaddedFormat(attVar);
    //                            res = (val as ValoreContabilita).FormatRealResult(format);
    //                        }
    //                        else
    //                        {
    //                            res = val.ToPlainText();
    //                        }
    //                    }
    //                }
    //            }
    //            return res;
    //        }
    //    }

    //    public bool IsReadOnly => true;

    //    public void UpdateUI()
    //    {
    //        RaisePropertyChanged(GetPropertyName(() => Testo));
    //    }

    //    public void UpdateValore(Valore val)
    //    {
    //    }
    //}



}