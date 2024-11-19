using Commons;
using MasterDetailModel;
using MasterDetailModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class DivisioneView : NotificationBase
    {
        List<Categoria> _categorie; //ref
        public List<Categoria> Categorie { get => _categorie; }
        public event EventHandler CurrentItemChanged;

        string NewTextCode = "[Codice]";//da tradurre
        string NewTextName = "[Nome]";//da tradurre

        public void Init(List<Categoria> categorie)
        {
            _categorie = categorie;
            LoadCategorieView(CategorieView, Guid.Empty);
        }
        
        //public List<Categoria> Categorie { get; set; } = new List<Categoria>();

        private ObservableCollection<CategoriaView> _categorieView = new ObservableCollection<CategoriaView>();
        public ObservableCollection<CategoriaView> CategorieView
        {
            get { return _categorieView; }
            set { SetProperty(ref _categorieView, value); }
        }

        CategoriaView _currentItem = null;
        public CategoriaView CurrentItem
        {
            get => _currentItem;
            internal set
            {
                _currentItem = value;
                OnCurrentItemChanged(new EventArgs());
            }
        }

        protected void OnCurrentItemChanged(EventArgs e)
        {
            CurrentItemChanged?.Invoke(this, e);
        }

        internal void AddSibling(ObservableCollection<CategoriaView> categorieView, CategoriaView currCatView)
        {
            Categoria newCat = new Categoria() { Id = Guid.NewGuid(), Code = NewTextCode, Name = NewTextName, ParentId = (currCatView!=null)?currCatView.GetParentId():Guid.Empty };//da tradurre

            Categoria currCat = null;
            if (currCatView != null)
                currCat = Categorie.FirstOrDefault(item => item.Id == currCatView.GetId());


            //Aggiungo Categoria
            int currIndex = -1;
            if (currCat != null)
                currIndex = Categorie.IndexOf(currCat);

            if (currIndex < Categorie.Count - 1)
                Categorie.Insert(currIndex + 1, newCat);
            else
                Categorie.Add(newCat);//aggiungo in coda

            //Aggiungo CategoriaView
            currIndex = -1;
            if (currCatView != null)
                currIndex = categorieView.IndexOf(currCatView);
            if (currIndex == categorieView.Count - 1)
            {
                categorieView.Add(new CategoriaView(this, newCat));
            }
            else
            {
                categorieView.Insert(currIndex + 1, new CategoriaView(this, newCat));
            }
        }



        internal void AddChild(ObservableCollection<CategoriaView> categorieView, CategoriaView currCatView)
        {
            Categoria currCat = null;
            currCat = Categorie.LastOrDefault(item => item.ParentId == currCatView.GetId());
            if (currCat == null)
                currCat = Categorie.FirstOrDefault(item => item.Id == currCatView.GetId());


            int currIndex = -1;

            //Aggiungo Categoria [Nuovo]
            if (!currCatView.Children.Any())//se sto aggiungendo un livello in realtà aggiungo un padre e faccio slittare di un livello il figlio
            {
                Categoria newCat = new Categoria() { Id = Guid.NewGuid(), Code = currCat.Code, Name = currCat.Name, ParentId = currCatView.GetParentId() };//da tradurre

                //Aggiungo Categoria [Nessuno]
                currIndex = Categorie.IndexOf(currCat);
                if (currIndex < Categorie.Count)//aggiungo in coda
                    Categorie.Insert(currIndex, newCat);
                else
                    Categorie.Add(newCat);

                currCat.ParentId = newCat.Id;
                //currCat.Name = currCat.Name;//AttributoDivisione.NothingText;

                //Aggiungo CategoriaView
                int index = categorieView.IndexOf(currCatView);
                categorieView.Remove(currCatView);

                CategoriaView newCatView = new CategoriaView(this, newCat);
                if (index < categorieView.Count)
                    categorieView.Insert(index, newCatView);
                else
                    categorieView.Add(newCatView);
                newCatView.Children.Add(new CategoriaView(this, currCat));

                currCatView = newCatView;
            }
            else
            {
                Categoria newCat = new Categoria() { Id = Guid.NewGuid(), Code = NewTextCode, Name = NewTextName, ParentId = currCatView.GetId() };//da tradurre

                //Aggiungo Categoria
                currIndex = Categorie.IndexOf(currCat);
                if (currIndex < Categorie.Count - 1)
                    Categorie.Insert(currIndex + 1, newCat);
                else
                    Categorie.Add(newCat);//aggiungo in coda

                //Aggiungo CategoriaView
                ObservableCollection<CategoriaView> children = currCatView.Children;
                children.Add(new CategoriaView(this, newCat));

            }
        }

        internal void ReorderCategorie(ObservableCollection<CategoriaView> categorieView)
        {
            List<Categoria> categorie = new List<Categoria>();
            ReorderCategorie(categorieView, categorie);
            _categorie = categorie;
        }

        internal void ReorderCategorie(ObservableCollection<CategoriaView> categorieView, List<Categoria> categorie)
        {
            foreach (CategoriaView catView in categorieView)
            {
                categorie.Add(catView.Categoria);
                ReorderCategorie(catView.Children, categorie);
            }
        }

        public void DeleteCategorie(IEnumerable<object> categorieView)//object = CategoriaView
        {
            List<Guid> categorieId = new List<Guid>();

            foreach (CategoriaView catView in categorieView)
            {
                categorieId.Add(catView.GetId());
                GetDeepChildrenId(catView, categorieId);
            }
            Categorie.RemoveAll(item => categorieId.Contains(item.Id));
        }

        public void LoadCategorieView(ObservableCollection<CategoriaView> categorieView, Guid parentId)
        {
            IEnumerable<Categoria> categorieSameParent = Categorie.Where(item => item.ParentId == parentId);

            foreach (Categoria cat in categorieSameParent)
            {
                CategoriaView catView = new CategoriaView(this, cat);
                categorieView.Add(catView);
                LoadCategorieView(catView.Children, catView.GetId());
            }
        }

        /// <summary>
        /// Ritorna tutti i figli nipoti etc.. di una categoria
        /// </summary>
        /// <param name="catView"></param>
        /// <param name="childrenIds"></param>
        public void GetDeepChildrenId(CategoriaView catView, List<Guid> childrenIds)
        {
            foreach (CategoriaView child in catView.Children)
            {
                childrenIds.Add(child.GetId());
                GetDeepChildrenId(child, childrenIds);
            }
        }


    }
}
