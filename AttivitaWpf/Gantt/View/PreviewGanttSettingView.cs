using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class PreviewGanttSettingView : NotificationBase
    {
        private int _Codice;
        public int Codice
        {
            get
            {

                return _Codice;
            }
            set
            {
                SetProperty(ref _Codice, value);
            }
        }

        private int _Descrizione;
        public int Descrizione
        {
            get
            {

                return _Descrizione;
            }
            set
            {
                SetProperty(ref _Descrizione, value);
            }
        }

        private int _Durata;
        public int Durata
        {
            get
            {

                return _Durata;
            }
            set
            {
                SetProperty(ref _Durata, value);
            }
        }

        private int _Duratacalendario;
        public int Duratacalendario
        {
            get
            {

                return _Duratacalendario;
            }
            set
            {
                SetProperty(ref _Duratacalendario, value);
            }
        }

        private int _Inizio;
        public int Inizio
        {
            get
            {

                return _Inizio;
            }
            set
            {
                SetProperty(ref _Inizio, value);
            }
        }
        private int _Fine;
        public int Fine
        {
            get
            {

                return _Fine;
            }
            set
            {
                SetProperty(ref _Fine, value);
            }
        }

        //public bool Stored { get; set; }
        public PreviewGanttSettingView()
        {
            Codice = 70;
            Descrizione = 300;
            Durata = 70;
            Duratacalendario = 70;
            Inizio = 70;
            Fine = 70;
        }
        public void AcceptButton()
        {
            //Stored = true;
        }
    }
}
