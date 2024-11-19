using CommonResources;
using CommonResources.Controls;
using Commons;
using DatiGeneraliWpf;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;

namespace StampeWpf.Wizard
{
    public class TextBoxItemView : TextBoxItem
    {
        public event EventHandler<ClosePopUpEventArgs> CloseAllPopUpWhenOneIsOpenHandler;
        public event EventHandler<EventArgs> RigeneraListaAttributiPerFunzioni;

        private bool _PopUpIsOpen;
        public bool PopUpIsOpen
        {
            get
            {
                return _PopUpIsOpen;
            }
            set
            {
                if (SetProperty(ref _PopUpIsOpen, value))
                {
                    _PopUpIsOpen = value;
                    if (_PopUpIsOpen)
                    {
                        ClosePopUp(false);
                    }
                }
            }
        }

        public TextBoxItemView()
        {
            ListaComboBox = new ObservableCollection<TreeviewItem>();
            GuidIdentificativo = Guid.NewGuid();
            Etichetta = StampeKeys.LocalizeEtichettaWizard;
            IsEtichettaVisible = Visibility.Visible;
            IsStyleCommandVisible = Visibility.Visible;
            MinWidth = 80;
            IsEtichettaEnable = true;
            HideAttributeColor = System.Windows.Media.Brushes.Black;
        }

        public void Init(bool IsViewBlocked = true)
        {
            CreateParamsWindow(true, IsViewBlocked);
        }

        private void CreateParamsWindow(bool FirstInitilization, bool IsViewBlocked)
        {
            FormatCharacterView.DataService = DataService;
            FormatCharacterView.TextBoxItemView = this;
            if (FormatCharacterView.ListStiliConPropieta == null) { FormatCharacterView.SettaStiliProgetto(); }

            FormatCharacterWnd.DataContext = FormatCharacterView;
            FormatCharacterView.IsModificatoVisible = Visibility.Collapsed;
            FormatCharacterView.IsViewBlocked = IsViewBlocked;


            if (FirstInitilization)
            {
                switch (Origine)
                {
                    case 0://FromIntestazioniColonna
                        FormatCharacterView.SettaStileProgetto("Heading 4");
                        break;
                    case 1://FromHeaderFooterGroup
                        FormatCharacterView.SettaStileProgetto("Heading 5");
                        break;
                    case 2://FromCorpo
                        FormatCharacterView.SettaStileProgetto("Normal");
                        break;
                    case 3://FromIntestazioniDocumento
                        FormatCharacterView.SettaStileProgetto("Normal");
                        break;
                    case 4://PiePaginaDocumento
                        FormatCharacterView.SettaStileProgetto("Normal");
                        break;
                    case 5://FineDocumento
                        FormatCharacterView.SettaStileProgetto("Normal");
                        break;
                }
            }

            StilePrecedente = new FormatCharacterView();
            StilePrecedente.DataService = DataService;
            StilePrecedente.SettaStiliProgetto();
            if (FormatCharacterView.ColorBackground != null) { StilePrecedente.ColorBackground = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.ColorBackground.HexValue).FirstOrDefault(); }
            if (FormatCharacterView.ColorCharacther != null) { StilePrecedente.ColorCharacther = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.ColorCharacther.HexValue).FirstOrDefault(); }
            StilePrecedente.FontFamily = FormatCharacterView.FontFamily;
            StilePrecedente.IsBarrato = FormatCharacterView.IsBarrato;
            StilePrecedente.IsGrassetto = FormatCharacterView.IsGrassetto;
            StilePrecedente.IsCorsivo = FormatCharacterView.IsCorsivo;
            StilePrecedente.IsSottolineato = FormatCharacterView.IsSottolineato;
            StilePrecedente.TextAlignement = FormatCharacterView.TextAlignement;
            StilePrecedente.TextAlignementCode = FormatCharacterView.TextAlignementCode;
            StilePrecedente.TextVerticalAlignement = FormatCharacterView.TextVerticalAlignement;
            if (!string.IsNullOrEmpty(FormatCharacterView.Size)) { StilePrecedente.Size = FormatCharacterView.ListSize.Where(c => c == FormatCharacterView.Size).FirstOrDefault(); }
            if (FormatCharacterView.StileConPropieta != null)
            {
                StilePrecedente.SettaStileProgettoPerNome(FormatCharacterView.StileConPropieta.NomeECodice);
            }

            FormatCharacterWnd.Title = CommonResources.LocalizationProvider.GetString("Impostazioni") + " : " + PathAttributeSelected;

            StilePrecedente.IsModificatoVisible = FormatCharacterView.IsModificatoVisible;
            StilePrecedente.Nascondi = FormatCharacterView.Nascondi;
            StilePrecedente.Rtf = FormatCharacterView.Rtf;
            StilePrecedente.ConcatenaEtichettaEValore = FormatCharacterView.ConcatenaEtichettaEValore;
            StilePrecedente.StampaFormula = FormatCharacterView.StampaFormula;

            InClosing = false;
        }

        public void ClosePopUp(bool FromInterface)
        {
            if (FromInterface)
            {
                CloseAllPopUpWhenOneIsOpenHandler?.Invoke(this, new ClosePopUpEventArgs(null));
            }
            else
            {
                CloseAllPopUpWhenOneIsOpenHandler?.Invoke(this, new ClosePopUpEventArgs(GuidIdentificativo.ToString()));
            }

        }

        private ICommand _StyleCommand;
        public ICommand StyleCommand
        {
            get
            {
                return _StyleCommand ?? (_StyleCommand = new CommandHandler(param => ExecuteStile(param), CanExecuteStile()));
            }
        }

        private bool CanExecuteStile()
        {
            return true;
        }

        public void ExecuteStile(object param)
        {
            try
            {
                CreateParamsWindow(false, false);
                FormatCharacterView.IsViewBlocked = false;
                FormatCharacterView.UpdateUi();
                FormatCharacterWnd.ShowDialog();
            }
            catch (Exception)
            {
            }

        }

        private ICommand _SelezioneAttributoCommand;
        public ICommand SelezioneAttributoCommand
        {
            get
            {
                return _SelezioneAttributoCommand ?? (_SelezioneAttributoCommand = new CommandHandler(param => SelezioneAttributo(param), CanExecuteSelezioneAttributo()));
            }
        }
        private bool CanExecuteSelezioneAttributo()
        {
            return true;
        }

        public void SelezioneAttributo(object param)
        {
            try
            {

                var trvItem = param as TreeviewItem;

                if (trvItem != null)
                {
                    if (trvItem.PropertyType == BuiltInCodes.DefinizioneAttributo.GuidCollection)
                    {
                        System.Windows.MessageBox.Show(LocalizationProvider.GetString("NonEPossibileSelezionareQuestoAttributo"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }



                    if (trvItem.Attrbuto == LocalizationProvider.GetString("Funzione")
                        || trvItem.Attrbuto == LocalizationProvider.GetString(StampeKeys.ConstSommaWizard)
                        || trvItem.Attrbuto == LocalizationProvider.GetString(StampeKeys.ConstContaWizard)
                        || trvItem.Attrbuto == LocalizationProvider.GetString(StampeKeys.ConstSommaStrutturaWizard))
                    {
                        ResettaOpzioniDiStampa();
                        return;
                    }

                    if (Origine == 0)
                    {
                        if (trvItem.Attrbuto == StampeKeys.LocalizeNessuno)
                        {
                            if (ItemsRaggruppamenti != null)
                            {
                                foreach (var Raggruppamento in ItemsRaggruppamenti)
                                {
                                    if (Raggruppamento.TextBoxItemView?.SelectedTreeViewItem != null)//Add by Ale 29/01/2024
                                    {
                                        if (Raggruppamento.TextBoxItemView?.SelectedTreeViewItem.Attrbuto == StampeKeys.LocalizeNessuno)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    string AttriubutoPrecedente = null;

                    if (!IsControlForOrderingData)
                    {
                        ResettaOpzioniDiStampa();

                        if (Origine == 0)
                        {
                            if (ItemsRaggruppamenti.Where(r => r.TextBoxItemView.AttributeSelected == trvItem.Attrbuto).FirstOrDefault() != null)
                            {
                                RaggruppamentiItemsView ItemJustInserted = ItemsRaggruppamenti.Where(r => r.TextBoxItemView.AttributeSelected == trvItem.Attrbuto).FirstOrDefault();
                                if (ItemJustInserted.TextBoxItemView.SelectedTreeViewItem.Attrbuto == trvItem.Attrbuto && ItemJustInserted.TextBoxItemView.SelectedTreeViewItem.AttrbutoCodice == trvItem.AttrbutoCodice
                                    && ItemJustInserted.TextBoxItemView.SelectedTreeViewItem.AttrbutoDestinazione == trvItem.AttrbutoDestinazione && ItemJustInserted.TextBoxItemView.SelectedTreeViewItem.AttrbutoOrigine == trvItem.AttrbutoOrigine
                                    && ItemJustInserted.TextBoxItemView.SelectedTreeViewItem.EntityType == trvItem.EntityType)
                                {
                                    System.Windows.MessageBox.Show(LocalizationProvider.GetString("Attenzione!NonEPossibileInserireUnoStessoRaggruppamentoPiuVolte"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    return;
                                }
                            }
                        }

                        AbilitazioneOpzioniDiStampa(trvItem);

                        AttriubutoPrecedente = AttributeSelected;

                        AssegnaVariabiliLocali(trvItem);

                        //SOLO SE STO GESTENDO UN RAGGRUPPAMENTO LO FACCIO
                        if (!string.IsNullOrEmpty(AttriubutoPrecedente) && AttriubutoPrecedente != trvItem.Attrbuto)
                            if (Origine == 0)
                                if (ItemsRaggruppamenti.FirstOrDefault() != null)
                                    ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SostituzioneAttributoRaggruppamento = true;

                        if (ItemsRaggruppamenti != null)
                        {
                            var ElementFounded = ItemsRaggruppamenti.Where(r => r.TextBoxItemView.AttributeSelected == AttributeSelected && r.TextBoxItemView.SelectedTreeViewItem.EntityType == EntitySelected).FirstOrDefault();
                            if (ElementFounded != null)
                            {
                                ElementFounded.ReportSettingDataViewHelper.ForceSelectionSelectedItemInGroup = ItemsRaggruppamenti.Where(r => r.TextBoxItemView.AttributeSelected == AttributeSelected && r.TextBoxItemView.SelectedTreeViewItem.EntityType == EntitySelected).FirstOrDefault();
                                ElementFounded.ReportSettingDataViewHelper.ForceSelectionSelectedItemInGroup.AssegnaListaAttributiOrdinamento();
                            }

                        }

                        RigeneraListaAttributiPerFunzioni?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        AssegnaVariabiliLocali(trvItem);
                    }
                    PopUpIsOpen = false;
                }

                if (SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaWizard && !string.IsNullOrEmpty(SelectedTreeViewItem?.Attrbuto))
                    AttributeSelected = "S(" + SelectedTreeViewItem.Attrbuto + ")";
                if (SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstContaWizard && !string.IsNullOrEmpty(SelectedTreeViewItem?.Attrbuto))
                    AttributeSelected = "C(" + SelectedTreeViewItem.Attrbuto + ")";
                if (SelectedTreeViewItem?.CodiceDigicorp == StampeKeys.ConstSommaStrutturaWizard && !string.IsNullOrEmpty(SelectedTreeViewItem?.Attrbuto))
                    AttributeSelected = "ST(" + SelectedTreeViewItem.Attrbuto + ")";
            }
            catch (Exception)
            {
            }

        }

        public bool ImpostaSelezioneAttributoInControllo(TreeviewItem Value, ObservableCollection<TreeviewItem> ListaAttributiLivelloSuccessivo = null)
        {
            if (ListaComboBox == null) return false;
            ObservableCollection<TreeviewItem> ListaAttributi = null;

            if (ListaAttributiLivelloSuccessivo == null)
                ListaAttributi = ListaComboBox;
            else
                ListaAttributi = ListaAttributiLivelloSuccessivo;

            if (Value.CodiceDigicorp != StampeKeys.ConstSommaWizard && Value.CodiceDigicorp != StampeKeys.ConstContaWizard && Value.CodiceDigicorp != StampeKeys.ConstSommaStrutturaWizard)
            {
                List<string> attsCodicePath = new List<string>();
                if (FindAttributoPath(ListaAttributi, Value.AttributoCodicePath, ref attsCodicePath))
                {
                    attsCodicePath.Add(Value.AttributoCodicePath);

                    ObservableCollection<TreeviewItem> listaAttributi = ListaAttributi;
                    foreach (var attCodicePath in attsCodicePath)
                    {
                        var att = listaAttributi.FirstOrDefault(x => x.AttributoCodicePath == attCodicePath);
                        if (att.AttributoCodicePath == Value.AttributoCodicePath)
                        {
                            att.IsSelected = true;
                            return true;
                        }
                        else
                        {
                            att.IsExpanded = true;
                            listaAttributi = att.Items;
                        }
                    }
                }

                //foreach (TreeviewItem Item in ListaAttributi)
                //{
                //    /// Find in current
                //    if (Item.EntityType == Value.EntityType && Item.AttrbutoCodice == Value.AttrbutoCodice)
                //    {
                //        Item.IsSelected = true;
                //        return true;
                //    }
                //    /// Find in Childs
                //    if (Item.Items.Count() != 0 && !Item.IsSelected && Item.EntityType == Value.EntityType)
                //    {
                //        Item.IsExpanded = true;
                //        return ImpostaSelezioneAttributoInControllo(Value, Item.Items);
                //    }
                //}
            }
            else
            {
                foreach (var Funzione in ListaAttributi.LastOrDefault().Items)
                {
                    if (Funzione.CodiceDigicorp == Value.CodiceDigicorp)
                    {
                        foreach (var Item in Funzione.Items)
                        {
                            if (Item.EntityType == Value.EntityType && Item.AttrbutoCodice == Value.AttrbutoCodice)
                            {
                                ListaAttributi.LastOrDefault().IsExpanded = true;
                                Funzione.IsExpanded = true;
                                Item.IsSelected = true;
                                return true;
                            }
                        }
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// Ricorsiva
        /// </summary>
        /// <param name="listaAttributi"></param>
        /// <param name="attributoCodicePath"></param>
        /// <param name="attCodicePath"></param>
        /// <returns></returns>
        private bool FindAttributoPath(ObservableCollection<TreeviewItem> listaAttributi, string attributoCodicePath, ref List<string> attsCodicePath)
        {
            foreach (var att in  listaAttributi)
            {
                if (att.AttributoCodicePath == attributoCodicePath)
                    return true;

                if (FindAttributoPath(att.Items, attributoCodicePath, ref attsCodicePath))
                {
                    attsCodicePath.Insert(0, att.AttributoCodicePath);
                    return true;
                }
            }

            return false;
        }

        public void DeselectAll()
        {
            foreach (var item in ListaComboBox)
            {
                item.IsSelected = false;
            }
        }
        private void AssegnaVariabiliLocali(TreeviewItem TreeviewItem)
        {
            AttributeSelected = TreeviewItem.Attrbuto;
            EntitySelected = TreeviewItem.EntityType;
            SelectedTreeViewItem = TreeviewItem;

            if (String.IsNullOrEmpty(TreeviewItem.Padre))
                if (TreeviewItem.Attrbuto == StampeKeys.LocalizeNessuno)
                    AttributeSelected = "";
        }

        private ICommand _PreviewMouseDownCommand;
        public ICommand PreviewMouseDownCommand
        {
            get
            {
                return _PreviewMouseDownCommand ?? (_PreviewMouseDownCommand = new CommandHandler(param => PreviewMouseDown(param), CanExecutePreviewMouseDown()));
            }
        }

        private bool CanExecutePreviewMouseDown()
        {
            return true;
        }
        private void PreviewMouseDown(object param)
        {
            if (PopUpIsOpen)
            {
                PopUpIsOpen = false;
                return;
            }



            PopUpIsOpen = true;
            if (Origine == 0)
            {
                //FORZO SELEZIONE IN CASO NON CI SIA IL FOCUS SULLA RIGA
                if (IsControlForOrderingData)
                {
                    if (ItemsRaggruppamenti != null)
                    {
                        if (ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SelectedItemInGroup != null)
                        {
                            ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.ForceSelectionSelectedItemInGroup = ItemsRaggruppamenti.Where(r => r.TextBoxItemViewOrdinamento.GuidIdentificativo == GuidIdentificativo).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    if (ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SelectedItemInGroup != null)
                    {
                        if (ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SelectedItemInGroup.TextBoxItemView.AttributeSelected != AttributeSelected)
                        {
                            ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.ForceSelectionSelectedItemInGroup = ItemsRaggruppamenti.Where(r => r.TextBoxItemView.AttributeSelected == AttributeSelected).FirstOrDefault();
                        }

                        if (ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SelectedItemInGroup.IsGroupReferencedDown)
                        {
                            System.Windows.MessageBox.Show(LocalizationProvider.GetString("GruppoRiferitoSotto"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }

                }
            }



            //if (ItemsRaggruppamenti != null && ItemsRaggruppamenti.Any() && ItemsRaggruppamenti.FirstOrDefault().ReportSettingDataViewHelper.SelectedItemInGroup.IsGroupReferencedDown)
            //{
            //    System.Windows.MessageBox.Show(LocalizationProvider.GetString("GruppoRiferitoSotto"), LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return;
            //}
        }

        private ICommand _AcceptCommand;
        public ICommand AcceptCommand
        {
            get
            {
                return _AcceptCommand ?? (_AcceptCommand = new CommandHandler(param => ExecuteAccept(param), CanExecuteAccept()));
            }
        }

        private bool CanExecuteAccept()
        {
            return true;
        }

        public void ExecuteAccept(object param)
        {
            if (FormatCharacterView.StileConPropieta != null)
            {
                if (FormatCharacterView.IsModificatoVisible == Visibility.Visible)
                {
                    FormatCharacterView.StileConPropieta = FormatCharacterView.ListStiliConPropieta.FirstOrDefault();
                }
            }

            if (FormatCharacterView.Nascondi)
            {
                HideAttributeColor = System.Windows.Media.Brushes.Gray;
            }
            else
            {
                HideAttributeColor = System.Windows.Media.Brushes.Black;
            }

            FormatCharacterWnd.Visibility = Visibility.Collapsed;
        }

        public void AbilitazioneOpzioniDiStampa(TreeviewItem trvItem)
        {
            switch (Origine)
            {
                case 0://FromIntestazioniColonna
                    break;
                case 1://FromHeaderFooterGroup
                    SettaVisibilitàComandi(trvItem);
                    break;
                case 2://FromCorpo
                    SettaVisibilitàComandi(trvItem);
                    break;
                case 3://FromIntestazioniDocumento

                    break;
                case 4://PiePaginaDocumento

                    break;
                case 5://FineDocumento

                    break;
            }
        }

        public void ResettaOpzioniDiStampa()
        {
            if (FormatCharacterView != null)
            {
                FormatCharacterView.Rtf = false;
                FormatCharacterView.StampaFormula = false;
                FormatCharacterView.ConcatenaEtichettaEValore = false;
                FormatCharacterView.Nascondi = false;
                HideAttributeColor = System.Windows.Media.Brushes.Black;
            }
        }

        private void SettaVisibilitàComandi(TreeviewItem trvItem)
        {
            if (FormatCharacterView != null)
            {
                if (trvItem.PropertyType == BuiltInCodes.DefinizioneAttributo.TestoRTF)
                    FormatCharacterView.IsRTFVisible = Visibility.Visible;
                else
                    FormatCharacterView.IsRTFVisible = Visibility.Collapsed;

                if (trvItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Reale || trvItem.PropertyType == BuiltInCodes.DefinizioneAttributo.Contabilita)
                    FormatCharacterView.IsStampaFormulaVisible = Visibility.Visible;
                else
                    FormatCharacterView.IsStampaFormulaVisible = Visibility.Collapsed;
            }
        }
    }
}
