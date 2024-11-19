using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf
{
    public class DateProgettoView : NotificationBase
    {
        private DateTime _DataInizioGantt;
        public DateTime DataInizioGantt
        {
            get
            {
                return _DataInizioGantt;
            }
            set
            {
                DataInizioPrecedente = _DataInizioGantt;
                if (SetProperty(ref _DataInizioGantt, value))
                    _DataInizioGantt = value;
            }
        }
        private DateTime _DataFineGantt;
        public DateTime DataFineGantt
        {
            get
            {
                return _DataFineGantt;
            }
            set
            {
                if (SetProperty(ref _DataFineGantt, value))
                    _DataFineGantt = value;
            }
        }

        private string _DataInizioLavori;
        public string DataInizioLavori
        {
            get
            {
                return _DataInizioLavori;
            }
            set
            {
                if (SetProperty(ref _DataInizioLavori, value))
                    _DataInizioLavori = value;
            }
        }
        private string _DataFineLavori;
        public string DataFineLavori
        {
            get
            {
                return _DataFineLavori;
            }
            set
            {
                if (SetProperty(ref _DataFineLavori, value))
                    _DataFineLavori = value;
            }
        }

        private string _Format;
        public string Format
        {
            get
            {
                return _Format;
            }
            set
            {
                if (SetProperty(ref _Format, value))
                    _Format = value;
            }
        }

        private bool _Offset;
        public bool Offset
        {
            get
            {
                return _Offset;
            }
            set
            {
                if (SetProperty(ref _Offset, value))
                    _Offset = value;
            }
        }

        private bool _UseDefaultCalendar;
        public bool UseDefaultCalendar
        {
            get
            {
                return _UseDefaultCalendar;
            }
            set
            {
                if (SetProperty(ref _UseDefaultCalendar, value))
                    _UseDefaultCalendar = value;
            }
        }

        private double _DurataGiorniLavorativi;
        public double DurataGiorniLavorativi
        {
            get
            {
                return _DurataGiorniLavorativi;
            }
            set
            {
                SetProperty(ref _DurataGiorniLavorativi, value);
            }
        }

        private double _DurataCalendario;
        public double DurataCalendario
        {
            get
            {
                return _DurataCalendario;
            }
            set
            {
                SetProperty(ref _DurataCalendario, value);
            }
        }


        public DateTime DataInizioPrecedente { get; internal set; }

        public DateProgettoView()
        {
            //if (!UseDefaultCalendar)
            //    UseDefaultCalendar = true;

            _Format = "dd/MM/yyyy";
        }

        public bool Accept()
        {
            return true;
        }
    }
}
