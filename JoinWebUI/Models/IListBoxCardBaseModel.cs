using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public interface IListBoxCardBaseModel //: INotifyPropertyChanged
    {
        public Guid Id { get; }

        public string IdString { get; }

        public string TextLine1 { get; }
        public string TextLine2 { get; }
        public bool MultiLine { get; }

    }

}
