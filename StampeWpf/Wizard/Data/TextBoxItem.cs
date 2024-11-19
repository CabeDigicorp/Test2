using CommonResources.Controls;
using Commons;
using DatiGeneraliWpf;
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
    public class TextBoxItem : NotificationBase
    {
        public bool IsControlForOrderingData { get; set; }
        public int Origine { get; set; }
        public Guid GuidIdentificativo { get; set; }
        public FormatCharacterView FormatCharacterView { get; set; }
        public FormatCharacterView StilePrecedente { get; set; }
        public Window FormatCharacterWnd { get; set; }
        public bool InClosing;
        public ClientDataService DataService { get; set; }

        private bool _IsEtichettaEnable;
        public bool IsEtichettaEnable
        {
            get
            {
                return _IsEtichettaEnable;
            }
            set
            {
                if (SetProperty(ref _IsEtichettaEnable, value))
                {
                    _IsEtichettaEnable = value;
                }
            }
        }

        private double _MinWidth;
        public double MinWidth
        {
            get
            {
                return _MinWidth;
            }
            set
            {
                if (SetProperty(ref _MinWidth, value))
                {
                    _MinWidth = value;
                }
            }
        }

        private System.Windows.Media.Brush _HideAttributeColor;
        public System.Windows.Media.Brush HideAttributeColor
        {
            get
            {
                return _HideAttributeColor;
            }
            set
            {
                if (SetProperty(ref _HideAttributeColor, value))
                {
                    _HideAttributeColor = value;
                }
            }
        }

        private ObservableCollection<RaggruppamentiItemsView> _ItemsRaggruppamenti;
        public ObservableCollection<RaggruppamentiItemsView> ItemsRaggruppamenti
        {
            get
            {
                return _ItemsRaggruppamenti;
            }
            set
            {
                if (SetProperty(ref _ItemsRaggruppamenti, value))
                {
                    _ItemsRaggruppamenti = value;
                }
            }
        }

        private Visibility _IsEtichettaVisible;
        public Visibility IsEtichettaVisible
        {
            get
            {
                return _IsEtichettaVisible;
            }
            set
            {
                if (SetProperty(ref _IsEtichettaVisible, value))
                {
                    _IsEtichettaVisible = value;
                }
            }
        }

        private Visibility _IsStyleCommandVisible;
        public Visibility IsStyleCommandVisible
        {
            get
            {
                return _IsStyleCommandVisible;
            }
            set
            {
                if (SetProperty(ref _IsStyleCommandVisible, value))
                {
                    _IsStyleCommandVisible = value;
                }
            }
        }

        private string _Etichetta;
        public string Etichetta
        {
            get
            {
                return _Etichetta;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = StampeKeys.LocalizeEtichettaWizard;
                }
                if (SetProperty(ref _Etichetta, value))
                {
                    _Etichetta = value;
                }
            }
        }

        private string _AttributeSelected;
        public string AttributeSelected
        {
            get
            {
                return _AttributeSelected;
            }
            set
            {
                if (SetProperty(ref _AttributeSelected, value))
                {
                    _AttributeSelected = value;
                }
            }
        }

        private string _EntitySelected;
        public string EntitySelected
        {
            get
            {
                return _EntitySelected;
            }
            set
            {
                if (SetProperty(ref _EntitySelected, value))
                {
                    _EntitySelected = value;
                }
            }
        }

        private string _PathAttributeSelected;
        public string PathAttributeSelected
        {
            get
            {
                return _PathAttributeSelected;
            }
            set
            {
                if (SetProperty(ref _PathAttributeSelected, value))
                {
                    _PathAttributeSelected = value;
                }
            }
        }

        private ObservableCollection<TreeviewItem> _ListaComboBox;
        public ObservableCollection<TreeviewItem> ListaComboBox
        {
            get
            {
                return _ListaComboBox;
            }
            set
            {
                if (SetProperty(ref _ListaComboBox, value))
                {
                    _ListaComboBox = value;
                }
            }
        }

        private TreeviewItem _SelectedTreeViewItem;
        public TreeviewItem SelectedTreeViewItem
        {
            get
            {
                return _SelectedTreeViewItem;
            }
            set
            {
                if (SetProperty(ref _SelectedTreeViewItem, value))
                {
                    _SelectedTreeViewItem = value;
                    if (_SelectedTreeViewItem != null)
                    {
                        PathAttributeSelected = _SelectedTreeViewItem.PathAttribute;
                        AttributeSelected = _SelectedTreeViewItem.Attrbuto;
                    }
                }
            }
        }

        private bool _IsTextBoxItemViewOrdinamentoEnable;
        public bool IsTextBoxItemViewOrdinamentoEnable
        {
            get
            {
                return _IsTextBoxItemViewOrdinamentoEnable;
            }
            set
            {
                if (SetProperty(ref _IsTextBoxItemViewOrdinamentoEnable, value))
                {
                    _IsTextBoxItemViewOrdinamentoEnable = value;
                }
            }
        }
    }
    public class TreeviewItem : NotificationBase
    {
        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (SetProperty(ref _IsSelected, value))
                {
                    _IsSelected = value;
                }
            }
        }
        private bool _IsExpanded;
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (SetProperty(ref _IsExpanded, value))
                {
                    _IsExpanded = value;
                }
            }
        }
        private string _Padre;
        public string Padre
        {
            get
            {
                return _Padre;
            }
            set
            {
                if (SetProperty(ref _Padre, value))
                {
                    _Padre = value;
                }
            }
        }

        public string CodiceDigicorp { get; set; }
        public string DivisioneCodice { get; set; }

        private string _AttrbutoCodice;
        public string AttrbutoCodice
        {
            get
            {
                return _AttrbutoCodice;
            }
            set
            {
                if (SetProperty(ref _AttrbutoCodice, value))
                {
                    _AttrbutoCodice = value;
                }
            }
        }

        public string AttrbutoCodiceOrigine { get; set; }

        private string _Icona;
        public string Icona
        {
            get
            {
                return _Icona;
            }
            set
            {
                if (SetProperty(ref _Icona, value))
                {
                    _Icona = value;
                }
            }
        }

        private string _AttrbutoOrigine;
        public string AttrbutoOrigine
        {
            get
            {
                return _AttrbutoOrigine;
            }
            set
            {
                if (SetProperty(ref _AttrbutoOrigine, value))
                {
                    _AttrbutoOrigine = value;
                }
            }
        }

        private string _AttrbutoDestinazione;
        public string AttrbutoDestinazione
        {
            get
            {
                return _AttrbutoDestinazione;
            }
            set
            {
                if (SetProperty(ref _AttrbutoDestinazione, value))
                {
                    _AttrbutoDestinazione = value;
                }
            }
        }

        private string _Attrbuto;
        public string Attrbuto
        {
            get
            {
                return _Attrbuto;
            }
            set
            {
                if (SetProperty(ref _Attrbuto, value))
                {
                    _Attrbuto = value;
                }
            }
        }

        private int _Livello;
        public int Livello
        {
            get
            {
                return _Livello;
            }
            set
            {
                if (SetProperty(ref _Livello, value))
                {
                    _Livello = value;
                }
            }
        }

        private string _EntityType;
        public string EntityType
        {
            get
            {
                return _EntityType;
            }
            set
            {
                if (SetProperty(ref _EntityType, value))
                {
                    _EntityType = value;
                }
            }
        }

        private string _PropertyType;
        public string PropertyType
        {
            get
            {
                return _PropertyType;
            }
            set
            {
                if (SetProperty(ref _PropertyType, value))
                {
                    _PropertyType = value;
                }
            }
        }

        private string _PathAttribute;
        public string PathAttribute
        {
            get
            {
                return _PathAttribute;
            }
            set
            {
                if (SetProperty(ref _PathAttribute, value))
                {
                    _PathAttribute = value;
                }
            }
        }

        private ObservableCollection<TreeviewItem> _Items;
        public ObservableCollection<TreeviewItem> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                if (SetProperty(ref _Items, value))
                {
                    _Items = value;
                }
            }
        }

        public string GroupKey { get { return Attrbuto + "_" + EntityType; } }
        public TreeviewItem()
        {
            Items = new ObservableCollection<TreeviewItem>();
        }

        public string AttributoCodicePath { get; set; } = string.Empty;



    }
}
