using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf.View
{
    public class UpdateFoglioDiCalcoloView : NotificationBase
    {
        public ObservableCollection<UpdateView> _ListaFogli;
        public ObservableCollection<UpdateView> ListaFogli
        {
            get
            {
                return _ListaFogli;
            }
            set
            {
                if (SetProperty(ref _ListaFogli, value))
                {
                    _ListaFogli = value;
                }
            }
        }
        public UpdateFoglioDiCalcoloView(List<string> fogli)
        {
            ListaFogli = new ObservableCollection<UpdateView>();
            foreach (var foglio in fogli)
            {
                ListaFogli.Add(new UpdateView() { Name = foglio, Value = true });
            }
        }
        public bool Accept()
        {
            return true;
        }

        public List<UpdateView> GetFogliToUpdate()
        {
            return ListaFogli.Where(x => x.Value).ToList();
        }
    }
}
