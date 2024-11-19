using CommonResources;
using Commons;
using DatiGeneraliWpf;
using Model;
using Model.Report;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
//using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StampeWpf.Wizard
{
    public class DocumentoView : NotificationBase
    {        
        public event EventHandler ForceCloseOfPopUps;

        private double _LarghezzaLista;
        public double LarghezzaLista
        {
            get
            {
                return _LarghezzaLista;
            }
            set
            {
                if (SetProperty(ref _LarghezzaLista, value))
                {
                    _LarghezzaLista = value;
                    LarghezzaListaIntestazinePiePagina = value;
                }
            }
        }

        private double _LarghezzaListaIntestazinePiePagina;
        public double LarghezzaListaIntestazinePiePagina
        {
            get
            {
                return _LarghezzaListaIntestazinePiePagina;
            }
            set
            {
                if (SetProperty(ref _LarghezzaListaIntestazinePiePagina, value))
                {
                    _LarghezzaListaIntestazinePiePagina = value;
                }
            }
        }

        private Visibility _IsRaggruppamentiInDocumentTestaVisible;
        public Visibility IsRaggruppamentiInDocumentTestaVisible
        {
            get
            {
                return _IsRaggruppamentiInDocumentTestaVisible;
            }
            set
            {
                if (SetProperty(ref _IsRaggruppamentiInDocumentTestaVisible, value))
                {
                    _IsRaggruppamentiInDocumentTestaVisible = value;
                }
            }
        }

        private Visibility _IsRaggruppamentiInDocumentCodaVisible;
        public Visibility IsRaggruppamentiInDocumentCodaVisible
        {
            get
            {
                return _IsRaggruppamentiInDocumentCodaVisible;
            }
            set
            {
                if (SetProperty(ref _IsRaggruppamentiInDocumentCodaVisible, value))
                {
                    _IsRaggruppamentiInDocumentCodaVisible = value;
                }
            }
        }

        private ObservableCollection<IntestazioneColonnaEntity> _IntestazioniColonne;
        public ObservableCollection<IntestazioneColonnaEntity> IntestazioniColonne
        {
            get
            {
                return _IntestazioniColonne;
            }
            set
            {
                if (SetProperty(ref _IntestazioniColonne, value))
                {
                    _IntestazioniColonne = value;
                }
            }
        }

        private ObservableCollection<Dettaglio> _RaggruppamentoTeste;
        public ObservableCollection<Dettaglio> RaggruppamentoTeste
        {
            get
            {
                return _RaggruppamentoTeste;
            }
            set
            {
                if (SetProperty(ref _RaggruppamentoTeste, value))
                {
                    _RaggruppamentoTeste = value;
                }
            }
        }

        private ObservableCollection<Dettaglio> _DocumentoCorpi;
        public ObservableCollection<Dettaglio> DocumentoCorpi
        {
            get
            {
                return _DocumentoCorpi;
            }
            set
            {
                if (SetProperty(ref _DocumentoCorpi, value))
                {
                    _DocumentoCorpi = value;
                }
            }
        }

        private ObservableCollection<Dettaglio> _DocumentoFine;
        public ObservableCollection<Dettaglio> DocumentoFine
        {
            get
            {
                return _DocumentoFine;
            }
            set
            {
                if (SetProperty(ref _DocumentoFine, value))
                {
                    _DocumentoFine = value;
                }
            }
        }

        private ObservableCollection<Dettaglio> _RaggruppamentoCode;
        public ObservableCollection<Dettaglio> RaggruppamentoCode
        {
            get
            {
                return _RaggruppamentoCode;
            }
            set
            {
                if (SetProperty(ref _RaggruppamentoCode, value))
                {
                    _RaggruppamentoCode = value;
                }
            }
        }

        private ObservableCollection<LarghezzaColonnaEntity> _LarghezzaColonne;
        public ObservableCollection<LarghezzaColonnaEntity> LarghezzaColonne
        {
            get
            {
                return _LarghezzaColonne;
            }
            set
            {
                if (SetProperty(ref _LarghezzaColonne, value))
                {
                    _LarghezzaColonne = value;
                }
            }
        }

        private string _TotaleLarghezzaColonne;
        public string TotaleLarghezzaColonne
        {
            get
            {
                return _TotaleLarghezzaColonne;
            }
            set
            {
                if (SetProperty(ref _TotaleLarghezzaColonne, value))
                {
                    _TotaleLarghezzaColonne = value;
                }
            }
        }



        private string _RaggruppamentoSelected;
        public string RaggruppamentoSelected
        {
            get
            {
                return _RaggruppamentoSelected;
            }
            set
            {
                if (SetProperty(ref _RaggruppamentoSelected, value))
                {
                    _RaggruppamentoSelected = value;
                    IsRaggruppamentiInDocumentTestaVisible = Visibility.Visible;
                    IsRaggruppamentiInDocumentCodaVisible = Visibility.Visible;
                }
            }
        }

        private ObservableCollection<ComandiPerRiga> _ListaComandiTesta;
        public ObservableCollection<ComandiPerRiga> ListaComandiTesta
        {
            get
            {
                return _ListaComandiTesta;
            }
            set
            {
                if (SetProperty(ref _ListaComandiTesta, value))
                {
                    _ListaComandiTesta = value;
                }
            }
        }
        private ObservableCollection<ComandiPerRiga> _ListaComandiCorpo;
        public ObservableCollection<ComandiPerRiga> ListaComandiCorpo
        {
            get
            {
                return _ListaComandiCorpo;
            }
            set
            {
                if (SetProperty(ref _ListaComandiCorpo, value))
                {
                    _ListaComandiCorpo = value;
                }
            }
        }
        private ObservableCollection<ComandiPerRiga> _ListaComandiCoda;
        public ObservableCollection<ComandiPerRiga> ListaComandiCoda
        {
            get
            {
                return _ListaComandiCoda;
            }
            set
            {
                if (SetProperty(ref _ListaComandiCoda, value))
                {
                    _ListaComandiCoda = value;
                }
            }
        }

        private ObservableCollection<ComandiPerRiga> _ListaComandiFine;
        public ObservableCollection<ComandiPerRiga> ListaComandiFine
        {
            get
            {
                return _ListaComandiFine;
            }
            set
            {
                if (SetProperty(ref _ListaComandiFine, value))
                {
                    _ListaComandiFine = value;
                }
            }
        }

        private bool _IsEditableDocumento;
        public bool IsEditableDocumento
        {
            get
            {
                return _IsEditableDocumento;
            }
            set
            {
                if (SetProperty(ref _IsEditableDocumento, value))
                {
                    _IsEditableDocumento = value;
                }
            }
        }
        public DocumentoView()
        {
            IsRaggruppamentiInDocumentTestaVisible = Visibility.Collapsed;
            IsRaggruppamentiInDocumentCodaVisible = Visibility.Collapsed;
        }

        public void CreateANewIstanceOfThis(DocumentoView ExternalDocumento)
        {
            RaggruppamentoTeste = new ObservableCollection<Dettaglio>();
            RaggruppamentoCode = new ObservableCollection<Dettaglio>();

            foreach (var item in ExternalDocumento.RaggruppamentoTeste)
            {
                foreach (var Dettaglio in item.ListaDettaglio)
                    if (string.IsNullOrEmpty(Dettaglio.Etichetta)) { Dettaglio.Etichetta = StampeKeys.LocalizeEtichettaWizard; }

                RaggruppamentoTeste.Add(item);
            }

            foreach (var item in ExternalDocumento.RaggruppamentoCode)
            {
                foreach (var Dettaglio in item.ListaDettaglio)
                    if (string.IsNullOrEmpty(Dettaglio.Etichetta)) { Dettaglio.Etichetta = StampeKeys.LocalizeEtichettaWizard; }

                RaggruppamentoCode.Add(item);
            }

            RaggruppamentoSelected = RaggruppamentoSelected;
        }
        public DocumentoView CreateANewIstanceOfThis()
        {
            DocumentoView documentoView = new DocumentoView();
            documentoView.RaggruppamentoTeste = new ObservableCollection<Dettaglio>();
            documentoView.RaggruppamentoCode = new ObservableCollection<Dettaglio>();

            foreach (var item in RaggruppamentoTeste)
            {
                documentoView.RaggruppamentoTeste.Add(item);
            }

            foreach (var item in RaggruppamentoCode)
            {
                documentoView.RaggruppamentoCode.Add(item);
            }

            documentoView.RaggruppamentoSelected = RaggruppamentoSelected;

            return documentoView;
        }
        public void ForceCloseOfPopUpsMethod()
        {
            ForceCloseOfPopUps.Invoke(this, new EventArgs());
        }
    }
    public class ComandiPerRiga
    {
        public int IndiceComando { get; set; }
        public string Banda { get; set; }
        public static string Testa = "TESTA";
        public static string Corpo = "CORPO";
        public static string Coda = "CODA";
        public static string Fine = "FINE";

        public event EventHandler<AddDeleteRowEvent> AddDeleteRowHanlder;

        public ComandiPerRiga(string banda)
        {
            Banda = banda;
        }

        private ICommand _AddRowCommand;
        public ICommand AddRowCommand
        {
            get
            {
                return _AddRowCommand ?? (_AddRowCommand = new CommandHandler(param => ExecuteAddRow(param), CanExecuteAddRow()));
            }
        }

        private bool CanExecuteAddRow()
        {
            return true;
        }

        public void ExecuteAddRow(object param)
        {
            AddDeleteRowEvent AddDeleteRowEvent = new AddDeleteRowEvent();
            AddDeleteRowEvent.IsAdding = true;
            AddDeleteRowEvent.IndiceRiga = IndiceComando + 1;
            AddDeleteRowEvent.Banda = Banda;
            AddDeleteRowHanlder.Invoke(this, AddDeleteRowEvent);
        }

        private ICommand _AddUpRowCommand;
        public ICommand AddUpRowCommand
        {
            get
            {
                return _AddUpRowCommand ?? (_AddUpRowCommand = new CommandHandler(param => ExecuteAddUpRow(param), CanExecuteAddUpRow()));
            }
        }

        private bool CanExecuteAddUpRow()
        {
            return true;
        }

        public void ExecuteAddUpRow(object param)
        {
            AddDeleteRowEvent AddDeleteRowEvent = new AddDeleteRowEvent();
            AddDeleteRowEvent.IsAdding = true;
            AddDeleteRowEvent.IndiceRiga = IndiceComando;
            AddDeleteRowEvent.Banda = Banda;
            AddDeleteRowHanlder.Invoke(this, AddDeleteRowEvent);
        }

        private ICommand _DeleteRowCommand;
        public ICommand DeleteRowCommand
        {
            get
            {
                return _DeleteRowCommand ?? (_DeleteRowCommand = new CommandHandler(param => ExecuteDeleteRow(param), CanExecutDeleterow()));
            }
        }

        private bool CanExecutDeleterow()
        {
            return true;
        }

        public void ExecuteDeleteRow(object param)
        {
            ComandiPerRiga comandoRiga = (ComandiPerRiga)param;
            AddDeleteRowEvent AddDeleteRowEvent = new AddDeleteRowEvent();
            AddDeleteRowEvent.IsDeleting = true;
            AddDeleteRowEvent.IndiceRiga = comandoRiga.IndiceComando;
            AddDeleteRowEvent.Banda = Banda;
            AddDeleteRowHanlder.Invoke(this, AddDeleteRowEvent);
        }
    }
    public class AddDeleteRowEvent : EventArgs
    {
        public int IndiceRiga { get; set; }
        public bool IsAdding { get; set; }
        public bool IsDeleting { get; set; }
        public string Banda { get; set; }
    }
    public enum OriginFrom
    {
        FromIntestazioniColonna,
        FromHeaderFooterGroup,
        FromCorpo,
        FromIntestazioniDocumento,
        FromPiePaginaDocumento,
        FromFineDocumento,
    }
}
