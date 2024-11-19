using Commons;
using Model;
using Model.JoinService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace PrezzariWpf.View
{
    public class SelectRemotePrezzariView : NotificationBase
    {
        public IMainOperation MainOperation { get; set; }

        Dictionary<string, PrezzarioInfo> _prezzariInfoDownloaded { get; set; }

        private ObservableCollection<PrezzarioInfoView> _prezzarioInfoViewItems = new ObservableCollection<PrezzarioInfoView>();
        public ObservableCollection<PrezzarioInfoView> PrezzarioInfoViewItems
        {
            get { return _prezzarioInfoViewItems; }
            set
            {
                SetProperty(ref _prezzarioInfoViewItems, value);
            }
        }

        public void Load(Dictionary<string, PrezzarioInfo> prezzariInfoDownloaded)
        {
            _prezzariInfoDownloaded = prezzariInfoDownloaded;


            _prezzarioInfoViewItems.Clear();
            foreach (string filename in _prezzariInfoDownloaded.Keys)
            {
                PrezzarioInfoView prezInfoView = new PrezzarioInfoView();
                prezInfoView.FileName = filename;
                prezInfoView.Dimension = Extensions.GetBytesReadable(_prezzariInfoDownloaded[filename].Dimension);
                prezInfoView.LastWriteDate = _prezzariInfoDownloaded[filename].LastWriteTime.Date.ToShortDateString();
                prezInfoView.MinAppVersion = _prezzariInfoDownloaded[filename].MinAppVersion;
                prezInfoView.Note = _prezzariInfoDownloaded[filename].Note;
                prezInfoView.Group = _prezzariInfoDownloaded[filename].Group;
                prezInfoView.Year = _prezzariInfoDownloaded[filename].Year;
                prezInfoView.IsPrezzarioDownloadable = IsPrezzarioDownloadable(prezInfoView);

                _prezzarioInfoViewItems.Add(prezInfoView);
            }

            UpdateUI();

        }

        public bool IsPrezzarioDownloadable(PrezzarioInfoView modInfoView)
        {
            NumberFormatInfo formatProvider = new NumberFormatInfo();
            formatProvider.NumberDecimalSeparator = ".";
            formatProvider.NumberGroupSeparator = "";

            double minAppVersionInt = 0;
            double.TryParse(modInfoView.MinAppVersion, NumberStyles.AllowDecimalPoint, formatProvider, out minAppVersionInt);

            double deploymentVersionInt = 100;
            string deploymentVersion = MainOperation.GetDeploymentVersion();
            if (deploymentVersion != null && deploymentVersion.Any())
                double.TryParse(deploymentVersion, NumberStyles.AllowDecimalPoint, formatProvider, out deploymentVersionInt);

            if (deploymentVersionInt >= minAppVersionInt)
                return true;

            return false;

        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => PrezzarioInfoViewItems));
        }

        PrezzarioInfoView _selectedItem = null;
        public PrezzarioInfoView SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        
    }
}
