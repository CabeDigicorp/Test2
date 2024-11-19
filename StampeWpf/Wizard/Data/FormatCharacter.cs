using CommonResources;
using Commons;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StampeWpf.Wizard
{
    public class FormatCharacter : NotificationBase
    {
        protected List<MasterDetailModel.Entity> Entities;
        public ClientDataService DataService { get; set; } = null;
        private Visibility _IsModificatoVisible;
        public Visibility IsModificatoVisible
        {
            get
            {
                return _IsModificatoVisible;
            }
            set
            {
                if (SetProperty(ref _IsModificatoVisible, value))
                {
                    _IsModificatoVisible = value;
                }
            }
        }

        private bool _Nascondi;
        public bool Nascondi
        {
            get
            {
                return _Nascondi;
            }
            set
            {
                if (SetProperty(ref _Nascondi, value))
                {
                    _Nascondi = value;
                }
            }
        }

        private bool _Rtf;
        public bool Rtf
        {
            get
            {
                return _Rtf;
            }
            set
            {
                if (SetProperty(ref _Rtf, value))
                {
                    _Rtf = value;
                    if (value)
                    {
                        ConcatenaEtichettaEValore = false;
                        IsConcatenaEtichettaEValoreEnable = false;
                    }
                    else
                    {
                        IsConcatenaEtichettaEValoreEnable = true;
                    }
                }
            }
        }

        private bool _DescrBreve;
        public bool DescrBreve
        {
            get
            {
                return _DescrBreve;
            }
            set
            {
                SetProperty(ref _DescrBreve, value);
            }
        }

        private bool _StampaFormula;
        public bool StampaFormula
        {
            get
            {
                return _StampaFormula;
            }
            set
            {
                if (SetProperty(ref _StampaFormula, value))
                {
                    _StampaFormula = value;
                }
            }
        }

        private bool _ConcatenaEtichettaEValore;
        public bool ConcatenaEtichettaEValore
        {
            get
            {
                return _ConcatenaEtichettaEValore;
            }
            set
            {
                if (SetProperty(ref _ConcatenaEtichettaEValore, value))
                {
                    _ConcatenaEtichettaEValore = value;
                }
            }
        }

        private bool _IsConcatenaEtichettaEValoreEnable;
        public bool IsConcatenaEtichettaEValoreEnable
        {
            get
            {
                return _IsConcatenaEtichettaEValoreEnable;
            }
            set
            {
                if (SetProperty(ref _IsConcatenaEtichettaEValoreEnable, value))
                {
                    _IsConcatenaEtichettaEValoreEnable = value;
                }
            }
        }

        private bool _RiportoPagina;
        public bool RiportoPagina
        {
            get
            {
                return _RiportoPagina;
            }
            set
            {
                if (SetProperty(ref _RiportoPagina, value))
                {
                    _RiportoPagina = value;
                }
            }
        }

        private bool _RiportoRaggruppamento;
        public bool RiportoRaggruppamento
        {
            get
            {
                return _RiportoRaggruppamento;
            }
            set
            {
                if (SetProperty(ref _RiportoRaggruppamento, value))
                {
                    _RiportoRaggruppamento = value;
                }
            }
        }

        private Visibility _IsNascondiVisible;
        public Visibility IsNascondiVisible
        {
            get
            {
                return _IsNascondiVisible;
            }
            set
            {
                if (SetProperty(ref _IsNascondiVisible, value))
                {
                    _IsNascondiVisible = value;
                }
            }
        }

        private Visibility _IsRTFVisible;
        public Visibility IsRTFVisible
        {
            get
            {
                return _IsRTFVisible;
            }
            set
            {
                if (SetProperty(ref _IsRTFVisible, value))
                {
                    _IsRTFVisible = value;
                }
            }
        }
        private Visibility _IsStampaFormulaVisible;
        public Visibility IsStampaFormulaVisible
        {
            get
            {
                return _IsStampaFormulaVisible;
            }
            set
            {
                if (SetProperty(ref _IsStampaFormulaVisible, value))
                {
                    _IsStampaFormulaVisible = value;
                }
            }
        }

        private Visibility _IsOpzioniDiStampaVisible;
        public Visibility IsOpzioniDiStampaVisible
        {
            get
            {
                return _IsOpzioniDiStampaVisible;
            }
            set
            {
                if (SetProperty(ref _IsOpzioniDiStampaVisible, value))
                {
                    _IsOpzioniDiStampaVisible = value;
                }
            }
        }

        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (SetProperty(ref _Title, value))
                {
                    _Title = value;
                }
            }
        }
        private ObservableCollection<ColorInfo> _Colors;
        public ObservableCollection<ColorInfo> Colors
        {
            get
            {
                return _Colors;
            }
            set
            {
                if (SetProperty(ref _Colors, value))
                {
                    _Colors = value;
                }
            }
        }

        private ColorInfo _ColorCharacther;
        public ColorInfo ColorCharacther
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
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }

        private ColorInfo _ColorBackground;
        public ColorInfo ColorBackground
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
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault();*/ }
                }
            }
        }

        private List<string> _ListSize;
        public List<string> ListSize
        {
            get
            {
                return _ListSize;
            }
            set
            {
                if (SetProperty(ref _ListSize, value))
                {
                    _ListSize = value;
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
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }

        private List<string> _ListTextHorizontalAlignement;
        public List<string> ListTextHorizontalAlignement
        {
            get
            {
                return _ListTextHorizontalAlignement;
            }
            set
            {
                if (SetProperty(ref _ListTextHorizontalAlignement, value))
                {
                    _ListTextHorizontalAlignement = value;
                }
            }
        }

        private List<string> _ListTextVerticalAlignement;
        public List<string> ListTextVerticalAlignement
        {
            get
            {
                return _ListTextVerticalAlignement;
            }
            set
            {
                if (SetProperty(ref _ListTextVerticalAlignement, value))
                {
                    _ListTextVerticalAlignement = value;
                }
            }
        }

        private string _TextAlignement;
        public string TextAlignement
        {
            get
            {
                return _TextAlignement;
            }
            set
            {
                if (SetProperty(ref _TextAlignement, value))
                {
                    _TextAlignement = value;
                    IsModificatoVisible = Visibility.Visible;
                }
                if (IsViewBlocked == false)
                {
                    /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */
                    if (TextAlignement == LocalizationProvider.GetString(TextAlignementTpe.Sinistra.ToString())) { TextAlignementCode = 1; }
                    if (TextAlignement == LocalizationProvider.GetString(TextAlignementTpe.Centro.ToString())) { TextAlignementCode = 2; }
                    if (TextAlignement == LocalizationProvider.GetString(TextAlignementTpe.Destra.ToString())) { TextAlignementCode = 3; }
                    if (TextAlignement == LocalizationProvider.GetString(TextAlignementTpe.Giustifica.ToString())) { TextAlignementCode = 4; }
                }
            }
        }
        public int TextAlignementCode { get; set; }

        private string _TextVerticalAlignement;
        public string TextVerticalAlignement
        {
            get
            {
                return _TextVerticalAlignement;
            }
            set
            {
                if (SetProperty(ref _TextVerticalAlignement, value))
                {
                    _TextVerticalAlignement = value;
                    if (IsViewBlocked == false)
                    {
                        /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */
                        if (TextVerticalAlignement == LocalizationProvider.GetString(TextAlignementTpe.InAlto.ToString())) { TextVerticalAlignementCode = 1; }
                        if (TextVerticalAlignement == LocalizationProvider.GetString(TextAlignementTpe.InBasso.ToString())) { TextVerticalAlignementCode = 2; }
                    }
                }
            }
        }

        public int TextVerticalAlignementCode { get; set; }

        private bool _IsGrassetto;
        public bool IsGrassetto
        {
            get
            {
                return _IsGrassetto;
            }
            set
            {
                if (SetProperty(ref _IsGrassetto, value))
                {
                    _IsGrassetto = value;
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }

        private bool _IsCorsivo;
        public bool IsCorsivo
        {
            get
            {
                return _IsCorsivo;
            }
            set
            {
                if (SetProperty(ref _IsCorsivo, value))
                {
                    _IsCorsivo = value;
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }


        private bool _IsBarrato;
        public bool IsBarrato
        {
            get
            {
                return _IsBarrato;
            }
            set
            {
                if (SetProperty(ref _IsBarrato, value))
                {
                    _IsBarrato = value;
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }


        private bool _IsSottolineato;
        public bool IsSottolineato
        {
            get
            {
                return _IsSottolineato;
            }
            set
            {
                if (SetProperty(ref _IsSottolineato, value))
                {
                    _IsSottolineato = value;
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
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
                    if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Visible; /*StileConPropieta = ListStiliConPropieta.FirstOrDefault(); */}
                }
            }
        }



        private List<string> _ListFontFamily;
        public List<string> ListFontFamily
        {
            get
            {
                return _ListFontFamily;
            }
            set
            {
                if (SetProperty(ref _ListFontFamily, value))
                {
                    _ListFontFamily = value;
                }
            }
        }

        //private string _Stile;
        //public string Stile
        //{
        //    get
        //    {
        //        return _Stile;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _Stile, value))
        //        {
        //            _Stile = value;
        //            if (IsViewBlocked == false) { IsModificatoVisible = Visibility.Collapsed; }
        //            SetNewSelectionStiliChanged(_Stile);
        //        }
        //    }
        //}

        //private ObservableCollection<string> _ListStili;
        //public ObservableCollection<string> ListStili
        //{
        //    get
        //    {
        //        return _ListStili;
        //    }
        //    set
        //    {
        //        if (SetProperty(ref _ListStili, value))
        //        {
        //            _ListStili = value;
        //        }
        //    }
        //}

        private ObservableCollection<StileConProprieta> _ListStiliConPropieta;
        public ObservableCollection<StileConProprieta> ListStiliConPropieta
        {
            get
            {
                return _ListStiliConPropieta;
            }
            set
            {
                if (SetProperty(ref _ListStiliConPropieta, value))
                {
                    _ListStiliConPropieta = value;
                }
            }
        }

        public bool IsViewBlocked { get; set; }
    }
}
