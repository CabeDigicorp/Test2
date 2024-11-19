
using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using MasterDetailModel;

namespace MasterDetailView
{

    public interface RightPaneView
    {

    }

    /// <summary>
    /// Classe che rappresenta il pannello a destra (filtri, ordinamenti, raggruppamenti, ...)
    /// </summary>
    public class RightPanesView : NotificationBase
    {
        RightPaneView _currentControlView = null;
        public FilterView FilterView { get; set; } = null;
        public SortView SortView { get; set; } = null;
        public GroupView GroupView { get; set; } = null;
        //public static RightSplitView This { get; set; }

        protected EntitiesListMasterDetailView _master = null;
        EntitiesListMasterDetailView Master { get => _master; }

        public RightPanesView(EntitiesListMasterDetailView master)
        {
            _master = master;
            //This = this;
            FilterView = new FilterView(master);
            SortView = new SortView(master);
            GroupView = new GroupView(master);

            CurrentControlView = FilterView;
        }

        public void Clear()
        {
            FilterView.ClearAllFiltro(false);
            SortView.ClearAllSort(false);
            GroupView.ClearAllGroup(false);

            //rem by Ale 26/03/2021. Perchè se sto chiudendo il file join dovrei ricaricare il master?
            //Master.Load();

            //if (Master.ModelActionsStack != null)
            //    Master.ModelActionsStack.OnViewSettingsChanged();
        }

        public RightPaneView CurrentControlView
        {
            get { return _currentControlView; }
            set { SetProperty(ref _currentControlView, value); }
        }

        public bool IsPaneOpen
        {
            get { return _isFilterPaneOpen || _isSortPaneOpen || _isGroupPaneOpen; }
            set
            {
                if (IsPaneOpen == false)
                {
                    if (CurrentControlView is FilterView)
                        IsFilterPaneOpen = true;
                    if (CurrentControlView is SortView)
                        IsSortPaneOpen = true;
                    if (CurrentControlView is GroupView)
                        IsGroupPaneOpen = true;
                }
                else
                {
                    IsFilterPaneOpen = false;
                    IsSortPaneOpen = false;
                    IsGroupPaneOpen = false;
                }

            }
        }


        bool _isFilterPaneOpen = false;
        public bool IsFilterPaneOpen
        {
            get {return _isFilterPaneOpen; }
            set
            {
                if (SetProperty(ref _isFilterPaneOpen, value))
                {
                    _isSortPaneOpen = false;
                    _isGroupPaneOpen = false;
                    if (_isFilterPaneOpen)
                        CurrentControlView = FilterView;

                    RaisePropertyChanged(GetPropertyName(() => this.IsPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsSortPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsGroupPaneOpen));

                }
            }
        }



        public void ClosePanes()
        {
            IsFilterPaneOpen = IsGroupPaneOpen = IsSortPaneOpen = false;
        }

        bool _isSortPaneOpen = false;
        public bool IsSortPaneOpen
        {
            get {return _isSortPaneOpen;}
            set
            {
                if (SetProperty(ref _isSortPaneOpen, value))
                {
                    _isFilterPaneOpen = false;
                    _isGroupPaneOpen = false;
                    if (_isSortPaneOpen)
                        CurrentControlView = SortView;

                    RaisePropertyChanged(GetPropertyName(() => this.IsPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsFilterPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsGroupPaneOpen));
                    //if (!value)
                    //    UnLoad();
                }
            }
        }

        bool _isGroupPaneOpen = false;
        public bool IsGroupPaneOpen
        {
            get { return _isGroupPaneOpen; }
            set
            {
                if (SetProperty(ref _isGroupPaneOpen, value))
                {
                    _isFilterPaneOpen = false;
                    _isSortPaneOpen = false;
                    if (_isGroupPaneOpen)
                        CurrentControlView = GroupView;

                    RaisePropertyChanged(GetPropertyName(() => this.IsPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsFilterPaneOpen));
                    RaisePropertyChanged(GetPropertyName(() => this.IsSortPaneOpen));
                    //if (!value)
                    //    UnLoad();
                }
            }
        }

        internal void UpdateAttributi()
        {
            FilterView.UpdateAttributi();
            SortView.UpdateAttributi();
            GroupView.UpdateAttributi();
        }

        public void UpdateViewSettings(EntityTypeViewSettings currentViewSettings)
        {
            if (GroupView.Data != null)
                if (GroupView.Data.Items.Any())
                    currentViewSettings.Groups = GroupView.Data.Items.Select(item => item.Clone()).ToList();
                else
                    currentViewSettings.Groups = new List<AttributoGroupData>();

            if (SortView.Data != null)
                if (SortView.Data.Items.Any())
                    currentViewSettings.Sorts = SortView.Data.Items.Select(item => item.Clone()).ToList();
                else
                    currentViewSettings.Sorts = new List<AttributoSortData>();

            if (FilterView.Data != null)
                if (FilterView.Data.Items.Any())
                    currentViewSettings.Filters = FilterView.Data.Items.Select(item => item.Clone()).ToList();
                else
                    currentViewSettings.Filters = new List<AttributoFilterData>();
        }
    }


}