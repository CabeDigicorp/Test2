using CommonResources;
using Commons;
using MasterDetailView;
using Model;
using Model.JoinService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MainApp
{

    public class SelectRemoteProjectModelsView : NotificationBase
    {
        public IMainOperation MainOperation { get; set; }
        public IEntityWindowService WindowService { get; set; }

        Dictionary<string, ModelloInfo> _modelliInfoDownloaded = new Dictionary<string, ModelloInfo>();

        string _modelliFolder = string.Empty;

        public MessageBarView MessageBarView { get; set; } = new MessageBarView();

        public string TagNothing { get; set; }

        public SelectRemoteProjectModelsView()
        {
            TagNothing = LocalizationProvider.GetString("_Nessuno");
        }

        private ObservableCollection<ModelloInfoView> _modelloInfoViewItems = new ObservableCollection<ModelloInfoView>();
        public ObservableCollection<ModelloInfoView> ModelloInfoViewItems
        {
            get { return _modelloInfoViewItems; }
            set
            {
                SetProperty(ref _modelloInfoViewItems, value);
            }
        }

        /// <summary>
        /// Valori visualizzati nella lista dei tag
        /// </summary>
        ObservableCollection<RemoteTagView> _tagsView = new ObservableCollection<RemoteTagView>();
        public ObservableCollection<RemoteTagView> TagsView
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

        public void Load(Dictionary<string, ModelloInfo> modelliInfoDownloaded)
        {
            if (MainOperation == null)
                return;

            if (WindowService == null)
                return;

            _modelliFolder = MainOperation.GetModelliFolder();

            if (modelliInfoDownloaded != null)
                _modelliInfoDownloaded = modelliInfoDownloaded;

            LoadModelliView();

            LoadTagsView();

            UpdateUI();

        
        }


        public void LoadModelliView()
        {

            //IOrderedEnumerable<ModelloInfo> orderedList = _modelliInfoDownloaded.Values.OrderBy(item => item.FileName);
            IOrderedEnumerable<ModelloInfo> orderedList = _modelliInfoDownloaded.Values.OrderBy(item => item.UserName).ThenByDescending(item => item.MinAppVersion);
            //vengono ordinati per nome e dalla versione più recente

            HashSet<string> downlodableFilesName = new HashSet<string>(_modelliInfoDownloaded.Values.Where(item => IsModelDownloadable(item.MinAppVersion)).Select(item => item.FileName));

            HashSet<string> added = new HashSet<string>();

            _modelloInfoViewItems.Clear();
            foreach (ModelloInfo item in orderedList)
            {
                string filename = item.FileName;
                string minAppVersion = _modelliInfoDownloaded[filename].MinAppVersion;

                if (!downlodableFilesName.Contains(filename))
                {
                    //se il modello non è scaricabile ma ne esiste uno con lo stesso UserName scaricabile allora non lo aggiungo
                    if (_modelliInfoDownloaded.Values.FirstOrDefault(item1 => item1.UserName == item.UserName && downlodableFilesName.Contains(item1.FileName)) != null)
                        continue;
                }

                if (added.Contains(item.UserName))
                    continue;

                if (!CheckFilter(item))
                    continue;

                ModelloInfoView modInfoView = new ModelloInfoView();
                modInfoView.RemoteFileName = filename;
                modInfoView.FileName = item.UserName;// filename;
                modInfoView.Dimension = Extensions.GetBytesReadable(_modelliInfoDownloaded[filename].Dimension);
                modInfoView.LastWriteDate = _modelliInfoDownloaded[filename].LastWriteTime.Date.ToShortDateString();
                modInfoView.MinAppVersion = _modelliInfoDownloaded[filename].MinAppVersion;
                modInfoView.Note = _modelliInfoDownloaded[filename].Note;
                modInfoView.Tags = _modelliInfoDownloaded[filename].Tags;
                modInfoView.IsModelDownloadable = downlodableFilesName.Contains(filename);// true;// IsModelDownloadable(modInfoView);


                _modelloInfoViewItems.Add(modInfoView);
                added.Add(item.UserName);
            }
        }

        public bool IsModelDownloadable(string minAppVersion)
        {
            NumberFormatInfo formatProvider = new NumberFormatInfo();
            formatProvider.NumberDecimalSeparator = ".";
            formatProvider.NumberGroupSeparator = "";

            double minAppVersionInt = 0;
            double.TryParse(minAppVersion, NumberStyles.AllowDecimalPoint, formatProvider, out minAppVersionInt);

            double deploymentVersionDouble = 100;
            string deploymentVersion = MainOperation.GetDeploymentVersion();
            if (deploymentVersion != null && deploymentVersion.Any())
                double.TryParse(deploymentVersion, NumberStyles.AllowDecimalPoint, formatProvider, out deploymentVersionDouble);

            if (deploymentVersionDouble >= minAppVersionInt)
                return true;

            return false;

        }





        //Modello corrente
        ModelloInfoView _currentModello = null;
        public ModelloInfoView CurrentModello
        {
            get => _currentModello;
            set
            {
                if (SetProperty(ref _currentModello, value))
                {
                    UpdateUI();
                    
                }
            }
        }

        //modelli selezionati
        List<ModelloInfoView> _selectedModelli = new List<ModelloInfoView>();
        public List<ModelloInfoView> SelectedModelli
        {
            get => _selectedModelli;
            set
            {
                if (SetProperty(ref _selectedModelli, value))
                {
                    UpdateUI();
                }
            }
        }


        void LoadTagsView()
        {

            HashSet<string> tags = new HashSet<string>(_modelliInfoDownloaded.Values.SelectMany(item => item.Tags));

            List<string> tagsSorted = new List<string>(tags);
            tagsSorted.Sort();

            TagsView.Clear();

            var t = new RemoteTagView(this) {Name = TagNothing, IsFiltered = false };
            TagsView.Add(t);

            foreach (string tag in tagsSorted)
            {
                RemoteTagView tagView = new RemoteTagView(this) { Name = tag, IsFiltered = false };
                TagsView.Add(tagView);
            }

        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ModelloInfoViewItems));
            RaisePropertyChanged(GetPropertyName(() => CurrentModelloNote));
            //RaisePropertyChanged(GetPropertyName(() => IsAcceptButtonEnabled));


            foreach (ModelloInfoView itemView in _modelloInfoViewItems)
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


        #region Modelli

        
        public string CurrentModelloNote
        {
            get
            {
                if (CurrentModello != null && SelectedModelli.Count == 1)
                    return CurrentModello.Note;
                else
                    return string.Empty;
            }

        }
        
        #endregion Modelli


        internal List<string> GetSelectedModelsFileName()
        {
            List<string> selectedModelsFullFileName = new List<string>();
            foreach (var item in SelectedModelli)
            {
                string fileName = item.RemoteFileName;
                if (!fileName.Any())
                    continue;

                if (!item.IsModelDownloadable)
                    continue;

                //string fullFileName = string.Format("{0}\\{1}.join", _modelliFolder, fileName);
                selectedModelsFullFileName.Add(fileName);
            }

            return selectedModelsFullFileName;
        }
       

    }

    public class RemoteTagView : NotificationBase
    {
        SelectRemoteProjectModelsView _owner = null;

        public RemoteTagView(SelectRemoteProjectModelsView owner)
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
                    _owner.LoadModelliView();
                }
            }
        }

        bool _isFilterVisible = false;
        public bool IsFilterVisible
        {
            get => _isFilterVisible;
            set => SetProperty(ref _isFilterVisible, value);
        }

        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
            }
        }


    }
}
