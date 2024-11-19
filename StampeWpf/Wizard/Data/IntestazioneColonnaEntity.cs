using Commons;
using DatiGeneraliWpf;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StampeWpf.Wizard
{
    public class IntestazioneColonnaEntity : NotificationBase
    {
        public event EventHandler<ColumnCorpoEventArgs> ColumnCorpoHanlder;
        public FormatCharacterView FormatCharacterView { get; set; }
        public FormatCharacterView StilePrecedente { get; set; }

        public Window FormatCharacterWnd { get; set; }

        private string _IntestazioneColonna;
        public string IntestazioneColonna
        {
            get
            {
                return _IntestazioneColonna;
            }
            set
            {
                if (SetProperty(ref _IntestazioneColonna, value))
                {
                    _IntestazioneColonna = value;
                }
            }
        }

        public IntestazioneColonnaEntity(ClientDataService clientDataService)
        {
            if (FormatCharacterView == null)
                FormatCharacterView = new FormatCharacterView();
            FormatCharacterView.DataService = clientDataService;
            FormatCharacterView.IntestazioneColonnaEntity = this;
        }
        public void Init(bool FirstInitilization, bool IsViewBlocked)
        {
            if (FormatCharacterView.ListStiliConPropieta == null) { FormatCharacterView.SettaStiliProgetto(); }

            FormatCharacterWnd.DataContext = FormatCharacterView;
            FormatCharacterView.IsModificatoVisible = Visibility.Collapsed;
            FormatCharacterView.IsViewBlocked = IsViewBlocked;


            if (FirstInitilization)
            {
                FormatCharacterView.SettaStileProgetto("Heading 4");
            }

            FormatCharacterView.IsOpzioniDiStampaVisible = Visibility.Collapsed;

            //AGGIUNTO PER UNIFORMARE A SITUAZIONE CON TEXTBOX
            StilePrecedente = new FormatCharacterView();
            StilePrecedente.DataService = FormatCharacterView.DataService;
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

            StilePrecedente.IsModificatoVisible = FormatCharacterView.IsModificatoVisible;
            StilePrecedente.Nascondi = FormatCharacterView.Nascondi;
            StilePrecedente.Rtf = FormatCharacterView.Rtf;
            StilePrecedente.ConcatenaEtichettaEValore = FormatCharacterView.ConcatenaEtichettaEValore;
            StilePrecedente.StampaFormula = FormatCharacterView.StampaFormula;

            FormatCharacterWnd.Title = CommonResources.LocalizationProvider.GetString("Impostazioni") + " : " + CommonResources.LocalizationProvider.GetString("Colonna") + " " + IntestazioneColonna;
        }

        private ICommand _StyleCommand;
        public ICommand StyleCommand
        {
            get
            {
                return _StyleCommand ?? (_StyleCommand = new CommandHandler(param => ExecuteStyle(param), CanExecuteStyle()));
            }
        }

        private bool CanExecuteStyle()
        {
            return true;
        }

        public void ExecuteStyle(object param)
        {
            FormatCharacterView.IsViewBlocked = false;
            Init(false, false);
            FormatCharacterView.UpdateUi();
            FormatCharacterWnd.Show();
        }
        public void ExecuteAccept()
        {
            if (FormatCharacterView.StileConPropieta != null)
            {
                if (FormatCharacterView.IsModificatoVisible == Visibility.Visible)
                {
                    FormatCharacterView.StileConPropieta = FormatCharacterView.ListStiliConPropieta.FirstOrDefault();
                    FormatCharacterView.IsModificatoVisible = Visibility.Collapsed;
                }
            }

            FormatCharacterWnd.Visibility = Visibility.Collapsed;
            //FormatCharacterWnd.Close();
        }

        private ICommand _AddColumnCommand;
        public ICommand AddColumnCommand
        {
            get
            {
                return _AddColumnCommand ?? (_AddColumnCommand = new CommandHandler(param => ExecuteAddColumn(param), CanExecuteAddColumn()));
            }
        }

        private bool CanExecuteAddColumn()
        {
            return true;
        }

        public void ExecuteAddColumn(object param)
        {
            ColumnCorpoEventArgs eventArgs = new ColumnCorpoEventArgs(IntestazioneColonna, true, false);
            ColumnCorpoHanlder?.Invoke(this, eventArgs);
        }

        private ICommand _DeleteColumnCommand;
        public ICommand DeleteColumnCommand
        {
            get
            {
                return _DeleteColumnCommand ?? (_DeleteColumnCommand = new CommandHandler(param => ExecuteDeleteColumn(param), CanExecutDeleteColumn()));
            }
        }

        private bool CanExecutDeleteColumn()
        {
            return true;
        }

        public void ExecuteDeleteColumn(object param)
        {
            ColumnCorpoEventArgs eventArgs = new ColumnCorpoEventArgs(IntestazioneColonna, false, true);
            ColumnCorpoHanlder?.Invoke(this, eventArgs);
        }
    }

    public class ColumnCorpoEventArgs : EventArgs
    {
        public string ColumnTitle { get; set; }
        public bool Add { get; set; }
        public bool Delete { get; set; }
        public ColumnCorpoEventArgs(string columnTitle, bool add, bool delete)
        {
            ColumnTitle = columnTitle;
            Add = add;
            Delete = delete;
        }
    }
}
