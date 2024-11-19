using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampeWpf.Wizard
{
    public class OrdinamentoView : NotificationBase
    {
        public ReportWizardSettingDataView ReportWizardSettingDataView { get; set; }

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

        private bool _IsOrdineCrescente;
        public bool IsOrdineCrescente
        {
            get
            {
                return _IsOrdineCrescente;
            }
            set
            {
                GeneraListaAttributiDocumento();

                if (SetProperty(ref _IsOrdineCrescente, value))
                {
                    if (value)
                        IsOrdineDecrescente = !value;
                    ManageAZZAORder();
                    //    _IsOrdineCrescente = value;
                    //    if (value)
                    //    {
                    //        IsOrdineDecrescente = false;
                    //        TextBoxItemViewOrdinamento.SelectedTreeViewItem = TextBoxItemViewOrdinamento.ListaComboBox.FirstOrDefault();
                    //        TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = true;
                    //    }
                    //    else
                    //    {
                    //        TextBoxItemViewOrdinamento.AttributeSelected = null;
                    //        TextBoxItemViewOrdinamento.SelectedTreeViewItem = null;
                    //        if (!IsOrdineDecrescente)
                    //        {
                    //            TextBoxItemViewOrdinamento.IsTextBoxItemViewOrdinamentoEnable = false;
                    //        }
                    //    }
                }
                if (ReportWizardSettingDataView != null)
                {
                    ReportWizardSettingDataView.ForceCloseOfPopUps();
                }
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

                GeneraListaAttributiDocumento();

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
                if (ReportWizardSettingDataView != null)
                {
                    ReportWizardSettingDataView.ForceCloseOfPopUps();
                }
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

        public OrdinamentoView()
        {
            TextBoxItemViewOrdinamento = new TextBoxItemView();
            TextBoxItemViewOrdinamento.IsControlForOrderingData = true;
            TextBoxItemViewOrdinamento.IsEtichettaEnable = false;
            TextBoxItemViewOrdinamento.IsEtichettaVisible = System.Windows.Visibility.Collapsed;
            TextBoxItemViewOrdinamento.IsStyleCommandVisible = System.Windows.Visibility.Collapsed;
        }

        public void GeneraListaAttributiDocumento()
        {
            TextBoxItemViewOrdinamento.ListaComboBox.Clear();
            List<TreeviewItem> ListaParallela = new List<TreeviewItem>();

            foreach (var Attribute in ReportWizardSettingDataView.DocumentoCorpoView.DocumentoCorpi)
            {
                foreach (var Attributo in Attribute.ListaDettaglio)
                {
                    if (Attributo.SelectedTreeViewItem != null)
                    {
                        if (Attributo.SelectedTreeViewItem.PropertyType != MasterDetailModel.BuiltInCodes.DefinizioneAttributo.TestoRTF && Attributo.SelectedTreeViewItem.PropertyType != MasterDetailModel.BuiltInCodes.DefinizioneAttributo.Guid)
                        {
                            ListaParallela.Add(Attributo.SelectedTreeViewItem);
                        }
                    }
                }
            }

            ListaParallela = ListaParallela.Distinct().ToList();

            TextBoxItemViewOrdinamento.ListaComboBox = new ObservableCollection<TreeviewItem>(ListaParallela);
        }
    }
}
