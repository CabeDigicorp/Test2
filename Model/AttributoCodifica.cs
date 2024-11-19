using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Model
{
    public class AttributoCodingSetting : NotificationBase
    {
        public event EventHandler AttributoCodingSettingUpdaterHanlder;
        public event EventHandler<ExecuteOperationEventArgs> CanCodifyHanlder;
        public EntitiesCodingHelper EntitiesCodingHelper = new EntitiesCodingHelper();

        private int _Livello;
        public int Livello
        {
            get
            {
                return _Livello;
            }
            set
            {
                _Livello = value;
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private bool _Codifica;
        public bool Codifica
        {
            get
            {
                return _Codifica;
            }
            set
            {
                ExecuteOperationEventArgs ExecuteOperationEventArgs = new ExecuteOperationEventArgs();
                CanCodifyHanlder?.Invoke(this, ExecuteOperationEventArgs);
                //if (!ExecuteOperationEventArgs.Cancel)
                //{
                if (ExecuteOperationEventArgs.Force)
                {
                    value = ExecuteOperationEventArgs.Value;
                }

                if (SetProperty(ref _Codifica, value))
                {
                    _Codifica = value;
                }
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //}
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private string _Prefisso;
        public string Prefisso
        {
            get
            {
                return _Prefisso;
            }
            set
            {
                _Prefisso = value;
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private string _ValoreIncrementale;
        public string ValoreIncrementale
        {
            get
            {
                return _ValoreIncrementale;
            }
            set
            {
                _ValoreIncrementale = value;
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private string _Suffisso;
        public string Suffisso
        {
            get
            {
                return _Suffisso;
            }
            set
            {
                _Suffisso = value;
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private int _Passo;
        public int Passo
        {
            get
            {
                return _Passo;
            }
            set
            {
                _Passo = value;
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }
        private bool _AggiungiCodiceSuperiore;
        public bool AggiungiCodiceSuperiore
        {
            get
            {
                return _AggiungiCodiceSuperiore;
            }
            set
            {
                if (SetProperty(ref _AggiungiCodiceSuperiore, value))
                {
                    _AggiungiCodiceSuperiore = value;
                }
                if (EntitiesCodingHelper != null)
                    AttributoCodingSettingUpdaterHanlder?.Invoke(this, new EventArgs());
                //EntitiesCodingHelper.GeneraCodice(this);
            }
        }

        private string _Esempio;
        public string Esempio
        {
            get
            {
                return _Esempio;
            }
            set
            {
                if (SetProperty(ref _Esempio, value))
                {
                    _Esempio = value;
                }
            }
        }

        public AttributoCodingSetting AttributoCodingSettingPrecedente { get; set; }
    }
    public class ComportamentoCodice
    {
        public string Comportamento { get; set; }
        public int Codice { get; set; }
    }
    public class AttributoCodingHeader
    {
        public int PosizionamentoRispettoCodiceEsistente { get; set; }
        public string Etichetta { get; set; }
    }
    public class ExecuteOperationEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public bool Value { get; set; }
        public bool Force { get; set; }
    }

}
