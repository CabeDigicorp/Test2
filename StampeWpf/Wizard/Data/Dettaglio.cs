using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StampeWpf.Wizard
{
    public class Dettaglio : NotificationBase
    {
        private ObservableCollection<TextBoxItemView> _ListaDettaglio;
        public ObservableCollection<TextBoxItemView> ListaDettaglio
        {
            get
            {
                return _ListaDettaglio;
            }
            set
            {
                if (SetProperty(ref _ListaDettaglio, value))
                {
                    _ListaDettaglio = value;
                }
            }
        }

        private string _AttributoAssegnazioneGruppo;
        public string AttributoAssegnazioneGruppo
        {
            get
            {
                return _AttributoAssegnazioneGruppo;
            }
            set
            {
                if (SetProperty(ref _AttributoAssegnazioneGruppo, value))
                {
                    _AttributoAssegnazioneGruppo = value;
                }
            }
        }
        private string _EntityTypeAssegnazioneGruppo;
        public string EntityTypeAssegnazioneGruppo
        {
            get
            {
                return _EntityTypeAssegnazioneGruppo;
            }
            set
            {
                if (SetProperty(ref _EntityTypeAssegnazioneGruppo, value))
                {
                    _EntityTypeAssegnazioneGruppo = value;
                }
            }
        }

        public Dettaglio()
        {
            ListaDettaglio = new ObservableCollection<TextBoxItemView>();
        }
    }
}
