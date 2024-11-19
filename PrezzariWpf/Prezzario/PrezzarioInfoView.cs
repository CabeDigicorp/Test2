using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PrezzariWpf.View
{
    public class PrezzarioInfoView : NotificationBase
    {
        string _fileName = null;
        public string FileName
        {
            get => _fileName;
            set { SetProperty(ref _fileName, value); }
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

        string _group = null;
        public string Group
        {
            get => _group;
            set { SetProperty(ref _group, value); }
        }

        public bool IsPrezzarioDownloadable { get; set; }

        string _year = null;
        public string Year
        {
            get => _year;
            set { SetProperty(ref _year, value); }
        }

        public SolidColorBrush PrezzarioInfoViewForeground
        {
            get
            {
                if (IsPrezzarioDownloadable)
                    return Brushes.Black;

                return Brushes.Gray;
            }
        }





    }
}
