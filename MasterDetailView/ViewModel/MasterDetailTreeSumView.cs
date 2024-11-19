using _3DModelExchange;
using CommonResources;
using Commons;
using DevZest.Windows.DataVirtualization;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MasterDetailView
{
    public class MasterDetailTreeSumItemView : MasterDetailTreeItemView
    {
        public static string BusySymbol => "...";

        public MasterDetailTreeSumItemView(EntitiesTreeMasterDetailView master, TreeEntity ent = null) : base(master, ent)
        {
            _master = master;
        }

        new MasterDetailTreeSumViewVirtualized Master => _master as MasterDetailTreeSumViewVirtualized;

        string _sumAttributo3 = BusySymbol;
        public string SumAttributo3
        {
            get
            {
                if (_sumAttributo3 == BusySymbol)
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    {
                        _sumAttributo3 = await Master.GetAttributo3(Entity.EntityId);
                        if (_sumAttributo3 != null)
                            RaisePropertyChanged(GetPropertyName(() => SumAttributo3));
                    });
                }
                return _sumAttributo3;
            }
            set => SetProperty(ref _sumAttributo3, value);
        }

        string _sumAttributo4 = BusySymbol;
        public string SumAttributo4
        {
            get
            {
                if (_sumAttributo4 == BusySymbol)
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    {
                        _sumAttributo4 = await Master.GetAttributo4(Entity.EntityId);
                        if (_sumAttributo4 != null)
                            RaisePropertyChanged(GetPropertyName(() => SumAttributo4));
                    });
                }
                return _sumAttributo4;
            }
            set => SetProperty(ref _sumAttributo4, value);
        }



        public double WidthAttributo3
        {
            get => Master.WidthAttributo3;
        }

        public double WidthAttributo4
        {
            get => Master.WidthAttributo4;
        }

        public bool IsItemsSummarized { get => Master.IsItemsSummarized; }

        public override void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => SumAttributo3));
            RaisePropertyChanged(GetPropertyName(() => SumAttributo4));
            RaisePropertyChanged(GetPropertyName(() => WidthAttributo3));
            RaisePropertyChanged(GetPropertyName(() => WidthAttributo4));
            RaisePropertyChanged(GetPropertyName(() => IsItemsSummarized));

            base.UpdateUI();
        }
    }


    public class MasterDetailTreeSumView : MasterDetailTreeView
    {
        public AttributoRiferimento SenderAttRif { get; set; } = null;

        public bool IsItemsSummarized { get; set; } = false;

        new MasterDetailTreeSumViewVirtualized ItemsView => _itemsView as MasterDetailTreeSumViewVirtualized;
        
        
        public MasterDetailTreeSumView() :base()
        {
        }

        public override void Init(EntityTypeViewSettings viewSettings)
        {
            base.Init(viewSettings);

            if (IsItemsSummarized)
            {

                ItemsView.SetAttributoSum(3);
                ItemsView.SetAttributoSum(4);

                ItemsView.CalcolaSummarizeAttributiAsync();
            }
        }


        public void SetAttributoSum(int masterAttIndex, string targetCodiceAttributo, ValoreOperationType targetOperationType)
        {
            if (IsItemsSummarized)
            {
                ItemsView.SetAttributoSum(masterAttIndex);
                ItemsView.CalcolaSummarizeAttributiAsync();
            }
        }
    }

    /// <summary>
    /// View di master-detail con ListView a tree
    /// </summary>
    public class MasterDetailTreeSumViewVirtualized : MasterDetailTreeViewVirtualized
    {
        public Task<GroupData> taskSummarizeAttributi = null;

        MasterDetailTreeSumView Owner => _owner as MasterDetailTreeSumView;



        public MasterDetailTreeSumViewVirtualized(MasterDetailTreeView owner) : base(owner)
        {
            _owner = owner;
        }

        public override void Init()
        {
            base.Init();
        }

        string _nessunoAttributo3 = MasterDetailTreeSumItemView.BusySymbol;
        public string NessunoAttributo3
        {
            get
            {
                if (_nessunoAttributo3 == MasterDetailTreeSumItemView.BusySymbol)
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    {
                        _nessunoAttributo3 = await GetAttributo3(Guid.Empty);
                        if (_nessunoAttributo3 != null)
                            RaisePropertyChanged(GetPropertyName(() => NessunoAttributo3));
                    });
                }
                return _nessunoAttributo3;
            }
            set => SetProperty(ref _nessunoAttributo3, value);
        }

        string _nessunoAttributo4 = MasterDetailTreeSumItemView.BusySymbol;
        public string NessunoAttributo4
        {
            get
            {
                if (_nessunoAttributo4 == MasterDetailTreeSumItemView.BusySymbol)
                {
                    Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    {
                        _nessunoAttributo4 = await GetAttributo4(Guid.Empty);
                        if (_nessunoAttributo4 != null)
                            RaisePropertyChanged(GetPropertyName(() => NessunoAttributo4));
                    });
                }
                return _nessunoAttributo4;
            }
            set => SetProperty(ref _nessunoAttributo4, value);
        }

        string _headerAttrbuto3 = "header 3";
        public string HeaderAttributo3
        {
            get => _headerAttrbuto3;
            set => SetProperty(ref _headerAttrbuto3, value);
        }

        string _headerAttrbuto4 = "header 4";
        public string HeaderAttributo4
        {
            get => _headerAttrbuto4;
            set => SetProperty(ref _headerAttrbuto4, value);
        }

        internal async Task<string> GetAttributo3(Guid entityId)
        {
            GroupData groupData = await GetSummarizeAttributi();
            if (groupData != null)
            {
                Attributo sourceAttGuid = EntitiesHelper.GetSourceAttributo(Owner.SenderAttRif.EntityTypeKey, Owner.SenderAttRif.ReferenceCodiceGuid);
                ValoreAttributoGuid valAttGuid = sourceAttGuid.ValoreAttributo as ValoreAttributoGuid;
                if (valAttGuid != null)
                {
                    string strId = entityId.ToString();

                    if (groupData.GroupRecords.ContainsKey(strId))
                        if (groupData.GroupRecords[strId].Attributi.ContainsKey(valAttGuid.SummarizeAttributo3.CodiceAttributo))
                        {
                            string strVal = groupData.GroupRecords[strId].Attributi[valAttGuid.SummarizeAttributo3.CodiceAttributo];
                            return strVal;

                        }
                }
            }

            return string.Empty;
        }

        internal async Task<string> GetAttributo4(Guid entityId)
        {
            GroupData groupData = await GetSummarizeAttributi();
            if (groupData != null)
            {
                Attributo sourceAttGuid = EntitiesHelper.GetSourceAttributo(Owner.SenderAttRif.EntityTypeKey, Owner.SenderAttRif.ReferenceCodiceGuid);
                ValoreAttributoGuid valAttGuid = sourceAttGuid.ValoreAttributo as ValoreAttributoGuid;
                if (valAttGuid != null)
                {
                    string strId = entityId.ToString();

                    if (groupData.GroupRecords.ContainsKey(strId))
                        if (groupData.GroupRecords[strId].Attributi.ContainsKey(valAttGuid.SummarizeAttributo4.CodiceAttributo))
                        {
                            string strVal = groupData.GroupRecords[strId].Attributi[valAttGuid.SummarizeAttributo4.CodiceAttributo];
                            return strVal;

                        }
                }
            }

            return string.Empty;
        }

        
        public async Task<GroupData> GetSummarizeAttributi()
        {
            if (taskSummarizeAttributi != null)
                return await taskSummarizeAttributi;

            return null;
        }


        public void SetAttributoSum(int attributoSumIndex)
        {
            if (Owner.SenderAttRif == null)
                return;

            Attributo sourceAttGuid = EntitiesHelper.GetSourceAttributo(Owner.SenderAttRif.EntityTypeKey, Owner.SenderAttRif.ReferenceCodiceGuid);
            ValoreAttributoGuid valAttGuid = sourceAttGuid.ValoreAttributo as ValoreAttributoGuid;
            if (valAttGuid == null)
                return;

            EntityType senderEntType = DataService.GetEntityType(Owner.SenderAttRif.EntityTypeKey);
            if (senderEntType == null) return;

            Attributo att = null;
            if (attributoSumIndex == 3)
            {
                if (senderEntType.Attributi.TryGetValue(valAttGuid.SummarizeAttributo3.CodiceAttributo, out att))
                {
                    HeaderAttributo3 = att.Etichetta;

                    foreach (var item in Entities)
                        (item as MasterDetailTreeSumItemView).SumAttributo3 = MasterDetailTreeSumItemView.BusySymbol;

                    NessunoAttributo3 = MasterDetailTreeSumItemView.BusySymbol;
                }
                else
                {
                    HeaderAttributo3 = LocalizationProvider.GetString("_Nessuno");

                    foreach (var item in Entities)
                        (item as MasterDetailTreeSumItemView).SumAttributo3 = string.Empty;

                    NessunoAttributo3 = string.Empty;
                }

            }
            else if (attributoSumIndex == 4)
            {
                if (senderEntType.Attributi.TryGetValue(valAttGuid.SummarizeAttributo4.CodiceAttributo, out att))
                {
                    HeaderAttributo4 = att.Etichetta;

                    foreach (var item in Entities)
                        (item as MasterDetailTreeSumItemView).SumAttributo4 = MasterDetailTreeSumItemView.BusySymbol;
                    
                    NessunoAttributo4 = MasterDetailTreeSumItemView.BusySymbol;
                }
                else
                {
                    HeaderAttributo4 = LocalizationProvider.GetString("_Nessuno");

                    foreach (var item in Entities)
                        (item as MasterDetailTreeSumItemView).SumAttributo4 = string.Empty;
                    
                    NessunoAttributo4 = string.Empty;
                }
            }


        }

        public void CalcolaSummarizeAttributiAsync()
        {
            taskSummarizeAttributi = null;

            taskSummarizeAttributi = Task.Run(() =>
            {
                return CalcolaSummarizeAttributi();
            });
        }

        public GroupData CalcolaSummarizeAttributi()
        {


            if (Owner.SenderAttRif == null)
                return null;


            Attributo sourceAttGuid = EntitiesHelper.GetSourceAttributo(Owner.SenderAttRif.EntityTypeKey, Owner.SenderAttRif.ReferenceCodiceGuid);
            ValoreAttributoGuid valAttGuid = sourceAttGuid.ValoreAttributo as ValoreAttributoGuid;
            if (valAttGuid == null)
                return null;



            GroupData groupData = new GroupData();
            groupData.EntityTypeKey = Owner.SenderAttRif.EntityTypeKey;
            groupData.Items.Add(new AttributoGroupData() { CodiceAttributo = Owner.SenderAttRif.ReferenceCodiceGuid });
            groupData.GroupRecords = FilteredEntitiesViewInfo.Keys.ToDictionary(item => item.ToString(), item =>
            {
                var grd = new GroupRecordData();

                if (!string.IsNullOrEmpty(valAttGuid.SummarizeAttributo3.CodiceAttributo))
                    grd.Attributi.TryAdd(valAttGuid.SummarizeAttributo3.CodiceAttributo, null);

                if (!string.IsNullOrEmpty(valAttGuid.SummarizeAttributo4.CodiceAttributo))
                    grd.Attributi.TryAdd(valAttGuid.SummarizeAttributo4.CodiceAttributo, null);

                return grd;
            });

            //aggiungo il gruppo dei vuoti
            var voidGrd = new GroupRecordData();
            if (!string.IsNullOrEmpty(valAttGuid.SummarizeAttributo3.CodiceAttributo))
                voidGrd.Attributi.TryAdd(valAttGuid.SummarizeAttributo3.CodiceAttributo, null);

            if (!string.IsNullOrEmpty(valAttGuid.SummarizeAttributo4.CodiceAttributo))
                voidGrd.Attributi.TryAdd(valAttGuid.SummarizeAttributo4.CodiceAttributo, null);

            groupData.GroupRecords.Add(Guid.Empty.ToString(), voidGrd);



            //Aggiungo i figli per ogni gruppo
            var res = DataService.GetFilteredEntities(Owner.SenderAttRif.EntityTypeKey, null, null, groupData, out _);
            
            foreach (var item in res)
            {
                var groupKey = item.GroupKeys.FirstOrDefault();
                
                Guid parentId = Guid.Empty;

                if (!string.IsNullOrEmpty(groupKey))
                    parentId = new Guid(groupKey);

                if (parentId == Guid.Empty)
                {
                    if (groupData.GroupRecords.ContainsKey(groupKey))
                        groupData.GroupRecords[groupKey].ChildsId.Add(item.Id);
                }
                else
                {

                    while (parentId != Guid.Empty)
                    {
                        string parentIdstr = parentId.ToString();

                        if (groupData.GroupRecords.ContainsKey(parentIdstr))
                            groupData.GroupRecords[parentIdstr].ChildsId.Add(item.Id);

                        parentId = FilteredEntitiesViewInfo[parentId].ParentId;
                    }
                }
            }




            DataService.FillGroupData(groupData);

            return groupData;
        }

        public override void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => WidthAttributo3));
            RaisePropertyChanged(GetPropertyName(() => WidthAttributo4));
            RaisePropertyChanged(GetPropertyName(() => IsItemsSummarized));
            base.UpdateUI();
        }

        static double _widthAttributo3 = 80;
        public double WidthAttributo3
        {
            get => _widthAttributo3;
            set => SetProperty(ref _widthAttributo3, value);
        }

        static double _widthAttributo4 = 80;
        public double WidthAttributo4
        {
            get => _widthAttributo4;
            set => SetProperty(ref _widthAttributo4, value);
        }

        public override Task UpdateCache(bool updateDetail = false)
        {
            if (IsItemsSummarized)
            { 
                SetAttributoSum(3);
                SetAttributoSum(4);

                CalcolaSummarizeAttributiAsync();
            }

            return base.UpdateCache(updateDetail);
        }

        public bool IsItemsSummarized { get => Owner.IsItemsSummarized; }
    }


}
