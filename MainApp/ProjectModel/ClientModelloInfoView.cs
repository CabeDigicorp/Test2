using CommonResources;
using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainApp
{
    public class ClientModelloInfoView : NotificationBase
    {
        ProjectModelView _owner = null;

        public ClientModelloInfoView(ProjectModelView owner)
        {
            _owner = owner;
        }


        public IEnumerable<ModelloTagView> TagsView { get => Tags.Select(item => new ModelloTagView(this, item)); }

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

        List<string> _tags = null;
        public List<string> Tags
        {
            get => _tags;
            set { SetProperty(ref _tags, value); }
        }

        bool _isUpdateAvaliable = false;
        public bool IsUpdateAvaliable
        {
            get => _isUpdateAvaliable;
            set { SetProperty(ref _isUpdateAvaliable, value); }
        }

        public bool IsTagVisible { get => _owner.IsTagVisible; }

        string _userName = null;
        public string UserName
        {
            get => _userName;
            set { SetProperty(ref _userName, value); }
        }


        public ICommand UpdateModelloCommand { get { return new CommandHandler(() => this.UpdateModello()); } }
        void UpdateModello()
        {
            if (_owner != null)
                _owner.DownloadModello(FileName);
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ServiceLastWriteDate));
            RaisePropertyChanged(GetPropertyName(() => ClientLastWriteDate));
            RaisePropertyChanged(GetPropertyName(() => IsUpdateAvaliable));
            RaisePropertyChanged(GetPropertyName(() => TagsView));
            RaisePropertyChanged(GetPropertyName(() => IsTagVisible));
        }

        internal void AddTags(List<string> tags)
        {
            foreach (string tag in tags)
            {
                if (!Tags.Contains(tag) && tag != _owner.TagNothing && tag.Trim().Any())
                    Tags.Add(tag);
            }

            _owner.LoadView();
        }

        internal void RemoveTag(string tag)
        {
            _owner.RemoveModelloTag(FileName, tag);
            UpdateUI();
        }


    }
     
    public class ModelloTagView
    {
        ClientModelloInfoView _owner = null;

        public ModelloTagView(ClientModelloInfoView owner, string name)
        {
            _owner = owner;
            Name = name;
        }

        public string Name { get; set; }

        public ICommand RemoveTagCommand { get { return new CommandHandler(() => this.RemoveTag()); } }
        void RemoveTag()
        {
            _owner.RemoveTag(Name);
        }
    }
}
