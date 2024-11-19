using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServiceClient;
using WebServiceClient.Clients;

namespace MainApp
{
    public class ProgettiWebView : NotificationBase
    {
        Guid _operaId = Guid.Empty;

        ObservableCollection<ProgettoView> _progettiItems = new ObservableCollection<ProgettoView>();
        public ObservableCollection<ProgettoView> ProgettiItems { get => _progettiItems; set => SetProperty(ref _progettiItems, value); }

        private IEnumerable<ModelData.Dto.ProgettoDto> _progettiInfo = null;


        public async void Load(Guid operaId)
        {
            _operaId = operaId;

            _progettiInfo = await ProgettiWebClient.GetProgetti(_operaId);

            if (_progettiInfo == null)
                return;

            //NomeProgetto = String.Empty;
            SelectedItem = null;

            _progettiItems = new ObservableCollection<ProgettoView>(_progettiInfo.Select(x => new ProgettoView() { Id = x.Id, Nome = x.Nome }));
            UpdateUI();
        }

        string _nomeProgetto= string.Empty;
        public string NomeProgetto
        {
            get => _nomeProgetto;
            set => SetProperty(ref _nomeProgetto, value);
        }

        bool _isNomeProgettoReadOnly = false;
        public bool IsNomeProgettoReadOnly
        { 
            get => _isNomeProgettoReadOnly;
            set => SetProperty(ref _isNomeProgettoReadOnly, value);
        }

        Guid _progettoId = Guid.Empty;
        public Guid ProgettoId
        {
            get => _progettoId;
            set => SetProperty(ref _progettoId, value);
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ProgettiItems));
            RaisePropertyChanged(GetPropertyName(() => NomeProgetto));
        }

        ProgettoView _progettoView = null;
        public ProgettoView SelectedItem
        {
            get => _progettoView;
            set
            {
                if (SetProperty(ref _progettoView, value))
                {
                    if (_progettoView != null)
                    {
                        NomeProgetto = _progettoView.Nome;
                        ProgettoId = _progettoView.Id;
                    }
                    else
                    {
                        NomeProgetto = string.Empty;
                        ProgettoId = Guid.Empty;
                    }
                }
            }
        }

    }

    public class ProgettoView
    {
        public Guid Id  { get; set; }
        public string Nome { get; set; }
    }
}
