using Commons;
using Commons.View;
using ControlzEx.Standard;
using DatiGeneraliWpf;
using MasterDetailModel;
using MasterDetailView;
using Model;
using StampeWpf.Report;
using StampeWpf.View;
using StampeWpf.Wizard;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommonResources.Controls;
//using System.ServiceModel.Configuration;
using System.Data;
using FastReport.DataVisualization.Charting;
using System.Data.SqlClient;
using CommonResources;
using DevExpress.XtraRichEdit.Layout.Engine;

namespace StampeWpf
{
    public class ReportWizardSettingDataView : ReportWizardSettingData
    {
        public ClientDataService DataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        private RaggruppamentiItemsView _SelectedItemInGroup;
        public RaggruppamentiItemsView SelectedItemInGroup
        {
            get
            {
                return _SelectedItemInGroup;
            }
            set
            {
                if (SetProperty(ref _SelectedItemInGroup, value))
                {
                    _SelectedItemInGroup = value;
                }
                ReplaceGroupContentOnSelectetGroupAction(_SelectedItemInGroup);
                RigeneraListaAttributiPerFunzioni();
                AggiornaVisibilitaBottoni();

                if (_SelectedItemInGroup != null)
                    _SelectedItemInGroup.IsGroupReferencedDown = IsGroupReferencedDown();

                ReportSettingDataViewHelper.SelectedItemInGroup = _SelectedItemInGroup;
            }
        }
        public ReportWizardSettingDataView()
        {
            IsVisibleButtonForOperation = Visibility.Hidden;
            IsAcceptButtonVisible = Visibility.Visible;
            OrdinamentoView = new OrdinamentoView();
            OrdinamentoView.ReportWizardSettingDataView = this;
            OrdinamentoView.TextBoxItemViewOrdinamento.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;

            Sezioni = new ObservableCollection<CommonResources.Controls.ComboBoxTreeLevel>();

            ItemsRaggruppamenti = new ObservableCollection<RaggruppamentiItemsView>();
            DocumentoCorpoView = new DocumentoView();
            DocumentoCorpoView.ForceCloseOfPopUps += DocumentoCorpoView_DocumentoForceCloseOfPopUps;
            DocumentoCorpoView.IntestazioniColonne = new ObservableCollection<IntestazioneColonnaEntity>();
            DocumentoCorpoView.RaggruppamentoTeste = new ObservableCollection<Dettaglio>();
            DocumentoCorpoView.ListaComandiTesta = new ObservableCollection<ComandiPerRiga>();
            DocumentoCorpoView.DocumentoCorpi = new ObservableCollection<Dettaglio>();
            DocumentoCorpoView.DocumentoFine = new ObservableCollection<Dettaglio>();
            DocumentoCorpoView.ListaComandiCorpo = new ObservableCollection<ComandiPerRiga>();
            DocumentoCorpoView.ListaComandiFine = new ObservableCollection<ComandiPerRiga>();
            DocumentoCorpoView.RaggruppamentoCode = new ObservableCollection<Dettaglio>();
            DocumentoCorpoView.ListaComandiCoda = new ObservableCollection<ComandiPerRiga>();
            DocumentoCorpoView.LarghezzaColonne = new ObservableCollection<LarghezzaColonnaEntity>();
            DocumentoCorpoView.IsEditableDocumento = true;

            GroupSetting = new Dictionary<string, DocumentoView>();
        }
        public void Init()
        {
            ReportSettingViewHelper = new ReportSettingViewHelper(DataService, Sezione.Key);
            ReportSettingDataViewHelper = new ReportSettingDataViewHelper(DataService, Sezione.Key);
            ReportSettingDataViewHelper.ItemsRaggruppamenti = ItemsRaggruppamenti;
            ReportSettingDataViewHelper.ForceSelectionGroup += ReportSettingDataViewHelper_ForceSelectionGroup;

            ContatoreColonnaInserita = 1;

            WindowService.ShowWaitCursor(true);

            if (IsInLoadReportSaved)
            {
                CaricaLayoutReportSalvato();
            }
            else
            {
                GeneraColonne();
            }

            WindowService.ShowWaitCursor(false);

            UpdateCalcoloTotaleLarghezzaColonnaFunction();

            ContatoreColonnaInserita = 1;

            IsInLoadReportSaved = false;
        }

        private void ReportSettingDataViewHelper_ForceSelectionGroup(object sender, MyEventArgs e)
        {
            int Contatore = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                if (Raggruppamento.TextBoxItemView.SelectedTreeViewItem != null)
                {
                    if (Raggruppamento.TextBoxItemView.SelectedTreeViewItem.GroupKey == e.MyEventString)
                    {
                        break;
                    }
                }
                Contatore++;
            }
            SelectedItemInGroup = ItemsRaggruppamenti.ElementAt(Contatore);
        }

        private void CaricaLayoutReportSalvato()
        {
            ItemsRaggruppamenti.Clear();

            EliminoRaggruppamentiNonEsistenti();

            int indiceRaggruppamenti = 0;
            foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource)
            {
                var raggruppamento = new RaggruppamentiItemsView();
                raggruppamento.Indent = Raggruppamento.Indent;
                raggruppamento.IsCheckedDescrBreve = Raggruppamento.IsCheckedDescrBreve;
                raggruppamento.IsCheckedNuovapagina = Raggruppamento.IsCheckedNuovapagina;
                raggruppamento.IsCheckedRiepilogo = Raggruppamento.IsCheckedRiepilogo;
                raggruppamento.IsCheckedTotale = Raggruppamento.IsCheckedTotale;
                raggruppamento.IsOrdineCrescente = Raggruppamento.IsOrdineCrescente;
                raggruppamento.IsOrdineDecrescente = Raggruppamento.IsOrdineDecrescente;
                raggruppamento.ListAttributes = ReportSettingDataViewHelper.GetAttributeTree();
                raggruppamento.TextBoxItemView = new TextBoxItemView();
                raggruppamento.TextBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(raggruppamento.ListAttributes, Raggruppamento.Attributo, Raggruppamento.EntityType, Raggruppamento.AttributoCodice, Raggruppamento.AttributoCodiceOrigine, null);
                if (raggruppamento.TextBoxItemView.SelectedTreeViewItem != null)
                {
                    raggruppamento.TextBoxItemView.AttributeSelected = raggruppamento.TextBoxItemView.SelectedTreeViewItem.Attrbuto;
                    raggruppamento.TextBoxItemView.EntitySelected = raggruppamento.TextBoxItemView.SelectedTreeViewItem.EntityType;
                }
                raggruppamento.TextBoxItemView.FormatCharacterView = new FormatCharacterView();
                raggruppamento.TextBoxItemView.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                raggruppamento.TextBoxItemViewOrdinamento.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                raggruppamento.ForceCloseOfPopUps += Raggruppamento_ForceCloseOfPopUps;
                raggruppamento.ReportSettingDataViewHelper = ReportSettingDataViewHelper;
                raggruppamento.TextBoxItemView.IsEtichettaVisible = Visibility.Collapsed;
                raggruppamento.TextBoxItemView.IsStyleCommandVisible = Visibility.Collapsed;
                raggruppamento.OpzioniDiStampa = Raggruppamento.OpzioniDiStampa;
                raggruppamento.AssignDatasource();

                if (raggruppamento.TextBoxItemView.SelectedTreeViewItem != null)
                    raggruppamento.TextBoxItemView.ImpostaSelezioneAttributoInControllo(raggruppamento.TextBoxItemView.SelectedTreeViewItem);
                else
                    raggruppamento.TextBoxItemView.ImpostaSelezioneAttributoInControllo(raggruppamento.ListAttributes.FirstOrDefault());

                if (Raggruppamento.EntitaAttributoOrdinamento != null)
                {
                    raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem = raggruppamento.RicercaAttributoInAlberoOrdinamento(Raggruppamento.EntitaAttributoOrdinamento.Attributo);
                }
                raggruppamento.IndentazioneUi = indiceRaggruppamenti * 10;

                ItemsRaggruppamenti.Add(raggruppamento);

                indiceRaggruppamenti++;
            }

            GroupSetting = new Dictionary<string, DocumentoView>();

            foreach (var Intestazione in ReportSetting.Intestazioni)
            {
                IntestazioneColonnaEntity _Intestazione = new IntestazioneColonnaEntity(DataService);
                _Intestazione.FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                _Intestazione.Init(true, false);
                _Intestazione.IntestazioneColonna = Intestazione.Etichetta;
                _Intestazione.FormatCharacterView.StileConPropieta = null;
                _Intestazione.FormatCharacterView.DataService = DataService;
                if (Intestazione.StileCarattere.ColorBackground != null)
                {
                    _Intestazione.FormatCharacterView.ColorBackground = _Intestazione.FormatCharacterView.Colors.Where(r => r.HexValue == Intestazione.StileCarattere.ColorBackground.HexValue).FirstOrDefault();
                }
                if (Intestazione.StileCarattere.ColorCharacther != null)
                {
                    _Intestazione.FormatCharacterView.ColorCharacther = _Intestazione.FormatCharacterView.Colors.Where(r => r.HexValue == Intestazione.StileCarattere.ColorCharacther.HexValue).FirstOrDefault();
                }
                _Intestazione.FormatCharacterView.FontFamily = Intestazione.StileCarattere.FontFamily;
                _Intestazione.FormatCharacterView.IsBarrato = Intestazione.StileCarattere.IsBarrato;
                _Intestazione.FormatCharacterView.IsGrassetto = Intestazione.StileCarattere.IsGrassetto;
                _Intestazione.FormatCharacterView.IsCorsivo = Intestazione.StileCarattere.IsCorsivo;
                _Intestazione.FormatCharacterView.IsSottolineato = Intestazione.StileCarattere.IsSottolineato;
                _Intestazione.FormatCharacterView.Size = Intestazione.StileCarattere.Size;
                _Intestazione.FormatCharacterView.SettaStileProgetto(Intestazione.StileCarattere.Stile);
                _Intestazione.FormatCharacterView.SetAlignementFormExternal(Intestazione.StileCarattere.TextAlignementCode, Intestazione.StileCarattere.TextVerticalAlignementCode);
                DocumentoCorpoView.IntestazioniColonne.Add(_Intestazione);

                DocumentoCorpoView.LarghezzaColonne.Add(new LarghezzaColonnaEntity() { LarghezzaColonna = Intestazione.Size });
                DocumentoCorpoView.LarghezzaColonne.Last().UpdateCalcoloTotaleLarghezzaColonna += ReportWizardSettingDataView_UpdateCalcoloTotaleLarghezzaColonna;
            }

            foreach (var Colonna in DocumentoCorpoView.IntestazioniColonne) { Colonna.ColumnCorpoHanlder += Colonna_ColumnCorpoHanlder; }


            foreach (var CorpoDocumento in ReportSetting.CorpiDocumento)
            {
                Dettaglio Dettaglio = new Dettaglio();
                Dettaglio.ListaDettaglio = new ObservableCollection<TextBoxItemView>();

                foreach (var CorpoColonna in CorpoDocumento.CorpoColonna)
                {
                    TextBoxItemView textBoxItemView = new TextBoxItemView();
                    textBoxItemView.DataService = DataService;
                    textBoxItemView.ListaComboBox = ReportSettingDataViewHelper.GetAttributeTree();
                    if (!String.IsNullOrEmpty(CorpoColonna.Attributo))
                    {
                        textBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(textBoxItemView.ListaComboBox, CorpoColonna.Attributo, CorpoColonna.EntityType, CorpoColonna.AttributoCodice, CorpoColonna.AttributoCodiceOrigine, null);
                        if (textBoxItemView.SelectedTreeViewItem != null)
                        {
                            textBoxItemView.AttributeSelected = textBoxItemView.SelectedTreeViewItem.Attrbuto;
                            textBoxItemView.EntitySelected = textBoxItemView.SelectedTreeViewItem.EntityType;
                            textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.SelectedTreeViewItem);
                        }
                        else
                            textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.ListaComboBox.FirstOrDefault());
                    }
                    else
                        textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.ListaComboBox.FirstOrDefault());

                    textBoxItemView.Origine = (int)OriginFrom.FromCorpo;
                    textBoxItemView.FormatCharacterView = new FormatCharacterView();
                    textBoxItemView.FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    textBoxItemView.Init();
                    textBoxItemView.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    textBoxItemView.RigeneraListaAttributiPerFunzioni += ReportWizardSettingDataView_RigeneraListaAttributiPerFunzioni;
                    textBoxItemView.Etichetta = CorpoColonna.Etichetta;
                    textBoxItemView.FormatCharacterView.DataService = DataService;
                    textBoxItemView.FormatCharacterView.Nascondi = CorpoColonna.Nascondi;
                    if (textBoxItemView.FormatCharacterView.Nascondi)
                    {
                        textBoxItemView.HideAttributeColor = System.Windows.Media.Brushes.Gray;
                    }
                    else
                    {
                        textBoxItemView.HideAttributeColor = System.Windows.Media.Brushes.Black;
                    }
                    textBoxItemView.FormatCharacterView.IsNascondiVisible = Visibility.Visible;
                    textBoxItemView.FormatCharacterView.RiportoPagina = CorpoColonna.RiportoPagina;
                    textBoxItemView.FormatCharacterView.RiportoRaggruppamento = CorpoColonna.RiportoRaggruppamento;
                    textBoxItemView.FormatCharacterView.ConcatenaEtichettaEValore = CorpoColonna.ConcatenaEtichettaEValore;
                    if (textBoxItemView.SelectedTreeViewItem != null)
                    {
                        if (textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                        {
                            textBoxItemView.FormatCharacterView.IsRTFVisible = Visibility.Visible;
                            textBoxItemView.FormatCharacterView.Rtf = CorpoColonna.Rtf;
                            textBoxItemView.FormatCharacterView.DescrBreve = CorpoColonna.DescrBreve;
                        }
                        if (textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Reale || textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        {
                            textBoxItemView.FormatCharacterView.IsStampaFormulaVisible = Visibility.Visible;
                            textBoxItemView.FormatCharacterView.StampaFormula = CorpoColonna.StampaFormula;
                        }
                    }
                    AssegnazioneStileCellaTabella(textBoxItemView, CorpoColonna);
                    Dettaglio.ListaDettaglio.Add(textBoxItemView);
                }

                DocumentoCorpoView.DocumentoCorpi.Add(Dettaglio);
            }

            Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonnaInDocumento = GeneraDictionaryAttributiRealiContabilitaPerColonna();
            int Contatore = 0;

            if (ReportSetting.FineDocumento != null)
            {
                foreach (var FineDocumento in ReportSetting.FineDocumento)
                {
                    Dettaglio Dettaglio = new Dettaglio();
                    Dettaglio.ListaDettaglio = new ObservableCollection<TextBoxItemView>();

                    foreach (var FineColonna in FineDocumento.CorpoColonna)
                    {
                        TextBoxItemView textBoxItemView = new TextBoxItemView();
                        textBoxItemView.DataService = DataService;
                        textBoxItemView.ListaComboBox = GeneraListaAttributiFineDocumento();
                        AggiuntaSecondoLivelloAFunzioni(textBoxItemView.ListaComboBox, ListaAttributiPerColonnaInDocumento, Contatore);

                        if (!String.IsNullOrEmpty(FineColonna.Attributo))
                        {
                            if (string.IsNullOrEmpty(FineColonna.CodiceDigicorp))
                            {
                                textBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(textBoxItemView.ListaComboBox, FineColonna.Attributo, FineColonna.EntityType, FineColonna.AttributoCodice, FineColonna.AttributoCodiceOrigine, null);
                            }
                            else
                            {
                                textBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(textBoxItemView.ListaComboBox, FineColonna.Attributo, FineColonna.EntityType, null, null, FineColonna.CodiceDigicorp);
                            }
                            if (textBoxItemView.SelectedTreeViewItem != null)
                            {
                                textBoxItemView.AttributeSelected = textBoxItemView.SelectedTreeViewItem.Attrbuto;
                                textBoxItemView.EntitySelected = textBoxItemView.SelectedTreeViewItem.EntityType;
                                textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.SelectedTreeViewItem);
                            }
                            else
                                textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.ListaComboBox.FirstOrDefault());
                        }
                        else
                            textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.ListaComboBox.FirstOrDefault());

                        textBoxItemView.Origine = (int)OriginFrom.FromCorpo;
                        textBoxItemView.FormatCharacterView = new FormatCharacterView();
                        textBoxItemView.FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                        textBoxItemView.Init();
                        textBoxItemView.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                        textBoxItemView.Etichetta = FineColonna.Etichetta;
                        textBoxItemView.FormatCharacterView.DataService = DataService;
                        AssegnazioneStileCellaTabella(textBoxItemView, FineColonna);
                        textBoxItemView.FormatCharacterView.ConcatenaEtichettaEValore = FineColonna.ConcatenaEtichettaEValore;
                        Dettaglio.ListaDettaglio.Add(textBoxItemView);
                    }

                    DocumentoCorpoView.DocumentoFine.Add(Dettaglio);
                    Contatore++;
                }
            }

            List<List<Dettaglio>> Dettaglitesta = new List<List<Dettaglio>>();

            TreeviewItem Attributo = null;
            ObservableCollection<TreeviewItem> ListaComune = null;

            Contatore = 0;
            foreach (var CorpoDocumento in ReportSetting.Teste)
            {
                Attributo = ReportSettingDataViewHelper.RicercaAttributoInAlbero(ReportSettingDataViewHelper.GetAttributeTree(), CorpoDocumento.Attributo, CorpoDocumento.EntityType, null, null, null);
                ListaComune = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(Attributo);

                List<Dettaglio> Interna = new List<Dettaglio>();
                foreach (var CorpoTesta in CorpoDocumento.RaggruppamentiDocumento)
                {
                    CaricaDettagliTestaCoda(CorpoTesta, CorpoDocumento, Interna, ListaComune, Attributo, ListaAttributiPerColonnaInDocumento, Contatore);
                    Contatore++;
                }
                Contatore = 0;
                if (Attributo != null)
                    Dettaglitesta.Add(Interna);
            }

            List<List<Dettaglio>> Dettaglicoda = new List<List<Dettaglio>>();

            Contatore = 0;
            foreach (var CorpoDocumento in ReportSetting.Code)
            {
                Attributo = ReportSettingDataViewHelper.RicercaAttributoInAlbero(ReportSettingDataViewHelper.GetAttributeTree(), CorpoDocumento.Attributo, CorpoDocumento.EntityType, null, null, null);
                ListaComune = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(Attributo);

                List<Dettaglio> Interna = new List<Dettaglio>();
                foreach (var CorpoCoda in CorpoDocumento.RaggruppamentiDocumento)
                {
                    CaricaDettagliTestaCoda(CorpoCoda, CorpoDocumento, Interna, ListaComune, Attributo, ListaAttributiPerColonnaInDocumento, Contatore);
                    Contatore++;
                }
                Contatore = 0;
                if(Attributo != null)
                    Dettaglicoda.Add(Interna);
            }

            Contatore = 0;
            foreach (var Dettaglio in Dettaglitesta)
            {
                DocumentoView DocumentoViewForCicle = new DocumentoView();
                if (ItemsRaggruppamenti.Count > 0)
                {
                    if (Contatore + 1 > ItemsRaggruppamenti.Count())
                    {
                        break;
                    }
                    DocumentoViewForCicle.RaggruppamentoSelected = ItemsRaggruppamenti.ElementAt(Contatore).TextBoxItemView.AttributeSelected;
                }
                DocumentoViewForCicle.RaggruppamentoTeste = new ObservableCollection<Dettaglio>(Dettaglio);
                DocumentoViewForCicle.RaggruppamentoCode = new ObservableCollection<Dettaglio>(Dettaglicoda.ElementAt(Contatore));

                if (string.IsNullOrEmpty(Dettaglio.FirstOrDefault().EntityTypeAssegnazioneGruppo))
                {
                    Dettaglio.FirstOrDefault().EntityTypeAssegnazioneGruppo = ReportSetting.RaggruppamentiDatasource.Where(r => r.Attributo == Dettaglio.FirstOrDefault().AttributoAssegnazioneGruppo).FirstOrDefault().EntityType;
                }

                if (ItemsRaggruppamenti.ElementAt(Contatore).TextBoxItemView.SelectedTreeViewItem != null)
                {
                    GroupSetting.Add(Dettaglio.FirstOrDefault().AttributoAssegnazioneGruppo + "_" + Dettaglio.FirstOrDefault().EntityTypeAssegnazioneGruppo, DocumentoViewForCicle);
                }
                Contatore++;
            }

            if (GroupSetting.Count != 0)
            {
                SelectedItemInGroup = ItemsRaggruppamenti.LastOrDefault();
                PreviousSelectedItemInGroup = ItemsRaggruppamenti.LastOrDefault().TextBoxItemView.SelectedTreeViewItem.GroupKey;
            }

            if (ReportSetting.OrdinamentoCorpo != null)
            {
                if (!string.IsNullOrEmpty(ReportSetting.OrdinamentoCorpo.Attributo))
                {
                    OrdinamentoView.IsOrdineCrescente = ReportSetting.OrdinamentoCorpo.IsOrdinamentoCrescente;
                    OrdinamentoView.IsOrdineDecrescente = ReportSetting.OrdinamentoCorpo.IsOrdinamentoDecrescente;
                    OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem = OrdinamentoView.TextBoxItemViewOrdinamento.ListaComboBox.Where(r => r.AttrbutoCodice == ReportSetting.OrdinamentoCorpo.AttributoCodice && r.EntityType == ReportSetting.OrdinamentoCorpo.EntityType).FirstOrDefault();
                }
            }

            //I set null in order to avoid saving if i dFsaon't do any modificatio, if i save i recreate the variable
            ReportSetting = null;
            AggiuntiComandiDifferenti();

        }

        private void EliminoRaggruppamentiNonEsistenti()
        {
            List<int> Contatori = new List<int>();
            int Contatore = 0;
            foreach (var Raggruppamento in ReportSetting.RaggruppamentiDatasource)
            {
                var Attibuto = ReportSettingDataViewHelper.RicercaAttributoInAlbero(ReportSettingDataViewHelper.GetAttributeTree(), Raggruppamento.Attributo, Raggruppamento.EntityType, Raggruppamento.AttributoCodice, Raggruppamento.AttributoCodiceOrigine, null);
                if (Attibuto == null)
                    Contatori.Add(Contatore);
                Contatore++;
            }

            int contatoreDinamico = 0;

            foreach (int Cont in Contatori)
            {
                ReportSetting.RaggruppamentiDatasource.RemoveAt(Cont - contatoreDinamico);
                ReportSetting.Teste.RemoveAt(Cont - contatoreDinamico);
                ReportSetting.Code.RemoveAt(Cont - contatoreDinamico);
                contatoreDinamico++;
            }
        }

        private void ReplaceGroupContentOnSelectetGroupAction(RaggruppamentiItemsView selectedItemInGroup)
        {
            if (selectedItemInGroup != null)
            {
                if (String.IsNullOrEmpty(selectedItemInGroup.TextBoxItemView.AttributeSelected))
                {
                    DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Collapsed;
                    DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Collapsed;
                    return;
                }

                if (PreviousSelectedItemInGroup != null)
                {
                    GroupSetting[PreviousSelectedItemInGroup] = DocumentoCorpoView.CreateANewIstanceOfThis();
                    if (ReportSettingDataViewHelper.SostituzioneAttributoRaggruppamento)
                    {
                        GroupSetting.Remove(PreviousSelectedItemInGroup);
                        ReportSettingDataViewHelper.SostituzioneAttributoRaggruppamento = false;
                    }
                }

                if (!String.IsNullOrEmpty(_SelectedItemInGroup.TextBoxItemView.AttributeSelected))
                {
                    if (!GroupSetting.ContainsKey(_SelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem.GroupKey))
                    {
                        DocumentoCorpoView.RaggruppamentoTeste.Clear();
                        DocumentoCorpoView.RaggruppamentoCode.Clear();

                        for (int i = 0; i < Convert.ToInt32(NumeroColonne); i++)
                        {
                            DettaglioInitialization(DocumentoCorpoView.RaggruppamentoTeste, OriginFrom.FromHeaderFooterGroup);
                            DettaglioInitialization(DocumentoCorpoView.RaggruppamentoCode, OriginFrom.FromHeaderFooterGroup);
                            DocumentoCorpoView.RaggruppamentoSelected = _SelectedItemInGroup.TextBoxItemView.AttributeSelected;
                        }

                        DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Visible;
                        DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Visible;
                    }
                    else
                    {
                        DocumentoCorpoView.CreateANewIstanceOfThis(GroupSetting[_SelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem.GroupKey]);
                        DocumentoCorpoView.RaggruppamentoSelected = _SelectedItemInGroup.TextBoxItemView.AttributeSelected;
                        DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Visible;
                        DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Visible;
                    }

                    AggiuntiComandiDifferenti();
                }
                else
                {
                    DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Collapsed;
                    DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Collapsed;
                }
            }
            else
            {
                DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Collapsed;
                DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Collapsed;
            }

            if (_SelectedItemInGroup == null)
            {
                PreviousSelectedItemInGroup = null;
            }
            else
            {
                PreviousSelectedItemInGroup = _SelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem.GroupKey;
            }
        }
        private void AggiungiTogliColonnaInRaggruppamentiSalvati(int indiceColonna, bool IsAddFunction)
        {
            if (IsAddFunction)
            {
                foreach (var Group in GroupSetting)
                {
                    //foreach (var PrimoLivello in ListAttributes)
                    foreach (var PrimoLivello in ReportSettingDataViewHelper.GetAttributeTree())
                    {
                        if (PrimoLivello.GroupKey == Group.Key)
                        {
                            AggiungiColonnaInPosizioneSpecificata(indiceColonna, PrimoLivello, Group.Value);
                            break;
                        }
                        foreach (var SecondoLivello in PrimoLivello.Items)
                        {
                            if (SecondoLivello.GroupKey == Group.Key)
                            {
                                AggiungiColonnaInPosizioneSpecificata(indiceColonna, SecondoLivello, Group.Value);
                                break;
                            }
                            foreach (var TerzoLivello in SecondoLivello.Items)
                            {
                                if (TerzoLivello.GroupKey == Group.Key)
                                {
                                    AggiungiColonnaInPosizioneSpecificata(indiceColonna, TerzoLivello, Group.Value);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var Group in GroupSetting)
                {
                    EliminaColonnaInPosizioneSpecificata(indiceColonna, Group.Value);
                }
            }
        }
        private void AggiuntiComandiDifferenti()
        {
            int TotaleRighe = 0;

            if (DocumentoCorpoView.ListaComandiTesta == null) { DocumentoCorpoView.ListaComandiTesta = new ObservableCollection<ComandiPerRiga>(); }
            if (DocumentoCorpoView.ListaComandiCorpo == null) { DocumentoCorpoView.ListaComandiCorpo = new ObservableCollection<ComandiPerRiga>(); }
            if (DocumentoCorpoView.ListaComandiCoda == null) { DocumentoCorpoView.ListaComandiCoda = new ObservableCollection<ComandiPerRiga>(); }
            if (DocumentoCorpoView.ListaComandiFine == null) { DocumentoCorpoView.ListaComandiFine = new ObservableCollection<ComandiPerRiga>(); }

            if (DocumentoCorpoView.RaggruppamentoTeste.Count() != 0)
            {
                TotaleRighe = DocumentoCorpoView.RaggruppamentoTeste.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiTesta.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Testa) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiTesta.Add(ComandiPerRiga);
                }
            }

            if (DocumentoCorpoView.DocumentoCorpi.Count() != 0)
            {
                TotaleRighe = DocumentoCorpoView.DocumentoCorpi.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCorpo = new ObservableCollection<ComandiPerRiga>();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Corpo) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCorpo.Add(ComandiPerRiga);
                }
            }

            if (DocumentoCorpoView.RaggruppamentoCode.Count() != 0)
            {
                TotaleRighe = DocumentoCorpoView.RaggruppamentoCode.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCoda.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Coda) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCoda.Add(ComandiPerRiga);
                }
            }

            if (DocumentoCorpoView.DocumentoFine.Count() != 0)
            {
                TotaleRighe = DocumentoCorpoView.DocumentoFine.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiFine = new ObservableCollection<ComandiPerRiga>();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Fine) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiFine.Add(ComandiPerRiga);
                }
            }
        }

        private void AggiornaVisibilitaBottoni()
        {
            if (ItemsRaggruppamenti.Count > 1) { IsVisibleButtonForOperation = Visibility.Visible; } else { IsVisibleButtonForOperation = Visibility.Hidden; }
            if (ItemsRaggruppamenti.Count > 0) { IsVisibleDeleteButton = Visibility.Visible; } else { IsVisibleDeleteButton = Visibility.Hidden; }
        }

        private void AssegnazioneStileCellaTabella(TextBoxItemView TextBoxItemView, CellaTabella CellaTabella)
        {
            if (CellaTabella.StileCarattere.ColorBackground != null)
            {
                TextBoxItemView.FormatCharacterView.ColorBackground = TextBoxItemView.FormatCharacterView.Colors.Where(d => d.HexValue == CellaTabella.StileCarattere.ColorBackground.HexValue).FirstOrDefault();
            }
            if (CellaTabella.StileCarattere.ColorCharacther != null)
            {
                TextBoxItemView.FormatCharacterView.ColorCharacther = TextBoxItemView.FormatCharacterView.Colors.Where(d => d.HexValue == CellaTabella.StileCarattere.ColorCharacther.HexValue).FirstOrDefault();
            }
            TextBoxItemView.FormatCharacterView.FontFamily = CellaTabella.StileCarattere.FontFamily;
            TextBoxItemView.FormatCharacterView.IsBarrato = CellaTabella.StileCarattere.IsBarrato;
            TextBoxItemView.FormatCharacterView.IsGrassetto = CellaTabella.StileCarattere.IsGrassetto;
            TextBoxItemView.FormatCharacterView.IsCorsivo = CellaTabella.StileCarattere.IsCorsivo;
            TextBoxItemView.FormatCharacterView.IsSottolineato = CellaTabella.StileCarattere.IsSottolineato;
            TextBoxItemView.FormatCharacterView.Size = CellaTabella.StileCarattere.Size;
            if (CellaTabella.StileCarattere.Stile == StampeKeys.LocalizeNessunoStile)
            {
                TextBoxItemView.FormatCharacterView.StileConPropieta = TextBoxItemView.FormatCharacterView.ListStiliConPropieta.FirstOrDefault();
            }
            else
            {
                TextBoxItemView.FormatCharacterView.SettaStileProgetto(CellaTabella.StileCarattere.Stile);
            }
            TextBoxItemView.FormatCharacterView.SetAlignementFormExternal(CellaTabella.StileCarattere.TextAlignementCode, CellaTabella.StileCarattere.TextVerticalAlignementCode);
        }
        private void AssegnazioneStile(TextBoxItemView TextBoxItemView, CellaTabella CellaTabella)
        {
            CellaTabella.StileCarattere.ColorBackground = TextBoxItemView.FormatCharacterView.ColorBackground;
            CellaTabella.StileCarattere.ColorCharacther = TextBoxItemView.FormatCharacterView.ColorCharacther;
            CellaTabella.StileCarattere.FontFamily = TextBoxItemView.FormatCharacterView.FontFamily;
            CellaTabella.StileCarattere.IsBarrato = TextBoxItemView.FormatCharacterView.IsBarrato;
            CellaTabella.StileCarattere.IsGrassetto = TextBoxItemView.FormatCharacterView.IsGrassetto;
            CellaTabella.StileCarattere.IsCorsivo = TextBoxItemView.FormatCharacterView.IsCorsivo;
            CellaTabella.StileCarattere.IsSottolineato = TextBoxItemView.FormatCharacterView.IsSottolineato;
            CellaTabella.StileCarattere.Size = TextBoxItemView.FormatCharacterView.Size;
            if (TextBoxItemView.FormatCharacterView.StileConPropieta != null)
            {
                CellaTabella.StileCarattere.Stile = TextBoxItemView.FormatCharacterView.StileConPropieta.NomeECodice;
            }
            CellaTabella.StileCarattere.TextAlignementCode = TextBoxItemView.FormatCharacterView.TextAlignementCode;
            CellaTabella.StileCarattere.TextVerticalAlignementCode = TextBoxItemView.FormatCharacterView.TextVerticalAlignementCode;
            CellaTabella.StileCarattere.TextVerticalAlignement = TextBoxItemView.FormatCharacterView.TextVerticalAlignement;
        }
        private void CaricaDettagliTestaCoda(RaggruppamentiDocumento RaggruppamentiDocumento, Raggruppamenti Raggruppamenti, List<Dettaglio> Interna, ObservableCollection<TreeviewItem> listaComune, TreeviewItem attributo, Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonnaInDocumento, int Contatore)
        {
            Dettaglio Dettaglio = new Dettaglio();
            Dettaglio.ListaDettaglio = new ObservableCollection<TextBoxItemView>();
            Dettaglio.AttributoAssegnazioneGruppo = Raggruppamenti.Attributo;
            Dettaglio.EntityTypeAssegnazioneGruppo = Raggruppamenti.EntityType;
            CaricaDettagli(Dettaglio.ListaDettaglio, RaggruppamentiDocumento.RaggruppamentiValori, listaComune, attributo, ListaAttributiPerColonnaInDocumento, Contatore);
            Interna.Add(Dettaglio);
        }
        private void CaricaDettagli(ObservableCollection<TextBoxItemView> listaDettaglio, List<RaggruppamentiValori> raggruppamentiValori, ObservableCollection<TreeviewItem> listaComune, TreeviewItem SelectedTreeViewItem, Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonnaInDocumento, int Contatore)
        {
            foreach (var Valori in raggruppamentiValori)
            {
                TextBoxItemView textBoxItemView = new TextBoxItemView();
                textBoxItemView.DataService = DataService;
                textBoxItemView.Etichetta = Valori.Etichetta;
                textBoxItemView.Origine = (int)OriginFrom.FromHeaderFooterGroup;
                textBoxItemView.FormatCharacterView = new FormatCharacterView();
                textBoxItemView.FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                textBoxItemView.Init();
                textBoxItemView.ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedTreeViewItem);
                if (SelectedTreeViewItem != null)
                {
                    //textBoxItemView.ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedTreeViewItem);
                    AggiuntaSecondoLivelloAFunzioni(textBoxItemView.ListaComboBox, ListaAttributiPerColonnaInDocumento, Contatore);
                    if (string.IsNullOrEmpty(Valori.CodiceDigicorp))
                    {
                        textBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(textBoxItemView.ListaComboBox, Valori.Attributo, Valori.EntityType, Valori.AttributoCodice, Valori.AttributoCodiceOrigine, null);
                    }
                    else
                    {
                        textBoxItemView.SelectedTreeViewItem = ReportSettingDataViewHelper.RicercaAttributoInAlbero(textBoxItemView.ListaComboBox, Valori.Attributo, Valori.EntityType, null, null, Valori.CodiceDigicorp);
                    }
                    if (textBoxItemView.SelectedTreeViewItem != null)
                    {
                        if (textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                        {
                            textBoxItemView.FormatCharacterView.IsRTFVisible = Visibility.Visible;
                            textBoxItemView.FormatCharacterView.Rtf = Valori.Rtf;
                            textBoxItemView.FormatCharacterView.DescrBreve = Valori.DescrBreve;
                        }
                        if (textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Reale || textBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Contabilita)
                        {
                            textBoxItemView.FormatCharacterView.IsStampaFormulaVisible = Visibility.Visible;
                            textBoxItemView.FormatCharacterView.StampaFormula = Valori.StampaFormula;
                        }
                        textBoxItemView.AttributeSelected = textBoxItemView.SelectedTreeViewItem.Attrbuto;
                        textBoxItemView.EntitySelected = textBoxItemView.SelectedTreeViewItem.EntityType;
                        textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.SelectedTreeViewItem);
                    }
                    else
                        textBoxItemView.ImpostaSelezioneAttributoInControllo(textBoxItemView.ListaComboBox.FirstOrDefault());
                }
                textBoxItemView.FormatCharacterView.ConcatenaEtichettaEValore = Valori.ConcatenaEtichettaEValore;
                textBoxItemView.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                if (Valori.StileCarattere != null)
                    AssegnazioneStileCellaTabella(textBoxItemView, Valori);
                listaDettaglio.Add(textBoxItemView);
            }
        }
        private void GeneraColonne()
        {
            for (int i = 0; i < Convert.ToInt32(NumeroColonne); i++)
            {
                DocumentoCorpoView.IntestazioniColonne.Add(new IntestazioneColonnaEntity(DataService) { IntestazioneColonna = LocalizationProvider.GetString("Colonna") + ContatoreColonnaInserita.ToString() });
                DocumentoCorpoView.IntestazioniColonne.LastOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                DocumentoCorpoView.IntestazioniColonne.LastOrDefault().Init(true, false);
                ContatoreColonnaInserita++;

                DettaglioInitialization(DocumentoCorpoView.DocumentoCorpi, OriginFrom.FromCorpo);
                DettaglioInitialization(DocumentoCorpoView.DocumentoFine, OriginFrom.FromFineDocumento);

                DocumentoCorpoView.LarghezzaColonne.Add(new LarghezzaColonnaEntity());
                DocumentoCorpoView.LarghezzaColonne.Last().UpdateCalcoloTotaleLarghezzaColonna += ReportWizardSettingDataView_UpdateCalcoloTotaleLarghezzaColonna;
            }

            foreach (var Dettaglio in DocumentoCorpoView.DocumentoCorpi.LastOrDefault().ListaDettaglio)
            {
                ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Corpo) { IndiceComando = 0 };
                ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                DocumentoCorpoView.ListaComandiCorpo.Add(ComandiPerRiga);
            }
            foreach (var Dettaglio in DocumentoCorpoView.DocumentoFine.LastOrDefault().ListaDettaglio)
            {
                ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Fine) { IndiceComando = 0 };
                ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                DocumentoCorpoView.ListaComandiFine.Add(ComandiPerRiga);
            }

            //connet the event of the column
            foreach (var Colonna in DocumentoCorpoView.IntestazioniColonne) { Colonna.ColumnCorpoHanlder += Colonna_ColumnCorpoHanlder; }
        }
        private void DettaglioInitialization(ObservableCollection<Dettaglio> raggruppamentovalori, OriginFrom originInitialization)
        {
            raggruppamentovalori.Add(new Dettaglio());
            raggruppamentovalori.LastOrDefault().ListaDettaglio.Add(new TextBoxItemView() { DataService = DataService, Origine = (int)originInitialization });
            raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().FormatCharacterView = new FormatCharacterView();
            if (OriginFrom.FromCorpo == originInitialization)
            {
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().FormatCharacterView.IsNascondiVisible = Visibility.Visible;
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().FormatCharacterView.IsRTFVisible = Visibility.Collapsed;
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().FormatCharacterView.IsStampaFormulaVisible = Visibility.Collapsed;
            }
            raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
            raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().Init(false);
            if (originInitialization == OriginFrom.FromHeaderFooterGroup)
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem);
            if (originInitialization == OriginFrom.FromCorpo)
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().ListaComboBox = ReportSettingDataViewHelper.GetAttributeTree();
            if (originInitialization == OriginFrom.FromFineDocumento)
                raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().ListaComboBox = GeneraListaAttributiFineDocumento();
            raggruppamentovalori.LastOrDefault().ListaDettaglio.FirstOrDefault().CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
            raggruppamentovalori.LastOrDefault().ListaDettaglio.LastOrDefault().RigeneraListaAttributiPerFunzioni += ReportWizardSettingDataView_RigeneraListaAttributiPerFunzioni;
        }
        private void ComandiPerRiga_AddDeleteRowHanlder(object sender, AddDeleteRowEvent e)
        {
            if (e.IsAdding)
                AddRow(e.IndiceRiga, e.Banda);
            else
                DeleteRow(e.IndiceRiga, e.Banda);
        }
        private void AddRow(int IndiceComando, string Banda)
        {
            TreeviewItem SelectedTreeViewItem = SelectedItemInGroup?.TextBoxItemView.SelectedTreeViewItem;
            if (Banda == ComandiPerRiga.Testa)
            {
                int Contatore = 0;
                foreach (var ListaAttributi in DocumentoCorpoView.RaggruppamentoTeste)
                {
                    ListaAttributi.ListaDettaglio.Insert(IndiceComando, new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromHeaderFooterGroup });
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterView = new FormatCharacterView();
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    ListaAttributi.ListaDettaglio[IndiceComando].Init(false);
                    ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedTreeViewItem);
                    AggiuntaSecondoLivelloAFunzioni(ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox, GeneraDictionaryAttributiRealiContabilitaPerColonna(), Contatore);
                    ListaAttributi.ListaDettaglio[IndiceComando].CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    Contatore++;
                }

                int TotaleRighe = DocumentoCorpoView.RaggruppamentoTeste.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiTesta.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Testa) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiTesta.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Corpo)
            {
                foreach (var ListaAttributi in DocumentoCorpoView.DocumentoCorpi)
                {
                    ListaAttributi.ListaDettaglio.Insert(IndiceComando, new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromCorpo });
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterView = new FormatCharacterView();
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterView.IsNascondiVisible = Visibility.Visible;
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    ListaAttributi.ListaDettaglio[IndiceComando].Init(false);
                    ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox = ReportSettingDataViewHelper.GetAttributeTree();
                    ListaAttributi.ListaDettaglio[IndiceComando].CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    ListaAttributi.ListaDettaglio[IndiceComando].RigeneraListaAttributiPerFunzioni += ReportWizardSettingDataView_RigeneraListaAttributiPerFunzioni;
                }

                int TotaleRighe = DocumentoCorpoView.DocumentoCorpi.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCorpo.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Corpo) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCorpo.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Coda)
            {
                int Contatore = 0;
                foreach (var ListaAttributi in DocumentoCorpoView.RaggruppamentoCode)
                {
                    ListaAttributi.ListaDettaglio.Insert(IndiceComando, new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromHeaderFooterGroup });
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterView = new FormatCharacterView();
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    ListaAttributi.ListaDettaglio[IndiceComando].Init(false);
                    ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedTreeViewItem);
                    AggiuntaSecondoLivelloAFunzioni(ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox, GeneraDictionaryAttributiRealiContabilitaPerColonna(), Contatore);
                    ListaAttributi.ListaDettaglio[IndiceComando].CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    Contatore++;
                }

                int TotaleRighe = DocumentoCorpoView.RaggruppamentoCode.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCoda.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Coda) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCoda.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Fine)
            {
                int Contatore = 0;
                foreach (var ListaAttributi in DocumentoCorpoView.DocumentoFine)
                {
                    ListaAttributi.ListaDettaglio.Insert(IndiceComando, new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromFineDocumento });
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterView = new FormatCharacterView();
                    ListaAttributi.ListaDettaglio[IndiceComando].FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    ListaAttributi.ListaDettaglio[IndiceComando].Init(false);
                    ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox = GeneraListaAttributiFineDocumento();
                    AggiuntaSecondoLivelloAFunzioni(ListaAttributi.ListaDettaglio[IndiceComando].ListaComboBox, GeneraDictionaryAttributiRealiContabilitaPerColonna(), Contatore);
                    ListaAttributi.ListaDettaglio[IndiceComando].CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    Contatore++;
                }

                int TotaleRighe = DocumentoCorpoView.DocumentoFine.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiFine.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Fine) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiFine.Add(ComandiPerRiga);
                }
            }

            ForceCloseOfPopUps();
        }
        public void DeleteRow(int IndiceComando, string Banda)
        {
            TreeviewItem SelectedTreeViewItem = SelectedItemInGroup?.TextBoxItemView.SelectedTreeViewItem;

            int TotaleRighe;

            if (Banda == ComandiPerRiga.Testa)
            {
                if (DocumentoCorpoView.ListaComandiTesta.Count() == 1)
                {
                    MessageBox.Show(LocalizationProvider.GetString("AttenzioneNonEPossibileCancellareLUltimaRiga"), LocalizationProvider.GetString("AppName"));
                    return;
                }

                foreach (var ListaAttributi in DocumentoCorpoView.RaggruppamentoTeste)
                {
                    ListaAttributi.ListaDettaglio.RemoveAt(IndiceComando);
                }
                DocumentoCorpoView.ListaComandiTesta.RemoveAt(IndiceComando);

                TotaleRighe = DocumentoCorpoView.RaggruppamentoTeste.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiTesta.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Testa) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiTesta.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Corpo)
            {
                if (DocumentoCorpoView.ListaComandiCorpo.Count() == 1)
                {
                    MessageBox.Show(LocalizationProvider.GetString("AttenzioneNonEPossibileCancellareLUltimaRiga"), LocalizationProvider.GetString("AppName"));
                    return;
                }

                foreach (var ListaAttributi in DocumentoCorpoView.DocumentoCorpi)
                {
                    ListaAttributi.ListaDettaglio.RemoveAt(IndiceComando);
                }
                DocumentoCorpoView.ListaComandiCorpo.RemoveAt(IndiceComando);

                TotaleRighe = DocumentoCorpoView.DocumentoCorpi.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCorpo.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Corpo) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCorpo.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Coda)
            {
                if (DocumentoCorpoView.ListaComandiCoda.Count() == 1)
                {
                    MessageBox.Show(LocalizationProvider.GetString("AttenzioneNonEPossibileCancellareLUltimaRiga"), LocalizationProvider.GetString("AppName"));
                    return;
                }

                foreach (var ListaAttributi in DocumentoCorpoView.RaggruppamentoCode)
                {
                    ListaAttributi.ListaDettaglio.RemoveAt(IndiceComando);
                }
                DocumentoCorpoView.ListaComandiCoda.RemoveAt(IndiceComando);

                TotaleRighe = DocumentoCorpoView.RaggruppamentoCode.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiCoda.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Coda) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiCoda.Add(ComandiPerRiga);
                }
            }
            if (Banda == ComandiPerRiga.Fine)
            {
                if (DocumentoCorpoView.ListaComandiFine.Count() == 1)
                {
                    MessageBox.Show(LocalizationProvider.GetString("AttenzioneNonEPossibileCancellareLUltimaRiga"), LocalizationProvider.GetString("AppName"));
                    return;
                }

                foreach (var ListaAttributi in DocumentoCorpoView.DocumentoFine)
                {
                    ListaAttributi.ListaDettaglio.RemoveAt(IndiceComando);
                }
                DocumentoCorpoView.ListaComandiFine.RemoveAt(IndiceComando);

                TotaleRighe = DocumentoCorpoView.DocumentoFine.FirstOrDefault().ListaDettaglio.Count;
                DocumentoCorpoView.ListaComandiFine.Clear();
                for (int i = 0; i < TotaleRighe; i++)
                {
                    ComandiPerRiga ComandiPerRiga = new ComandiPerRiga(ComandiPerRiga.Fine) { IndiceComando = i };
                    ComandiPerRiga.AddDeleteRowHanlder += ComandiPerRiga_AddDeleteRowHanlder;
                    DocumentoCorpoView.ListaComandiFine.Add(ComandiPerRiga);
                }
            }

            ForceCloseOfPopUps();
            RigeneraListaAttributiPerFunzioni();
        }
        private void Colonna_ColumnCorpoHanlder(object sender, ColumnCorpoEventArgs e)
        {
            int IndiceColonna = 0;

            foreach (var Colonna in DocumentoCorpoView.IntestazioniColonne) { if (Colonna.IntestazioneColonna == e.ColumnTitle) { break; } IndiceColonna++; }

            if (e.Add)
            {
                NumeroColonne = Convert.ToString(Convert.ToInt32(NumeroColonne) + 1);
                AggiungiColonnaInPosizioneSpecificata(IndiceColonna, SelectedItemInGroup?.TextBoxItemView.SelectedTreeViewItem, DocumentoCorpoView, true);
                AggiungiTogliColonnaInRaggruppamentiSalvati(IndiceColonna, true);
            }


            if (e.Delete)
            {
                if (Convert.ToInt32(NumeroColonne) - 1 == 0)
                {
                    MessageBox.Show(LocalizationProvider.GetString("Attenzione!NonEPossibileCancellareLUltimaColonna"), LocalizationProvider.GetString("AppName"));
                    return;
                }
                NumeroColonne = Convert.ToString(Convert.ToInt32(NumeroColonne) - 1);
                EliminaColonnaInPosizioneSpecificata(IndiceColonna, DocumentoCorpoView, true);
                AggiungiTogliColonnaInRaggruppamentiSalvati(IndiceColonna, false);
            }
            UpdateCalcoloTotaleLarghezzaColonnaFunction();
            AggiuntiComandiDifferenti();
        }
        private void AggiungiColonnaInPosizioneSpecificata(int indiceColonna, TreeviewItem attributoSelezionato, DocumentoView documentiview, bool IsDocumentoView = false)
        {
            int ConteggioColonneTesta = documentiview.RaggruppamentoTeste.Count();
            int ConteggioColonneCoda = documentiview.RaggruppamentoCode.Count();
            int ConteggioDettagliTeste = 0;
            int ConteggioDettagliCode = 0;

            if (documentiview.RaggruppamentoTeste.Count() != 0)
            {
                ConteggioDettagliTeste = documentiview.RaggruppamentoTeste.FirstOrDefault().ListaDettaglio.Count();
                ConteggioDettagliCode = documentiview.RaggruppamentoCode.FirstOrDefault().ListaDettaglio.Count();
            }

            if (ConteggioColonneTesta < Convert.ToInt32(NumeroColonne))
            {
                if (documentiview.RaggruppamentoTeste.Count() != 0)
                {
                    documentiview.RaggruppamentoTeste.Insert(indiceColonna + 1, new Dettaglio());
                    documentiview.RaggruppamentoCode.Insert(indiceColonna + 1, new Dettaglio());
                }

                if (attributoSelezionato != null)
                {
                    for (int i = 0; i < ConteggioDettagliTeste; i++)
                    {
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.Add(new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromHeaderFooterGroup });
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView = new FormatCharacterView();
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.LastOrDefault().Init(false);
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.LastOrDefault().ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(attributoSelezionato);
                        documentiview.RaggruppamentoTeste[indiceColonna + 1].ListaDettaglio.LastOrDefault().CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    }

                    for (int i = 0; i < ConteggioDettagliCode; i++)
                    {
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.Add(new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromHeaderFooterGroup });
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView = new FormatCharacterView();
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.LastOrDefault().Init(false);
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.LastOrDefault().ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(attributoSelezionato);
                        documentiview.RaggruppamentoCode[indiceColonna + 1].ListaDettaglio.LastOrDefault().CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    }
                }
            }

            if (IsDocumentoView)
            {
                int ConteggioColonneCorpo = documentiview.DocumentoCorpi.Count();
                int ConteggioColonneFine = documentiview.DocumentoFine.Count();
                int ConteggioColonneLarghezza = documentiview.LarghezzaColonne.Count();
                int ConteggioColonneIntestazione = documentiview.IntestazioniColonne.Count();
                int ConteggioDettagliCorpo = documentiview.DocumentoCorpi.FirstOrDefault().ListaDettaglio.Count();
                int ConteggioDettagliFine = documentiview.DocumentoFine.FirstOrDefault().ListaDettaglio.Count();

                string titleColonna = LocalizationProvider.GetString("Colonna") + ContatoreColonnaInserita.ToString();
                ContatoreColonnaInserita++;

                if (documentiview.IntestazioniColonne.Count() < Convert.ToInt32(NumeroColonne))
                {
                    documentiview.IntestazioniColonne.Insert(indiceColonna + 1, new IntestazioneColonnaEntity(DataService) { IntestazioneColonna = titleColonna });
                    documentiview.IntestazioniColonne.ElementAt(indiceColonna + 1).FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                    documentiview.IntestazioniColonne.ElementAt(indiceColonna + 1).Init(true, false);
                    documentiview.IntestazioniColonne[indiceColonna + 1].ColumnCorpoHanlder += Colonna_ColumnCorpoHanlder;
                }

                if (documentiview.LarghezzaColonne.Count() < Convert.ToInt32(NumeroColonne))
                {
                    documentiview.LarghezzaColonne.Insert(indiceColonna + 1, new LarghezzaColonnaEntity() { LarghezzaColonna = 1 });
                    documentiview.LarghezzaColonne.ElementAt(indiceColonna + 1).UpdateCalcoloTotaleLarghezzaColonna += ReportWizardSettingDataView_UpdateCalcoloTotaleLarghezzaColonna;
                }


                if (ConteggioColonneCorpo < Convert.ToInt32(NumeroColonne))
                {
                    documentiview.DocumentoCorpi.Insert(indiceColonna + 1, new Dettaglio());

                    for (int i = 0; i < ConteggioDettagliCorpo; i++)
                    {
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.Add(new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromCorpo });
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView = new FormatCharacterView();
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView.IsNascondiVisible = Visibility.Visible;
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView.IsRTFVisible = Visibility.Collapsed;
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView.IsStampaFormulaVisible = Visibility.Collapsed;
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().Init(false);
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().ListaComboBox = ReportSettingDataViewHelper.GetAttributeTree();
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                        documentiview.DocumentoCorpi[indiceColonna + 1].ListaDettaglio.LastOrDefault().RigeneraListaAttributiPerFunzioni += ReportWizardSettingDataView_RigeneraListaAttributiPerFunzioni;
                    }
                }
                if (ConteggioColonneFine < Convert.ToInt32(NumeroColonne))
                {
                    documentiview.DocumentoFine.Insert(indiceColonna + 1, new Dettaglio());

                    for (int i = 0; i < ConteggioDettagliFine; i++)
                    {
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.Add(new TextBoxItemView() { DataService = DataService, Origine = (int)OriginFrom.FromFineDocumento });
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterView = new FormatCharacterView();
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.LastOrDefault().FormatCharacterWnd = ReportSettingDataViewHelper.FormatCharacterWnd;
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.LastOrDefault().Init(false);
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.LastOrDefault().ListaComboBox = GeneraListaAttributiFineDocumento();
                        documentiview.DocumentoFine[indiceColonna + 1].ListaDettaglio.LastOrDefault().CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
                    }
                }
            }
        }
        private void EliminaColonnaInPosizioneSpecificata(int indiceColonna, DocumentoView documentiview, bool IsDocumentoView = false)
        {
            if (documentiview.RaggruppamentoTeste.Count() > 0)
            {
                documentiview.RaggruppamentoTeste.RemoveAt(indiceColonna);
            }
            if (documentiview.RaggruppamentoCode.Count() > 0)
            {
                documentiview.RaggruppamentoCode.RemoveAt(indiceColonna);
            }

            if (IsDocumentoView)
            {
                documentiview.DocumentoCorpi.RemoveAt(indiceColonna);
                documentiview.LarghezzaColonne.RemoveAt(indiceColonna);
                documentiview.DocumentoFine.RemoveAt(indiceColonna);
                documentiview.IntestazioniColonne.RemoveAt(indiceColonna);
            }

        }
        private StampeData CreateSettingReportEntity()
        {
            StampeData ReportSetting = new StampeData();
            ReportSetting.RaggruppamentiDatasource = new List<Raggruppamento>();
            ReportSetting.OrdinamentiDatasource = new List<CellaTabellaBase>();
            ReportSetting.Intestazioni = new List<Intestazione>();
            ReportSetting.Teste = new List<Raggruppamenti>();
            ReportSetting.Code = new List<Raggruppamenti>();
            ReportSetting.CorpiDocumento = new List<CorpiDocumento>();
            ReportSetting.FineDocumento = new List<CorpiDocumento>();
            DocumentoView SelectedGroup = null;

            List<int> ContatoreColonnaGrandezzaZero = new List<int>();
            int ContatoreColonneGrandezza = 0;

            decimal SommaLarghezzaColonne = DocumentoCorpoView.LarghezzaColonne.Sum(y => y.LarghezzaColonna);
            if (SommaLarghezzaColonne == 0)
            {
                foreach (var LarghezzaColonna in DocumentoCorpoView.LarghezzaColonne)
                {
                    LarghezzaColonna.LarghezzaColonna = Math.Round((190 / (Convert.ToDecimal(NumeroColonne) * 10)), 2);
                }
            }
            else
            {
                foreach (var LarghezzaColonna in DocumentoCorpoView.LarghezzaColonne)
                {
                    if (LarghezzaColonna.LarghezzaColonna == 0)
                    {
                        ContatoreColonnaGrandezzaZero.Add(ContatoreColonneGrandezza);
                    }
                    ContatoreColonneGrandezza++;
                }
                decimal rimanenza = DocumentoCorpoView.LarghezzaColonne.Where(f => f.LarghezzaColonna != 0).Sum(y => y.LarghezzaColonna);
                decimal numerocolonne = DocumentoCorpoView.LarghezzaColonne.Where(f => f.LarghezzaColonna == 0).Count();

                if (numerocolonne != 0)
                {
                    decimal larghezzaCondivisa = Math.Round((190 - rimanenza) / (numerocolonne * 10), 2);

                    ContatoreColonneGrandezza = 0;
                    foreach (var LarghezzaColonna in DocumentoCorpoView.LarghezzaColonne)
                    {
                        foreach (var Zero in ContatoreColonnaGrandezzaZero)
                        {
                            if (ContatoreColonneGrandezza == Zero)
                            {
                                LarghezzaColonna.LarghezzaColonna = larghezzaCondivisa;
                            }
                        }

                        ContatoreColonneGrandezza++;
                    }
                }
            }

            int Contatore = 0;

            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                Raggruppamento raggruppamentoDataSource = new Raggruppamento();
                raggruppamentoDataSource.Attributo = Raggruppamento.TextBoxItemView.AttributeSelected;
                raggruppamentoDataSource.EntityType = Raggruppamento.TextBoxItemView.EntitySelected;
                if (Raggruppamento.TextBoxItemView.SelectedTreeViewItem != null)
                {
                    raggruppamentoDataSource.AttributoCodice = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice;
                    raggruppamentoDataSource.AttributoCodiceOrigine = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                    raggruppamentoDataSource.AttributoCodicePath = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttributoCodicePath;
                    raggruppamentoDataSource.DivisioneCodice = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.DivisioneCodice;
                    raggruppamentoDataSource.EntityType = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.EntityType;
                    raggruppamentoDataSource.PropertyType = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.PropertyType;
                    ReportSetting.AttributiDaEstrarrePerDataSource(Raggruppamento.TextBoxItemView.SelectedTreeViewItem.PropertyType, Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice, Raggruppamento.TextBoxItemView.SelectedTreeViewItem.EntityType, Raggruppamento.TextBoxItemView.SelectedTreeViewItem.DivisioneCodice, false, false,false, Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodiceOrigine, Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttributoCodicePath);
                    if (Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem != null)
                        ReportSetting.AttributiDaEstrarrePerDataSource(Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.PropertyType, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodice, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.EntityType, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.DivisioneCodice, false, false, false, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodiceOrigine, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttributoCodicePath);
                }

                raggruppamentoDataSource.Indent = Raggruppamento.Indent;
                raggruppamentoDataSource.IsCheckedDescrBreve = Raggruppamento.IsCheckedDescrBreve;
                raggruppamentoDataSource.IsCheckedNuovapagina = Raggruppamento.IsCheckedNuovapagina;
                raggruppamentoDataSource.IsCheckedRiepilogo = Raggruppamento.IsCheckedRiepilogo;
                raggruppamentoDataSource.IsCheckedTotale = Raggruppamento.IsCheckedTotale;
                raggruppamentoDataSource.IsOrdineCrescente = Raggruppamento.IsOrdineCrescente;
                raggruppamentoDataSource.IsOrdineDecrescente = Raggruppamento.IsOrdineDecrescente;
                raggruppamentoDataSource.OpzioniDiStampa = new OpzioniDiStampa();
                if (Raggruppamento.OpzioniDiStampa != null)
                {
                    raggruppamentoDataSource.OpzioniDiStampa.IsCheckedDescrizioneBreve = Raggruppamento.OpzioniDiStampa.IsCheckedDescrizioneBreve;
                    raggruppamentoDataSource.OpzioniDiStampa.IsCheckedNuovaPagina = Raggruppamento.OpzioniDiStampa.IsCheckedNuovaPagina;
                    raggruppamentoDataSource.OpzioniDiStampa.IsCheckedRiepilogo = Raggruppamento.OpzioniDiStampa.IsCheckedRiepilogo;
                }

                CellaTabellaBase ordinamentoDataSource = new CellaTabellaBase();
                if (Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem != null)
                {
                    ordinamentoDataSource.Attributo = Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.Attrbuto;
                    ordinamentoDataSource.AttributoCodice = Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodice;
                    ordinamentoDataSource.AttributoCodiceOrigine = Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                    ordinamentoDataSource.AttributoCodicePath = string.Format("{0}{1}{2}", Raggruppamento.TextBoxItemView.SelectedTreeViewItem.AttributoCodicePath, ReportSettingViewHelper.AttributoCodicePathSeparator, Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodice);
                    ordinamentoDataSource.EntityType = Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.EntityType;
                    ordinamentoDataSource.PropertyType = Raggruppamento.TextBoxItemViewOrdinamento.SelectedTreeViewItem.PropertyType;
                }
                raggruppamentoDataSource.EntitaAttributoOrdinamento = ordinamentoDataSource;
                ReportSetting.RaggruppamentiDatasource.Add(raggruppamentoDataSource);

            }

            ReportSetting.OrdinamentoCorpo = new OrdinamentoCorpo();
            if (OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem != null)
            {
                ReportSetting.OrdinamentoCorpo.Attributo = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.Attrbuto;
                ReportSetting.OrdinamentoCorpo.AttributoCodice = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodice;
                ReportSetting.OrdinamentoCorpo.AttributoCodiceOrigine = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                ReportSetting.OrdinamentoCorpo.AttributoCodicePath = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttributoCodicePath;
                ReportSetting.OrdinamentoCorpo.EntityType = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.EntityType;
                ReportSetting.OrdinamentoCorpo.PropertyType = OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.PropertyType;
                ReportSetting.OrdinamentoCorpo.IsOrdinamentoCrescente = OrdinamentoView.IsOrdineCrescente;
                ReportSetting.OrdinamentoCorpo.IsOrdinamentoDecrescente = OrdinamentoView.IsOrdineDecrescente;
                ReportSetting.AttributiDaEstrarrePerDataSource(ReportSetting.OrdinamentoCorpo.PropertyType, ReportSetting.OrdinamentoCorpo.AttributoCodice, ReportSetting.OrdinamentoCorpo.EntityType, null, false, false, false, ReportSetting.OrdinamentoCorpo.AttributoCodiceOrigine, OrdinamentoView.TextBoxItemViewOrdinamento.SelectedTreeViewItem.AttributoCodicePath);
            }

            Contatore = 0;
            foreach (var IntestazioneColonna in DocumentoCorpoView.IntestazioniColonne)
            {
                Intestazione IntestazioneCol = new Intestazione();
                IntestazioneCol.Etichetta = IntestazioneColonna.IntestazioneColonna;
                IntestazioneCol.Size = DocumentoCorpoView.LarghezzaColonne.ElementAt(Contatore).LarghezzaColonna;
                IntestazioneCol.StileCarattere = new ProprietaCarattere();
                if (IntestazioneColonna.FormatCharacterView != null)
                {
                    IntestazioneCol.StileCarattere.ColorBackground = IntestazioneColonna.FormatCharacterView.ColorBackground;
                    IntestazioneCol.StileCarattere.ColorCharacther = IntestazioneColonna.FormatCharacterView.ColorCharacther;
                    IntestazioneCol.StileCarattere.FontFamily = IntestazioneColonna.FormatCharacterView.FontFamily;
                    IntestazioneCol.StileCarattere.IsBarrato = IntestazioneColonna.FormatCharacterView.IsBarrato;
                    IntestazioneCol.StileCarattere.IsSottolineato = IntestazioneColonna.FormatCharacterView.IsSottolineato;
                    IntestazioneCol.StileCarattere.IsGrassetto = IntestazioneColonna.FormatCharacterView.IsGrassetto;
                    IntestazioneCol.StileCarattere.IsCorsivo = IntestazioneColonna.FormatCharacterView.IsCorsivo;
                    IntestazioneCol.StileCarattere.Size = IntestazioneColonna.FormatCharacterView.Size;
                    if (IntestazioneColonna.FormatCharacterView.StileConPropieta != null)
                    {
                        IntestazioneCol.StileCarattere.Stile = IntestazioneColonna.FormatCharacterView.StileConPropieta.NomeECodice;
                    }
                    IntestazioneCol.StileCarattere.TextAlignementCode = IntestazioneColonna.FormatCharacterView.TextAlignementCode;
                    IntestazioneCol.StileCarattere.TextVerticalAlignementCode = IntestazioneColonna.FormatCharacterView.TextVerticalAlignementCode;
                }
                ReportSetting.Intestazioni.Add(IntestazioneCol);
                Contatore++;
            }

            Contatore = 0;
            int ContatoreColonnaTesta = 0;
            int ContatoreColonnaCoda = 0;

            if (PreviousSelectedItemInGroup != null)
            {
                GroupSetting[PreviousSelectedItemInGroup] = DocumentoCorpoView.CreateANewIstanceOfThis();
            }

            foreach (var GroupSaved in GroupSetting)
            {
                SelectedGroup = GroupSaved.Value;

                Raggruppamenti testa = new Raggruppamenti();
                testa.Attributo = GroupSaved.Key.Split('_').ElementAt(0);
                testa.EntityType = GroupSaved.Key.Split('_').ElementAt(1);
                if (testa.RaggruppamentiDocumento == null) { testa.RaggruppamentiDocumento = new List<RaggruppamentiDocumento>(); }
                ContatoreColonnaTesta = 0;

                foreach (var CorpoRaggruppamento in SelectedGroup.RaggruppamentoTeste)
                {
                    RaggruppamentiDocumento RaggruppamentiDocumento = CreateSettingReportEntityRaggruppamentiDocumentoTesteCode(CorpoRaggruppamento, ContatoreColonnaTesta, ReportSetting);

                    ContatoreColonnaTesta++;
                    testa.RaggruppamentiDocumento.Add(RaggruppamentiDocumento);
                }

                ReportSetting.Teste.Add(testa);

                Raggruppamenti coda = new Raggruppamenti();
                coda.Attributo = GroupSaved.Key.Split('_').ElementAt(0);
                coda.EntityType = GroupSaved.Key.Split('_').ElementAt(1);
                if (coda.RaggruppamentiDocumento == null) { coda.RaggruppamentiDocumento = new List<RaggruppamentiDocumento>(); }
                ContatoreColonnaCoda = 0;

                foreach (var CorpoRaggruppamento in SelectedGroup.RaggruppamentoCode)
                {
                    RaggruppamentiDocumento RaggruppamentiDocumento = CreateSettingReportEntityRaggruppamentiDocumentoTesteCode(CorpoRaggruppamento, ContatoreColonnaCoda, ReportSetting);

                    ContatoreColonnaCoda++;
                    coda.RaggruppamentiDocumento.Add(RaggruppamentiDocumento);
                }

                ReportSetting.Code.Add(coda);

                Contatore++;
            }

            // Riordino testate e code secondo ragggruppamenti
            int ContatoreGruppo = 0;
            int ContatoreElementoTrovato = 0;
            foreach (var Raggr in ReportSetting.RaggruppamentiDatasource)
            {
                ContatoreElementoTrovato = 0;
                foreach (var Testa in ReportSetting.Teste)
                {
                    if (Testa.Attributo == Raggr.Attributo && Testa.EntityType == Raggr.EntityType)
                    {
                        ReportSetting.Teste.MoveTo(ContatoreElementoTrovato, ContatoreGruppo);
                        ReportSetting.Code.MoveTo(ContatoreElementoTrovato, ContatoreGruppo);
                        ContatoreElementoTrovato = 0;
                        break;
                    }
                    ContatoreElementoTrovato++;
                }
                ContatoreGruppo++;
            }

            Contatore = 0;

            foreach (var CorpoDocumento in DocumentoCorpoView.DocumentoCorpi)
            {
                CorpiDocumento CorpiDocumento = new CorpiDocumento();
                CorpiDocumento.CorpoColonna = new List<CorpoDocumento>();

                foreach (var Dettaglio in CorpoDocumento.ListaDettaglio)
                {
                    CorpoDocumento corpoDocumento = new CorpoDocumento();
                    CompilaCellaTabella(corpoDocumento, Dettaglio, true);
                    CorpiDocumento.CorpoColonna.Add(corpoDocumento);
                    if (Dettaglio.SelectedTreeViewItem != null) { ReportSetting.AttributiDaEstrarrePerDataSource(Dettaglio.SelectedTreeViewItem.PropertyType, Dettaglio.SelectedTreeViewItem.AttrbutoCodice, Dettaglio.SelectedTreeViewItem.EntityType, Dettaglio.SelectedTreeViewItem.DivisioneCodice, corpoDocumento.Rtf, corpoDocumento.DescrBreve, corpoDocumento.StampaFormula, Dettaglio.SelectedTreeViewItem.AttrbutoCodiceOrigine, Dettaglio.SelectedTreeViewItem.AttributoCodicePath); }
                }
                CorpiDocumento.Size = DocumentoCorpoView.LarghezzaColonne.ElementAt(Contatore).LarghezzaColonna;
                ReportSetting.CorpiDocumento.Add(CorpiDocumento);

                Contatore++;
            }

            Contatore = 0;

            foreach (var FineDocumento in DocumentoCorpoView.DocumentoFine)
            {
                CorpiDocumento CorpiDocumento = new CorpiDocumento();
                CorpiDocumento.CorpoColonna = new List<CorpoDocumento>();

                foreach (var Dettaglio in FineDocumento.ListaDettaglio)
                {
                    CorpoDocumento corpoDocumento = new CorpoDocumento();
                    CompilaCellaTabella(corpoDocumento, Dettaglio, false);
                    CorpiDocumento.CorpoColonna.Add(corpoDocumento);
                    if (Dettaglio.SelectedTreeViewItem != null) { ReportSetting.AttributiDaEstrarrePerDataSource(Dettaglio.SelectedTreeViewItem.PropertyType, Dettaglio.SelectedTreeViewItem.AttrbutoCodice, Dettaglio.SelectedTreeViewItem.EntityType, Dettaglio.SelectedTreeViewItem.DivisioneCodice, false, false, false, Dettaglio.SelectedTreeViewItem.AttrbutoCodiceOrigine, Dettaglio.SelectedTreeViewItem.AttributoCodicePath); }
                }
                CorpiDocumento.Size = DocumentoCorpoView.LarghezzaColonne.ElementAt(Contatore).LarghezzaColonna;
                ReportSetting.FineDocumento.Add(CorpiDocumento);

                Contatore++;
            }

            ReportSetting.GuidSezione = GuidSezione;
            ReportSetting.Sezione = "   " + Sezione.Content;
            ReportSetting.SezioneKey = Sezione.Key;
            ReportSetting.NumeroColonne = NumeroColonne;
            ReportSetting.DescrizioneReport = DescrizionrReport;
            ReportSetting.IsTreeMaster = IsTreeMaster;
            ReportSetting.IsAllFieldRtfFormat = IsAllFieldRtfFormat;

            return ReportSetting;
        }
        private RaggruppamentiDocumento CreateSettingReportEntityRaggruppamentiDocumentoTesteCode(Dettaglio CorpoRaggruppamento, int ContatoreColonnaCoda, StampeData reportSetting)
        {
            RaggruppamentiDocumento RaggruppamentiDocumento = new RaggruppamentiDocumento();
            RaggruppamentiDocumento.Size = DocumentoCorpoView.LarghezzaColonne.ElementAt(ContatoreColonnaCoda).LarghezzaColonna;
            RaggruppamentiDocumento.RaggruppamentiValori = new List<RaggruppamentiValori>();


            foreach (var Dettaglio in CorpoRaggruppamento.ListaDettaglio)
            {
                RaggruppamentiValori RaggruppamentoValori = new RaggruppamentiValori();
                CompilaCellaTabella(RaggruppamentoValori, Dettaglio, true);
                RaggruppamentiDocumento.RaggruppamentiValori.Add(RaggruppamentoValori);
                if (Dettaglio.SelectedTreeViewItem != null) { reportSetting.AttributiDaEstrarrePerDataSource(Dettaglio.SelectedTreeViewItem.PropertyType, Dettaglio.SelectedTreeViewItem.AttrbutoCodice, Dettaglio.SelectedTreeViewItem.EntityType, Dettaglio.SelectedTreeViewItem.DivisioneCodice, RaggruppamentoValori.Rtf, RaggruppamentoValori.DescrBreve, RaggruppamentoValori.StampaFormula, Dettaglio.SelectedTreeViewItem.AttrbutoCodiceOrigine, Dettaglio.SelectedTreeViewItem.AttributoCodicePath); }

            }

            return RaggruppamentiDocumento;
        }
        private void CompilaCellaTabella(CellaTabella CellaTabella, TextBoxItemView Dettaglio, bool CompilaOpzioniStampa)
        {
            CellaTabella.Etichetta = ReplaceEmptyEtichettaWithEmptyValue(Dettaglio.Etichetta);
            if (Dettaglio.SelectedTreeViewItem != null)
            {
                if (Dettaglio.SelectedTreeViewItem.Attrbuto != StampeKeys.LocalizeNessuno)
                    CellaTabella.Attributo = Dettaglio.SelectedTreeViewItem.Attrbuto;
                CellaTabella.PropertyType = Dettaglio.SelectedTreeViewItem.PropertyType;
                CellaTabella.AttributoCodice = Dettaglio.SelectedTreeViewItem.AttrbutoCodice;
                CellaTabella.AttributoCodiceOrigine = Dettaglio.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                CellaTabella.AttributoCodicePath = Dettaglio.SelectedTreeViewItem.AttributoCodicePath;
                CellaTabella.DivisioneCodice = Dettaglio.SelectedTreeViewItem.DivisioneCodice;
                CellaTabella.EntityType = Dettaglio.SelectedTreeViewItem.EntityType;
                CellaTabella.CodiceDigicorp = Dettaglio.SelectedTreeViewItem.CodiceDigicorp;
            }
            CellaTabella.StileCarattere = new ProprietaCarattere();
            if (Dettaglio.FormatCharacterView != null)
            {
                AssegnazioneStile(Dettaglio, CellaTabella);
                CellaTabella.ConcatenaEtichettaEValore = Dettaglio.FormatCharacterView.ConcatenaEtichettaEValore;

                if (CompilaOpzioniStampa)
                {
                    CellaTabella.Nascondi = Dettaglio.FormatCharacterView.Nascondi;
                    CellaTabella.IsNascondiVisible = Visibility.Visible;
                    CellaTabella.Rtf = Dettaglio.FormatCharacterView.Rtf;
                    CellaTabella.DescrBreve = Dettaglio.FormatCharacterView.DescrBreve;
                    CellaTabella.StampaFormula = Dettaglio.FormatCharacterView.StampaFormula;
                    CellaTabella.RiportoPagina = Dettaglio.FormatCharacterView.RiportoPagina;
                    CellaTabella.RiportoRaggruppamento = Dettaglio.FormatCharacterView.RiportoRaggruppamento;
                }
            }
        }
        public void AcceptButton()
        {
            var listaVuoti = ItemsRaggruppamenti.Where(r => string.IsNullOrEmpty(r.TextBoxItemView.AttributeSelected)).ToList();
            foreach (var item in listaVuoti)
            {
                ItemsRaggruppamenti.Remove(item);
            }
            ReportSetting = CreateSettingReportEntity();
        }

        public bool IsReportFilledUp()
        {
            foreach (Dettaglio Colonna in DocumentoCorpoView.DocumentoCorpi)
            {
                foreach (TextBoxItemView Cella in Colonna.ListaDettaglio)
                {
                    if (string.IsNullOrEmpty(Cella.Etichetta) || Cella.AttributeSelected != null)
                    {
                        return true;
                    }
                }
            }

            foreach (Dettaglio Colonna in DocumentoCorpoView.DocumentoFine)
            {
                foreach (TextBoxItemView Cella in Colonna.ListaDettaglio)
                {
                    if (string.IsNullOrEmpty(Cella.Etichetta) || Cella.AttributeSelected != null)
                    {
                        return true;
                    }
                }
            }

            foreach (RaggruppamentiItemsView Cella in ItemsRaggruppamenti)
            {
                if (Cella.TextBoxItemView != null)
                {
                    if (Cella.TextBoxItemView.AttributeSelected != null)
                    {
                        return true;
                    }
                }
            }

            foreach (var GroupSaved in GroupSetting)
            {
                foreach (Dettaglio Colonna in GroupSaved.Value.RaggruppamentoTeste)
                {
                    foreach (TextBoxItemView Cella in Colonna.ListaDettaglio)
                    {
                        if (string.IsNullOrEmpty(Cella.Etichetta) || Cella.AttributeSelected != null)
                        {
                            return true;
                        }
                    }
                }

                foreach (Dettaglio Colonna in GroupSaved.Value.RaggruppamentoCode)
                {
                    foreach (TextBoxItemView Cella in Colonna.ListaDettaglio)
                    {
                        if (string.IsNullOrEmpty(Cella.Etichetta) || Cella.AttributeSelected != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void PreviewButton()
        {
            var RaggruppamentoVuoto = ItemsRaggruppamenti.FirstOrDefault(r => string.IsNullOrEmpty(r.TextBoxItemView.AttributeSelected));
            if (RaggruppamentoVuoto != null)
            {
                MessageBox.Show(LocalizationProvider.GetString("Attenzione!CompilareTuttiIRaggruppamentiPrimaDiProcedere"), LocalizationProvider.GetString("AppName"));
                return;
            }

            FastReport.Preview.PreviewControl previewControl1 = new FastReport.Preview.PreviewControl();
            PreviewWnd win = new PreviewWnd();
            ReportPreviewGenerator ReportGenerator = new ReportPreviewGenerator();

            try
            {
                WindowService.ShowWaitCursor(true);
                string DbName = StampeKeys.ConstDataSourceName + "1";
                //CREATE SETTING ONLY FOR PREVIEW
                StampeData _ReportSetting = new StampeData();
                _ReportSetting = CreateSettingReportEntity();

                if (_ReportSetting.AttributiDaEstrarrePerDatasource == null) { return; }

                //CREATE DATASOURCE
                ReportGenerator.ReportDataSourceGenerator = new ReportDataSourceGenerator();
                ReportSettingViewHelper.EstraiAlberoDaAttributoSelezionato(_ReportSetting.AttributiDaEstrarrePerDatasource);
                ReportGenerator.ReportDataSourceGenerator.AttributiDaEstrarrePerDatasource = _ReportSetting.AttributiDaEstrarrePerDatasource;
                ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasource = _ReportSetting.RaggruppamentiDatasource;
                ReportGenerator.ReportDataSourceGenerator.OrdinamentoCorpo = _ReportSetting.OrdinamentoCorpo;
                if (IsTreeMaster)
                {
                    if (!Sezione.Key.StartsWith(BuiltInCodes.EntityType.Divisione))
                        ReportGenerator.ReportDataSourceGenerator.AttributiDaEstrarrePerDatasource.Add(new AttributiUtilizzati()
                        {
                            EntityType = Sezione.Key,
                            AttributiPerEntityType = new List<string>() { Sezione.Key + StampeKeys.Const_Guid },
                            AttributiCodicePathPerEntityType = new List<string>() { Sezione.Key + StampeKeys.Const_Guid }

                        });
                }

                ReportGenerator.ReportDataSourceGenerator.DataService = DataService;
                ReportGenerator.ReportDataSourceGenerator.IsAllFieldRtfFormat = IsAllFieldRtfFormat;
                ReportGenerator.ReportDataSourceGenerator.CreateGenericDataSource(Sezione.Key, true, MainOperation);
                if (ReportGenerator.ReportDataSourceGenerator.IsDataSourceEmpty)
                {
                    MessageBox.Show(LocalizationProvider.GetString("Attenzione!NonCiSonoDatiDisponibiliPerLElaborazioneDelReport"), LocalizationProvider.GetString("AppName"));
                    WindowService.ShowWaitCursor(false);
                    return;
                }

                //CREATE STRUCTURE
                ReportGenerator.ReportStructureGenerator = new ReportStructureGenerator(_ReportSetting, false);

                DataSet Dataset = ReportGenerator.ReportDataSourceGenerator.RetrieveDataSource(Sezione.Key, DbName);

                //SE HO UN RAGGRUPPAMENTO A NULL ON GLI FACCIO FARE IL GRUPPO PER EVITARE I SOMMANO
                foreach (var RaggruppamentoToRemove in ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasourceToRemove)
                {
                    Raggruppamenti TestaToRemove = ReportGenerator.ReportStructureGenerator.ReportSetting.Teste.Where(t => t.Attributo == RaggruppamentoToRemove.Attributo).FirstOrDefault();
                    Raggruppamenti CodaToRemove = ReportGenerator.ReportStructureGenerator.ReportSetting.Code.Where(t => t.Attributo == RaggruppamentoToRemove.Attributo).FirstOrDefault();
                    ReportGenerator.ReportStructureGenerator.ReportSetting.Teste.Remove(TestaToRemove);
                    ReportGenerator.ReportStructureGenerator.ReportSetting.Code.Remove(CodaToRemove);
                    ReportGenerator.ReportStructureGenerator.ReportSetting.RaggruppamentiDatasource.Remove(RaggruppamentoToRemove);
                }

                ReportGenerator.ReportStructureGenerator.ReportSetting.RaggruppamentiDatasource = ReportGenerator.ReportDataSourceGenerator.RaggruppamentiDatasource;

                ReportGenerator.ReportStructureGenerator.CreateAndAddNewPage(Dataset, DbName, ReportGenerator.ReportDataSourceGenerator.ListParentItem, Sezione.Key, IsTreeMaster, MainOperation, 0, Title.Replace(LocalizationProvider.GetString("Layout") + " ", ""), IsTabellaBordata, false, DataService);

                string FrxFile = ReportGenerator.ReportStructureGenerator.ReportObject.SaveToString();

                WindowService.ShowWaitCursor(false);

                ReportGenerator.ReportDataSourceGenerator = null;
                GC.Collect();

                win = new PreviewWnd();
                win.SourceInitialized += (x, y) => win.HideMinimizeAndMaximizeButtons();
                win.report = ReportGenerator.ReportStructureGenerator.ReportObject;
                win.Init(true);

                //FastReport.Export.XAML.XAMLExport ex = new FastReport.Export.XAML.XAMLExport();
                //ex.HasMultipleFiles = true;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    ex.Export((FastReport.Report)win.report, ms);
                //    foreach (MemoryStream page in ex.GeneratedStreams)
                //    {
                //        string p = "";
                //    }
                //}

                win.ShowDialog();
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }
            finally
            {
                previewControl1 = null;
                win = null;
                if (ReportGenerator.ReportStructureGenerator != null)
                {
                    ReportGenerator.ReportStructureGenerator.ReportObject = null;
                }
                WindowService.ShowWaitCursor(false);
            }

            GC.Collect();
        }

        private ICommand _AddGroupCommand;
        public ICommand AddGroupCommand
        {
            get
            {
                return _AddGroupCommand ?? (_AddGroupCommand = new CommandHandler(param => ExecuteAddGroup(param), CanExecuteAddGroup()));
            }
        }
        private bool CanExecuteAddGroup() { return true; }
        public void ExecuteAddGroup(object param)
        {
            if (VerificaSituazioneRaggruppamentiPrimaDiUtilizzarneFunzioni(true, false) == false) { return; }

            int index = 0;

            if (SelectedItemInGroup != null)
            {
                foreach (var item in ItemsRaggruppamenti)
                {
                    index++;
                    if (item.TextBoxItemView.AttributeSelected == SelectedItemInGroup.TextBoxItemView.AttributeSelected && item.TextBoxItemView.EntitySelected == SelectedItemInGroup.TextBoxItemView.EntitySelected)
                    {
                        break;
                    }
                }
            }

            RaggruppamentiItemsView Raggruppamento = new RaggruppamentiItemsView();
            Raggruppamento.ListAttributes = ReportSettingDataViewHelper.GetAttributeTree();
            Raggruppamento.TextBoxItemView.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
            Raggruppamento.TextBoxItemViewOrdinamento.CloseAllPopUpWhenOneIsOpenHandler += ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler;
            Raggruppamento.ReportSettingDataViewHelper = ReportSettingDataViewHelper;
            Raggruppamento.AssignDatasource();
            ItemsRaggruppamenti.Insert(index, Raggruppamento);

            if (ItemsRaggruppamenti.Count > 1) { IsVisibleButtonForOperation = Visibility.Visible; }
            if (ItemsRaggruppamenti.Count > 0) { IsVisibleDeleteButton = Visibility.Visible; } else { IsVisibleDeleteButton = Visibility.Hidden; }
            if (ItemsRaggruppamenti.Count > 0) { DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Visible; DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Visible; }

            SelectedItemInGroup = Raggruppamento;

            AggiuntiComandiDifferenti();
            RigeneraIndentazioneRaggruppamenti();
        }
        private bool VerificaSituazioneRaggruppamentiPrimaDiUtilizzarneFunzioni(bool IsAddGroup, bool IsDelete)
        {
            if (IsAddGroup)
            {
                if (ItemsRaggruppamenti.Count() > 0)
                {
                    if (ItemsRaggruppamenti.Where(r => String.IsNullOrEmpty(r.TextBoxItemView.AttributeSelected)).FirstOrDefault() != null)
                    {
                        MessageBox.Show(LocalizationProvider.GetString("CompilareIlRaggruppamentoVuotoPrimaDiInserireUnNuovoGruppo"), LocalizationProvider.GetString("AppName"));
                        return false;
                    }
                }
            }

            if (IsDelete)
            {
                if (ItemsRaggruppamenti.Count() > 0)
                {
                    if (!String.IsNullOrEmpty(SelectedItemInGroup.TextBoxItemView.AttributeSelected))
                    {
                        if (ItemsRaggruppamenti.Where(r => String.IsNullOrEmpty(r.TextBoxItemView.AttributeSelected)).FirstOrDefault() != null)
                        {
                            MessageBox.Show(LocalizationProvider.GetString("Attenzione!CompilareIlRaggruppamentoVuotoPrimaDiCancellarneUnAltro"), LocalizationProvider.GetString("AppName"));
                            return false;
                        }
                    }
                }
            }

            if (!IsAddGroup && !IsDelete)
            {
                if (ItemsRaggruppamenti.Count() > 0)
                {
                    if (SelectedItemInGroup != null)
                    {
                        if (ItemsRaggruppamenti.Where(r => String.IsNullOrEmpty(r.TextBoxItemView.AttributeSelected)).FirstOrDefault() != null)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }

        private ICommand _DeleteGroupCommand;
        public ICommand DeleteGroupCommand
        {
            get
            {
                return _DeleteGroupCommand ?? (_DeleteGroupCommand = new CommandHandler(param => ExecuteDeleteGroup(param), CanExecuteDeleteGroup()));
            }
        }

        private bool CanExecuteDeleteGroup() => true;

        bool GroupReferencedDown { get; set; }
        private bool IsGroupReferencedDown()
        {

            if (SelectedItemInGroup == null)
                return false; 

            Int16 IndiceRaggruppamento = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                if (Raggruppamento == SelectedItemInGroup)
                {
                    break;
                }
                IndiceRaggruppamento++;
            }

            var selectedTreeViewItem = ItemsRaggruppamenti[IndiceRaggruppamento].TextBoxItemView.SelectedTreeViewItem;

            if (selectedTreeViewItem == null)//raggruppamento di cui non ancora settato l'attributo
                return false;

            HashSet<string> sottoAttributiRaggrup = new HashSet<string>();
            GetSottoAttributiRaggrup(selectedTreeViewItem, ref sottoAttributiRaggrup);
            //sottoAttributiRaggrup = selectedTreeViewItem.Items?.Select(d => d.EntityType + d.AttributoCodicePath).ToHashSet();


            bool deleteConsentito = true;

            for (int indiceRaggSuccuesivo = IndiceRaggruppamento + 1; indiceRaggSuccuesivo < ItemsRaggruppamenti.Count(); indiceRaggSuccuesivo++)
            {    
                
                if (ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem == null)//raggruppamento di cui non ancora settato l'attributo
                    continue;

                var itemRaggruppamenti = ItemsRaggruppamenti[indiceRaggSuccuesivo];
                var groupKey = itemRaggruppamenti.TextBoxItemView.SelectedTreeViewItem?.GroupKey;

                if (groupKey == null || groupKey == "Nessuno_")
                {
                    MessageBox.Show(LocalizationProvider.GetString("RaggruppatoreSenzaAttributo"), LocalizationProvider.GetString("AppName"));
                    continue;
                }

                var documentoCorpoSuccessivo = GroupSetting[groupKey];


                //var documentoCorpoSuccessivo = GroupSetting[ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem.GroupKey];



                //Testa
                foreach (var RaggruppamentoTesta in documentoCorpoSuccessivo.RaggruppamentoTeste)
                {
                    if (deleteConsentito == false) { break; }
                    foreach (var Dettaglio in RaggruppamentoTesta.ListaDettaglio)//righe di testa
                    {
                        if (deleteConsentito)
                        {
                            string attCodice = ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice;
                            string entType = ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem.EntityType;
                            if (Dettaglio.SelectedTreeViewItem != null)
                            {
                                string dettaglioAttCodicePath = Dettaglio.SelectedTreeViewItem.AttributoCodicePath;

                                if (sottoAttributiRaggrup.Contains(Dettaglio.SelectedTreeViewItem.EntityType + dettaglioAttCodicePath))
                                    deleteConsentito = false;
                            }
                        }
                        else
                            break;
                    }
                    if (!deleteConsentito)
                        break;
                }

                if (!deleteConsentito)
                    break;

                //coda
                foreach (var RaggruppamentoCoda in documentoCorpoSuccessivo.RaggruppamentoCode)
                {
                    if (!deleteConsentito) { break; }
                    foreach (var Dettaglio in RaggruppamentoCoda.ListaDettaglio)//righe di coda
                    {
                        if (deleteConsentito)
                        {
                            string attCodice = ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice;
                            string entType = ItemsRaggruppamenti[indiceRaggSuccuesivo].TextBoxItemView.SelectedTreeViewItem.EntityType;
                            if (Dettaglio.SelectedTreeViewItem != null)
                            {
                                string dettaglioAttCodicePath = Dettaglio.SelectedTreeViewItem.AttributoCodicePath;

                                if (sottoAttributiRaggrup.Contains(Dettaglio.SelectedTreeViewItem.EntityType + dettaglioAttCodicePath))
                                    deleteConsentito = false;
                            }
                        }
                        else
                            break;
                    }
                    if (!deleteConsentito)
                        break;
                }


                if (!deleteConsentito)
                    break;
            }

            return !deleteConsentito;
        }

        /// <summary>
        /// Funzione ricorsiva
        /// </summary>
        /// <param name="sottoAttributiRaggrup"></param>
        private void GetSottoAttributiRaggrup(TreeviewItem selectedTreeViewItem, ref HashSet<string> sottoAttributiRaggrup)
        {
            sottoAttributiRaggrup.Add(selectedTreeViewItem.EntityType + selectedTreeViewItem.AttributoCodicePath);

            foreach (var item in selectedTreeViewItem.Items)
                GetSottoAttributiRaggrup(item, ref sottoAttributiRaggrup);
        }
        public void ExecuteDeleteGroup(object param)
        {
            if (SelectedItemInGroup == null)
                return;

            if (SelectedItemInGroup.IsGroupReferencedDown)
            {
                MessageBox.Show(LocalizationProvider.GetString("GruppoRiferitoSotto"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (ItemsRaggruppamenti != null)
            {
                foreach (var Raggruppamento in ItemsRaggruppamenti)
                {
                    if (Raggruppamento.TextBoxItemView?.SelectedTreeViewItem == null)
                    {
                        MessageBox.Show(LocalizationProvider.GetString("CompilareIlRaggruppamentoVuotoPrimaDiEliminareIlGruppoSelezionato"), LocalizationProvider.GetString("AppName"));
                        return;
                    }
                }
            }

            RaggruppamentiItemsView ForceRaggruppamentiItemsSelected = null;

            if (SelectedItemInGroup != null)
            {
                var SelectedAttibute = SelectedItemInGroup;
                int IndiceRaggruppamentoDaForzare = 0;

                foreach (var Raggruppamento in ItemsRaggruppamenti)
                {
                    if (Raggruppamento.TextBoxItemView.AttributeSelected == SelectedItemInGroup.TextBoxItemView.AttributeSelected && Raggruppamento.TextBoxItemView.EntitySelected == SelectedItemInGroup.TextBoxItemView.EntitySelected)
                    {
                        break;
                    }
                    ForceRaggruppamentiItemsSelected = Raggruppamento;
                    IndiceRaggruppamentoDaForzare++;
                }

                DocumentoCorpoView.RaggruppamentoSelected = null;

                if (ForceRaggruppamentiItemsSelected != null)
                {
                    SelectedItemInGroup = ForceRaggruppamentiItemsSelected;
                }
                else
                {
                    if (ItemsRaggruppamenti.Count() > 1)
                    {
                        SelectedItemInGroup = ItemsRaggruppamenti.ElementAt(IndiceRaggruppamentoDaForzare + 1);
                    }
                    else
                    {
                        DocumentoCorpoView.RaggruppamentoTeste.Clear();
                        DocumentoCorpoView.ListaComandiTesta.Clear();
                        DocumentoCorpoView.RaggruppamentoCode.Clear();
                        DocumentoCorpoView.ListaComandiCoda.Clear();
                    }
                }

                ItemsRaggruppamenti.Remove(SelectedAttibute);

                DocumentoCorpoView.IsEditableDocumento = VerificaSituazioneRaggruppamentiPrimaDiUtilizzarneFunzioni(false, false);

                if (!string.IsNullOrEmpty(SelectedAttibute.TextBoxItemView.AttributeSelected))
                {
                    if (GroupSetting.ContainsKey(SelectedAttibute.TextBoxItemView.SelectedTreeViewItem.GroupKey))
                    {
                        GroupSetting.Remove(SelectedAttibute.TextBoxItemView.SelectedTreeViewItem.GroupKey);
                    }
                }
            }

            if (ItemsRaggruppamenti.Count == 1) { IsVisibleButtonForOperation = Visibility.Hidden; }
            if (ItemsRaggruppamenti.Count > 0) { IsVisibleDeleteButton = Visibility.Visible; } else { IsVisibleDeleteButton = Visibility.Hidden; }
            if (ItemsRaggruppamenti.Count == 0) { DocumentoCorpoView.IsRaggruppamentiInDocumentTestaVisible = Visibility.Hidden; DocumentoCorpoView.IsRaggruppamentiInDocumentCodaVisible = Visibility.Hidden; }
            RigeneraIndentazioneRaggruppamenti();

            short IndiceRaggruppamentoDaForzareLocal = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                RigenerateAttributeListForEachCellInHeaderAndFooterDocument(IndiceRaggruppamentoDaForzareLocal);
                IndiceRaggruppamentoDaForzareLocal++;
            }
        }
        private ICommand _UpGroupCommand;
        public ICommand UpGroupCommand
        {
            get
            {
                return _UpGroupCommand ?? (_UpGroupCommand = new CommandHandler(param => ExecuteUpGroup(param), CanExecuteUpGroup()));
            }
        }
        private bool CanExecuteUpGroup() { return true; }
        public void ExecuteUpGroup(object param)
        {
            // At least two items  for managing up and down command
            if (SelectedItemInGroup == null || ItemsRaggruppamenti.Count == 1) { return; }

            if (String.IsNullOrEmpty(SelectedItemInGroup.TextBoxItemView.AttributeSelected))
            {
                MessageBox.Show(LocalizationProvider.GetString("SelezionareLAttributoPrimaDiSpostareLaRigaDiRaggruppamento"), LocalizationProvider.GetString("AppName"));
                return;
            }

            Int16 IndiceRaggruppamento = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                if (Raggruppamento == SelectedItemInGroup)
                {
                    break;
                }
                IndiceRaggruppamento++;
            }

            if (IndiceRaggruppamento == 0) { return; }

            HashSet<string> sottoAttributiRaggrupPrecedente = new HashSet<string>();
            GetSottoAttributiRaggrup(ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem, ref sottoAttributiRaggrupPrecedente);
            //sottoAttributiRaggrupPrecedente = ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem.Items?.Select(d => d.AttrbutoCodice + d.EntityType).ToList();

            bool SpostamentoConsentito = true;

            foreach (var RaggruppamentoTesta in DocumentoCorpoView.RaggruppamentoTeste)
            {
                if (SpostamentoConsentito == false) { break; }
                foreach (var Dettaglio in RaggruppamentoTesta.ListaDettaglio)
                {
                    SpostamentoConsentito = VerificaEsistenzaAttributo(Dettaglio, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem.EntityType, sottoAttributiRaggrupPrecedente);
                    if (SpostamentoConsentito == false) { break; }
                }
            }

            if (SpostamentoConsentito)
            {
                foreach (var RaggruppamentoTesta in DocumentoCorpoView.RaggruppamentoCode)
                {
                    if (SpostamentoConsentito == false) { break; }
                    foreach (var Dettaglio in RaggruppamentoTesta.ListaDettaglio)
                    {
                        SpostamentoConsentito = VerificaEsistenzaAttributo(Dettaglio, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento - 1).TextBoxItemView.SelectedTreeViewItem.EntityType, sottoAttributiRaggrupPrecedente);
                        if (SpostamentoConsentito == false) { break; }
                    }
                }
            }

            if (SpostamentoConsentito)
            {
                ItemsRaggruppamenti.Move(IndiceRaggruppamento, IndiceRaggruppamento - 1);

                RigenerateAttributeListForEachCellInHeaderAndFooterDocument(IndiceRaggruppamento);
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("GruppoRiferitoSotto"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            RigeneraIndentazioneRaggruppamenti();
        }
        private bool VerificaEsistenzaAttributo(TextBoxItemView dettaglio, string AttributoCodice, string EntityType, HashSet<string> sottoAttributiRaggrupPrecedente)
        {
            //if (sottoAttributiRaggrupPrecedente.Count() == 0)
            //{
            //    if (dettaglio.SelectedTreeViewItem != null)
            //    {
            //        if (AttributoCodice == dettaglio.SelectedTreeViewItem.AttrbutoCodice && EntityType == dettaglio.SelectedTreeViewItem.EntityType)
            //        {
            //            //MessageBox.Show(LocalizationProvider.GetString("ImpossibileSpostareIlRaggruppamento"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //            return false;
            //        }
            //    }
            //}
            //else
            //{
                if (dettaglio.SelectedTreeViewItem != null)
                {
                    if (sottoAttributiRaggrupPrecedente.Contains(dettaglio.SelectedTreeViewItem.EntityType + dettaglio.SelectedTreeViewItem.AttributoCodicePath))
                    {
                        //MessageBox.Show(LocalizationProvider.GetString("ImpossibileSpostareIlRaggruppamento"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return false;
                    }
                }
            //}
            return true;
        }

        private ICommand _DownGroupCommand;
        public ICommand DownGroupCommand
        {
            get
            {
                return _DownGroupCommand ?? (_DownGroupCommand = new CommandHandler(param => ExecuteDownGroup(param), CanExecuteDownGroup()));
            }
        }
        //public bool SostituzioneAttributoRaggruppamento { get; set; }
        public bool IsTreeMaster { get; internal set; }
        private bool CanExecuteDownGroup() { return true; }
        public void ExecuteDownGroup(object param)
        {
            // At least two items  for managing up and down command
            if (SelectedItemInGroup == null || ItemsRaggruppamenti.Count == 1) { return; }

            Int16 IndiceRaggruppamento = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                if (Raggruppamento == SelectedItemInGroup)
                {
                    break;
                }
                IndiceRaggruppamento++;
            }


            if (IndiceRaggruppamento + 1 > ItemsRaggruppamenti.Count() - 1) { return; }

            if (String.IsNullOrEmpty(ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.AttributeSelected))
            {
                MessageBox.Show(LocalizationProvider.GetString("Attenzione!SelezionareLattributoDellaRigaSuccessivaPrimaDiSpostareLaRigaDiRaggruppamento"), LocalizationProvider.GetString("AppName"));
                return;
            }

            HashSet<string> sottoAttributiRaggrup = new HashSet<string>();
            GetSottoAttributiRaggrup(ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.SelectedTreeViewItem, ref sottoAttributiRaggrup);
            //sottoAttributiRaggrup = ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.SelectedTreeViewItem.Items?.Select(d => d.AttrbutoCodice + d.EntityType).ToList();

            bool SpostamentoConsentito = true;

            var itemRaggruppamenti = ItemsRaggruppamenti[IndiceRaggruppamento + 1];
            var groupKey = itemRaggruppamenti.TextBoxItemView.SelectedTreeViewItem?.GroupKey;

            if (groupKey  == null || groupKey == "Nessuno_")
            {
                MessageBox.Show(LocalizationProvider.GetString("RaggruppatoreSenzaAttributo"), LocalizationProvider.GetString("AppName"));
                return;
            }

            var documentoCorpoSuccessivo = GroupSetting[groupKey];

            //Testa
            foreach (var RaggruppamentoTesta in documentoCorpoSuccessivo.RaggruppamentoTeste)
            {
                if (SpostamentoConsentito == false) { break; }
                foreach (var Dettaglio in RaggruppamentoTesta.ListaDettaglio)//righe di testa
                {
                    SpostamentoConsentito = VerificaEsistenzaAttributo(Dettaglio, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento + 1).TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento + 1).TextBoxItemView.SelectedTreeViewItem.EntityType, sottoAttributiRaggrup);
                    if (SpostamentoConsentito == false) { break; }
                }

            }

            //coda
            if (SpostamentoConsentito)
            {
                foreach (var RaggruppamentoCoda in documentoCorpoSuccessivo.RaggruppamentoCode)
                {
                    if (SpostamentoConsentito == false) { break; }
                    foreach (var Dettaglio in RaggruppamentoCoda.ListaDettaglio)//righe di coda
                    {
                        SpostamentoConsentito = VerificaEsistenzaAttributo(Dettaglio, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento + 1).TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice, ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento + 1).TextBoxItemView.SelectedTreeViewItem.EntityType, sottoAttributiRaggrup);
                        if (SpostamentoConsentito == false) { break; }
                    }

                }
            }

            if (SpostamentoConsentito)
            {
                ItemsRaggruppamenti.Move(IndiceRaggruppamento, IndiceRaggruppamento + 1);

                RigenerateAttributeListForEachCellInHeaderAndFooterDocument(IndiceRaggruppamento);
            }
            else
            {
                MessageBox.Show(LocalizationProvider.GetString("GruppoRiferitoSotto"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            RigeneraIndentazioneRaggruppamenti();
        }
        private void RigenerateAttributeListForEachCellInHeaderAndFooterDocument(short IndiceRaggruppamento)
        {
            //Rigenero lista attributi causa spostamento raggruppamenti
            foreach (var RaggruppamentoTesta in DocumentoCorpoView.RaggruppamentoTeste)
                InizializzazioneTesteCodeListaAttributi(RaggruppamentoTesta);

            foreach (var RaggruppamentoCode in DocumentoCorpoView.RaggruppamentoCode)
                InizializzazioneTesteCodeListaAttributi(RaggruppamentoCode);

            foreach (var RaggruppamentoTesta in GroupSetting[ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.SelectedTreeViewItem.GroupKey].RaggruppamentoTeste)
                InizializzazioneTesteCodeListaAttributiGruppiInMemoria(RaggruppamentoTesta, IndiceRaggruppamento);

            foreach (var RaggruppamentoCoda in GroupSetting[ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.SelectedTreeViewItem.GroupKey].RaggruppamentoCode)
                InizializzazioneTesteCodeListaAttributiGruppiInMemoria(RaggruppamentoCoda, IndiceRaggruppamento);
        }
        private void InizializzazioneTesteCodeListaAttributi(Dettaglio Raggruppamento)
        {
            foreach (var Dettaglio in Raggruppamento.ListaDettaglio)
            {
                Dettaglio.ListaComboBox.Clear();
                Dettaglio.ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(SelectedItemInGroup.TextBoxItemView.SelectedTreeViewItem);
            }
        }
        private void InizializzazioneTesteCodeListaAttributiGruppiInMemoria(Dettaglio Raggruppamento, short IndiceRaggruppamento)
        {
            foreach (var Dettaglio in Raggruppamento.ListaDettaglio)
            {
                Dettaglio.ListaComboBox.Clear();
                Dettaglio.ListaComboBox = ReportSettingDataViewHelper.FilterAttrivbuteListForGroups(ItemsRaggruppamenti.ElementAt(IndiceRaggruppamento).TextBoxItemView.SelectedTreeViewItem);
            }
        }
        private void Raggruppamento_ForceCloseOfPopUps(object sender, EventArgs e) { ForceCloseOfPopUps(); }
        private void DocumentoCorpoView_DocumentoForceCloseOfPopUps(object sender, EventArgs e) { ForceCloseOfPopUps(); }
        private void RigeneraIndentazioneRaggruppamenti()
        {
            int indiceRaggruppamento = 0;
            foreach (var Raggruppamento in ItemsRaggruppamenti)
            {
                Raggruppamento.IndentazioneUi = indiceRaggruppamento * 10;
                indiceRaggruppamento++;
            }
        }
        public void ForceCloseOfPopUps()
        {
            if (DocumentoCorpoView != null)
            {
                if (DocumentoCorpoView.DocumentoCorpi != null)
                {
                    foreach (var RaggruppamentoTesta in DocumentoCorpoView.RaggruppamentoTeste) { foreach (var Dettaglio in RaggruppamentoTesta.ListaDettaglio) { Dettaglio.PopUpIsOpen = false; } }
                    foreach (var DocumentoCorpi in DocumentoCorpoView.DocumentoCorpi) { foreach (var Dettaglio in DocumentoCorpi.ListaDettaglio) { Dettaglio.PopUpIsOpen = false; } }
                    foreach (var DocumentoFine in DocumentoCorpoView.DocumentoFine) { foreach (var Dettaglio in DocumentoFine.ListaDettaglio) { Dettaglio.PopUpIsOpen = false; } }
                    foreach (var RaggruppamentoCode in DocumentoCorpoView.RaggruppamentoCode) { foreach (var Dettaglio in RaggruppamentoCode.ListaDettaglio) { Dettaglio.PopUpIsOpen = false; } }
                    foreach (var Raggruppamento in ItemsRaggruppamenti) { Raggruppamento.TextBoxItemView.PopUpIsOpen = false; }
                    foreach (var Raggruppamento in ItemsRaggruppamenti) { Raggruppamento.TextBoxItemViewOrdinamento.PopUpIsOpen = false; }
                    OrdinamentoView.TextBoxItemViewOrdinamento.PopUpIsOpen = false;
                }
            }
        }
        public void ReportWizardSettingDataView_CloseAllPopUpWhenOneIsOpenHandler(object sender, ClosePopUpEventArgs e)
        {
            foreach (var RaggruppamentoTesta in DocumentoCorpoView.RaggruppamentoTeste) { ClosePopUp(RaggruppamentoTesta.ListaDettaglio, e.Identificativo); }
            foreach (var DocumentoCorpi in DocumentoCorpoView.DocumentoCorpi) { ClosePopUp(DocumentoCorpi.ListaDettaglio, e.Identificativo); }
            foreach (var DocumentoFine in DocumentoCorpoView.DocumentoFine) { ClosePopUp(DocumentoFine.ListaDettaglio, e.Identificativo); }
            foreach (var RaggruppamentoCode in DocumentoCorpoView.RaggruppamentoCode) { ClosePopUp(RaggruppamentoCode.ListaDettaglio, e.Identificativo); }
            foreach (var Raggruppamento in ItemsRaggruppamenti) { if (e.Identificativo != Raggruppamento.TextBoxItemView.GuidIdentificativo.ToString()) { Raggruppamento.TextBoxItemView.PopUpIsOpen = false; } }
            foreach (var Raggruppamento in ItemsRaggruppamenti) { if (e.Identificativo != Raggruppamento.TextBoxItemViewOrdinamento.GuidIdentificativo.ToString()) { Raggruppamento.TextBoxItemViewOrdinamento.PopUpIsOpen = false; } }
        }
        private void ClosePopUp(ObservableCollection<TextBoxItemView> listaDettaglio, string identificativo)
        {
            foreach (var Dettaglio in listaDettaglio)
            {
                if (string.IsNullOrEmpty(identificativo)) { Dettaglio.PopUpIsOpen = false; }
                else { if (identificativo != Dettaglio.GuidIdentificativo.ToString()) { Dettaglio.PopUpIsOpen = false; } }
            }
        }
        private void ReportWizardSettingDataView_UpdateCalcoloTotaleLarghezzaColonna(object sender, EventArgs e) { UpdateCalcoloTotaleLarghezzaColonnaFunction(); }
        public ObservableCollection<TreeviewItem> GeneraListaAttributiFineDocumento()
        {
            return new ObservableCollection<TreeviewItem>()
            {
                new TreeviewItem(){Attrbuto = StampeKeys.LocalizeNessuno, AttrbutoOrigine = StampeKeys.LocalizeNessuno, CodiceDigicorp = StampeKeys.ConstNessuno},
                new TreeviewItem(){Attrbuto = StampeKeys.LocalizeSommaWizard, AttrbutoOrigine = StampeKeys.LocalizeSommaWizard, CodiceDigicorp = StampeKeys.ConstSommaWizard},
                new TreeviewItem(){Attrbuto = StampeKeys.LocalizeContaWizard, AttrbutoOrigine = StampeKeys.LocalizeContaWizard, CodiceDigicorp = StampeKeys.ConstContaWizard},
            };
        }
        private void UpdateCalcoloTotaleLarghezzaColonnaFunction()
        {
            decimal SommaLarghezzaColonne = DocumentoCorpoView.LarghezzaColonne.Sum(y => y.LarghezzaColonna);
            DocumentoCorpoView.TotaleLarghezzaColonne = "∑ " + SommaLarghezzaColonne.ToString() + " cm";
            DocumentoCorpoView.TotaleLarghezzaColonne = DocumentoCorpoView.TotaleLarghezzaColonne.Replace(",", ".");
        }
        private string ReplaceEmptyEtichettaWithEmptyValue(string Etichetta)
        {
            if (Etichetta == StampeKeys.LocalizeEtichettaWizard)
                return null;
            else
            {
                if (string.IsNullOrEmpty(Etichetta))
                    return null;
                else
                    return Etichetta;
            }
        }
        public void ReportWizardSettingDataView_RigeneraListaAttributiPerFunzioni(object sender, EventArgs e) { RigeneraListaAttributiPerFunzioni(); }
        private bool Active = false;
        public void RigeneraListaAttributiPerFunzioni()
        {
            //VIENE RICHIAMATA PIU VOLTE MOTIVO PER CUI HO MESSO BOOLEANO PER L'ESECUZIONE ONE TIME
            if (!Active)
            {
                Active = true;
                Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonnaInDocumento = GeneraDictionaryAttributiRealiContabilitaPerColonna();

                SvuotaSecondoLivelloFunzioni(DocumentoCorpoView.RaggruppamentoTeste);
                SvuotaSecondoLivelloFunzioni(DocumentoCorpoView.RaggruppamentoCode);
                SvuotaSecondoLivelloFunzioni(DocumentoCorpoView.DocumentoFine);

                AggiuntaSecondoLivello(DocumentoCorpoView.RaggruppamentoCode, ListaAttributiPerColonnaInDocumento);
                AggiuntaSecondoLivello(DocumentoCorpoView.RaggruppamentoTeste, ListaAttributiPerColonnaInDocumento);
                AggiuntaSecondoLivello(DocumentoCorpoView.DocumentoFine, ListaAttributiPerColonnaInDocumento);
                Active = false;
            }
        }
        public Dictionary<int, List<TextBoxItemView>> GeneraDictionaryAttributiRealiContabilitaPerColonna()
        {
            Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonnaERigaInDocumento = new Dictionary<int, List<TextBoxItemView>>();

            int ContatoreColonna = 0;
            int ContatoreRiga = 0;
            foreach (Dettaglio Colonna in DocumentoCorpoView.DocumentoCorpi)
            {
                List<TextBoxItemView> ListaAttributiPerColonnaLocal = new List<TextBoxItemView>();
                foreach (var TextBoxItemView in Colonna.ListaDettaglio)
                {
                    if (!string.IsNullOrEmpty(TextBoxItemView.AttributeSelected))
                    {
                        if (TextBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Contabilita || TextBoxItemView.SelectedTreeViewItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Reale)
                        {
                            TextBoxItemView textBoxItemView = new TextBoxItemView();
                            textBoxItemView.AttributeSelected = TextBoxItemView.AttributeSelected;
                            textBoxItemView.EntitySelected = TextBoxItemView.EntitySelected;
                            textBoxItemView.SelectedTreeViewItem = new TreeviewItem();
                            textBoxItemView.SelectedTreeViewItem = TextBoxItemView.SelectedTreeViewItem;
                            ListaAttributiPerColonnaLocal.Add(TextBoxItemView);
                        }
                    }
                    ContatoreRiga++;
                }
                if (ListaAttributiPerColonnaLocal.Count() > 0)
                    ListaAttributiPerColonnaERigaInDocumento[ContatoreColonna] = ListaAttributiPerColonnaLocal;

                ContatoreRiga = 0;
                ContatoreColonna++;
            }

            return ListaAttributiPerColonnaERigaInDocumento;
        }
        private void SvuotaSecondoLivelloFunzioni(ObservableCollection<Dettaglio> Colonne)
        {
            foreach (Dettaglio Colonna in Colonne)
                foreach (var TextBoxItemView in Colonna.ListaDettaglio)
                    foreach (var ItemComboBox in TextBoxItemView.ListaComboBox)
                        if (ItemComboBox.Attrbuto == LocalizationProvider.GetString("Funzione"))
                            foreach (var Item in ItemComboBox.Items)
                                Item.Items.Clear();
                        else
                            if (ItemComboBox.CodiceDigicorp == StampeKeys.ConstSommaWizard || ItemComboBox.CodiceDigicorp == StampeKeys.ConstContaWizard)
                            ItemComboBox.Items.Clear();
        }
        private void AggiuntaSecondoLivello(ObservableCollection<Dettaglio> Colonne, Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonna)
        {
            int ContatoreColonna = 0;
            int ContatoreRiga = 0;
            foreach (Dettaglio Colonna in Colonne)
            {
                foreach (var TextBoxItemView in Colonna.ListaDettaglio)
                {
                    AggiuntaSecondoLivelloAFunzioni(TextBoxItemView.ListaComboBox, ListaAttributiPerColonna, ContatoreColonna);

                    if (ListaAttributiPerColonna.ContainsKey(ContatoreColonna))
                    {
                        if (TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaWizard || TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstContaWizard || TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard)
                        {
                            if (ListaAttributiPerColonna[ContatoreColonna].Where(a => a.SelectedTreeViewItem.AttrbutoCodice == TextBoxItemView.SelectedTreeViewItem?.AttrbutoCodice && a.SelectedTreeViewItem.EntityType == TextBoxItemView.SelectedTreeViewItem?.EntityType).FirstOrDefault() == null)
                            {
                                TextBoxItemView.SelectedTreeViewItem = null;
                                TextBoxItemView.AttributeSelected = null;
                            }
                            else
                            {
                                if (TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaWizard && !string.IsNullOrEmpty(TextBoxItemView.SelectedTreeViewItem?.Attrbuto))
                                    TextBoxItemView.AttributeSelected = "S(" + TextBoxItemView.SelectedTreeViewItem.Attrbuto + ")";
                                if (TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstContaWizard && !string.IsNullOrEmpty(TextBoxItemView.SelectedTreeViewItem?.Attrbuto))
                                    TextBoxItemView.AttributeSelected = "C(" + TextBoxItemView.SelectedTreeViewItem.Attrbuto + ")";
                                if (TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard && !string.IsNullOrEmpty(TextBoxItemView.SelectedTreeViewItem?.Attrbuto))
                                    TextBoxItemView.AttributeSelected = "ST(" + TextBoxItemView.SelectedTreeViewItem.Attrbuto + ")";
                                //FORZO SELEZIONE E APERTURA ALBERONEL PUNTO CORRETTO
                                foreach (var Funzione in TextBoxItemView.ListaComboBox.LastOrDefault().Items)
                                {
                                    if (Funzione.CodiceDigicorp == TextBoxItemView.SelectedTreeViewItem.CodiceDigicorp)
                                    {
                                        foreach (var Item in Funzione.Items)
                                        {
                                            if (Item.EntityType == TextBoxItemView.SelectedTreeViewItem.EntityType && Item.AttrbutoCodice == TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice)
                                            {
                                                Funzione.IsExpanded = true;
                                                Item.IsSelected = true;
                                            }
                                            else
                                            {
                                                Funzione.IsExpanded = false;
                                                Item.IsSelected = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaWizard ||
                        TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstContaWizard ||
                        TextBoxItemView.SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard) && !ListaAttributiPerColonna.ContainsKey(ContatoreColonna))
                        {
                            TextBoxItemView.SelectedTreeViewItem = null;
                            TextBoxItemView.AttributeSelected = null;
                        }
                    }

                    ContatoreRiga++;
                }
                ContatoreRiga = 0;
                ContatoreColonna++;
            }
        }
        public void AggiuntaSecondoLivelloAFunzioni(ObservableCollection<TreeviewItem> ListaAlbero, Dictionary<int, List<TextBoxItemView>> ListaAttributiPerColonna, int ContatoreColonna)
        {
            foreach (var ItemComboBox in ListaAlbero)
            {
                if (ItemComboBox.Attrbuto == LocalizationProvider.GetString("Funzione"))
                {
                    foreach (var Item in ItemComboBox.Items)
                    {
                        if (ListaAttributiPerColonna.ContainsKey(ContatoreColonna))
                        {
                            foreach (var AttributoPerColonna in ListaAttributiPerColonna[ContatoreColonna])
                            {
                                TreeviewItem TreeViewItem = new TreeviewItem();
                                TreeViewItem.Attrbuto = AttributoPerColonna.SelectedTreeViewItem.Attrbuto;
                                TreeViewItem.AttrbutoCodice = AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodice;
                                TreeViewItem.AttrbutoCodiceOrigine = AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                                TreeViewItem.AttributoCodicePath = AttributoPerColonna.SelectedTreeViewItem.AttributoCodicePath;
                                TreeViewItem.AttrbutoDestinazione = AttributoPerColonna.SelectedTreeViewItem.AttrbutoDestinazione;
                                TreeViewItem.AttrbutoOrigine = AttributoPerColonna.SelectedTreeViewItem.AttrbutoOrigine;
                                TreeViewItem.CodiceDigicorp = Item.CodiceDigicorp;
                                TreeViewItem.DivisioneCodice = AttributoPerColonna.SelectedTreeViewItem.DivisioneCodice;
                                TreeViewItem.EntityType = AttributoPerColonna.SelectedTreeViewItem.EntityType;
                                TreeViewItem.PropertyType = AttributoPerColonna.SelectedTreeViewItem.PropertyType;
                                TreeViewItem.Padre = Item.Attrbuto;
                                TreeViewItem.PathAttribute = TreeViewItem.Padre + " > " + TreeViewItem.Attrbuto;
                                if (Item.Items.Where(a => a.EntityType == AttributoPerColonna.SelectedTreeViewItem.EntityType
                                && a.AttrbutoCodice == AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodice && a.Attrbuto == AttributoPerColonna.SelectedTreeViewItem.Attrbuto).FirstOrDefault() == null)
                                {
                                    Item.Items.Add(TreeViewItem);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ItemComboBox.CodiceDigicorp == StampeKeys.ConstSommaWizard || ItemComboBox.CodiceDigicorp == StampeKeys.ConstContaWizard || ItemComboBox.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard)
                    {
                        if (ListaAttributiPerColonna.ContainsKey(ContatoreColonna))
                        {
                            foreach (var AttributoPerColonna in ListaAttributiPerColonna[ContatoreColonna])
                            {
                                TreeviewItem TreeViewItem = new TreeviewItem();
                                TreeViewItem.Attrbuto = AttributoPerColonna.SelectedTreeViewItem.Attrbuto;
                                TreeViewItem.AttrbutoCodice = AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodice;
                                TreeViewItem.AttrbutoCodiceOrigine = AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodiceOrigine;
                                TreeViewItem.AttributoCodicePath = AttributoPerColonna.SelectedTreeViewItem.AttributoCodicePath;
                                TreeViewItem.AttrbutoDestinazione = AttributoPerColonna.SelectedTreeViewItem.AttrbutoDestinazione;
                                TreeViewItem.AttrbutoOrigine = AttributoPerColonna.SelectedTreeViewItem.AttrbutoOrigine;
                                TreeViewItem.CodiceDigicorp = ItemComboBox.CodiceDigicorp;
                                TreeViewItem.DivisioneCodice = AttributoPerColonna.SelectedTreeViewItem.DivisioneCodice;
                                TreeViewItem.EntityType = AttributoPerColonna.SelectedTreeViewItem.EntityType;
                                TreeViewItem.PropertyType = AttributoPerColonna.SelectedTreeViewItem.PropertyType;
                                TreeViewItem.Padre = ItemComboBox.Attrbuto;
                                TreeViewItem.PathAttribute = TreeViewItem.Padre + " > " + TreeViewItem.Attrbuto;
                                if (ItemComboBox.Items.Where(a => a.EntityType == AttributoPerColonna.SelectedTreeViewItem.EntityType
                                && a.AttrbutoCodice == AttributoPerColonna.SelectedTreeViewItem.AttrbutoCodice && a.Attrbuto == AttributoPerColonna.SelectedTreeViewItem.Attrbuto).FirstOrDefault() == null)
                                {
                                    ItemComboBox.Items.Add(TreeViewItem);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public class CommandHandler : ICommand
    {
        private Action<object> _action;
        private bool _canExecute;
        public CommandHandler(Action<object> action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }

}
