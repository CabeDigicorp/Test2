using Commons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MainApp
{
    public class ModelloInfoView : NotificationBase
    {
        string _fileName = null;
        public string FileName
        {
            get => _fileName;
            set { SetProperty(ref _fileName, value); }
        }

        string _remoteFileName = null;
        public string RemoteFileName
        {
            get => _remoteFileName;
            set { SetProperty(ref _remoteFileName, value); }
        }

        string _minAppVersion = null;
        public string MinAppVersion
        {
            get => _minAppVersion;
            set { SetProperty(ref _minAppVersion, value); }
        }

        string _LastWriteAccessDate = null;
        public string LastWriteDate
        {
            get => _LastWriteAccessDate;
            set { SetProperty(ref _LastWriteAccessDate, value); }
        }

        string _dimension = null;
        public string Dimension
        {
            get => _dimension;
            set { SetProperty(ref _dimension, value); }
        }

        string _note = null;
        public string Note
        {
            get => _note;
            set { SetProperty(ref _note, value); }
        }

        List<string> _tags = null;
        public List<string> Tags
        {
            get => _tags;
            set { SetProperty(ref _tags, value); }
        }

        internal void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Tags));
            RaisePropertyChanged(GetPropertyName(() => ModelloInfoViewForeground));
        }

        public bool IsModelDownloadable { get; set; }

        public SolidColorBrush ModelloInfoViewForeground
        {
            get
            {
                if (IsModelDownloadable)
                    return Brushes.Black;

                return Brushes.Gray;
            }
        }



    }
}
