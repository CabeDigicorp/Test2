using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class GanttChartStyleSettingView : NotificationBase
    {
        public IDataService DataService { get; set; }

        private ObservableCollection<ColorInfo> _ColorsHorizontal;
        public ObservableCollection<ColorInfo> ColorsHorizontal
        {
            get
            {
                return _ColorsHorizontal;
            }
            set
            {
                if (SetProperty(ref _ColorsHorizontal, value))
                {
                    _ColorsHorizontal = value;
                }
            }
        }
        private ObservableCollection<ColorInfo> _ColorsVertical;
        public ObservableCollection<ColorInfo> ColorsVertical
        {
            get
            {
                return _ColorsVertical;
            }
            set
            {
                if (SetProperty(ref _ColorsVertical, value))
                {
                    _ColorsVertical = value;
                }
            }
        }

        private ColorInfo _ColorHorizontal;
        public ColorInfo ColorHorizontal
        {
            get
            {
                return _ColorHorizontal;
            }
            set
            {
                if (SetProperty(ref _ColorHorizontal, value))
                {
                    _ColorHorizontal = value;
                }
            }
        }

        private ColorInfo _ColorVertical;
        public ColorInfo ColorVertical
        {
            get
            {
                return _ColorVertical;
            }
            set
            {
                if (SetProperty(ref _ColorVertical, value))
                {
                    _ColorVertical = value;
                }
            }
        }

        private bool _IsCheckedHorizontal;
        public bool IsCheckedHorizontal
        {
            get
            {
                return _IsCheckedHorizontal;
            }
            set
            {
                if (SetProperty(ref _IsCheckedHorizontal, value))
                {
                    _IsCheckedHorizontal = value;
                }
            }
        }

        private bool _IsCheckedVertical;
        public bool IsCheckedVertical
        {
            get
            {
                return _IsCheckedVertical;
            }
            set
            {
                if (SetProperty(ref _IsCheckedVertical, value))
                {
                    _IsCheckedVertical = value;
                }
            }
        }

        public System.Windows.Media.Brush ColorBackground
        {
            get
            {
                return StileConPropieta?.ColorBackground;
            }
        }

        public System.Windows.Media.Brush ColorCharacther
        {
            get
            {
                return StileConPropieta?.ColorCharacther;
            }
        }

        public string FontFamily
        {
            get
            {
                return StileConPropieta?.FontFamily;
            }
        }

        public double FontSize
        {
            get
            {
                return (double)StileConPropieta?.Size;
            }
        }

        public string FontWeight
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Grassetto)
                        return "Bold";
                    return null;
                }
                return null;
            }
        }
        public string TextDecorations
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Barrato)
                        return "Strikethrough";

                    if (StileConPropieta.Sottolineato)
                        return "Underline";
                    return null;
                }
                else
                    return null;
            }
        }

        public string FontStyle
        {
            get
            {
                if (StileConPropieta != null)
                {
                    if (StileConPropieta.Corsivo)
                        return "Italic";
                    return null;
                }
                return null;
            }
        }

        private ObservableCollection<ColorInfo> _ColorsTaskNode;
        public ObservableCollection<ColorInfo> ColorsTaskNode
        {
            get
            {
                return _ColorsTaskNode;
            }
            set
            {
                if (SetProperty(ref _ColorsTaskNode, value))
                {
                    _ColorsTaskNode = value;
                }
            }
        }

        private ColorInfo _ColorTaskNode;
        public ColorInfo ColorTaskNode
        {
            get
            {
                return _ColorTaskNode;
            }
            set
            {
                if (SetProperty(ref _ColorTaskNode, value))
                {
                    _ColorTaskNode = value;
                }
            }
        }

        private ObservableCollection<ColorInfo> _ColorsHeaderTaskNode;
        public ObservableCollection<ColorInfo> ColorsHeaderTaskNode
        {
            get
            {
                return _ColorsHeaderTaskNode;
            }
            set
            {
                if (SetProperty(ref _ColorsHeaderTaskNode, value))
                {
                    _ColorsHeaderTaskNode = value;
                }
            }
        }

        private ColorInfo _ColorHeaderTaskNode;
        public ColorInfo ColorHeaderTaskNode
        {
            get
            {
                return _ColorHeaderTaskNode;
            }
            set
            {
                if (SetProperty(ref _ColorHeaderTaskNode, value))
                {
                    _ColorHeaderTaskNode = value;
                }
            }
        }

        private ObservableCollection<ColorInfo> _ColorsConnectorStroke;
        public ObservableCollection<ColorInfo> ColorsConnectorStroke
        {
            get
            {
                return _ColorsConnectorStroke;
            }
            set
            {
                if (SetProperty(ref _ColorsConnectorStroke, value))
                {
                    _ColorsConnectorStroke = value;
                }
            }
        }

        private ColorInfo _ColorConnectorStroke;
        public ColorInfo ColorConnectorStroke
        {
            get
            {
                return _ColorConnectorStroke;
            }
            set
            {
                if (SetProperty(ref _ColorConnectorStroke, value))
                {
                    _ColorConnectorStroke = value;
                }
            }
        }

        private ObservableCollection<ColorInfo> _ColorsNonWorkingHours;
        public ObservableCollection<ColorInfo> ColorsNonWorkingHours
        {
            get
            {
                return _ColorsNonWorkingHours;
            }
            set
            {
                if (SetProperty(ref _ColorsNonWorkingHours, value))
                {
                    _ColorsNonWorkingHours = value;
                }
            }
        }

        private ColorInfo _ColorNonWorkingHours;
        public ColorInfo ColorNonWorkingHours
        {
            get
            {
                return _ColorNonWorkingHours;
            }
            set
            {
                if (SetProperty(ref _ColorNonWorkingHours, value))
                {
                    _ColorNonWorkingHours = value;
                }
            }
        }

        private ObservableCollection<ColorInfo> _ColorsCriticalPath;
        public ObservableCollection<ColorInfo> ColorsCriticalPath
        {
            get
            {
                return _ColorsCriticalPath;
            }
            set
            {
                if (SetProperty(ref _ColorsCriticalPath, value))
                {
                    _ColorsCriticalPath = value;
                }
            }
        }

        private ColorInfo _ColorCriticalPath;
        public ColorInfo ColorCriticalPath
        {
            get
            {
                return _ColorCriticalPath;
            }
            set
            {
                if (SetProperty(ref _ColorCriticalPath, value))
                {
                    _ColorCriticalPath = value;
                }
            }
        }

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
                }
            }
        }
        public GanttChartStyleSettingView(IDataService dataService)
        {
            DataService = dataService;

            ColorsHorizontal = new ObservableCollection<ColorInfo>();
            ColorsVertical = new ObservableCollection<ColorInfo>();

            var coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    ColorsHorizontal.Add(colInfo);
                    ColorsVertical.Add(colInfo);
                }
            }

            IsCheckedHorizontal = false;
            IsCheckedVertical = false;
            ColorHorizontal = ColorsHorizontal.Where(d => d.HexValue == "#FFF5F5F5").FirstOrDefault();
            ColorVertical = ColorsVertical.Where(d => d.HexValue == "#FFF5F5F5").FirstOrDefault();

            ColorsTaskNode = new ObservableCollection<ColorInfo>();
            ColorsHeaderTaskNode = new ObservableCollection<ColorInfo>();
            ColorsConnectorStroke = new ObservableCollection<ColorInfo>();
            ColorsNonWorkingHours = new ObservableCollection<ColorInfo>();
            ColorsCriticalPath = new ObservableCollection<ColorInfo>();

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                ColorInfo colInfo = null;
                if (coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    ColorsTaskNode.Add(colInfo);
                    ColorsHeaderTaskNode.Add(colInfo);
                    ColorsConnectorStroke.Add(colInfo);
                    ColorsNonWorkingHours.Add(colInfo);
                    ColorsCriticalPath.Add(colInfo);
                }
            }

            ColorTaskNode = ColorsTaskNode.Where(c => c.HexValue == "#FFADD8E6").FirstOrDefault();
            ColorHeaderTaskNode = ColorsHeaderTaskNode.Where(c => c.HexValue == "#FF000000").FirstOrDefault();
            ColorConnectorStroke = ColorsConnectorStroke.Where(c => c.HexValue == "#FF000000").FirstOrDefault();
            ColorNonWorkingHours = ColorsNonWorkingHours.Where(c => c.HexValue == "#FFF0F8FF").FirstOrDefault();
            ColorCriticalPath = ColorsCriticalPath.Where(c => c.HexValue == "#FFFF0000").FirstOrDefault();

            //System.Windows.Media.Brushes.LightBlue #FFADD8E6;
            //System.Windows.Media.Brushes.Black "#FF000000";
            //System.Windows.Media.Brushes.AliceBlue "#FFF0F8FF";

            if (ListStiliConPropieta == null) { ListStiliConPropieta = new ObservableCollection<StileConProprieta>(); }

            List<Guid> entitiesFound = null;
            List<EntityMasterInfo> MasterInfo = DataService.GetFilteredEntities(MasterDetailModel.BuiltInCodes.EntityType.Stili, new MasterDetailModel.FilterData(), null, null, out entitiesFound);
            List<Entity> Entities = DataService.GetEntitiesById(MasterDetailModel.BuiltInCodes.EntityType.Stili, entitiesFound);
            Model.EntitiesHelper entsHelper = new Model.EntitiesHelper(DataService);

            var converter = new System.Windows.Media.BrushConverter();
            string Hexadecimal = null;

            foreach (var Ent in Entities)
            {
                StileConProprieta carattere = new StileConProprieta();
                MasterDetailModel.Valore val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Nome, true, true);
                carattere.Nome = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Codice, true, true);
                carattere.Codice = val.PlainText;
                carattere.NomeECodice = carattere.Nome + " (" + carattere.Codice + ")";
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.Carattere, true, true);
                carattere.FontFamily = val.PlainText;
                val = entsHelper.GetValoreAttributo(Ent, MasterDetailModel.BuiltInCodes.Attributo.DimensioneCarattere, true, true);
                carattere.Size = double.Parse(val.PlainText);
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreCarattere].Valore).Hexadecimal;
                if (ColorsTaskNode.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorCharacther = ColorsTaskNode.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
                }
                Hexadecimal = ((ValoreColore)Ent.Attributi[MasterDetailModel.BuiltInCodes.Attributo.ColoreSfondo].Valore).Hexadecimal;
                if (ColorsTaskNode.Where(c => c.HexValue == Hexadecimal).FirstOrDefault() != null)
                {
                    carattere.ColorBackground = ColorsTaskNode.Where(c => c.HexValue == Hexadecimal).FirstOrDefault().SampleBrush;
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
            StileConPropieta = ListStiliConPropieta.FirstOrDefault();
        }

        public bool Accept()
        {
            return true;
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ColorBackground));
            RaisePropertyChanged(GetPropertyName(() => ColorCharacther));
            RaisePropertyChanged(GetPropertyName(() => FontFamily));
            RaisePropertyChanged(GetPropertyName(() => FontSize));
            RaisePropertyChanged(GetPropertyName(() => FontWeight));
            RaisePropertyChanged(GetPropertyName(() => FontStyle));
            RaisePropertyChanged(GetPropertyName(() => TextDecorations));
            RaisePropertyChanged(GetPropertyName(() => ColorConnectorStroke));
            RaisePropertyChanged(GetPropertyName(() => ColorHeaderTaskNode));
            RaisePropertyChanged(GetPropertyName(() => ColorTaskNode));
            RaisePropertyChanged(GetPropertyName(() => ColorNonWorkingHours));
            RaisePropertyChanged(GetPropertyName(() => ColorCriticalPath));
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

        private double _Size;
        public double Size
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
}
