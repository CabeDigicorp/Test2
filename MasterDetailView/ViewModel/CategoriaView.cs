using Commons;
using MasterDetailModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class CategoriaView : NotificationBase<Categoria>
    {
        DivisioneView MainView { get; set; }

        public CategoriaView(DivisioneView mainView, Categoria cat) : base(cat)
        {
            MainView = mainView;
        }


        public Guid GetId() { return This.Id; }
        public Guid GetParentId() { return This.ParentId; }

        public Categoria Categoria { get => This; }
        public string Code
        {
            get => This.Code;
            set => SetProperty(This.Code, value, () => This.Code = value);
        }
        public string Name
        {
            get => This.Name;
            set => SetProperty(This.Name, value, () => This.Name = value);
        }
        //Color Colore { get; set; }

        ObservableCollection<CategoriaView> _children = new ObservableCollection<CategoriaView>();
        public ObservableCollection<CategoriaView> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Children));
            RaisePropertyChanged(GetPropertyName(() => Name));
        }
    }
}
