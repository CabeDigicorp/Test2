using CommonResources;
using CommonResources.Controls;
using Commons;
using Commons.View;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StampeWpf.Wizard
{
    public class RaggruppamentiItemsView : NotificationBase
    {
        public ReportSettingDataViewHelper ReportSettingDataViewHelper { get; set; }
        public event EventHandler ForceCloseOfPopUps;

        private GroupSettingView GroupSettingView;

        private decimal _IndentazioneUi;
        public decimal IndentazioneUi
        {
            get
            {
                return _IndentazioneUi;
            }
            set
            {
                if (SetProperty(ref _IndentazioneUi, value))
                {
                    _IndentazioneUi = value;
                }
            }
        }
        public OpzioniDiStampa OpzioniDiStampa { get; set; }

        private decimal _Indent;
        public decimal Indent
        {
            get
            {
                return _Indent;
            }
            set
            {
                if (SetProperty(ref _Indent, value))
                {
                    _Indent = value;
                }
            }
        }


        private bool _IsCheckedRiepilogo;
        public bool IsCheckedRiepilogo
        {
            get
            {
                return _IsCheckedRiepilogo;
            }
            set
            {
                if (SetProperty(ref _IsCheckedRiepilogo, value))
                {
                    _IsCheckedRiepilogo = value;
                }
            }
        }

        private bool _IsCheckedTotale;
        public bool IsCheckedTotale
        {
            get
            {
                return _IsCheckedTotale;
            }
            set
            {
                if (SetProperty(ref _IsCheckedTotale, value))
                {
                    _IsCheckedTotale = value;
                }
            }
        }

        private bool _IsCheckedNuovapagina;
        public bool IsCheckedNuovapagina
        {
            get
            {
                return _IsCheckedNuovapagina;
            }
            set
            {
                if (SetProperty(ref _IsCheckedNuovapagina, value))
                {
                    _IsCheckedNuovapagina = value;
                }
            }
        }

        private bool _IsCheckedDescrBreve;
        public bool IsCheckedDescrBreve
        {
            get
            {
                return _IsCheckedDescrBreve;
            }
            set
            {
                if (SetProperty(ref _IsCheckedDescrBreve, value))
                {
                    _IsCheckedDescrBreve = value;
                }
            }
        }

        private TextBoxItemView _TextBoxItemView;
        public TextBoxItemView TextBoxItemView
        {
            get
            {
                return _TextBoxItemView;
            }
            set
            {
                if (SetProperty(ref _TextBoxItemView, value))
                {
                    _TextBoxItemView = value;
                }
            }
        }

        private TextBoxItemView _TextBoxItemViewOrdinament;
        public TextBoxItemView TextBoxItemViewOrdinamento
        {
            get
            {
                return _TextBoxItemViewOrdinament;
            }
            set
            {
                if (SetProperty(ref _TextBoxItemViewOrdinament, value))
                {
                    _TextBoxItemViewOrdinament = value;
                }
            }
        }

        private ObservableCollection<TreeviewItem> _ListAttributes;
        public ObservableCollection<TreeviewItem> ListAttributes
        {
            get
            {
                return _ListAttributes;
            }
            set
            {
                if (SetProperty(ref _ListAttributes, value))
                {
                    _ListAttributes = value;
                }
            }
        }

        private bool _IsOrdineCrescente;
        public bool IsOrdineCrescente
        {
            get
            {
                return _IsOrdineCrescente;
            }
            set
            {
                if (SetProperty(ref _IsOrdineCrescente, value))
                {
                    if (value)
                        IsOrdineDecrescente = !value;

                    ManageAZZAORder();

                    //_IsOrdineCrescente = value;
                    //if (value)
                    //{
                    //    IsOrdineDecrescente = false;
                    //    TextBoxItemViewOrdinamento.SelectedTreeViewItem = TextBoxItemViewOrdinamento.ListaComboBox.FirstOrDefault();
                    //    TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = true;
                    //}
                    //else
                    //{
                    //    TextBoxItemViewOrdinamento.AttributeSelected = null;
                    //    TextBoxItemViewOrdinamento.SelectedTreeViewItem = null;
                    //    if (!IsOrdineDecrescente)
                    //    {
                    //        TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = false;
                    //    }
                    //}
                }
                ForceCloseOfPopUps?.Invoke(this, new EventArgs());
            }
        }
        

        private bool _IsOrdineDecrescente;
        public bool IsOrdineDecrescente
        {
            get
            {
                return _IsOrdineDecrescente;
            }
            set
            {
                if (SetProperty(ref _IsOrdineDecrescente, value))
                {
                    if (value)
                        IsOrdineCrescente = !value;
                    ManageAZZAORder();

                    //_IsOrdineDecrescente = value;
                    //if (value)
                    //{
                    //    IsOrdineCrescente = false;
                    //    TextBoxItemViewOrdinamento.SelectedTreeViewItem = TextBoxItemViewOrdinamento.ListaComboBox.FirstOrDefault();
                    //    TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = true;
                    //}
                    //else
                    //{
                    //    TextBoxItemViewOrdinamento.AttributeSelected = null;
                    //    TextBoxItemViewOrdinamento.SelectedTreeViewItem = null;
                    //    if (!IsOrdineCrescente)
                    //    {
                    //        TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = false;
                    //    }
                    //}
                }
                ForceCloseOfPopUps?.Invoke(this, new EventArgs());
            }
        }

        private void ManageAZZAORder()
        {
            if (IsOrdineCrescente)
            {
                if (TextBoxItemViewOrdinamento.SelectedTreeViewItem == null)
                    TextBoxItemViewOrdinamento.SelectedTreeViewItem = TextBoxItemViewOrdinamento.ListaComboBox.FirstOrDefault();
                IsOrdineDecrescente = false;
                TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = true;
            }
            if (IsOrdineDecrescente)
            {
                if (TextBoxItemViewOrdinamento.SelectedTreeViewItem == null)
                    TextBoxItemViewOrdinamento.SelectedTreeViewItem = TextBoxItemViewOrdinamento.ListaComboBox.FirstOrDefault();
                IsOrdineCrescente = false;
                TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = true;
            }
            if (!IsOrdineCrescente && !IsOrdineDecrescente)
            {
                TextBoxItemViewOrdinamento.AttributeSelected = null;
                TextBoxItemViewOrdinamento.SelectedTreeViewItem = null;
                TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = false;
                TextBoxItemViewOrdinamento.DeselectAll();
            }

        }

        public RaggruppamentiItemsView()
        {
            TextBoxItemView = new TextBoxItemView();
            TextBoxItemView.IsEtichettaVisible = System.Windows.Visibility.Collapsed;
            TextBoxItemView.IsStyleCommandVisible = System.Windows.Visibility.Collapsed;
            TextBoxItemViewOrdinamento = new TextBoxItemView();
            TextBoxItemViewOrdinamento.IsControlForOrderingData = true;
            TextBoxItemViewOrdinamento.IsEtichettaVisible = System.Windows.Visibility.Collapsed;
            TextBoxItemViewOrdinamento.IsStyleCommandVisible = System.Windows.Visibility.Collapsed;
            OpzioniDiStampa = new OpzioniDiStampa();
        }
        public void AssignDatasource()
        {
            TextBoxItemView.ItemsRaggruppamenti = ReportSettingDataViewHelper.ItemsRaggruppamenti;
            TextBoxItemView.ListaComboBox = ListAttributes;
            TextBoxItemView.DataService = ReportSettingDataViewHelper.DataService;

            TextBoxItemViewOrdinamento.ItemsRaggruppamenti = ReportSettingDataViewHelper.ItemsRaggruppamenti;
            AssegnaListaAttributiOrdinamento();
            TextBoxItemViewOrdinamento.DataService = ReportSettingDataViewHelper.DataService;
        }

        public void AssegnaListaAttributiOrdinamento()
        {
            TextBoxItemViewOrdinamento.ListaComboBox = new ObservableCollection<TreeviewItem>(GenerateListOfAttributeForOrder());
        }
        public ObservableCollection<TreeviewItem> GenerateListOfAttributeForOrder()
        {
            ObservableCollection<TreeviewItem> ListAttributiOrdinamento = new ObservableCollection<TreeviewItem>();

            if (TextBoxItemView.SelectedTreeViewItem != null)
            {
                foreach (var PrimoLivello in ListAttributes)
                {
                    if (PrimoLivello.Attrbuto == TextBoxItemView.SelectedTreeViewItem.Attrbuto && PrimoLivello.EntityType == TextBoxItemView.SelectedTreeViewItem.EntityType && PrimoLivello.AttrbutoCodice == TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice)
                    {
                        AggigungiAListaAttributiOrdinamento(PrimoLivello, ListAttributiOrdinamento);
                        break;
                    }
                    foreach (var SecondoLivello in PrimoLivello.Items)
                    {
                        if (SecondoLivello.Attrbuto == TextBoxItemView.SelectedTreeViewItem.Attrbuto && SecondoLivello.EntityType == TextBoxItemView.SelectedTreeViewItem.EntityType && SecondoLivello.AttrbutoCodice == TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice)
                        {
                            AggigungiAListaAttributiOrdinamento(SecondoLivello, ListAttributiOrdinamento);
                            break;
                        }
                        foreach (var TerzoLivello in SecondoLivello.Items)
                        {
                            if (TerzoLivello.Attrbuto == TextBoxItemView.SelectedTreeViewItem.Attrbuto && TerzoLivello.EntityType == TextBoxItemView.SelectedTreeViewItem.EntityType && TerzoLivello.AttrbutoCodice == TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice)
                            {
                                AggigungiAListaAttributiOrdinamento(TerzoLivello, ListAttributiOrdinamento);
                                break;
                            }
                        }
                    }
                }
            }
            return ListAttributiOrdinamento;
        }

        private void AggigungiAListaAttributiOrdinamento(TreeviewItem Attributo, ObservableCollection<TreeviewItem> List)
        {
            if (Attributo.Items.Count() == 0)
            {
                if (Attributo.PropertyType == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                {
                    return;
                }
                TreeviewItem LivelloDaAggungere = new TreeviewItem();
                LivelloDaAggungere.Attrbuto = Attributo.Attrbuto;
                LivelloDaAggungere.AttrbutoCodice = Attributo.AttrbutoCodice;
                LivelloDaAggungere.AttrbutoDestinazione = Attributo.AttrbutoDestinazione;
                LivelloDaAggungere.AttrbutoOrigine = Attributo.AttrbutoOrigine;
                LivelloDaAggungere.CodiceDigicorp = Attributo.CodiceDigicorp;
                LivelloDaAggungere.DivisioneCodice = Attributo.DivisioneCodice;
                LivelloDaAggungere.EntityType = Attributo.EntityType;
                List.Add(LivelloDaAggungere);
            }
            else
            {
                foreach (var AttributoItem in Attributo.Items)
                {
                    if (AttributoItem.Items.Count() > 0 || AttributoItem.PropertyType == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                    {
                        continue;
                    }
                    TreeviewItem LivelloDaAggungere = new TreeviewItem();
                    LivelloDaAggungere.Attrbuto = AttributoItem.Attrbuto;
                    LivelloDaAggungere.AttrbutoCodice = AttributoItem.AttrbutoCodice;
                    LivelloDaAggungere.AttrbutoDestinazione = AttributoItem.AttrbutoDestinazione;
                    LivelloDaAggungere.AttrbutoOrigine = AttributoItem.AttrbutoOrigine;
                    LivelloDaAggungere.CodiceDigicorp = AttributoItem.CodiceDigicorp;
                    LivelloDaAggungere.DivisioneCodice = AttributoItem.DivisioneCodice;
                    LivelloDaAggungere.EntityType = AttributoItem.EntityType;
                    LivelloDaAggungere.AttributoCodicePath = string.Format("{0}{1}{2}", Attributo.AttributoCodicePath, ReportSettingViewHelper.AttributoCodicePathSeparator, AttributoItem.AttrbutoCodice);
                    List.Add(LivelloDaAggungere);
                }
            }
        }

        public TreeviewItem RicercaAttributoInAlberoOrdinamento(string attributoOrdinamento)
        {
            foreach (var PrimoLivello in TextBoxItemViewOrdinamento.ListaComboBox)
            {
                if (PrimoLivello.Attrbuto == attributoOrdinamento) { return PrimoLivello; }
                foreach (var SecondoLivello in PrimoLivello.Items)
                {
                    if (SecondoLivello.Attrbuto == attributoOrdinamento) { return SecondoLivello; }
                    foreach (var TerzoLivello in SecondoLivello.Items)
                    {
                        if (TerzoLivello.Attrbuto == attributoOrdinamento) { return TerzoLivello; }
                    }
                }
            }
            return null;
        }

        private ICommand _GroupSettingButtonCommand;
        public ICommand GroupSettingButtonCommand
        {
            get
            {
                return _GroupSettingButtonCommand ?? (_GroupSettingButtonCommand = new CommandHandler(param => ExecuteGroupSettingButtonCommand(param), CanExecuteGroupSettingButtonCommand()));
            }
        }

        /// <summary>
        /// Indica se il raggruppamento viene riferito da altri raggruppamenti inferiori
        /// </summary>
        public bool IsGroupReferencedDown { get; internal set; }

        private void ExecuteGroupSettingButtonCommand(object param)
        {


//            GroupSettingView = new GroupSettingView();
//            GroupSettingView.Init(OpzioniDiStampa);

//            GroupSettingWnd GroupSettingWnd = new GroupSettingWnd();
//            GroupSettingWnd.SourceInitialized += (x, y) => GroupSettingWnd.HideMinimizeAndMaximizeButtons();
//            GroupSettingWnd.Owner = System.Windows.Application.Current.MainWindow;
//            GroupSettingWnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
//            GroupSettingWnd.DataContext = GroupSettingView;
//            GroupSettingWnd.ShowDialog();

//            OpzioniDiStampa.IsCheckedDescrizioneBreve = GroupSettingView.IsCheckedDescrizioneBreve;
//            OpzioniDiStampa.IsCheckedNuovaPagina = GroupSettingView.IsCheckedNuovaPagina;
//            OpzioniDiStampa.IsCheckedRiepilogo = GroupSettingView.IsCheckedRiepilogo;


        }
        private bool CanExecuteGroupSettingButtonCommand()
        {
            return true;
        }
    }
}
