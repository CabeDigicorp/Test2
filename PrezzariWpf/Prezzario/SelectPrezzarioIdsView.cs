using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using Model.JoinService;
using PrezzariWpf.Prezzario;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebServiceClient;
using WebServiceClient.Clients;

namespace PrezzariWpf.View
{
    public class SelectPrezzarioIdsView : NotificationBase
    {
        public SelectPrezzarioIdsIOData IOData { get; set; } = null;
        internal PrezzarioView PrezzarioView { get; set; } = null;

        bool _isLoading = false;

        Dictionary<string, PrezzarioInfo> _prezzariInfoDownloaded = new Dictionary<string, PrezzarioInfo>();
        Dictionary<string, PrezzarioInfo> _prezzariInfoClient = new Dictionary<string, PrezzarioInfo>();
        bool ExcludePrezzarioInterno { get; set; }

        Dictionary<string, IDataService> _prezzariCache;
        string _prezzariFolder = string.Empty;

        internal Dictionary<string, PrezzarioInfo> PrezzariInfoDownloaded { get => _prezzariInfoDownloaded; }

        public MessageBarView MessageBarView { get; set; } = new MessageBarView();

        public event EventHandler SelectionChanging;
        protected virtual void OnSelectionChanging(object sender, EventArgs e)
        {
            SelectionChanging?.Invoke(sender, e);
        }
        public event EventHandler SelectionChanged;
        protected virtual void OnSelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }


        private ObservableCollection<ClientPrezzarioInfoView> _prezzarioInfoViewItems = new ObservableCollection<ClientPrezzarioInfoView>();
        public ObservableCollection<ClientPrezzarioInfoView> PrezzarioInfoViewItems
        {
            get { return _prezzarioInfoViewItems; }
            set
            {
                SetProperty(ref _prezzarioInfoViewItems, value);
            }
        }


        internal void Init()
        {
        }


        public void Load()
        {
            _isLoading = true;

            //if (PrezzarioView == null)
            //    return;

            if (IOData == null)
                return;

            if (IOData.AllowPrezzarioInterno)
                InitPrezzarioView(null);

            _prezzariCache = IOData.MainOperation.GetPrezzariCache();

            _prezzariFolder = IOData.MainOperation.GetPrezzariFolder();

            try
            {
                if (!Directory.Exists(_prezzariFolder))
                {
                    Directory.CreateDirectory(_prezzariFolder);
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }
   
            LoadClientPrezzari();

            LoadView();

            LoadAsync();

            SelectedItem = PrezzarioInfoViewItems.FirstOrDefault(item => item.FileName == IOData.ExternalPrezzarioFileName);

            UpdateUI();
            
            _isLoading = false;
        }

        ClientPrezzarioInfoView _selectedItem = null;
        public ClientPrezzarioInfoView SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (!_isLoading)
                        IOData.CurrentViewSettings = new EntityTypeViewSettings();


                    if (_selectedItem != null)
                    {
                        LoadPrezzarioEsternoAsync(_selectedItem.FileName);
                        //UpdateUI(); viene fatta alla fine di LoadPrezzarioEsterno
                    }
                    else if (IOData.AllowPrezzarioInterno)
                    {
                        InitPrezzarioView(null);
                        UpdateUI();
                    }
                    
                }
            }
        }

        public bool IsPrezzarioInterno
        {
            get => SelectedItem == null;
            set
            {
                SelectedItem = null;
                UpdateUI();
            }
        }

        //public bool IsPrezzarioCtrlVisible
        //{
        //    get
        //    {
        //        if (IOData != null && IOData.AllowPrezzarioInterno)
        //            return true;

        //        if (SelectedItem != null)
        //            return true;

        //        return false;
        //    }
        //}

        public bool IsDeletePrezzarioEnabled
        {
            get => !IsPrezzarioInterno;
        }

        public string PrezzarioName
        {
            get
            {
                if (SelectedItem != null)
                    return SelectedItem.FileName;
                else
                    return LocalizationProvider.GetString("PrezzarioInterno");
            }
            

        }

        void LoadClientPrezzari()
        {
            List<string> files = new List<string>(Directory.GetFiles(_prezzariFolder));

            _prezzariInfoClient.Clear();
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".join")
                    continue;

                PrezzarioInfo prezInfo = new PrezzarioInfo();
                prezInfo.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                prezInfo.LastWriteTime = DateTime.MinValue;// fileInfo.LastWriteTime;
                prezInfo.Dimension = fileInfo.Length;

                _prezzariInfoClient.Add(prezInfo.FileName, prezInfo);
            }

            LoadAndUpdateClientManifest();
        }

        void LoadAndUpdateClientManifest()
        {
            ClientPrezzariManifest prezzariManifest = GetClientManifest();

            if (prezzariManifest == null)
                return;

            int i = 0;
            List<int> toDeleteIndexes = new List<int>();

            for (i=0; i<prezzariManifest.Items.Count; i++)
            {
                ClientPrezzariManifestItem prezManifest = prezzariManifest.Items[i];
                if (_prezzariInfoClient.ContainsKey(prezManifest.FileName))
                {
                    _prezzariInfoClient[prezManifest.FileName].Note = prezManifest.Note;
                    _prezzariInfoClient[prezManifest.FileName].Group = prezManifest.Group;
                    _prezzariInfoClient[prezManifest.FileName].Year = prezManifest.Year;
                    _prezzariInfoClient[prezManifest.FileName].LastWriteTime = prezManifest.ServiceLastWriteTime;
                }
                else
                {
                    toDeleteIndexes.Add(i);
                }
            }

            for (i = toDeleteIndexes.Count-1; i>=0;  i--)
            {
                prezzariManifest.Items.RemoveAt(toDeleteIndexes[i]);
            }

            SaveClientPrezzariManifest(prezzariManifest);
        }

        async void LoadAsync()
        {
            await LoadPrezzariInfoDownloaded();

            UpdateUI();
        }

        private async Task LoadPrezzariInfoDownloaded()
        {
            IsConnectionFailedVisible = false;

            List<PrezzarioInfo> prezzariInfoDownloaded = await PrezzariWebClient.GetPrezzariInfoAsync();

            if (PrezzariWebClient.ErrorMessage.Any())
                IsConnectionFailedVisible = true;

            _prezzariInfoDownloaded = prezzariInfoDownloaded.ToDictionary(item => item.FileName, item => item);

            foreach (var modInfoView in _prezzarioInfoViewItems)
            {
                var item = _prezzariInfoClient[modInfoView.FileName];
                UpdateDownloadDates(item, modInfoView);
            }

            UpdateUI();
        }


        async void LoadView()
        {
            //IOData.WindowService.ShowWaitCursor(true);

            //List<PrezzarioInfo> prezzariInfoDownloaded = await PrezzariWebProvider.GetPrezzariInfoAsync();

            //IOData.WindowService.ShowWaitCursor(false);

            //_prezzariInfoDownloaded = prezzariInfoDownloaded.ToDictionary(item => item.FileName, item => item);


            IOrderedEnumerable<PrezzarioInfo> orderedList = _prezzariInfoClient.Values.OrderBy(item => item.FileName);

            _prezzarioInfoViewItems.Clear();
            foreach (var item in orderedList)
            {
                ClientPrezzarioInfoView prezInfoView = new ClientPrezzarioInfoView(this);
                prezInfoView.FileName = item.FileName;
                prezInfoView.Note = item.Note;
                prezInfoView.Group = item.Group;
                prezInfoView.Year = item.Year;

                //UpdateDownloadDates(item, prezInfoView);
                prezInfoView.ClientLastWriteDate = string.Empty;

                _prezzarioInfoViewItems.Add(prezInfoView);
            }


            UpdateUI();
        }

        private void UpdateDownloadDates(PrezzarioInfo item, ClientPrezzarioInfoView prezInfoView)
        {
            prezInfoView.IsUpdateAvaliable = false;
            prezInfoView.ClientLastWriteDate = "-";
            if (_prezzariInfoDownloaded.ContainsKey(item.FileName))
            {
                if (item.LastWriteTime > DateTime.MinValue)
                {
                    prezInfoView.ClientLastWriteDate = item.LastWriteTime.Date.ToShortDateString();
                    prezInfoView.ServiceLastWriteDate = _prezzariInfoDownloaded[item.FileName].LastWriteTime.Date.ToShortDateString();

                    if (item.LastWriteTime < _prezzariInfoDownloaded[item.FileName].LastWriteTime)
                    {
                        //icona aggiornamento disponibile
                        prezInfoView.IsUpdateAvaliable = true;
                    }
                }
            }

        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => PrezzarioInfoViewItems));
            RaisePropertyChanged(GetPropertyName(() => SelectedItemNote));
            RaisePropertyChanged(GetPropertyName(() => SelectedItem));
            RaisePropertyChanged(GetPropertyName(() => IsPrezzarioInterno)); 
            RaisePropertyChanged(GetPropertyName(() => PrezzarioName));
            RaisePropertyChanged(GetPropertyName(() => IsDeletePrezzarioEnabled));
            RaisePropertyChanged(GetPropertyName(() => AllowPrezzarioInterno));
            RaisePropertyChanged(GetPropertyName(() => AllowPrezzariEsterni));
            RaisePropertyChanged(GetPropertyName(() => PrezzariColumnWidth));
            //RaisePropertyChanged(GetPropertyName(() => IsPrezzarioCtrlVisible));




            foreach (ClientPrezzarioInfoView itemView in _prezzarioInfoViewItems)
                itemView.UpdateUI();
        }

        public ClientPrezzariManifest GetClientManifest()
        {
            string prezzariManifestPath = string.Format("{0}\\{1}", _prezzariFolder, "prezzariManifest.json");

            if (!File.Exists(prezzariManifestPath))
            {
                return new ClientPrezzariManifest();
            }

            string json = File.ReadAllText(prezzariManifestPath);

            ClientPrezzariManifest prezzariManifest = null;
            JsonSerializer.JsonDeserialize<ClientPrezzariManifest>(json, out prezzariManifest, typeof(ClientPrezzariManifest));

            if (prezzariManifest == null)
                prezzariManifest = new ClientPrezzariManifest();

            return prezzariManifest;

        }

        internal async void DownloadPrezzari(IEnumerable<string> filenames)
        {
            IOData.WindowService.ShowWaitCursor(true);

            ClientPrezzariManifest prezzariManifest = GetClientManifest();

            foreach (string filename in filenames)
            {
                await DownloadPrezzarioAsync(filename, prezzariManifest);
            }

            SaveClientPrezzariManifest(prezzariManifest);
            Load();

            IOData.WindowService.ShowWaitCursor(false);
            MessageBarView.Ok();
        }

        internal async void DownloadPrezzario(string filename)
        {
            IOData.WindowService.ShowWaitCursor(true);

            ClientPrezzariManifest prezzariManifest = GetClientManifest();
            
            string fileFullName = await DownloadPrezzarioAsync(filename, prezzariManifest);

            SaveClientPrezzariManifest(prezzariManifest);
            Load();

            IOData.WindowService.ShowWaitCursor(false);
            MessageBarView.Ok();
        }

        async Task<string> DownloadPrezzarioAsync(string filename, ClientPrezzariManifest prezzariManifest)
        {
            int prezzariManifestItemIndex = prezzariManifest.Items.FindIndex(item2 => item2.FileName == filename);

            ClientPrezzariManifestItem oldItem = null;
            if (prezzariManifestItemIndex >= 0)
                oldItem = prezzariManifest.Items[prezzariManifestItemIndex];



            string fullFileName = string.Format("{0}\\{1}.join", _prezzariFolder, filename);
            if (File.Exists(fullFileName))
            {
                //check file
                FileInfo fileInfo = new FileInfo(fullFileName);
                var localDate = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));
                //if (oldItem != null && (fileInfo.CreationTime != oldItem.DownloadDate || fileInfo.LastWriteTime != oldItem.DownloadDate))
                if (oldItem != null && (oldItem.DownloadDate != localDate))
                {
                    //il file è stato modificato dopo la data di download
                    string str = string.Format("{0} {1}", filename,  LocalizationProvider.GetString("FileModificatoDopoDownload"));
                    MessageBoxResult res = MessageBox.Show(str, LocalizationProvider.GetString("AppName"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (res == MessageBoxResult.No)
                        return null;
                }
            }
            PrezzariWebClient.ProgressNotify += PrezzariWebProvider_ProgressNotify;


            string errorMsg = await PrezzariWebClient.DownloadFile(fullFileName);
            if (!errorMsg.Any())
            {

                FileInfo fileInfo = new FileInfo(fullFileName);
                var localDate = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));

                ClientPrezzariManifestItem item = new ClientPrezzariManifestItem();
                item.FileName = filename;
                item.DownloadDate = localDate;// fileInfo.CreationTime;//DateTime.Now;
                item.Note = PrezzariInfoDownloaded[filename].Note;
                item.Group = PrezzariInfoDownloaded[filename].Group;
                item.Year = PrezzariInfoDownloaded[filename].Year;
                item.ServiceLastWriteTime = PrezzariInfoDownloaded[filename].LastWriteTime;

                //update client manifest
                if (prezzariManifestItemIndex >= 0)
                    prezzariManifest.Items.RemoveAt(prezzariManifestItemIndex);

                prezzariManifest.Items.Add(item);

                //se presente nella cache lo libero per permettere che venga ricaricato in memoria
                if (_prezzariCache.ContainsKey(filename))
                    _prezzariCache.Remove(filename);
            }
            else
            {
                //error
                //MessageBarView.Show(errorMsg);

            }
            

            return fullFileName;

        }

        private void PrezzariWebProvider_ProgressNotify(object sender, EventArgs e)
        {
            PrezzariWebClientProgressNotifyEventArgs args = e as PrezzariWebClientProgressNotifyEventArgs;

            if (_prezzariInfoDownloaded.ContainsKey(args.FileName))
            {
                long dim = _prezzariInfoDownloaded[args.FileName].Dimension;
                int percent = Convert.ToInt32((args.DownloadedBytes * 100.0) / dim);
                string str = string.Format("{0}: {1}", args.FileName, LocalizationProvider.GetString("DownloadInCorso"));
                MessageBarView.Show(str, false, percent);
            }
        }

        void SaveClientPrezzariManifest(ClientPrezzariManifest prezzariManifest)
        {
            string fullFileName = string.Format("{0}\\{1}", _prezzariFolder, "prezzariManifest.json");

            string json = "";
            JsonSerializer.JsonSerialize(prezzariManifest, out json);

            //File.WriteAllText(fullFileName, json);
            try
            {
                File.WriteAllText(fullFileName, json);
            }
            catch (UnauthorizedAccessException exc)
            {
                //MessageBarView.Show(exc.Message);
                MessageBox.Show(exc.Message, LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string SelectedItemNote
        {
            get
            {
                if (SelectedItem != null)
                    return SelectedItem.Note;
                return string.Empty;

            }
        }

        public ICommand DeletePrezzarioCommand { get { return new CommandHandler(() => this.DeletePrezzario()); } }
        void DeletePrezzario()
        {
            try
            {
                if (_selectedItem == null)
                    return;

                string fullFileName = string.Format("{0}\\{1}.join", _prezzariFolder, _selectedItem.FileName);

                if (File.Exists(fullFileName))
                {
                    File.Delete(fullFileName);
                    Load();
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }

        }

        public ICommand OpenFolderCommand { get { return new CommandHandler(() => this.OpenFolder()); } }
        void OpenFolder()
        {
            //System.Diagnostics.Process.Start(_prezzariFolder);
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = _prezzariFolder;
            process.Start();
        }

        bool _isConnectionFailedVisible = false;
        public bool IsConnectionFailedVisible
        {
            get => _isConnectionFailedVisible;
            set => SetProperty(ref _isConnectionFailedVisible, value);
        }

        public void InitPrezzarioView(ClientDataService prezDataService)
        {
            PrezzarioView.CalculatorFunctions.Add(IOData.NoteCalculatorFunction);
            PrezzarioView.CalculatorFunctions.Add(IOData.EPCalculatorFunction);

            if (prezDataService == null) //inizializzo prezzario interno
            {
                PrezzarioView.DataService = IOData.DataService;
                PrezzarioView.WindowService = IOData.WindowService;

            }
            else //inizializzo prezzario esterno
            {
                PrezzarioView.DataService = prezDataService;
                PrezzarioView.WindowService = IOData.WindowService.CreateWindowService(prezDataService, null, IOData.MainOperation);
            }

            //PrezzarioView.WindowService = IOData.WindowService;
            PrezzarioView.ModelActionsStack = IOData.ModelActionsStack;
            PrezzarioView.MainOperation = IOData.MainOperation;
            PrezzarioView.Init(IOData.CurrentViewSettings);
            //PrezzarioView.Init(null);//per ora non imposto filtri e ordinamento all'apertura del dialogo

            PrezzarioView.ItemsView.RightPanesView.ClosePanes();

            HashSet<Guid> prezzarioItemSelectedIds = new HashSet<Guid>(IOData.PrezzarioItemSelectedIds);
            if (prezzarioItemSelectedIds.Contains(Guid.Empty))
            {
                PrezzarioView.ItemsView.SetNoSelectionChecked(true);
                prezzarioItemSelectedIds.Remove(Guid.Empty);
            }
            else
                PrezzarioView.ItemsView.SetNoSelectionChecked(false);


            //se c'è un filtro o ricerca attiva vengono selezonate le voci risultato della ricerca al posto di quelle di prezzarioItemSelectedIds
            if (PrezzarioView.ItemsView.RightPanesView.FilterView.AnyFilter)
            {
                PrezzarioView.ItemsView.CheckedEntitiesId = PrezzarioView.ItemsView.RightPanesView.FilterView.FoundEntitiesId.ToHashSet();
                PrezzarioView.ItemsView.ExpandCheckedEntities();

            }
            else
                PrezzarioView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(prezzarioItemSelectedIds);

            //PrezzarioView.ItemsView.CheckedEntitiesId = new HashSet<Guid>(prezzarioItemSelectedIds);


            PrezzarioView.ItemsView.AllowNoSelection = IOData.AllowNoSelection;
            //PrezzarioView.ItemsView.SetNoSelectionChecked(true);
            PrezzarioView.ItemsView.IsSingleSelection = IOData.IsSingleSelection;
            PrezzarioView.ItemsView.IsImportItemsEnabled = false;

            Guid currentPrezzarioId = IOData.PrezzarioItemSelectedIds.FirstOrDefault();
            PrezzarioView.ItemsView.SelectEntityById(currentPrezzarioId);

        }


        public bool AllowPrezzarioInterno { get => IOData == null ? false : IOData.AllowPrezzarioInterno; }

        public bool AllowPrezzariEsterni { get => IOData == null ? false : IOData.AllowPrezzariEsterni; }

        public double PrezzariColumnWidth { get => (IOData != null && IOData.AllowPrezzariEsterni) ? 200 : 0; }


        async void LoadPrezzarioEsternoAsync(string filename)
        {

            ///oss: Questo azzeramento serve per bloccare qualsiasi aggiornamento della vista di ListView.
            //questo è necesssario per non consentire più di aggiornare gli ItemContainer durante la selezione di un'altro prezzario
            if (PrezzarioView != null)
                PrezzarioView.ItemsView.VirtualEntities = null;

            IOData.WindowService.ShowWaitCursor(true);

            if (!_prezzariCache.ContainsKey(filename))
            {
                await Task.Run(() =>
                {

                    IOData.MainOperation.ProgressChanged += MainOperation_ProgressChanged;

                    //open prezzario
                    string fullFileName = string.Format("{0}\\{1}.join", _prezzariFolder, filename);
                    IDataService ds = IOData.MainOperation.GetDataServiceByFile(fullFileName, out _);
                    if (ds != null)
                        _prezzariCache.Add(filename, ds);

                    IOData.MainOperation.ProgressChanged -= MainOperation_ProgressChanged;

                });
            }


            IDataService dataService = _prezzariCache[filename];

            if (dataService == null)
            {
                IOData.MainOperation.ShowMessageBarView(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                return;
            }

            ClientDataService clientDataService = new ClientDataService(dataService, new ModelActionsStack(dataService));
            clientDataService.IsReadOnly = true;
            clientDataService.Init();

            if (PrezzarioView != null)
                InitPrezzarioView(clientDataService);
                
            MessageBarView.Ok();

            UpdateUI();


            IOData.WindowService.ShowWaitCursor(false);
        }

        private void MainOperation_ProgressChanged(object sender, EventArgs e)
        {
            ProgressChangedEventArgs args = e as ProgressChangedEventArgs;
            MessageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, args.ProgressPercentage);
        }
    }

    public class SelectPrezzarioIdsIOData
    {
        public ClientDataService DataService { get; set; } = null;
        public IMainOperation MainOperation { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public PrezzarioView PrezzarioView { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        /// <summary>
        /// Items selezionati (checkati) alla chiusura o apertura del dialogo
        /// </summary>
        public List<Guid> PrezzarioItemSelectedIds { get; set; } = new List<Guid>();

        /// <summary>
        /// POssibilità di selezionare solo un elemento
        /// </summary>
        public bool IsSingleSelection { get; set; } = false;

        /// <summary>
        /// Possibilità di assegnare nessun articolo
        /// </summary>
        public bool AllowNoSelection { get; set; } = false;

        /// <summary>
        /// Impostazioni di Filtro, Ordine e Raggruppamento all'avvio del dialogo 
        /// </summary>
        public EntityTypeViewSettings CurrentViewSettings { get; set; } = new EntityTypeViewSettings();

        public NoteCalculatorFunction NoteCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;

        /// <summary>
        /// Permette la selezione di un articolo dal prezzario interno
        /// </summary>
        public bool AllowPrezzarioInterno { get; set; } = false;

        /// <summary>
        /// Permette la selezione di un articolo dai prezzari esterni (cartella predefinita)
        /// </summary>
        public bool AllowPrezzariEsterni { get; set; } = false;

        /// <summary>
        /// Nome file (Friuli2019) del prezzario a cui appartengono gli id selezionati.
        /// Empty se appartengono dal prezzario interno
        /// </summary>
        public string ExternalPrezzarioFileName { get; set; } = string.Empty;



  }


}
