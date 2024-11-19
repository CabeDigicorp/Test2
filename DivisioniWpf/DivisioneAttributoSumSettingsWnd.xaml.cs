using CommonResources;
using DevExpress.Mvvm.Native;
using DevExpress.Office.Utils;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DivisioniWpf
{

    /// </summary>
    public partial class DivisioneAttributoSumSettingsWnd : Window
    {
        //In
        public IDataService DataService { get; set; }
        public AttributoRiferimento SenderAttributoRiferimento { get; set; } = null;
        public int SummarizeAttributoIndex { get; set; } = 0;

        //Out
        public string SummarizeCodiceAttributo { get => _summarizeCodiceAttributo; }
        public ValoreOperationType SummarizeOperation { get => _summarizeOperation; }

        //private
        string _senderEntityTypeKey = string.Empty;
        EntityType _senderEntityType = null;
        string _senderReferenceCodiceGuid = string.Empty;

        string _summarizeCodiceAttributo = string.Empty;
        ValoreOperationType _summarizeOperation = ValoreOperationType.Nothing;
        EntitiesHelper _entitiesHelper = null;

        List<AttributiComboItem> _comboAttributiItems = new List<AttributiComboItem>();
        List<OperationsComboItem> _comboOperations = new List<OperationsComboItem>();
        ValoreAttributoGuid _senderValoreAttributoGuid = null;


        public DivisioneAttributoSumSettingsWnd()
        {
            InitializeComponent();
        }

        public void Init()
        {
            if (SenderAttributoRiferimento == null)
                return;

            if (DataService == null)
                return;

            _entitiesHelper = new EntitiesHelper(DataService);

            _senderEntityTypeKey = SenderAttributoRiferimento.EntityTypeKey;
            _senderReferenceCodiceGuid = SenderAttributoRiferimento.ReferenceCodiceGuid;

            _senderEntityType = DataService.GetEntityType(_senderEntityTypeKey);
            if (_senderEntityType == null)
                return;

            _senderEntityType = _senderEntityType.Clone();


            if (_senderEntityType.Attributi.ContainsKey(_senderReferenceCodiceGuid))
            {
                if (_senderEntityType.Attributi[_senderReferenceCodiceGuid].ValoreAttributo == null)
                    _senderEntityType.Attributi[_senderReferenceCodiceGuid].ValoreAttributo = new ValoreAttributoGuid();

                _senderValoreAttributoGuid = _senderEntityType.Attributi[_senderReferenceCodiceGuid].ValoreAttributo as ValoreAttributoGuid;
            }

            if (_senderValoreAttributoGuid == null)
                return;

            if (SummarizeAttributoIndex == 3)
            {
                _summarizeCodiceAttributo = _senderValoreAttributoGuid.SummarizeAttributo3.CodiceAttributo;
            }
            else if (SummarizeAttributoIndex == 4)
            {
                _summarizeCodiceAttributo = _senderValoreAttributoGuid.SummarizeAttributo4.CodiceAttributo;
            }


            //Attributi
            _comboAttributiItems = _senderEntityType.Attributi.Values
                .Where(item => !item.IsInternal)
                .Where(item =>
                {
                    var sourceAtt = _entitiesHelper.GetSourceAttributo(item);
                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
                                sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
                                sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                        return true;

                    return false;
                })
                .OrderBy(item => item.Etichetta)
                .Select(item => new AttributiComboItem() { CodiceAttributo = item.Codice, Etichetta = item.Etichetta })
                .ToList();


            _comboAttributiItems.Insert(0, new AttributiComboItem() { CodiceAttributo = string.Empty, Etichetta = LocalizationProvider.GetString("_Nessuno") });

            AttributiComboBox.Items.Clear();
            foreach (var item in _comboAttributiItems)
            {
                AttributiComboBox.Items.Add(item);
                if (item.CodiceAttributo == _summarizeCodiceAttributo)
                    AttributiComboBox.SelectedItem = item;
            }
            

            //Operation
            _comboOperations.Add(new OperationsComboItem() { Operation = ValoreOperationType.Equivalent, Etichetta = LocalizationProvider.GetString("VisualizzaSeUguale") });
            _comboOperations.Add(new OperationsComboItem() { Operation = ValoreOperationType.Sum, Etichetta = LocalizationProvider.GetString("Somma") });


        }



        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            _summarizeCodiceAttributo = _comboAttributiItems[AttributiComboBox.SelectedIndex].CodiceAttributo;

            //salvataggio
            if (SummarizeAttributoIndex == 3)
            {
                _senderValoreAttributoGuid.SummarizeAttributo3.CodiceAttributo = _summarizeCodiceAttributo;
            }
            else if (SummarizeAttributoIndex == 4)
            {
                _senderValoreAttributoGuid.SummarizeAttributo4.CodiceAttributo = _summarizeCodiceAttributo;
            }

            DataService.SetEntityType(_senderEntityType, false);

            DialogResult = true;
        }
    }
    public class AttributiComboItem
    {
        public string CodiceAttributo { get; set; }
        public string Etichetta { get; set; }
    }

    public class OperationsComboItem
    {
        public ValoreOperationType Operation { get; set; }
        public string Etichetta { get; set; }
    }
}


