using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrezzariWpf.View
{
    public class ClientPrezzarioInfoView : NotificationBase
    {
        SelectPrezzarioIdsView _owner = null;

        public ClientPrezzarioInfoView(SelectPrezzarioIdsView owner)
        {
            _owner = owner;
        }

        string _fileName = null;
        public string FileName
        {
            get => _fileName;
            set { SetProperty(ref _fileName, value); }
        }

        string _clientLastWriteDate = null;
        public string ClientLastWriteDate
        {
            get => _clientLastWriteDate;
            set { SetProperty(ref _clientLastWriteDate, value); }
        }

        string _serviceLastWriteAccessDate = null;
        public string ServiceLastWriteDate
        {
            get => _serviceLastWriteAccessDate;
            set { SetProperty(ref _serviceLastWriteAccessDate, value); }
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

        bool _isUpdateAvaliable = false;
        public bool IsUpdateAvaliable
        {
            get => _isUpdateAvaliable;
            set { SetProperty(ref _isUpdateAvaliable, value); }
        }


        public ICommand UpdatePrezzarioCommand { get { return new CommandHandler(() => this.UpdatePrezzario()); } }
        void UpdatePrezzario()
        {
            if (_owner != null)
                _owner.DownloadPrezzario(FileName);
        }

        string _year = null;
        public string Year
        {
            get => _year;
            set { SetProperty(ref _year, value); }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ClientLastWriteDate));
            RaisePropertyChanged(GetPropertyName(() => ServiceLastWriteDate));
            RaisePropertyChanged(GetPropertyName(() => IsUpdateAvaliable));
        }

    }
}
