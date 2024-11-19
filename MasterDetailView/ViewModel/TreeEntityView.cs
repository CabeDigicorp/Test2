using Commons;
using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;



namespace MasterDetailView
{

    /// <summary>
    /// Rappresenta la vista di un'entità nell'albero master
    /// </summary>
    public class TreeEntityView : EntityView
    {
        internal new EntitiesTreeMasterDetailView Master { get => _master as EntitiesTreeMasterDetailView; }

        public TreeEntityView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
        }

        TreeEntity TreeEntity { get => This as TreeEntity; }

        public bool CanBeParent()
        {
            bool canBeParent = TreeEntity.CanBeParent();
            return canBeParent;
        }

        public int Depth
        {
            get { return TreeEntity.Depth; }
        }

        /// <summary>
        /// Obsoleta: da sostituire con IsParent
        /// </summary>
        public bool HasChildren
        {
            //oss: devo sapere se l'entità ha figli (per mettere l'icona) anche senza realizzarli quindi non mi basta Depth
            get { return Master.HasChildren(Id); }
        }

        public bool IsParent { get => TreeEntity.IsParent; }


        #region Expand
        public ICommand ExpandCommand
        {
            get
            {
                return new CommandHandler(() => this.Expand(!_isExpanded));
            }
        }

        public void Expand(bool expand)
        {
            //EntitiesTreeMasterDetailView.This.SaveScrollPosition();

            IsExpanded = expand;
            Guid idToSelect = Master.ExpandEntityById(Id, _isExpanded);
            Master.FilteredEntitiesViewInfo[this.Id].IsExpanded = IsExpanded;

            Master.SelectEntityById(idToSelect, false);

        }

        bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        public ICommand ExpandBranchCommand
        {
            get
            {
                return new CommandHandler(() => this.ExpandBranch());
            }
        }

        /// <summary>
        /// Espande tutte le entità fino alla foglia
        /// </summary>
        public async void ExpandBranch()
        {

            //EntitiesTreeMasterDetailView.This.SaveScrollPosition();

            Master.ExpandEntityById(Id, true);

            List<Guid> descendants = Master.FilteredDescendantsId(Id).ToList();
            descendants.ForEach(item => Master.ExpandEntityById(item, true));
            await Master.UpdateCache();
        }

        public bool ExpandBranchEnabled
        {
            get { return HasChildren; }
        }

        #endregion Expand




        public bool IsAnyDescendantChecked
        {
            get { return Master.IsAnyDescendantChecked(this); }

        }


        bool _isCheckedInNullState = false;
        public bool? IsChecked3State
        {
            get
            {
                if (IsAnyDescendantChecked && !IsChecked)
                {
                    _isCheckedInNullState = true;
                    return null;
                }
                else return IsChecked;
            }
            set
            {
                //ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<TreeEntityView>();
                //log.Trace("IsChecked");

                //EntitiesTreeMasterDetailView.This.IsThreeState

                if (value == null)
                    return;

                bool isChecked = value == true;
                if (SetProperty(ref _isChecked, isChecked))
                {


                    if (_isChecked)
                    {
                        //Master.LastEntityCheckStateChanged = this.Id;
                        Master.CheckedEntitiesId.Add(this.Id);
                    }
                    else
                    {
                        //Master.LastEntityCheckStateChanged = Guid.Empty;
                        Master.CheckedEntitiesId.Remove(this.Id);
                    }
                    Master.CheckDescendantsOf(this.Id, isChecked);


                    Master.OnCheckedEntity();

                    Master.UpdateCache();
                }
                else if (_isCheckedInNullState && value == false)//value == false, IsChecked == false
                {
                    _isCheckedInNullState = false;
                    Master.CheckDescendantsOf(this.Id, false);
                    Master.OnCheckedEntity();


                    Master.UpdateCache();
                }

            }
        }

        bool _is3State = false;
        public bool Is3State
        {
            get { return _is3State; }
            set { SetProperty(ref _is3State, value); }
        }

        public override bool IsChecked
        {
            get { return base.IsChecked; }
            set
            {
                IsChecked3State = value;
            }

        }


        protected override void RightTapped()
        {

            //Master.SelectedIndex = Master.DisplayedIndexOf(Id);
            Master.SelectIndex(Master.DisplayedIndexOf(Id));
            RaisePropertyChanged(GetPropertyName(() => IsPasteClipboardEnabled));
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            RaisePropertyChanged(GetPropertyName(() => Depth));
            RaisePropertyChanged(GetPropertyName(() => HasChildren));
            RaisePropertyChanged(GetPropertyName(() => IsExpanded));
            RaisePropertyChanged(GetPropertyName(() => IsPasteClipboardEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsCopyToClipboardEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsChecked3State));
        }

        public override bool IsValoreAttributoReadOnly(string attCode)
        {
            if (IsParent)
            {
                if (Master.EntityParentType.Attributi.ContainsKey(attCode))
                {
                    Attributo att = Master.EntityParentType.Attributi[attCode];
                    if (att.IsValoreReadOnly || att.IsValoreLockedByDefault || att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                        return true;
                }
                return false;

            }
            else
            {
                return base.IsValoreAttributoReadOnly(attCode);
            }

        }



    }

}