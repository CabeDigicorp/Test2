using CommonResources;
using Commons;
using MasterDetailView;
using Model;
using Model.JoinService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WebServiceClient;
using WebServiceClient.Clients;

namespace MainApp
{
    public class ProjectModelView : NotificationBase
    {
        //public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        Dictionary<string, ModelloInfo> _modelliInfoDownloaded = new Dictionary<string, ModelloInfo>();
        Dictionary<string, ModelloInfo> _modelliInfoClient = new Dictionary<string, ModelloInfo>();
        string _modelliFolder = string.Empty;

        internal Dictionary<string, ModelloInfo> ModelliInfoDownloaded { get => _modelliInfoDownloaded; }

        public MessageBarView MessageBarView { get; set; } = new MessageBarView();

        string ManifestFileName { get => "modelliManifest.json"; }

        ClientModelliManifest _modelliManifest = null;

        static public char TagSeparator { get => ',' ;}

        public string TagNothing { get; set; }

        public event EventHandler CurrentModelloChanged;


        public ProjectModelView()
        {
            TagNothing = LocalizationProvider.GetString("_Nessuno");
        }

        /// <summary>
        /// Valori visualizzati nella lista dei tag
        /// </summary>
        ObservableCollection <ClientTagView> _tagsView = new ObservableCollection<ClientTagView>();
        public ObservableCollection<ClientTagView> TagsView
        {
            get
            {
                return _tagsView;
            }

            set
            {
                SetProperty(ref _tagsView, value);
            }
        }

        private ObservableCollection<ClientModelloInfoView> _modelloInfoViewItems = new ObservableCollection<ClientModelloInfoView>();
        public ObservableCollection<ClientModelloInfoView> ModelloInfoViewItems
        {
            get { return _modelloInfoViewItems; }
            set
            {
                SetProperty(ref _modelloInfoViewItems, value);
            }
        }

        //public void Load()
        //{
        //    if (MainOperation == null)
        //        return;

        //    if (WindowService == null)
        //        return;

        //    _modelliFolder = MainOperation.GetModelliFolder();

        //    try
        //    {
        //        if (!Directory.Exists(_modelliFolder))
        //        {
        //            Directory.CreateDirectory(_modelliFolder);
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
        //    }

            
            
        //    LoadClientModelli();

        //    LoadTagsView();

        //    LoadView();

        //    LoadModelliInfoAsync();

        //    if (ModelloInfoViewItems.Any())
        //        CurrentModello = ModelloInfoViewItems[0];

        //}

        public void Load()
        {
            LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (MainOperation == null)
                return;

            if (WindowService == null)
                return;

            _modelliFolder = MainOperation.GetModelliFolder();

            try
            {
                if (!Directory.Exists(_modelliFolder))
                {
                    Directory.CreateDirectory(_modelliFolder);
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
            }



            LoadClientModelli();

            LoadTagsView();

            LoadView();

            await LoadModelliInfoAsync();

            if (ModelloInfoViewItems.Any())
                CurrentModello = ModelloInfoViewItems[0];

        }

        async Task LoadModelliInfoAsync()
        {
            await LoadModelliInfoDownloaded();

            UpdateUI();
        }



        //async Task LoadModelli()
        //{
        //    LoadClientModelli();

        //    await LoadModelliInfoDownloaded();

        //    WindowService.ShowWaitCursor(false);
        //}


        //Modello corrente
        ClientModelloInfoView _currentModello = null;
        public ClientModelloInfoView CurrentModello
        {
            get => _currentModello;
            set
            {
                if (SetProperty(ref _currentModello, value))
                {
                    UpdateUI();
                    OnCurrentModelloChanged(new EventArgs());
                }
            }
        }

        //modelli selezionati
        List<ClientModelloInfoView> _selectedModelli = new List<ClientModelloInfoView>();
        public List<ClientModelloInfoView> SelectedModelli
        {
            get => _selectedModelli;
            set
            {
                if (SetProperty(ref _selectedModelli, value))
                {
                }
            }
        }

        //tags selezionati
        List<ClientTagView> _selectedTags = new List<ClientTagView>();
        public List<ClientTagView> SelectedTags
        {
            get => _selectedTags;
            set
            {
                if (SetProperty(ref _selectedTags, value))
                {
                }
            }
        }


        //Tag corrente
        ClientTagView _currentTag = null;
        public ClientTagView CurrentTag
        {
            get => _currentTag;
            set
            {
                if (SetProperty(ref _currentTag, value))
                {
                }
            }
        }


        internal void LoadView()
        {
            string currentModelloFileName = _currentModello == null ? string.Empty : _currentModello.FileName;

            IOrderedEnumerable<ModelloInfo> orderedList = _modelliInfoClient.Values.OrderBy(item => item.UserName).ThenByDescending(item => item.MinAppVersion); ;

            string deploymentVersion = MainOperation.GetDeploymentVersion();

            HashSet<string> added = new HashSet<string>();

            _modelloInfoViewItems.Clear();
            foreach (var item in orderedList)
            {
                if (string.Compare(deploymentVersion, item.MinAppVersion) < 0)
                    continue;

                if (added.Contains(item.UserName))
                    continue;

                if (!CheckFilter(item))
                    continue;

                ClientModelloInfoView modInfoView = new ClientModelloInfoView(this);
                modInfoView.FileName = item.FileName;
                modInfoView.UserName = item.UserName;
                modInfoView.Note = item.Note;
                modInfoView.Tags = item.Tags;

                modInfoView.ClientLastWriteDate = string.Empty;
                //UpdateDownloadDates(item, modInfoView);

                _modelloInfoViewItems.Add(modInfoView);
                
                added.Add(item.UserName);

            }

            CurrentModello = _modelloInfoViewItems.FirstOrDefault(item => item.FileName == currentModelloFileName);

            UpdateUI();
        }


        private void UpdateDownloadDates(ModelloInfo item, ClientModelloInfoView modInfoView)
        {
            modInfoView.IsUpdateAvaliable = false;
            modInfoView.ClientLastWriteDate = "-";
            if (_modelliInfoDownloaded.ContainsKey(item.FileName))
            {

                IOrderedEnumerable<ModelloInfo> ordered = _modelliInfoDownloaded.Values.Where(item1 => item1.UserName == item.UserName).OrderByDescending(item1 => item1.MinAppVersion);
                if (ordered != null && ordered.Any())
                {
                    ModelloInfo modInfo = ordered.FirstOrDefault();
                    if (string.Compare(item.MinAppVersion, modInfo.MinAppVersion) < 0)
                    {
                        modInfoView.IsUpdateAvaliable = true;
                        return;
                    }
                }

                if (item.LastWriteTime > DateTime.MinValue)
                {
                    modInfoView.ClientLastWriteDate = item.LastWriteTime.Date.ToShortDateString();
                    modInfoView.ServiceLastWriteDate = _modelliInfoDownloaded[item.FileName].LastWriteTime.Date.ToShortDateString();

                    if (item.LastWriteTime < _modelliInfoDownloaded[item.FileName].LastWriteTime)
                    {
                        //icona aggiornamento disponibile
                        modInfoView.IsUpdateAvaliable = true;
                    }
                }
            }
        }

        private async Task LoadModelliInfoDownloaded()
        {
            //WindowService.ShowWaitCursor(true);
            IsConnectionFailedVisible = false;

            List<ModelloInfo> modelliInfoDownloaded = await ModelliWebClient.GetModelliInfoAsync();

            if (ModelliWebClient.ErrorMessage.Any())
            {
                IsConnectionFailedVisible = true;
                UpdateUI();
            }

            _modelliInfoDownloaded = modelliInfoDownloaded.ToDictionary(item => item.FileName, item => item);

            foreach (var modInfoView in _modelloInfoViewItems)
            {
                var item = _modelliInfoClient[modInfoView.FileName];
                UpdateDownloadDates(item, modInfoView);
            }


            UpdateUI();

            //WindowService.ShowWaitCursor(false);
        }

        void LoadTagsView()
        {

            if (_modelliManifest == null)
                return;

            HashSet<string> tags = new HashSet<string>(_modelliManifest.Items.SelectMany(item => item.Tags));

            List<string> tagsSorted = new List<string>(tags);
            tagsSorted.Sort();

            TagsView.Clear();

            var t = new ClientTagView(this) { IsFiltered = false };
            t.SetName(TagNothing);
            TagsView.Add(t);

            foreach (string tag in tagsSorted)
            {
                ClientTagView tagView = new ClientTagView(this) { Name = tag, IsFiltered = false };
                TagsView.Add(tagView);
            }

        }

        void LoadClientModelli()
        {
            List<string> files = new List<string>(Directory.GetFiles(_modelliFolder));

            _modelliInfoClient.Clear();
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension != ".join")
                    continue;

                ModelloInfo modInfo = new ModelloInfo();
                modInfo.FileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                modInfo.UserName = modInfo.FileName;
                modInfo.LastWriteTime = DateTime.MinValue;// fileInfo.LastWriteTime;
                modInfo.Dimension = fileInfo.Length;

                _modelliInfoClient.Add(modInfo.FileName, modInfo);
            }

            LoadAndUpdateClientManifest();
        }

        public void LoadAndUpdateClientManifest(bool load = true)
        {
            //legge il manifest
            ClientModelliManifest modelliManifest = GetClientManifest();

            if (modelliManifest == null)
                return;

            int i = 0;
            List<int> toDeleteIndexes = new List<int>();

            HashSet<string> modManifestFilesName = new HashSet<string>();

            //aggiorna _modelliInfoClient (modelliManifest -> _modelliInfoClient)
            for (i = 0; i < modelliManifest.Items.Count; i++)
            {
                ClientModelliManifestItem modManifest = modelliManifest.Items[i];
                if (_modelliInfoClient.ContainsKey(modManifest.FileName))
                {
                    if (load)
                    {
                        _modelliInfoClient[modManifest.FileName].Note = modManifest.Note;
                        _modelliInfoClient[modManifest.FileName].Tags = modManifest.Tags;
                        _modelliInfoClient[modManifest.FileName].LastWriteTime = modManifest.ServiceLastWriteTime;
                        _modelliInfoClient[modManifest.FileName].MinAppVersion = modManifest.MinAppVersion;
                        if (modManifest.UserName != null && modManifest.UserName.Any())
                            _modelliInfoClient[modManifest.FileName].UserName = modManifest.UserName;
                    }
                    else
                    {
                        modManifest.Note = _modelliInfoClient[modManifest.FileName].Note;
                        modManifest.Tags = _modelliInfoClient[modManifest.FileName].Tags;
                        modManifest.MinAppVersion = _modelliInfoClient[modManifest.FileName].MinAppVersion;
                        modManifest.UserName = _modelliInfoClient[modManifest.FileName].UserName;
                    }


                    modManifestFilesName.Add(modManifest.FileName);
                }
                else
                {
                    toDeleteIndexes.Add(i);
                }
            }

            //rimuovo nel manifest i modelli che non ci sono più nel client
            for (i = toDeleteIndexes.Count - 1; i >= 0; i--)
            {
                modelliManifest.Items.RemoveAt(toDeleteIndexes[i]);
            }

            //aggiungo nel manifest i modelli che trovo nel client e non ancora nel manifest
            foreach (ModelloInfo modInfo in _modelliInfoClient.Values)
            {
                if (!modManifestFilesName.Contains(modInfo.FileName))
                {
                    ClientModelliManifestItem newModManifest = new ClientModelliManifestItem();
                    newModManifest.FileName = modInfo.FileName;
                    newModManifest.Note = modInfo.Note;
                    newModManifest.Tags = modInfo.Tags;
                    //newModManifest.DownloadDate = modInfo.
                    //newModManifest.ServiceLastWriteTime = modInfo.

                    modelliManifest.Items.Add(newModManifest);

                }
            }

            _modelliManifest = modelliManifest;

            SaveClientModelliManifest(modelliManifest);

        }



        void SaveClientModelliManifest(ClientModelliManifest modelliManifest)
        {
            string fullFileName = string.Format("{0}\\{1}", _modelliFolder, ManifestFileName);

            string json = "";
            JsonSerializer.JsonSerialize(modelliManifest, out json);

            //File.WriteAllText(fullFileName, json);
            try
            {
                File.WriteAllText(fullFileName, json);
            }
            catch (UnauthorizedAccessException exc)
            {
                //MainOperation.ShowMessageBarView(exc.Message);
                MessageBox.Show(exc.Message, LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        public ClientModelliManifest GetClientManifest()
        {
            string modelliManifestPath = string.Format("{0}\\{1}", _modelliFolder, ManifestFileName);

            if (!File.Exists(modelliManifestPath))
            {
                return new ClientModelliManifest();
            }

            string json = File.ReadAllText(modelliManifestPath);

            ClientModelliManifest modelliManifest = null;
            JsonSerializer.JsonDeserialize<ClientModelliManifest>(json, out modelliManifest, typeof(ClientModelliManifest));

            if (modelliManifest == null)
                modelliManifest = new ClientModelliManifest();

            return modelliManifest;

        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ModelloInfoViewItems));
            RaisePropertyChanged(GetPropertyName(() => CurrentModelloNote));
            RaisePropertyChanged(GetPropertyName(() => IsNotaInEditMode));
            RaisePropertyChanged(GetPropertyName(() => IsAcceptButtonEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsConnectionFailedVisible));

            foreach (ClientModelloInfoView itemView in _modelloInfoViewItems)
                itemView.UpdateUI();
        }

        bool CheckFilter(ModelloInfo mod)
        {
            if (!TagsView.Any(item => item.IsFiltered))
                return true;

            HashSet<string> tagsFiltered = new HashSet<string>(TagsView.Where(item => item.IsFiltered).Select(item => item.Name));
            foreach (string modTag in mod.Tags)
            {
                if (tagsFiltered.Contains(modTag))
                    return true;
            }

            if (!mod.Tags.Any() && tagsFiltered.Contains(TagNothing))
                return true;


            return false;
        }

        internal async void DownloadModelli(IEnumerable<string> filenames)
        {
            WindowService.ShowWaitCursor(true);

            ClientModelliManifest modelliManifest = GetClientManifest();

            foreach (string filename in filenames)
            {
                await DownloadModelloAsync(filename, modelliManifest);
            }

            SaveClientModelliManifest(modelliManifest);
            Load();

            WindowService.ShowWaitCursor(false);
            MessageBarView.Ok();
        }
        internal async void DownloadModello(string filename)
        {      
            WindowService.ShowWaitCursor(true);

            bool ret = await DownloadModelloAsync(filename);

            WindowService.ShowWaitCursor(false);
            MessageBarView.Ok();
        }

        internal async Task<bool> DownloadModelloAsync(string filename)
        {
            //WindowService.ShowWaitCursor(true);

            string newFileName = filename;

            //Vedo se esiste un file con stesso userName ma di una versione successiva e scarico questa
            if (_modelliInfoDownloaded.ContainsKey(filename))
            {
                string userName = _modelliInfoDownloaded[filename].UserName;
                string minAppVersion = _modelliInfoDownloaded[filename].MinAppVersion;

                IOrderedEnumerable<ModelloInfo> ordered = _modelliInfoDownloaded.Values.Where(item1 => item1.UserName == userName).OrderByDescending(item1 => item1.MinAppVersion);
                if (ordered != null)
                {
                    ModelloInfo modInfo = ordered.FirstOrDefault();
                    if (string.Compare(minAppVersion, modInfo.MinAppVersion) < 0)
                        newFileName = modInfo.FileName;
                }

            }


            ClientModelliManifest modelliManifest = GetClientManifest();

            string fileFullName = await DownloadModelloAsync(newFileName, modelliManifest);

            SaveClientModelliManifest(modelliManifest);
            Load();

            return File.Exists(fileFullName);

            //WindowService.ShowWaitCursor(false);
            //MessageBarView.Ok();
        }

        async Task<string> DownloadModelloAsync(string remoteFilename, ClientModelliManifest modelliManifest)
        {

            if (!_modelliInfoDownloaded.ContainsKey(remoteFilename))
            {
                Debug.WriteLine(remoteFilename);
                return null;
            }

            //string clientFileName = _modelliInfoDownloaded[remoteFilename].UserName;
            string clientFileName = remoteFilename;

            int modelliManifestItemIndex = modelliManifest.Items.FindIndex(item2 => item2.FileName == clientFileName);

            ClientModelliManifestItem oldItem = null;
            if (modelliManifestItemIndex >= 0)
                oldItem = modelliManifest.Items[modelliManifestItemIndex];

            string clientFullFileName = string.Format("{0}\\{1}.join", _modelliFolder, clientFileName);
            if (File.Exists(clientFullFileName))
            {
                //check file
                FileInfo fileInfo = new FileInfo(clientFullFileName);
                //if (oldItem != null && (fileInfo.CreationTime != oldItem.DownloadDate || fileInfo.LastWriteTime != oldItem.DownloadDate))
                var localDate = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));
                if (oldItem != null && (oldItem.DownloadDate != localDate))
                {
                    //il file è stato modificato dopo la data di download
                    string str = string.Format("\"{0}\" {1}", clientFileName, LocalizationProvider.GetString("FileModificatoDopoDownload"));
                    MessageBoxResult res = MessageBox.Show(str, LocalizationProvider.GetString("AppName"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (res == MessageBoxResult.No)
                        return null;
                }
            }
            ModelliWebClient.ProgressNotify += ModelliWebProvider_ProgressNotify;

            string errorMsg = await ModelliWebClient.DownloadFile(remoteFilename, clientFullFileName);
            if (!errorMsg.Any())
            {

                FileInfo fileInfo = new FileInfo(clientFullFileName);
                var localDate = new DateTime(Math.Max(fileInfo.CreationTime.Ticks, fileInfo.LastWriteTime.Ticks));

                ClientModelliManifestItem item = new ClientModelliManifestItem();
                item.FileName = clientFileName;
                item.DownloadDate = localDate;// DateTime.Now;//
                item.Note = ModelliInfoDownloaded[remoteFilename].Note;
                item.Tags = ModelliInfoDownloaded[remoteFilename].Tags;
                item.ServiceLastWriteTime = ModelliInfoDownloaded[remoteFilename].LastWriteTime;
                item.MinAppVersion = ModelliInfoDownloaded[remoteFilename].MinAppVersion;
                item.UserName = ModelliInfoDownloaded[remoteFilename].UserName;

                //update client manifest
                if (modelliManifestItemIndex >= 0)
                    modelliManifest.Items.RemoveAt(modelliManifestItemIndex);

                modelliManifest.Items.Add(item);

                return clientFullFileName;

            }
            else
            {
                //error
                //MessageBarView.Show(errorMsg);

            }

            return null;

        }

        private void ModelliWebProvider_ProgressNotify(object sender, EventArgs e)
        {
            ModelliWebClientProgressNotifyEventArgs args = e as ModelliWebClientProgressNotifyEventArgs;

            if (_modelliInfoDownloaded.ContainsKey(args.FileName))
            {
                long dim = _modelliInfoDownloaded[args.FileName].Dimension;
                int percent = Convert.ToInt32((args.DownloadedBytes * 100.0) / dim);
                string str = string.Format("{0}: {1}", args.FileName, LocalizationProvider.GetString("DownloadInCorso"));
                MessageBarView.Show(str, false, percent);
            }
        }

        #region Modelli


        public ICommand SelectAllModelsCommand { get => new CommandHandler(() => this.SelectAllModels()); }
        void SelectAllModels()
        {

        }

        public ICommand RemoveModelCommand { get => new CommandHandler(() => this.RemoveModels()); }
        void RemoveModels()
        {
            try
            {
                if (_currentModello == null)
                    return;

                string fullFileName = string.Format("{0}\\{1}.join", _modelliFolder, _currentModello.FileName);

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

        public ICommand OpenFolderCommand { get => new CommandHandler(() => this.OpenFolder()); }
        void OpenFolder()
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = _modelliFolder;
            process.Start();
            //System.Diagnostics.Process.Start(_modelliFolder);
        }

        bool _isTagVisible = false;
        public bool IsTagVisible
        {
            get => _isTagVisible;
            set
            {
                if (SetProperty(ref _isTagVisible, value))
                    UpdateUI();
            }
        }


        bool _isNotaInEditMode = false;
        public bool IsNotaInEditMode
        {
            get => _isNotaInEditMode;
            set
            {
                if (CurrentModello != null)
                    SetProperty(ref _isNotaInEditMode, value);
            }
        }

        public string CurrentModelloNote
        {
            get => CurrentModello==null ? string.Empty : CurrentModello.Note;
            set
            {
                if (CurrentModello != null)
                {
                    _modelliInfoClient[CurrentModello.FileName].Note = value;
                    IsNotaInEditMode = false;
                    LoadView();
                }
            }
        }
        public ICommand LostFocusCommand { get => new CommandHandler(() => this.LostFocus()); }
        void LostFocus()
        {
            IsNotaInEditMode = false;
        }

        #endregion Modelli


        #region Tag
        public ICommand AddTagCommand { get => new CommandHandler(() => this.AddTag()); }
        void AddTag()
        {
            if (TagsView.FirstOrDefault(item => item.Name == string.Empty) != null)
                return;


            ClientTagView tagView = new ClientTagView(this)
            {
                Name = LocalizationProvider.GetString("Nuovo"),
                IsFiltered = false
            };
            TagsView.Add(tagView);
            CurrentTag = tagView;
        }
        
        //public ICommand SelectAllTagsCommand { get => new CommandHandler(() => this.SelectAllTags()); }
        //void SelectAllTags()
        //{

        //}

        public ICommand RemoveTagsCommand { get => new CommandHandler(() => this.RemoveTags()); }
        void RemoveTags()
        {
            foreach (ClientTagView tag in SelectedTags)
            {
                if (tag.Name == TagNothing)
                    continue;

                TagsView.Remove(tag);

                foreach (ModelloInfo mod in _modelliInfoClient.Values)
                {
                    RemoveModelloTag(mod, tag.Name);
                }
                
            }

            //LoadTagsView();
            LoadView();
        }

        public void RemoveModelloTag(string fileName, string tag)
        {
            ModelloInfo mod = _modelliInfoClient.Values.FirstOrDefault(item => item.FileName == fileName);
            RemoveModelloTag(mod, tag);
        }

        public void RemoveModelloTag(ModelloInfo mod, string tag)
        {
            if (mod == null)
                return;

            if (mod.Tags.Contains(tag))
                mod.Tags.Remove(tag);
        }

        internal void RenameTag(string oldName, string name)
        {
            foreach (ModelloInfo mod in _modelliInfoClient.Values)
            {
                int index = mod.Tags.IndexOf(oldName);
                if (index >= 0)
                    mod.Tags[index] = name;
            }
            LoadView();
        }

        internal bool IsValidName(string name)
        {
            if (TagsView.FirstOrDefault(item => item.Name == name) != null)
                return false;

            if (name.Contains(TagSeparator))
                return false;

            if (name.Contains(TagNothing))
                return false;

            return true;
        }

        internal string GetValidName(string name)
        {
            string tmpName = name;

            if (!(tmpName.Trim()).Any())
                tmpName = LocalizationProvider.GetString("Nuovo");

            if (tmpName.Contains(TagSeparator))
                tmpName = tmpName.Replace(TagSeparator, ';');

            if (tmpName.Contains(TagNothing))
                tmpName = tmpName.Replace(TagNothing, "");


            while (TagsView.FirstOrDefault(item => item.Name == tmpName) != null)
            {
                tmpName += string.Format(" - {0}", LocalizationProvider.GetString("Copia2"));
            }

            return tmpName;
        }

        internal string GetCurrentModelloFullFileName()
        {
            string fileName = CurrentModello == null ? string.Empty : CurrentModello.FileName;
            if (!fileName.Any())
                return string.Empty;
            
            string fullFileName = string.Format("{0}\\{1}.join", _modelliFolder, fileName);
            return fullFileName;
        }
        #endregion Tag

        public bool IsAcceptButtonEnabled
        {
            get => CurrentModello != null;
        }

        protected void OnCurrentModelloChanged(EventArgs e)
        {
            CurrentModelloChanged?.Invoke(this, e);
        }

        bool _isConnectionFailedVisible = false;
        public bool IsConnectionFailedVisible 
        {
            get => _isConnectionFailedVisible;
            set => SetProperty(ref _isConnectionFailedVisible, value);
        }

        public bool IsUpdateAvailable(string modelFileName)
        {
            var modelloInfo = _modelloInfoViewItems.FirstOrDefault(item => item.FileName == modelFileName);
            if (modelloInfo != null)
            {
                return modelloInfo.IsUpdateAvaliable;
            }

            return false;
        }

    }

    public class ClientTagView : NotificationBase
    {
        ProjectModelView _owner = null;

        public ClientTagView(ProjectModelView owner)
        {
            _owner = owner;
        }


        bool _isFiltered = false;
        public bool IsFiltered
        {
            get => _isFiltered;
            set
            {
                if (SetProperty(ref _isFiltered, value))
                {
                    _owner.LoadView();
                }
            }
        }

        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                //if (!_owner.IsValidName(value))
                //    return;
                string newName = _owner.GetValidName(value);


                string oldName = _name;
                if (SetProperty(ref _name, newName))
                {
                    _owner.RenameTag(oldName, _name);
                }
            }
        }

        public void SetName(string name) { _name = name; }

        bool _isFilterVisible = false;
        public bool IsFilterVisible
        {
            get => _isFilterVisible;
            set => SetProperty(ref _isFilterVisible, value);
        }

        public ICommand TagMouseEnterCommand { get => new CommandHandler(() => this.TagMouseEnter()); }
        void TagMouseEnter()
        {
            IsFilterVisible = true;
        }

        public ICommand TagMouseLeaveCommand { get => new CommandHandler(() => this.TagMouseLeave()); }
        void TagMouseLeave()
        {
            IsFilterVisible = false;
        }


    }



}
