using _3DModelExchange;
using CommonResources;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;

namespace DivisioniWpf
{
    public class DivisioneAttributiSettingsView : AttributiSettingsView
    {
        bool _isLoading = false;

        public override void Load()
        {
            _isLoading = true;

            base.Load();

            List<CategoryItem> items = new List<CategoryItem>();
            //carico suddivisioni Ifc
            List<Model3dClassEnum> values = new List<Model3dClassEnum>(Enum.GetValues(typeof(Model3dClassEnum)).Cast<Model3dClassEnum>());


            List<Model3dClassEnum> valuesIfc = values.Where(item => Model3dHelper.GetModel3dType(item) == Model3dType.Ifc).ToList();
            List<Model3dClassEnum> valuesRvt = values.Where(item => Model3dHelper.GetModel3dType(item) == Model3dType.Revit).ToList();

            //Model3dClassesName = values.Select(item => item.ToString()).ToList();
            //Model3dClassesName[0] = LocalizationProvider.GetString("Nessuno");
            items.AddRange(valuesIfc.Select(item => new CategoryItem() { Name = item.ToString(), Category = Model3dType.Ifc.ToString() }).ToList());
            items.AddRange(valuesRvt.Select(item => new CategoryItem() { Name = item.ToString(), Category = Model3dType.Revit.ToString() }).ToList());

            items.Insert(0, new CategoryItem() { Name = LocalizationProvider.GetString("Nessuno"), Category = LocalizationProvider.GetString("Modello") });
            
      
            Model3dClassesName = new ListCollectionView(items);
            Model3dClassesName.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            int model3dClassIndex = GetModel3dClassIndexByEntityType();

            //SetModel3dClassIndex(model3dClassIndex);
            SelectedModel3dClassIndex = model3dClassIndex;

            RaisePropertyChanged(GetPropertyName(() => EntityTypeName));
            //RaisePropertyChanged(GetPropertyName(() => Model3dClassesName));
            //RaisePropertyChanged(GetPropertyName(() => SelectedModel3dClassIndex));
            RaisePropertyChanged(GetPropertyName(() => IsSelectedModel3dClassIndexEnabled));

            _isLoading = false;
        }


        protected override void LoadDefinizioneAttributi()
        {
            //combo definizione attributo
            DefinizioniAttributo = DataService.GetDefinizioniAttributo();
            _definizioniAttributoCodice.Clear();
            DefinizioniAttributoLoc.Clear();

            _definizioniAttributoCodice = DefinizioniAttributo.Values.Where(item => item.AllowAttributoCustom).Select(item => item.Codice).ToList();

            if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.Riferimento))
                _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.Riferimento);

            if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.Guid))
                _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.Guid);

            //if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.GuidCollection))
            //    _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.GuidCollection);

            foreach (string codice in _definizioniAttributoCodice)
            {
                string comboItem = AttributiSettingsView.GetDefinizioneAttributoLocalizedName(codice);//LocalizationProvider.GetString(codice);

                if (comboItem != null)
                    DefinizioniAttributoLoc.Add(comboItem);
            }
        }

        public string EntityTypeName
        {
            get
            {
                if (EntityType != null)
                    return EntityType.Name;
                return string.Empty;
            }
        }

        int _selectedModel3dClassIndex = 0;
        public int SelectedModel3dClassIndex
        {
            get
            {
                return _selectedModel3dClassIndex;
            }
            set
            {
                if (SetProperty(ref _selectedModel3dClassIndex, value))
                {
                    if (!_isLoading)
                        SetEntityTypeModel3dClassIndex(_selectedModel3dClassIndex);
                }

            }
        }



        int GetModel3dClassIndexByEntityType()
        {
            if (EntityType != null)
            {
                DivisioneItemType divItemType = EntityType as DivisioneItemType;
                if (divItemType != null)
                {
                    return (int)divItemType.Model3dClassName;
                }
            }
          
            
            return 0;
        }

        void SetEntityTypeModel3dClassIndex(int model3dClassIndex)
        {
            if (EntityType != null)
            {
                DivisioneItemType divItemType = EntityType as DivisioneItemType;
                if (divItemType != null)
                {
                    divItemType.Model3dClassName = (Model3dClassEnum)model3dClassIndex;
                    divItemType.Attributi.Clear();
                    divItemType.CreaAttributi(DataService.GetDefinizioniAttributo(), DataService.GetEntityTypes());
                    base.Load();
                }
            }
        }



        //public int SelectedModel3dClassIndex
        //{
        //    get
        //    {
        //        if (EntityType != null)
        //        {
        //            DivisioneItemType divItemType = EntityType as DivisioneItemType;
        //            if (divItemType != null)
        //            {
        //                return (int) divItemType.Model3dClassName;
        //            }
        //        }
        //        return 0;
        //    }
        //    set
        //    {
        //        if (EntityType != null)
        //        {
        //            DivisioneItemType divItemType = EntityType as DivisioneItemType;
        //            if (divItemType != null)
        //            {
        //                _isModel3dClassSelecting = true;
        //                divItemType.Model3dClassName = (Model3dClassEnum) value;
        //                divItemType.Attributi.Clear();
        //                divItemType.CreaAttributi(DataService.GetDefinizioniAttributo(), DataService.GetEntityTypes());
        //                base.Load();

        //                _isModel3dClassSelecting = false;
        //            }
        //        }
        //    }
        //}

        protected bool IsEntityTypeBuiltIn
        {
            get
            {
                if (EntityType != null)
                {
                    DivisioneItemType divType = EntityType as DivisioneItemType;
                    return divType.IsBuiltIn;
                }
                return true;
            }
        }

        public bool IsSelectedModel3dClassIndexEnabled { get => !IsEntityTypeBuiltIn; }

        ListCollectionView _model3dClassesName = null;
        public ListCollectionView Model3dClassesName
        {
            get => _model3dClassesName;
            set => SetProperty(ref _model3dClassesName, value);
        }



    }

    public class CategoryItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
