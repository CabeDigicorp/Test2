using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using StampeWpf.Wizard;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StampeWpf.Wizard
{
    public class FormatCharacterView : FormatCharacter
    {
        private StileConProprieta _StileConPropieta;
        public StileConProprieta StileConPropieta
        {
            get
            {
                return _StileConPropieta;
            }
            set
            {
                if (SetProperty(ref _StileConPropieta, value))
                {
                    _StileConPropieta = value;
                    if (IsViewBlocked == false)
                    {
                        IsModificatoVisible = Visibility.Collapsed;
                        if (value != null)
                        {
                            SetNewSelectionStiliChanged(_StileConPropieta.NomeECodice);
                        }
                    }
                }
            }
        }

        public TextBoxItemView TextBoxItemView { get; set; }
        public IntestazioneColonnaEntity IntestazioneColonnaEntity { get; set; }

        public FormatCharacterView()
        {
            Colors = new ObservableCollection<ColorInfo>();

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);
            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    Colors.Add(colInfo);
                }
            }

            ListSize = new List<string>() { "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };
            ListTextHorizontalAlignement = new List<string>() { LocalizationProvider.GetString(TextAlignementTpe.Sinistra.ToString()), LocalizationProvider.GetString(TextAlignementTpe.Centro.ToString()), LocalizationProvider.GetString(TextAlignementTpe.Destra.ToString()), LocalizationProvider.GetString(TextAlignementTpe.Giustifica.ToString()) };
            ListTextVerticalAlignement = new List<string>() { LocalizationProvider.GetString(TextAlignementTpe.InAlto.ToString()), LocalizationProvider.GetString(TextAlignementTpe.InBasso.ToString()) };
            ListFontFamily = new List<string>(Fonts.SystemFontFamilies.Select(X => X.Source).ToList().OrderBy(t => t));
            IsViewBlocked = true;
            IsModificatoVisible = Visibility.Collapsed;
            IsNascondiVisible = Visibility.Collapsed;
            IsRTFVisible = Visibility.Collapsed;
            IsStampaFormulaVisible = Visibility.Collapsed;
            IsConcatenaEtichettaEValoreEnable = true;

            //DEFAULT ALLINEAMENTO VERTICALE
            TextVerticalAlignement = ListTextVerticalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.InAlto.ToString())).FirstOrDefault();
            TextVerticalAlignementCode = 1;
        }

        public void UpdateUi()
        {
            RaisePropertyChanged("FontFamily");
            RaisePropertyChanged("Size");
            RaisePropertyChanged("IsGrassetto");
            RaisePropertyChanged("IsCorsivo");
            RaisePropertyChanged("IsSottolineato");
            RaisePropertyChanged("ColorCharacther");
            RaisePropertyChanged("ColorBackground");
        }

        public void SettaStiliProgetto()
        {
            if (ListStiliConPropieta == null) { ListStiliConPropieta = new ObservableCollection<StileConProprieta>(); }

            List<Guid> entitiesFound = null;
            List<MasterDetailModel.EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            StileConProprieta carattere = new StileConProprieta();
            carattere.NomeECodice = LocalizationProvider.GetString("NessunoStile");
            carattere.Nome = LocalizationProvider.GetString("NessunoStile");
            ListStiliConPropieta.Add(carattere);

            var converter = new System.Windows.Media.BrushConverter();
            string Hexadecimal = null;

            foreach (var Ent in Entities)
            {
                carattere = new StileConProprieta();
                MasterDetailModel.Valore val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Nome, true, true);
                carattere.Nome = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Codice, true, true);
                carattere.Codice = val.PlainText;
                carattere.NomeECodice = carattere.Nome + " (" + carattere.Codice + ")";
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Carattere, true, true);
                carattere.FontFamily = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.DimensioneCarattere, true, true);
                carattere.Size = val.PlainText;
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreCarattere].Valore).Hexadecimal;
                if (Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorCharacther = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
                }
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreSfondo].Valore).Hexadecimal;
                if (Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorBackground = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Grassetto].Valore).V.Value)
                {
                    carattere.Grassetto = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Grassetto].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Italic].Valore).V.Value)
                {
                    carattere.Corsivo = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Italic].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Sottolineato].Valore).V.Value)
                {
                    carattere.Sottolineato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Sottolineato].Valore).V.Value;
                }
                if (((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Barrato].Valore).V.Value)
                {
                    carattere.Barrato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Barrato].Valore).V.Value;
                }
                ListStiliConPropieta.Add(carattere);
            }
        }

        public void SettaStileProgetto(string stile)
        {
            List<Guid> entitiesFound = null;
            List<MasterDetailModel.EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            foreach (var Ent in Entities)
            {
                MasterDetailModel.Valore val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Codice, true, true);
                if (val is MasterDetailModel.ValoreElenco)
                {
                    MasterDetailModel.ValoreElenco valTesto = val as MasterDetailModel.ValoreElenco;
                    string str = valTesto.V;
                    if (stile != null)
                        if (stile.Contains(str))
                            StileConPropieta = ListStiliConPropieta.Where(s => s.Codice == str).FirstOrDefault();                    
                }
            }
        }

        public void SettaStileProgettoPerNome(string stile)
        {
            List<Guid> entitiesFound = null;
            List<MasterDetailModel.EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            var Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            foreach (var Ent in Entities)
            {
                MasterDetailModel.Valore val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Nome, true, true);
                if (val is MasterDetailModel.ValoreTesto)
                {
                    MasterDetailModel.ValoreTesto valTesto = val as MasterDetailModel.ValoreTesto;
                    string str = valTesto.V;
                    if (stile != null)
                        if (stile.Contains(str))
                            StileConPropieta = ListStiliConPropieta.Where(s => s.Nome == str).FirstOrDefault();
                }
            }
        }

        public void SetNewSelectionStiliChanged(string stile)
        {
            List<Guid> entitiesFound = null;
            List<MasterDetailModel.EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            if (ListStiliConPropieta.Where(s => s.NomeECodice == stile).FirstOrDefault() == null) return;

            string NomeStile = ListStiliConPropieta.Where(s => s.NomeECodice == stile).FirstOrDefault().Nome;

            foreach (var Ent in Entities)
            {
                if (Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Nome].Valore.PlainText == NomeStile)
                {
                    FontFamily = Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Carattere].Valore.PlainText;
                    string Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreCarattere].Valore).Hexadecimal;
                    ColorCharacther = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault();
                    Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreSfondo].Valore).Hexadecimal;
                    ColorBackground = Colors.Where(c => c.HexValue == Hexadecimal).FirstOrDefault();
                    Size = Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.DimensioneCarattere].Valore.PlainText;
                    IsGrassetto = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Grassetto].Valore).V.Value;
                    IsBarrato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Barrato].Valore).V.Value;
                    IsSottolineato = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Sottolineato].Valore).V.Value;
                    IsCorsivo = ((ValoreBooleano)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Italic].Valore).V.Value;

                    switch (((ValoreElenco)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.Allineamento].Valore).ValoreAttributoElencoId)
                    {
                        case 1:
                            TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Sinistra.ToString())).FirstOrDefault();
                            TextAlignementCode = 1;
                            break;
                        case 2:
                            TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Centro.ToString())).FirstOrDefault();
                            TextAlignementCode = 2;
                            break;
                        case 3:
                            TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Destra.ToString())).FirstOrDefault();
                            TextAlignementCode = 3;
                            break;
                        case 4:
                            TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Giustifica.ToString())).FirstOrDefault();
                            TextAlignementCode = 4;
                            break;
                    }
                    IsModificatoVisible = Visibility.Collapsed;
                }
            }
        }

        public void SetAlignementFormExternal(int HorizontalAlignementCode, int VerticalAlignemntCode)
        {
            if (HorizontalAlignementCode == 0)
            {
                HorizontalAlignementCode = 1;
            }

            switch (HorizontalAlignementCode)
            {
                case 1:
                    TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Sinistra.ToString())).FirstOrDefault();
                    TextAlignementCode = 1;
                    break;
                case 2:
                    TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Centro.ToString())).FirstOrDefault();
                    TextAlignementCode = 2;
                    break;
                case 3:
                    TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Destra.ToString())).FirstOrDefault();
                    TextAlignementCode = 3;
                    break;
                case 4:
                    TextAlignement = ListTextHorizontalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.Giustifica.ToString())).FirstOrDefault();
                    TextAlignementCode = 4;
                    break;
            }

            if (VerticalAlignemntCode == 0)
            {
                VerticalAlignemntCode = 1;
            }
            switch (VerticalAlignemntCode)
            {
                case 1:
                    TextVerticalAlignement = ListTextVerticalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.InAlto.ToString())).FirstOrDefault();
                    TextVerticalAlignementCode = 1;
                    break;
                case 2:
                    TextVerticalAlignement = ListTextVerticalAlignement.Where(h => h == LocalizationProvider.GetString(TextAlignementTpe.InBasso.ToString())).FirstOrDefault();
                    TextVerticalAlignementCode = 2;
                    break;
            }
        }

    }

    public class StileConProprieta : NotificationBase
    {

        private System.Windows.Media.Brush _ColorBackground;
        public System.Windows.Media.Brush ColorBackground
        {
            get
            {
                return _ColorBackground;
            }
            set
            {
                if (SetProperty(ref _ColorBackground, value))
                {
                    _ColorBackground = value;
                }
            }
        }

        private System.Windows.Media.Brush _ColorCharacther;
        public System.Windows.Media.Brush ColorCharacther
        {
            get
            {
                return _ColorCharacther;
            }
            set
            {
                if (SetProperty(ref _ColorCharacther, value))
                {
                    _ColorCharacther = value;
                }
            }
        }

        private bool _Grassetto;
        public bool Grassetto
        {
            get
            {
                return _Grassetto;
            }
            set
            {
                if (SetProperty(ref _Grassetto, value))
                {
                    _Grassetto = value;
                }
            }
        }

        private bool _Corsivo;
        public bool Corsivo
        {
            get
            {
                return _Corsivo;
            }
            set
            {
                if (SetProperty(ref _Corsivo, value))
                {
                    _Corsivo = value;
                }
            }
        }

        private bool _Sottolineato;
        public bool Sottolineato
        {
            get
            {
                return _Sottolineato;
            }
            set
            {
                if (SetProperty(ref _Sottolineato, value))
                {
                    _Sottolineato = value;
                }
            }
        }

        private bool _Barrato;
        public bool Barrato
        {
            get
            {
                return _Barrato;
            }
            set
            {
                if (SetProperty(ref _Barrato, value))
                {
                    _Barrato = value;
                }
            }
        }

        private string _FontFamily;
        public string FontFamily
        {
            get
            {
                return _FontFamily;
            }
            set
            {
                if (SetProperty(ref _FontFamily, value))
                {
                    _FontFamily = value;
                }
            }
        }

        private string _Size;
        public string Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (SetProperty(ref _Size, value))
                {
                    _Size = value;
                }
            }
        }

        private string _Nome;
        public string Nome
        {
            get
            {
                return _Nome;
            }
            set
            {
                if (SetProperty(ref _Nome, value))
                {
                    _Nome = value;
                }
            }
        }

        private string _Codice;
        public string Codice
        {
            get
            {
                return _Codice;
            }
            set
            {
                if (SetProperty(ref _Codice, value))
                {
                    _Codice = value;
                }
            }
        }

        private string _NomeECodice;
        public string NomeECodice
        {
            get
            {
                return _NomeECodice;
            }
            set
            {
                if (SetProperty(ref _NomeECodice, value))
                {
                    _NomeECodice = value;
                }
            }
        }
    }
    public enum TextAlignementTpe
    {
        Sinistra,
        Centro,
        Destra,
        Giustifica,
        InAlto,
        InBasso,
    }
}
