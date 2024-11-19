using CommonResources;
using Commons;
using DevExpress.Data.Extensions;
using DevExpress.Mvvm.Native;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MasterDetailView
{
    public interface ValoreConditionView
    {
    }


    public class ValoreConditionsGroupView : NotificationBase, ValoreConditionView
    {
        //IN
        public string CodiceAttributoFixed { get; set; } = null;
        public EntityType EntityType { get; set; } = null;
        public IDataService DataService { get; set; } = null;
        public ValoreConditions Data { get; set; } = null;
        public bool AND_Limited { get; set; } = false;
        public bool AllowAsItem { get; set; } = false;

        ValoreConditionsGroupView _owner { get; set; } = null;
        ValoreConditionsGroupView Owner { get => _owner; }

        EntitiesHelper _entitiesHelper = null;
        public EntitiesHelper EntitiesHelper { get => _entitiesHelper; }

        public ValoreConditionsGroupView()
        {
        }

        /// <summary>
        /// Fake constructor, la classe viene reinstanziata al momento del istanziamento di ValoreConditionsGroupCtrl e vengono trasferiti dati
        /// </summary>
        /// <param name="owner"></param>
        public ValoreConditionsGroupView(ValoreConditionsGroupView owner)
        {
            _owner = owner;
            _entitiesHelper = new EntitiesHelper(owner.DataService);

            AttributiCodice = owner.AttributiCodice;
        }

        public static Dictionary<int, ValoreConditionsGroupView> groups = new Dictionary<int, ValoreConditionsGroupView>();


        public static ValoreConditionView FakeGroupView { get; set; } = null;
        public static List<ValoreConditionsGroupView> FakeGroupsView = new List<ValoreConditionsGroupView>();

        public List<string> AttributiCodice { get; protected set; } = new List<string>();

        ObservableCollection<ValoreConditionView> _conditionItems = new ObservableCollection<ValoreConditionView>();
        public ObservableCollection<ValoreConditionView> ConditionItems
        {
            get => _conditionItems;
            set => SetProperty(ref _conditionItems, value);
        }

        

        public void Load()
        {
            if (EntityType == null)
                return;

            if (Data == null)
                return;

            if (DataService == null)
                return;

            _entitiesHelper = new EntitiesHelper(DataService);

            if (CodiceAttributoFixed != null)
            {
                AttributiCodice = new List<string> { CodiceAttributoFixed};
            }
            else
            {
                AttributiCodice = EntityType.Attributi.Values.Where(item =>
                {
                    var sourceAtt = _entitiesHelper.GetSourceAttributo(item);
                    if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo ||
                        sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale ||
                        sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita ||
                        sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data ||
                        sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco ||
                        sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                    {
                        return true;
                    }
                    return false;
                }).OrderBy(item => item.Etichetta).Select(item => item.Codice).ToList();
            }
            


            if (Data.MainGroup == null || !Data.MainGroup.Conditions.Any())
            {
                ConditionItems.Clear();
                GroupOperatorIndex = (int)ValoreConditionsGroupOperator.And - 1;

                AttributoValoreConditionSingleView attCondSingle = NewAttributoValoreConditionSingleView();
                attCondSingle.Load();
                ConditionItems.Add(attCondSingle);

            }
            else
            {
                ConditionItems.Clear();
                GroupOperatorIndex = (int)ValoreConditionsGroupOperator.And - 1;
                Load(Data.MainGroup/*, valoriUnivociResult*/);
            }

        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        void Load(ValoreConditionsGroup group)
        {
            GroupOperatorIndex = (int)group.Operator - 1;

            foreach (ValoreCondition valCond in group.Conditions)
            {
                if (valCond is ValoreConditionsGroup)
                {
                    ValoreConditionsGroupView groupView = new ValoreConditionsGroupView(this);
                    groupView.CodiceAttributoFixed = CodiceAttributoFixed;
                    groupView.EntityType = EntityType;
                    groupView.DataService = DataService;

                    ConditionItems.Add(groupView);
                    groupView.Load(valCond as ValoreConditionsGroup/*, valoriUnivociResult*/);
                    
                }
                else if (valCond is ValoreConditionSingle)//per file vecchi dove non veniva salvato il codice attributo nelle condizioni
                {
                    AttributoValoreConditionSingleView attCondSingle = NewAttributoValoreConditionSingleView();

                    ValoreConditionSingle valCondSingle = valCond as ValoreConditionSingle;
                    
                    //attCondSingle.Load(/*ValoriUnivociResult, */valCondSingle);

                    //ConditionItems.Add(attCondSingle);
                }
                else if (valCond is AttributoValoreConditionSingle)
                {
                    AttributoValoreConditionSingleView attCondSingle = NewAttributoValoreConditionSingleView();

                    AttributoValoreConditionSingle valCondSingle = valCond as AttributoValoreConditionSingle;
                    attCondSingle.Load(valCondSingle);

                    ConditionItems.Add(attCondSingle);
                }
                UpdateUI();

            }

        }

        public void UpdateData()
        {   
            Data.MainGroup.Conditions.Clear();
            Data.MainGroup.Operator = ValoreConditionsGroupOperator.Nothing;
            UpdateData(this, Data.MainGroup);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="groupView"></param>
        /// <param name="groupData"></param>
        public void UpdateData(ValoreConditionsGroupView groupView, ValoreConditionsGroup groupData)
        {
            groupData.Operator = (ValoreConditionsGroupOperator) groupView.GroupOperatorIndex + 1;

            foreach (var cond in groupView.ConditionItems)
            {
                if (cond is ValoreConditionsGroupView)
                {
                    ValoreConditionsGroupView condGroupView = cond as ValoreConditionsGroupView;
                    ValoreConditionsGroup condGroup = new ValoreConditionsGroup();
                    condGroup.Operator = (ValoreConditionsGroupOperator)condGroupView.GroupOperatorIndex + 1;
                    groupData.Conditions.Add(condGroup);
                    UpdateData(condGroupView, condGroup);
                }
                else if (cond is AttributoValoreConditionSingleView)
                {
                    AttributoValoreConditionSingleView attCondSingleView = cond as AttributoValoreConditionSingleView;

                    AttributoValoreConditionSingle condSingle = attCondSingleView.GetData();
                    if (condSingle != null)
                        groupData.Conditions.Add(condSingle);
                }
            }
        }



        int _groupOperatorIndex = (int) ValoreConditionsGroupOperator.Nothing;
        public int GroupOperatorIndex
        {
            get => _groupOperatorIndex;
            set
            {
                if (SetProperty(ref _groupOperatorIndex, value))
                    UpdateUI();
            }
        }

        public SolidColorBrush GroupBorderBrush
        {
            get
            {
                if (_groupOperatorIndex == (int)ValoreConditionsGroupOperator.And)
                    return new SolidColorBrush(Colors.SteelBlue);
                else                    
                    return new SolidColorBrush(Colors.LightSteelBlue);
            }
        }

        AttributoValoreConditionSingleView NewAttributoValoreConditionSingleView()
        {
            return new AttributoValoreConditionSingleView(this);

        }




        public ICommand AddConditionCommand { get => new CommandHandler(() => this.AddCondition()); }
        void AddCondition()
        {
            AttributoValoreConditionSingleView attCondSingle = NewAttributoValoreConditionSingleView();
            attCondSingle.Load();

            ConditionItems.Add(attCondSingle);
            UpdateUI();
        }

        public ICommand AddConditionsGroupCommand { get => new CommandHandler(() => this.AddConditionsGroup()); }
        void AddConditionsGroup()
        {
            //Attenzione! ValoreConditionsGroupView viene reistanziata al momento dell'istanziamento di ValoreConditionsGroupCtrl
            ValoreConditionsGroupView groupView = new ValoreConditionsGroupView(this);
            groupView.CodiceAttributoFixed = CodiceAttributoFixed;
            groupView.EntityType = EntityType;
            groupView.DataService = DataService;


            AttributoValoreConditionSingleView attCondSingle = NewAttributoValoreConditionSingleView();
            attCondSingle.Load();
            groupView.ConditionItems.Add(attCondSingle);

            ConditionItems.Add(groupView);
            UpdateUI();
        }



        public ICommand RemoveConditionsGroupCommand { get => new CommandHandler(() => this.RemoveConditionsGroup()); }
        void RemoveConditionsGroup()
        {
            if (_owner == null)
                return;

            _owner.RemoveCondition(this);
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ConditionItems));
            RaisePropertyChanged(GetPropertyName(() => IsConditionsGroupRemovable));
            RaisePropertyChanged(GetPropertyName(() => GroupOperatorIndex));
            RaisePropertyChanged(GetPropertyName(() => GroupBorderBrush));
        }


        public object ValoreConditionsView { get => this; }

        public void RemoveCondition(ValoreConditionView condView)
        {
            ConditionItems.Remove(condView);
            UpdateUI();
        }

        public bool IsConditionsGroupRemovable { get => _owner != null; }


    }

    public class ValoreConditionSingleView : NotificationBase
    {
        AttributoValoreConditionSingleView _owner = null;
        public AttributoValoreConditionSingleView Owner {get => _owner;}

        public ValoreConditionSingleView(AttributoValoreConditionSingleView owner)
        {
            _owner = owner;
        }

      

        ObservableCollection<string> _valoriUnivociView = new ObservableCollection<string>();
        public ObservableCollection<string> ValoriUnivociView { get => _valoriUnivociView; set => SetProperty(ref _valoriUnivociView, value); }

        public virtual void Load(ValoreConditionSingle valCondSingle = null) { }
        protected virtual void LoadConditions() { }

        protected ObservableCollection<ValoreConditionEnumView> _conditionsView = new ObservableCollection<ValoreConditionEnumView>();
        public ObservableCollection<ValoreConditionEnumView> ConditionsView
        {
            get => _conditionsView;
        }

        ValoreConditionEnumView _currentCondition = null;
        public ValoreConditionEnumView CurrentCondition
        {
            get => _currentCondition;
            set => SetProperty(ref _currentCondition, value);
        }

        public virtual ValoreConditionSingle GetData() { return null; }

        public static string RemoveNonNumeric(string value) => Regex.Replace(value, @"[^0-9,.-]", string.Empty);

        public static string ComparisonValueCultureInvariant(string comparisonValue)
        {
            if (LocalizationProvider.GetString(ValoreHelper.ValoreAsItem) == comparisonValue)
                return ValoreHelper.ValoreAsItem;

            return comparisonValue;

        }

    }

    public class ValoreContabilitaConditionSingleView : ValoreConditionSingleView
    {
        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreContabilitaConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }
        
        string _comparisonValue = string.Empty;
        public string ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (this._loadValoriFiltroCancellationTokenSource == null)
                {
                    if (SetProperty(ref _comparisonValue, value))
                    {
                        LoadValoriUnivociResult();
                    }
                }
            }
        }

        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();

            if (valCondSingle != null)
            {
                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                if (valCondSingle.Valore != null)
                    ComparisonValue = (valCondSingle.Valore as ValoreContabilita).V;
            }
            else
                ComparisonValue = string.Empty;

            LoadValoriUnivociResult();


        }

        public override ValoreConditionSingle GetData()
        {
            double reale = 0.0;
            if (Double.TryParse(ComparisonValue, out reale))
            {
                return new ValoreConditionSingle()
                {
                    Valore = new ValoreContabilita() { V = ComparisonValue },
                    Condition = CurrentCondition.Id,
                };
            }

            return null;
        }

        protected override void LoadConditions()
        {
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Unequal, Name = LocalizationProvider.GetString("DiversoDa") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterThan, Name = LocalizationProvider.GetString("MaggioreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterOrEqualThan, Name = LocalizationProvider.GetString("MaggioreUgualeDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessThan, Name = LocalizationProvider.GetString("MinoreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessOrEqualThan, Name = LocalizationProvider.GetString("MinoreUgualeDi") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        async void LoadValoriUnivociResult()
        {
            // If a cancellation token already exists (for a previous task),
            // cancel it.
            if (this._loadValoriFiltroCancellationTokenSource != null)
                this._loadValoriFiltroCancellationTokenSource.Cancel();

            // Create a new cancellation token for the new task.
            this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;


            List<Guid> entsFound = null;
            DataService.GetFilteredEntities(EntityType.GetKey(), null, null, null, out entsFound);
            List<string> valoriUnivociResult = await DataService.GetValoriUnivociAsync(EntityType.GetKey(), entsFound, Attributo.Codice, 1, ComparisonValue);


            if (cancellationToken.IsCancellationRequested)
                return;

            List<double> valoriUnivoci = new List<double>();
            foreach (string str in valoriUnivociResult)
            {
                string strNum = RemoveNonNumeric(str);

                double number = 0.0;
                if (Double.TryParse(strNum, out number))
                {
                    valoriUnivoci.Add(number);
                }
            }

            valoriUnivociResult = valoriUnivoci.Select(item => item.ToString()).Order().ToList();

            ValoriUnivociView = new ObservableCollection<string>(valoriUnivociResult);

            UpdateUI();

            this._loadValoriFiltroCancellationTokenSource = null;
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }

    }

    public class ValoreRealeConditionSingleView : ValoreConditionSingleView
    {

        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreRealeConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }

        string _comparisonValue = null;
        public string ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (this._loadValoriFiltroCancellationTokenSource == null)
                {
                    if (SetProperty(ref _comparisonValue, value))
                    {
                        LoadValoriUnivociResult();
                    }
                }
            }
        }


        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();

            if (valCondSingle != null)
            {

                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                //if (valCondSingle.Valore != null)
                //    ComparisonValue = (valCondSingle.Valore as ValoreReale).V;

                string v = (valCondSingle.Valore as ValoreReale).V;
                if (v == ValoreHelper.ValoreAsItem)
                    ComparisonValue = LocalizationProvider.GetString(ValoreHelper.ValoreAsItem);
                else
                    ComparisonValue = v;
            }
            
            LoadValoriUnivociResult();
        }

        public override ValoreConditionSingle GetData()
        {
            var comparisonCulturalInvariant = ComparisonValueCultureInvariant(ComparisonValue);

            double reale = 0.0;
            if (Double.TryParse(ComparisonValue, out reale))
            {
                return new ValoreConditionSingle()
                {
                    Valore = new ValoreReale() { V = ComparisonValue },
                    Condition = CurrentCondition.Id,
                };
            }
            else if (comparisonCulturalInvariant == ValoreHelper.ValoreAsItem)
            {
                return new ValoreConditionSingle()
                {
                    Valore = new ValoreReale() { V = comparisonCulturalInvariant },
                    Condition = CurrentCondition.Id,
                };
            }

            return null;
        }

        protected override void LoadConditions()
        {
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Unequal, Name = LocalizationProvider.GetString("DiversoDa") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterThan, Name = LocalizationProvider.GetString("MaggioreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterOrEqualThan, Name = LocalizationProvider.GetString("MaggioreUgualeDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessThan, Name = LocalizationProvider.GetString("MinoreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessOrEqualThan, Name = LocalizationProvider.GetString("MinoreUgualeDi") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        async void LoadValoriUnivociResult()
        {

            // If a cancellation token already exists (for a previous task),
            // cancel it.
            if (this._loadValoriFiltroCancellationTokenSource != null)
                this._loadValoriFiltroCancellationTokenSource.Cancel();

            // Create a new cancellation token for the new task.
            this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

            List<Guid> entsFound = null;
            DataService.GetFilteredEntities(EntityType.GetKey(), null, null, null, out entsFound);
            List<string> valoriUnivociResult = await DataService.GetValoriUnivociAsync(EntityType.GetKey(), entsFound, Attributo.Codice, 1, ComparisonValue);


            if (cancellationToken.IsCancellationRequested)
                return;




            List<double> valoriUnivoci = new List<double>();
            foreach (string str in valoriUnivociResult)
            {
                string strNum = RemoveNonNumeric(str);

                double number = 0.0;
                if (Double.TryParse(strNum, out number))
                {
                    valoriUnivoci.Add(number);
                }
            }  
            

            valoriUnivociResult = valoriUnivoci.Order().Select(item => item.ToString()).ToList();

            if (Owner.Owner.AllowAsItem)
                valoriUnivociResult.Insert(0, LocalizationProvider.GetString(ValoreHelper.ValoreAsItem));

            ValoriUnivociView = new ObservableCollection<string>(valoriUnivociResult);

            UpdateUI();

            this._loadValoriFiltroCancellationTokenSource = null;
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }
    }

    public class ValoreDataConditionSingleView : ValoreConditionSingleView
    {

        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreDataConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }

        DateTime? _comparisonValue = null;
        public DateTime? ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (SetProperty(ref _comparisonValue, value))
                {

                }
            }
        }


        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();

            if (valCondSingle != null)
            {

                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                if (valCondSingle.Valore != null)
                {
                    ValoreData valData = valCondSingle.Valore as ValoreData;
                    ComparisonValue = (valCondSingle.Valore as ValoreData).V;
                }
            }
            
            ValoriUnivociView = new ObservableCollection<string>();
        }

        public override ValoreConditionSingle GetData()
        {

            return new ValoreConditionSingle()
            {
                Valore = new ValoreData() { V = ComparisonValue },
                Condition = CurrentCondition.Id,
            };
        }

        protected override void LoadConditions()
        {

            //Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);

            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Unequal, Name = LocalizationProvider.GetString("DiversoDa") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterThan, Name = LocalizationProvider.GetString("MaggioreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.GreaterOrEqualThan, Name = LocalizationProvider.GetString("MaggioreUgualeDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessThan, Name = LocalizationProvider.GetString("MinoreDi") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.LessOrEqualThan, Name = LocalizationProvider.GetString("MinoreUgualeDi") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        public string FormatString
        {
            get
            {
                Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);
                return sourceAtt.ValoreFormat;
            }
        }

        public string Mask { get => TimePickerVisibility? "g" : "d"; } //for devexpress

        public bool TimePickerVisibility
        {
            get
            {
                Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);
                if (sourceAtt.ValoreFormat.Contains("H") || sourceAtt.ValoreFormat.Contains("m"))
                    return true;
                else
                    return false;
            }
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }

    }

    public class ValoreTestoConditionSingleView : ValoreConditionSingleView
    {
        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreTestoConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }

        string _comparisonValue = null;
        public string ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (this._loadValoriFiltroCancellationTokenSource == null)
                {

                    if (SetProperty(ref _comparisonValue, value))
                    {
                        LoadValoriUnivociResult();
                    }
                }
            }
        }


        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();      
            
            if (valCondSingle != null)
            {

                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                if (valCondSingle.Valore is ValoreTesto)
                {
                    string v = (valCondSingle.Valore as ValoreTesto).V;
                    if (v == ValoreHelper.ValoreAsItem)
                        ComparisonValue = LocalizationProvider.GetString(ValoreHelper.ValoreAsItem);
                    else
                        ComparisonValue = v;
                }
            }

            LoadValoriUnivociResult();


        }

        public override ValoreConditionSingle GetData()
        {
            return new ValoreConditionSingle()
            {
                Valore = new ValoreTesto() { V = ComparisonValueCultureInvariant(ComparisonValue) },
                Condition = CurrentCondition.Id,
            };
        }

        protected override void LoadConditions()
        {

            //Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);

            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Unequal, Name = LocalizationProvider.GetString("DiversoDa") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.StartsWith, Name = LocalizationProvider.GetString("IniziaCon") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.EndsWith, Name = LocalizationProvider.GetString("TerminaCon") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Contains, Name = LocalizationProvider.GetString("Contiene") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.NotContains, Name = LocalizationProvider.GetString("Non contiene") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        async void LoadValoriUnivociResult()
        {
            // If a cancellation token already exists (for a previous task),
            // cancel it.
            if (this._loadValoriFiltroCancellationTokenSource != null)
                this._loadValoriFiltroCancellationTokenSource.Cancel();

            // Create a new cancellation token for the new task.
            this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

            List<Guid> entsFound = null;
            DataService.GetFilteredEntities(EntityType.GetKey(), null, null, null, out entsFound);
            List<string> valoriUnivociResult = await DataService.GetValoriUnivociAsync(EntityType.GetKey(), entsFound, Attributo.Codice, 1, ComparisonValue);

            if (cancellationToken.IsCancellationRequested)
                return;

            if (Owner.Owner.AllowAsItem)
                valoriUnivociResult.Add(LocalizationProvider.GetString(ValoreHelper.ValoreAsItem));

            ValoriUnivociView = new ObservableCollection<string>(valoriUnivociResult.Order()) ;

            UpdateUI();

            this._loadValoriFiltroCancellationTokenSource = null;
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }

    }

    public class ValoreElencoConditionSingleView : ValoreConditionSingleView
    {
        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreElencoConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }

        string _comparisonValue = null;
        public string ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (this._loadValoriFiltroCancellationTokenSource == null)
                {
                    if (SetProperty(ref _comparisonValue, value))
                    {
                        LoadValoriUnivociResult();
                    }
                }
            }
        }


        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();

            if (valCondSingle != null)
            {

                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                if (valCondSingle.Valore is ValoreElenco)
                {
                    string v = (valCondSingle.Valore as ValoreElenco).V;
                    if (v == ValoreHelper.ValoreAsItem)
                        ComparisonValue = LocalizationProvider.GetString(ValoreHelper.ValoreAsItem);
                    else
                        ComparisonValue = v;
                    
                }
            }

            LoadValoriUnivociResult();


        }

        public override ValoreConditionSingle GetData()
        {
            return new ValoreConditionSingle()
            {
                Valore = new ValoreElenco() { V = ComparisonValueCultureInvariant(ComparisonValue) },
                Condition = CurrentCondition.Id,
            };
        }

        protected override void LoadConditions()
        {

            //Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);

            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Unequal, Name = LocalizationProvider.GetString("DiversoDa") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.StartsWith, Name = LocalizationProvider.GetString("IniziaCon") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.EndsWith, Name = LocalizationProvider.GetString("TerminaCon") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Contains, Name = LocalizationProvider.GetString("Contiene") });
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.NotContains, Name = LocalizationProvider.GetString("Non contiene") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        // Cancellation token for the latest task.
        private CancellationTokenSource _loadValoriFiltroCancellationTokenSource;

        async void LoadValoriUnivociResult()
        {
            // If a cancellation token already exists (for a previous task),
            // cancel it.
            if (this._loadValoriFiltroCancellationTokenSource != null)
                this._loadValoriFiltroCancellationTokenSource.Cancel();

            // Create a new cancellation token for the new task.
            this._loadValoriFiltroCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = this._loadValoriFiltroCancellationTokenSource.Token;

            List<Guid> entsFound = null;
            DataService.GetFilteredEntities(EntityType.GetKey(), null, null, null, out entsFound);
            List<string> valoriUnivociResult = await DataService.GetValoriUnivociAsync(EntityType.GetKey(), entsFound, Attributo.Codice, 1, ComparisonValue);

            if (cancellationToken.IsCancellationRequested)
                return;

            if (Owner.Owner.AllowAsItem)
                valoriUnivociResult.Add(LocalizationProvider.GetString(ValoreHelper.ValoreAsItem));

            ValoriUnivociView = new ObservableCollection<string>(valoriUnivociResult.Order());

            UpdateUI();

            this._loadValoriFiltroCancellationTokenSource = null;
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }

    }

    public class ValoreBooleanoConditionSingleView : ValoreConditionSingleView
    {

        IDataService DataService { get; set; }
        EntityType EntityType { get; set; }
        Attributo Attributo { get; set; }

        public ValoreBooleanoConditionSingleView(AttributoValoreConditionSingleView owner) : base(owner)
        {
            DataService = Owner.Owner.DataService;
            EntityType = Owner.Owner.EntityType;
            Attributo = Owner.Attributo;
        }

        bool? _comparisonValue = null;
        public bool? ComparisonValue
        {
            get => _comparisonValue;
            set
            {
                if (SetProperty(ref _comparisonValue, value))
                {

                }
            }
        }


        public override void Load(ValoreConditionSingle valCondSingle = null)
        {
            LoadConditions();

            if (valCondSingle != null)
            {

                if (valCondSingle.Condition != ValoreConditionEnum.Nothing)
                    CurrentCondition = _conditionsView.FirstOrDefault(item => item.Id == valCondSingle.Condition);

                if (valCondSingle.Valore is ValoreBooleano)
                {
                    ValoreBooleano valData = valCondSingle.Valore as ValoreBooleano;
                    ComparisonValue = (valCondSingle.Valore as ValoreBooleano).V;
                }
            }
            else
                ComparisonValue = true;

            ValoriUnivociView = new ObservableCollection<string>();
        }

        public override ValoreConditionSingle GetData()
        {

            return new ValoreConditionSingle()
            {
                Valore = new ValoreBooleano() { V = ComparisonValue },
                Condition = CurrentCondition.Id,
            };
        }

        protected override void LoadConditions()
        {
            _conditionsView.Add(new ValoreConditionEnumView() { Id = ValoreConditionEnum.Equal, Name = LocalizationProvider.GetString("UgualeA") });

            CurrentCondition = _conditionsView.FirstOrDefault();
        }

        public string FormatString
        {
            get
            {
                Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);
                return sourceAtt.ValoreFormat;
            }
        }

        public string Mask { get => TimePickerVisibility ? "g" : "d"; } //for devexpress

        public bool TimePickerVisibility
        {
            get
            {
                Attributo sourceAtt = Owner.EntitiesHelper.GetSourceAttributo(Owner.Attributo);
                if (sourceAtt.ValoreFormat.Contains("H") || sourceAtt.ValoreFormat.Contains("m"))
                    return true;
                else
                    return false;
            }
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ValoriUnivociView));
        }

    }

    public class ValoreConditionEnumView
    {
        public ValoreConditionEnum Id { get; set; } = ValoreConditionEnum.Nothing;
        public string Name { get; set; } = string.Empty;
    }

    public class AttributoValoreConditionSingleView : NotificationBase, ValoreConditionView
    {
        ValoreConditionsGroupView _owner = null;
        public ValoreConditionsGroupView Owner { get => _owner; }

        public Attributo Attributo { get; protected set; }
        public ValoreConditionSingleView ValoreConditionSingleView { get; set; }

        public EntitiesHelper EntitiesHelper { get => _owner.EntitiesHelper; }

        AttributoValoreConditionSingle _valoreConditionSingle { get; set; }

        ObservableCollection<string> _attributiEtichetta = new ObservableCollection<string>();
        public ObservableCollection<string> AttributiEtichetta
        {
            get => _attributiEtichetta;
            set => SetProperty(ref _attributiEtichetta, value);
        }



        public AttributoValoreConditionSingleView(ValoreConditionsGroupView owner)
        {
            _owner = owner;
        }

        public void Load(AttributoValoreConditionSingle valCondSingle = null)
        {
            _valoreConditionSingle = valCondSingle;

            var attsEtichetta = new ObservableCollection<string>(_owner.AttributiCodice.Select(item => _owner.EntityType.Attributi[item].Etichetta));

            int existNessuno = 0;
            if (attsEtichetta.Count > 1)
            {
                existNessuno = 1;
                attsEtichetta.Insert(0, LocalizationProvider.GetString("_Nessuno"));
            } 

            AttributiEtichetta = attsEtichetta;

            if (valCondSingle != null)
            {
                int index = _owner.AttributiCodice.IndexOf(valCondSingle.CodiceAttributo);
                if ((index + existNessuno) < attsEtichetta.Count)
                {
                    if (index + existNessuno > 0)
                        _selectedAttributoIndex = index + existNessuno;
                    else
                        _selectedAttributoIndex = 0;


                    SelectAttributo(true);
                    //SelectedAttributoIndex = index + existNessuno;
                }
            }
            else
                SelectAttributo(true);

            UpdateUI();

        }

        public object ValoreConditionsView => this;
        

        int _selectedAttributoIndex = 0;
        public int SelectedAttributoIndex
        {
            get => _selectedAttributoIndex;
            set
            {
                if (SetProperty(ref _selectedAttributoIndex, value))
                    SelectAttributo();
            }
        }

        void SelectAttributo(bool isLoading = false)
        {
            Attributo att = null;
            


            if (Owner.CodiceAttributoFixed != null)
            {
                _owner.EntityType.Attributi.TryGetValue(Owner.CodiceAttributoFixed, out att);
            }
            if (0 < _selectedAttributoIndex && _selectedAttributoIndex <= _owner.AttributiCodice.Count)
            {
                _owner.EntityType.Attributi.TryGetValue(_owner.AttributiCodice[_selectedAttributoIndex - 1], out att);
            }

            Attributo = att;

            if (Attributo != null)
            {
                ValoreConditionSingleView = NewValoreConditionSingleView();
                if (_valoreConditionSingle != null && isLoading)
                    ValoreConditionSingleView.Load(_valoreConditionSingle.ValoreConditionSingle);
                else
                    ValoreConditionSingleView.Load();
            }
            else
            {
                ValoreConditionSingleView = null;
            }

            UpdateUI();

        }

        public AttributoValoreConditionSingle GetData()
        {
            if (ValoreConditionSingleView != null)
            {
                return new AttributoValoreConditionSingle()
                {
                    CodiceAttributo = Attributo.Codice,
                    ValoreConditionSingle = ValoreConditionSingleView.GetData(),
                };
            }

            return null;
        }

        ValoreConditionSingleView NewValoreConditionSingleView()
        {
            Attributo sourceAtt = _owner.EntitiesHelper.GetSourceAttributo(Attributo);


            if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                return new ValoreContabilitaConditionSingleView(this);
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                return new ValoreRealeConditionSingleView(this);
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
                return new ValoreDataConditionSingleView(this);
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                return new ValoreTestoConditionSingleView(this);
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
                return new ValoreElencoConditionSingleView(this);
            else if (sourceAtt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                return new ValoreBooleanoConditionSingleView(this);
            else
                return null;

        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => AttributiEtichetta));
            RaisePropertyChanged(GetPropertyName(() => ValoreConditionsView));
            RaisePropertyChanged(GetPropertyName(() => ValoreConditionSingleView));
        }

        public ICommand RemoveConditionCommand { get => new CommandHandler(() => this.RemoveCondition()); }
        void RemoveCondition()
        {
            _owner.RemoveCondition(this);
        }
    }
}


