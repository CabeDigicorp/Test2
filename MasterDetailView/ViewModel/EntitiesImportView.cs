using CommonResources;
using Commons;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class EntitiesImportView : NotificationBase
    {
        EntitiesImportStatus _status = null;

        HashSet<string> _targetEntityTypes = null;

        public void Init(EntitiesImportStatus importStatus)
        {
            _status = importStatus;

            _importText = string.Format("{0} {1} {2}",
                LocalizationProvider.GetString("ImportazioneDi"),
                _status.StartingEntitiesId.Count,
                LocalizationProvider.GetString("da")
                );

            _sourceName = _status.SourceName;

            _sourceEntityTypeName = _status.WaitingInfo.SourceEntityTypeName;
            _targetEntityTypeName = _status.WaitingInfo.TargetEntityTypeName;

            _entityKey = _status.WaitingInfo.SourceEntityKey;

            _ignoreItem = string.Format("{0} {1}",
                LocalizationProvider.GetString("MantieniLaVoce"),
                _status.WaitingInfo.SourceEntityKey);

            _overwriteItem = string.Format("{0} {1}",
                LocalizationProvider.GetString("SostituisciLaVoce"),
                _status.WaitingInfo.SourceEntityKey);

            _ignoreSectionItems = string.Format("{0} {1}",
                LocalizationProvider.GetString("MantieniNellaSezione"),
                _status.WaitingInfo.TargetEntityTypeName);

            _overwriteSectionItems = string.Format("{0} {1}",
                LocalizationProvider.GetString("SostituisciNellaSezione"),
                _status.WaitingInfo.TargetEntityTypeName);


            _targetEntityTypes = new HashSet<string>(_status.EntitiesBySourceId.Select(item => item.Value.TargetEntityTypeName));
            //_targetEntityTypes = new HashSet<string>(_status.LoadedSourceEntitiesId.Select(item => item.TargetEntityTypeName));

            _ignoreAllSectionItems = string.Format("{0} {1}",
                LocalizationProvider.GetString("MantieniInTutteLeSezioni"),
                string.Join(", ", _targetEntityTypes));

            _overwriteAllSectionItems = string.Format("{0} {1}",
                LocalizationProvider.GetString("SostituisciInTutteLeSezioni"),
                string.Join(", ", _targetEntityTypes));



            UpdateUI();
        }

        public void Accept()
        {
            EntityImportStatus waitingEntityStatus = null;
            if (_status.EntitiesBySourceId.ContainsKey(_status.WaitingInfo.SourceId))
                waitingEntityStatus = _status.EntitiesBySourceId[_status.WaitingInfo.SourceId];

            if (waitingEntityStatus == null)
                return;

            switch (CurrentSelection)
            {
                case ImportOverwriteSelection.IgnoreItem:
                    {
                        waitingEntityStatus.ConflictAction = EntityImportConflictAction.Ignore;
                        break;
                    }
                case ImportOverwriteSelection.OverwriteItem:
                    {
                        waitingEntityStatus.ConflictAction = EntityImportConflictAction.Overwrite;
                        break;
                    }
                case ImportOverwriteSelection.IgnoreSectionItems:
                    {
                        if (_status.EntityTypes.ContainsKey(waitingEntityStatus.TargetEntityTypeKey))
                        {
                            EntityTypeImportStatus entTypeStatus = _status.EntityTypes[waitingEntityStatus.TargetEntityTypeKey];
                            entTypeStatus.ConflictAction = EntityImportConflictAction.Ignore;
                        }
                        break;
                    }
                case ImportOverwriteSelection.OverwriteSectionItems:
                    {
                        if (_status.EntityTypes.ContainsKey(waitingEntityStatus.TargetEntityTypeKey))
                        {
                            EntityTypeImportStatus entTypeStatus = _status.EntityTypes[waitingEntityStatus.TargetEntityTypeKey];
                            entTypeStatus.ConflictAction = EntityImportConflictAction.Overwrite;
                        }
                        break;
                    }
                case ImportOverwriteSelection.IgnoreAllSectionItems:
                    {
                        _status.ConflictAction = EntityImportConflictAction.Ignore;
                        break;
                    }
                case ImportOverwriteSelection.OverwriteAllSectionItems:
                    {
                        _status.ConflictAction = EntityImportConflictAction.Overwrite;
                        break;
                    }

            }

        }

        ImportOverwriteSelection _currentSelection = ImportOverwriteSelection.IgnoreAllSectionItems;
        ImportOverwriteSelection CurrentSelection
        { 
            get => _currentSelection;
            set
            {
                _currentSelection = value;
                UpdateUI();
            }
        }


        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ImportText));
            RaisePropertyChanged(GetPropertyName(() => SourceName));
            RaisePropertyChanged(GetPropertyName(() => SourceEntityTypeName));
            RaisePropertyChanged(GetPropertyName(() => TargetEntityTypeName));
            RaisePropertyChanged(GetPropertyName(() => EntityKey));
            RaisePropertyChanged(GetPropertyName(() => IgnoreItem));
            RaisePropertyChanged(GetPropertyName(() => OverwriteItem));
            RaisePropertyChanged(GetPropertyName(() => IgnoreSectionItems));
            RaisePropertyChanged(GetPropertyName(() => OverwriteSectionItems));
            RaisePropertyChanged(GetPropertyName(() => IgnoreAllSectionItems));
            RaisePropertyChanged(GetPropertyName(() => OverwriteAllSectionItems));
            //
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreItem));
            RaisePropertyChanged(GetPropertyName(() => IsOverwriteItem));
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreSectionItems));
            RaisePropertyChanged(GetPropertyName(() => IsOverwriteSectionItems));
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreAllSectionItems));
            RaisePropertyChanged(GetPropertyName(() => IsOverwriteAllSectionItems));
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreSectionItemsVisible));
            RaisePropertyChanged(GetPropertyName(() => IsOverwriteSectionItemsVisible));
            RaisePropertyChanged(GetPropertyName(() => IsIgnoreAllSectionItemsVisible));
            RaisePropertyChanged(GetPropertyName(() => IsOverwriteAllSectionItemsVisible));
            //
            RaisePropertyChanged(GetPropertyName(() => IsOtherOptionsOpen));



        }


        string _importText = null;
        public string ImportText { get => _importText; }

        string _sourceName = null;
        public string SourceName { get => _sourceName; }

        string _sourceEntityTypeName = null;
        public string SourceEntityTypeName { get => _sourceEntityTypeName; }

        string _targetEntityTypeName = null;
        public string TargetEntityTypeName { get => _targetEntityTypeName; }

        string _entityKey = null;
        public string EntityKey { get => _entityKey; }

        string _ignoreItem = null;
        public string IgnoreItem { get => _ignoreItem; }

        string _overwriteItem = null;
        public string OverwriteItem { get => _overwriteItem; }

        string _ignoreSectionItems = null;
        public string IgnoreSectionItems { get => _ignoreSectionItems; }

        string _overwriteSectionItems = null;
        public string OverwriteSectionItems { get => _overwriteSectionItems; }

        string _ignoreAllSectionItems = null;
        public string IgnoreAllSectionItems { get => _ignoreAllSectionItems; }

        string _overwriteAllSectionItems = null;
        public string OverwriteAllSectionItems { get => _overwriteAllSectionItems; }

        public bool IsIgnoreItem
        {
            get => CurrentSelection == ImportOverwriteSelection.IgnoreItem;
            set => CurrentSelection = ImportOverwriteSelection.IgnoreItem;
        }

        public bool IsOverwriteItem
        {
            get => CurrentSelection == ImportOverwriteSelection.OverwriteItem;
            set => CurrentSelection = ImportOverwriteSelection.OverwriteItem;
        }

        public bool IsIgnoreSectionItems
        {
            get => CurrentSelection == ImportOverwriteSelection.IgnoreSectionItems;
            set => CurrentSelection = ImportOverwriteSelection.IgnoreSectionItems;
        }

        public bool IsOverwriteSectionItems
        {
            get => CurrentSelection == ImportOverwriteSelection.OverwriteSectionItems;
            set => CurrentSelection = ImportOverwriteSelection.OverwriteSectionItems;
        }

        public bool IsIgnoreAllSectionItems
        {
            get => CurrentSelection == ImportOverwriteSelection.IgnoreAllSectionItems;
            set => CurrentSelection = ImportOverwriteSelection.IgnoreAllSectionItems;
        }

        public bool IsOverwriteAllSectionItems
        {
            get => CurrentSelection == ImportOverwriteSelection.OverwriteAllSectionItems;
            set => CurrentSelection = ImportOverwriteSelection.OverwriteAllSectionItems;
        }

        public bool IsIgnoreSectionItemsVisible
        {
            //get => _status == null? false : GetDeepStartingEntitiesCount() > 1;
            get => _status == null ? false : (_targetEntityTypes.Count > 1 && GetDeepStartingEntitiesCount() > 1);
        }

        public bool IsOverwriteSectionItemsVisible
        {
            //get => _status == null ? false : GetDeepStartingEntitiesCount() > 1;
            get => _status == null ? false : (_targetEntityTypes.Count > 1 && GetDeepStartingEntitiesCount() > 1);
        }

        int GetDeepStartingEntitiesCount()
        {
            if (_status == null)
                return 0;

            EntityImportId entId = _status.StartingEntitiesId.FirstOrDefault();
            if (entId == null)
                return 0;

            int count = _status.EntitiesBySourceId.Values.Where(item => item.SourceEntityTypeKey == entId.SourceEntityTypeKey).Count();
            return count;
        }

        public bool IsIgnoreAllSectionItemsVisible
        {
            //get => _status == null ? false : _targetEntityTypes.Count > 1;
            get => _status != null;
        }

        public bool IsOverwriteAllSectionItemsVisible
        {
            //get => _status == null ? false : _targetEntityTypes.Count > 1;
            get => _status != null;
        }

        bool _isOtherOptionsOpen = false;
        public bool IsOtherOptionsOpen
        {
            get => _isOtherOptionsOpen;
            set => SetProperty(ref _isOtherOptionsOpen, value);
        }

    }

    public enum ImportOverwriteSelection
    {
        Nothing,
        IgnoreItem,
        OverwriteItem,
        IgnoreSectionItems,
        OverwriteSectionItems,
        IgnoreAllSectionItems,
        OverwriteAllSectionItems,
    }


}
