using Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class SALProgrammatoView : NotificationBase
    {
        private int? _ContatoreSAL;
        public int? ContatoreSAL
        {
            get { return _ContatoreSAL; }
            set { SetProperty(ref _ContatoreSAL, value); }
        }
        private DateTime _Data;
        public DateTime Data
        {
            get { return _Data; }
            set { SetProperty(ref _Data, value); }
        }
        private double _GiorniPeriodo;
        public double GiorniPeriodo
        {
            get { return _GiorniPeriodo; }
            set { SetProperty(ref _GiorniPeriodo, value); }
        }
        private double _GiorniProgressivo;
        public double GiorniProgressivo
        {
            get { return _GiorniProgressivo; }
            set { SetProperty(ref _GiorniProgressivo, value); }
        }
        private bool _IsParen;
        public bool IsParent
        {
            get { return _IsParen; }
            set { SetProperty(ref _IsParen, value); }
        }
        private int _Livello;
        public int Livello
        {
            get { return _Livello; }
            set { SetProperty(ref _Livello, value); }
        }
        private Guid _Guid;
        public Guid Guid
        {
            get { return _Guid; }
            set { SetProperty(ref _Guid, value); }
        }
        private string _Codice;
        public string Codice
        {
            get { return _Codice; }
            set { SetProperty(ref _Codice, value); }
        }
        private string _Descrizione;
        public string Descrizione
        {
            get { return _Descrizione; }
            set { SetProperty(ref _Descrizione, value); }
        }
        private string _Formato;
        public string Formato
        {
            get { return _Formato; }
            set { SetProperty(ref _Formato, value); }
        }
        private double? _ColonnaAttributo1;
        public double? ColonnaAttributo1
        {
            get { return _ColonnaAttributo1; }
            set { SetProperty(ref _ColonnaAttributo1, value); }
        }
        private double? _ColonnaAttributo2;
        public double? ColonnaAttributo2
        {
            get { return _ColonnaAttributo2; }
            set { SetProperty(ref _ColonnaAttributo2, value); }
        }
        private double? _ColonnaAttributo3;
        public double? ColonnaAttributo3
        {
            get { return _ColonnaAttributo3; }
            set { SetProperty(ref _ColonnaAttributo3, value); }
        }
        private double? _ColonnaAttributo4;
        public double? ColonnaAttributo4
        {
            get { return _ColonnaAttributo4; }
            set { SetProperty(ref _ColonnaAttributo4, value); }
        }
        private double? _ColonnaAttributo5;
        public double? ColonnaAttributo5
        {
            get { return _ColonnaAttributo5; }
            set { SetProperty(ref _ColonnaAttributo5, value); }
        }
        private double? _ColonnaAttributo6;
        public double? ColonnaAttributo6
        {
            get { return _ColonnaAttributo6; }
            set { SetProperty(ref _ColonnaAttributo6, value); }
        }
        private double? _ColonnaAttributo7;
        public double? ColonnaAttributo7
        {
            get { return _ColonnaAttributo7; }
            set { SetProperty(ref _ColonnaAttributo7, value); }
        }
        private double? _ColonnaAttributo8;
        public double? ColonnaAttributo8
        {
            get { return _ColonnaAttributo8; }
            set { SetProperty(ref _ColonnaAttributo8, value); }
        }
        private double? _ColonnaAttributo9;
        public double? ColonnaAttributo9
        {
            get { return _ColonnaAttributo9; }
            set { SetProperty(ref _ColonnaAttributo9, value); }
        }
        private double? _ColonnaAttributo10;
        public double? ColonnaAttributo10
        {
            get { return _ColonnaAttributo10; }
            set { SetProperty(ref _ColonnaAttributo10, value); }
        }
        private double? _ColonnaAttributo11;
        public double? ColonnaAttributo11
        {
            get { return _ColonnaAttributo11; }
            set { SetProperty(ref _ColonnaAttributo11, value); }
        }
        private double? _ColonnaAttributo12;
        public double? ColonnaAttributo12
        {
            get { return _ColonnaAttributo12; }
            set { SetProperty(ref _ColonnaAttributo12, value); }
        }
        private double? _ColonnaAttributo13;
        public double? ColonnaAttributo13
        {
            get { return _ColonnaAttributo13; }
            set { SetProperty(ref _ColonnaAttributo13, value); }
        }
        private double? _ColonnaAttributo14;
        public double? ColonnaAttributo14
        {
            get { return _ColonnaAttributo14; }
            set { SetProperty(ref _ColonnaAttributo14, value); }
        }
        private double? _ColonnaAttributo15;
        public double? ColonnaAttributo15
        {
            get { return _ColonnaAttributo15; }
            set { SetProperty(ref _ColonnaAttributo15, value); }
        }
        private double? _ColonnaAttributo16;
        public double? ColonnaAttributo16
        {
            get { return _ColonnaAttributo16; }
            set { SetProperty(ref _ColonnaAttributo16, value); }
        }
        private double? _ColonnaAttributo17;
        public double? ColonnaAttributo17
        {
            get { return _ColonnaAttributo17; }
            set { SetProperty(ref _ColonnaAttributo17, value); }
        }
        private double? _ColonnaAttributo18;
        public double? ColonnaAttributo18
        {
            get { return _ColonnaAttributo18; }
            set { SetProperty(ref _ColonnaAttributo18, value); }
        }
        private double? _ColonnaAttributo19;
        public double? ColonnaAttributo19
        {
            get { return _ColonnaAttributo19; }
            set { SetProperty(ref _ColonnaAttributo19, value); }
        }
        private double? _ColonnaAttributo20;
        public double? ColonnaAttributo20
        {
            get { return _ColonnaAttributo20; }
            set { SetProperty(ref _ColonnaAttributo20, value); }
        }
        private double? _ColonnaAttributo21;
        public double? ColonnaAttributo21
        {
            get { return _ColonnaAttributo21; }
            set { SetProperty(ref _ColonnaAttributo21, value); }
        }
        private double? _ColonnaAttributo22;
        public double? ColonnaAttributo22
        {
            get { return _ColonnaAttributo22; }
            set { SetProperty(ref _ColonnaAttributo22, value); }
        }
        private double? _ColonnaAttributo23;
        public double? ColonnaAttributo23
        {
            get { return _ColonnaAttributo23; }
            set { SetProperty(ref _ColonnaAttributo23, value); }
        }
        private double? _ColonnaAttributo24;
        public double? ColonnaAttributo24
        {
            get { return _ColonnaAttributo24; }
            set { SetProperty(ref _ColonnaAttributo24, value); }
        }

        private bool _IsSAL;
        public bool IsSAL
        {
            get { return _IsSAL; }
            set { SetProperty(ref _IsSAL, value); }
        }

        //public bool IsUserInsert { get; set; }

        //public string Error
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        return string.Empty;
        //    }
        //}

        public double? GetValue(string propertyName)
        {
            if (propertyName == GanttKeys.ColonnaAttributo + 1)
            {
                if (ColonnaAttributo1.HasValue)
                {
                    return ColonnaAttributo1.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 2)
            {
                if (ColonnaAttributo2.HasValue)
                {
                    return ColonnaAttributo2.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 3)
            {
                if (ColonnaAttributo3.HasValue)
                {
                    return ColonnaAttributo3.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 4)
            {
                if (ColonnaAttributo4.HasValue)
                {
                    return ColonnaAttributo4.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 5)
            {
                if (ColonnaAttributo5.HasValue)
                {
                    return ColonnaAttributo5.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 6)
            {
                if (ColonnaAttributo6.HasValue)
                {
                    return ColonnaAttributo6.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 7)
            {
                if (ColonnaAttributo7.HasValue)
                {
                    return ColonnaAttributo7.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 8)
            {
                if (ColonnaAttributo8.HasValue)
                {
                    return ColonnaAttributo8.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 9)
            {
                if (ColonnaAttributo9.HasValue)
                {
                    return ColonnaAttributo9.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 10)
            {
                if (ColonnaAttributo10.HasValue)
                {
                    return ColonnaAttributo10.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 11)
            {
                if (ColonnaAttributo11.HasValue)
                {
                    return ColonnaAttributo11.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 12)
            {
                if (ColonnaAttributo12.HasValue)
                {
                    return ColonnaAttributo12.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 13)
            {
                if (ColonnaAttributo13.HasValue)
                {
                    return ColonnaAttributo13.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 14)
            {
                if (ColonnaAttributo14.HasValue)
                {
                    return ColonnaAttributo14.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 15)
            {
                if (ColonnaAttributo15.HasValue)
                {
                    return ColonnaAttributo15.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 16)
            {
                if (ColonnaAttributo16.HasValue)
                {
                    return ColonnaAttributo16.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 17)
            {
                if (ColonnaAttributo17.HasValue)
                {
                    return ColonnaAttributo17.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 18)
            {
                if (ColonnaAttributo18.HasValue)
                {
                    return ColonnaAttributo18.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 19)
            {
                if (ColonnaAttributo19.HasValue)
                {
                    return ColonnaAttributo19.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 20)
            {
                if (ColonnaAttributo20.HasValue)
                {
                    return ColonnaAttributo20.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 21)
            {
                if (ColonnaAttributo21.HasValue)
                {
                    return ColonnaAttributo21.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 22)
            {
                if (ColonnaAttributo22.HasValue)
                {
                    return ColonnaAttributo22.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 23)
            {
                if (ColonnaAttributo23.HasValue)
                {
                    return ColonnaAttributo23.Value;
                }
                else
                {
                    return null;
                }
            }
            if (propertyName == GanttKeys.ColonnaAttributo + 24)
            {
                if (ColonnaAttributo24.HasValue)
                {
                    return ColonnaAttributo24.Value;
                }
                else
                {
                    return null;
                }
            }
            return 0;
        }

        public void SetValue(string propertyName, double? value)
        {
            if (propertyName == GanttKeys.ColonnaAttributo + 1)
                ColonnaAttributo1 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 2)
                ColonnaAttributo2 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 3)
                ColonnaAttributo3 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 4)
                ColonnaAttributo4 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 5)
                ColonnaAttributo5 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 6)
                ColonnaAttributo6 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 7)
                ColonnaAttributo7 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 8)
                ColonnaAttributo8 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 9)
                ColonnaAttributo9 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 10)
                ColonnaAttributo10 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 11)
                ColonnaAttributo11 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 12)
                ColonnaAttributo12 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 13)
                ColonnaAttributo13 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 14)
                ColonnaAttributo14 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 15)
                ColonnaAttributo15 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 16)
                ColonnaAttributo16 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 17)
                ColonnaAttributo17 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 18)
                ColonnaAttributo18 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 19)
                ColonnaAttributo19 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 20)
                ColonnaAttributo20 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 21)
                ColonnaAttributo21 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 22)
                ColonnaAttributo22 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 23)
                ColonnaAttributo23 = value;
            if (propertyName == GanttKeys.ColonnaAttributo + 24)
                ColonnaAttributo24 = value;
        }

        public static int GetTotalColumnForCycle()
        {
            return 25;
        }
    }
}
