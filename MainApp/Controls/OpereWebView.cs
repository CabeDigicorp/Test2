using Commons;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebServiceClient;
using WebServiceClient.Clients;

namespace MainApp
{
    public class OpereWebView : NotificationBase
    {
        ObservableCollection<OperaView> _opereItems = new ObservableCollection<OperaView>();
        public ObservableCollection<OperaView> OpereItems { get => _opereItems; set => SetProperty(ref _opereItems, value); }

        private IEnumerable<ModelData.Dto.OperaDto> _opereInfo = null;

        private Dictionary<Guid, ModelData.Dto.TagDto> _tagsInfo = null;

        public string GestioneOpereWebUILink { get => string.Format("{0}/opere", ServerAddress.WebUICurrent); }

        public event EventHandler ErrorMsg;

        internal async void Load()
        {
            GenericResponse gr = new GenericResponse(false);
            
            
            try
            {
                _opereInfo = await OpereWebClient.GetOpere(gr);

                _tagsInfo = (await TagsWebClient.GetTags())?.ToDictionary(k => k.Id, v => v);

                if (_opereInfo != null && _tagsInfo != null)
                    LoadSearched();

            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                gr.Set(false, exc.Message);
            }

            if (!gr.Success)
                OnOpereWebViewMessage(new OpereWebViewMessageEventArgs() { Message = gr.Message });

        }

        protected void OnOpereWebViewMessage(EventArgs e)
        {
            ErrorMsg?.Invoke(this, e);
        }

        private void LoadSearched()
        {
            _ = Task.Run(() =>
            {
                //ricerca per nome e tag

                HashSet<Guid> tagsIdSearched = new HashSet<Guid>(_tagsInfo.Values.Where(x => x.Nome.ToUpper().Contains(SearchText.ToUpper())).Select(x => x.Id));

                _opereItems = new ObservableCollection<OperaView>(_opereInfo.Where(x => string.IsNullOrEmpty(SearchText)
                                                                                    || x.Nome.ToUpper().Contains(SearchText.ToUpper())
                                                                                    || x.TagIds.Intersect(tagsIdSearched).Any())
                                                                           .Select(x => new OperaView()
                                                                           {
                                                                               Id = x.Id,
                                                                               Nome = x.Nome,
                                                                               Tags = "# " + string.Join(", ", x.TagIds.Select(tagId =>
                                                                               {
                                                                                   if (_tagsInfo.ContainsKey(tagId))
                                                                                       return (_tagsInfo[tagId].Nome);
                                                                                   else
                                                                                       return string.Empty;
                                                                               }))                                                                          }
                                                                           ));
                UpdateUI();
            });
        }

        OperaView _selectedItem = null;
        public OperaView SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadSearched();
                }
            }

        }

        public ICommand ClearSearchTextCommand
        {
            get
            {
                return new CommandHandler(() => this.ClearSearchText());
            }
        }
        void ClearSearchText()
        {
            SearchText = String.Empty;
        }


        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(()=>OpereItems));
        }
    }

    public class OperaView
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Tags { get; set; }
    }

    public class OpereWebViewMessageEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }
}
