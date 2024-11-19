using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using CommonResources;
using DevExpress.Xpf.Core.ConditionalFormattingManager;
using Microsoft.Extensions.ObjectPool;
using Rejo.UI;
using ReJo.Utility;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;


namespace ReJo.UI
{
    public class RulesView : NotificationBase
    {
        string _FilterName = string.Empty;

        string _RvtFilterUniqueId = string.Empty;

        //elementi
        List<ElementId> _ElementsId = new List<ElementId>();
        List<Element> _Elements = new List<Element>();
        List<Element> _ElementsType = new List<Element>();
        Dictionary<ElementId, int> _ElementIndexById = new Dictionary<ElementId, int>();
        Dictionary<ElementId, List<ElementId>> _ElementsByType = new Dictionary<ElementId, List<ElementId>>();
        public List<ElementId> ElementsId { get => _ElementsId; }
        public List<Element> Elements { get => _Elements; }
        public List<Element> ElementsType { get => _ElementsType; }
        public Dictionary<ElementId, int> ElementIndexById { get => _ElementIndexById; }
        public Dictionary<ElementId, List<ElementId>> ElementsByType { get => _ElementsByType; }

        //parametri
        Dictionary<string, ParamInfo> _ParamsElInfo = new Dictionary<string, ParamInfo>();//key: nome parametro
        Dictionary<string, ParamInfo> _ParamsElTypeInfo = new Dictionary<string, ParamInfo>();//key: nome parametro

        //materiali
        Dictionary<string, MaterialQtaInfo> _MatsElInfo = new Dictionary<string, MaterialQtaInfo>();//key: nome materiale

        FiltersData? _FiltersData = null;

        AttributoRegola _AttributoQuantita = null;


        FiltersDataItem? _FiltersDataItem = null;

        RvtCalculator _Calculator = new RvtCalculator();

        List<RuleError> _RuleErrorsPrezzo = new List<RuleError>();
        List<RuleError> _RuleErrorsConflict = new List<RuleError>();


        ObservableCollection<ParameterItemView> _ParameterItems = new ObservableCollection<ParameterItemView>();
        public ObservableCollection<ParameterItemView> ParameterItems
        {
            get => _ParameterItems;
            set => SetProperty(ref _ParameterItems, value);
        }

        ObservableCollection<MaterialQtaItemView> _MaterialItems = new ObservableCollection<MaterialQtaItemView>();
        public ObservableCollection<MaterialQtaItemView> MaterialItems
        {
            get => _MaterialItems;
            set => SetProperty(ref _MaterialItems, value);
        }

        ObservableCollection<FormuleAttributiItemView> _FormuleAttributi = new ObservableCollection<FormuleAttributiItemView>();
        public ObservableCollection<FormuleAttributiItemView> FormuleAttributi
        {
            get => _FormuleAttributi;
            set => SetProperty(ref _FormuleAttributi, value);
        }

        string _FormulaQuantita = string.Empty;
        public string FormulaQuantita
        {
            get => _FormulaQuantita;
            set
            {
                if (SetProperty(ref _FormulaQuantita, value))
                {
                    if (CurrentRule != null)
                        CurrentRule.FormulaQta = _FormulaQuantita;
                }
            }
        }

        ObservableCollection<RulesItemView> _RulesItem = new ObservableCollection<RulesItemView>();
        public ObservableCollection<RulesItemView> RulesItem
        {
            get => _RulesItem;
            set => SetProperty(ref _RulesItem, value);
        }

        public void Load(string filterId, FiltersData filtersData)
        {
            Load(filterId, filtersData, Guid.Empty);
        }

        public void Load(string filterUniqueId, FiltersData filtersData, Guid currentRuleId)
        {
            _FiltersData = filtersData;
            _RvtFilterUniqueId = filterUniqueId;

            _Calculator.RvtCalculatorFunction = new RvtCalculatorFunction();
            _Calculator.Clear();


            LoadElements();
            LoadParameters();
            LoadMaterials();
            LoadRules(currentRuleId);

            CurrentElementFirst();
            LoadParametersList();
            LoadMaterialsQtaList();
        }

        public void OnOk()
        {
            //salvo i filtri con ID univoci e diversi da 000-000
            HashSet<Guid> filtersId = _FiltersData.Items.Where(item => item.ID != Guid.Empty).Select(item => item.ID).ToHashSet();
            _FiltersData.Items = _FiltersData.Items
                .Where(item => filtersId.Contains(item.ID))
                .Where(item =>item.RvtFilter != null && !string.IsNullOrEmpty(item.RvtFilter.RvtFilterUniqueId))
                .ToList();

            JoinService.This.SetCurrentProjectModel3dFilters(_FiltersData);


            var rulesCalc = new RulesCalculator();
            List<RuleError> ruleErrors = rulesCalc.CheckRules();

            FiltersPane.This.View?.SetRuleErrors(ruleErrors);
            FiltersPane.This.View?.Update();

        }

        void LoadElements()
        {
            FilterElement? filterElement = null;

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            //long longFilterId = 0;
            //if (long.TryParse(_RvtFilterId, out longFilterId))
            //{
            //    ElementId filterId = new ElementId(longFilterId);

            //    filterElement = doc.GetElement(filterId) as FilterElement;
            //}

            filterElement = doc.GetElement(_RvtFilterUniqueId) as FilterElement;

            if (filterElement == null)
                return;

            FilterName = filterElement.Name;

            List<ElementId> elementsId = Utils.GetFilterElementsId(doc, filterElement);


            List<Element> elements = new List<Element>();
            foreach (ElementId elId in elementsId)
            {
                Element el = doc.GetElement(elId);
                elements.Add(el);
            }


            _Elements.Clear();
            var catGroups = elements.GroupBy(item => item.Category.Name);
            foreach (var catGroup in catGroups)
            {
                foreach (var el in catGroup)
                {
                    _Elements.Add(el);
                    _ElementIndexById.TryAdd(el.Id, _Elements.Count - 1);
                }
            }

            _ElementsId = _Elements.Select(item => item.Id).ToList();


            ///Load elements type
            _ElementsByType.Clear();
            _Elements.ForEach(item =>
            {
                ElementId elTypeId = item.GetTypeId();
                if (!_ElementsByType.ContainsKey(elTypeId))
                {
                    Element elType = doc.GetElement(elTypeId);
                    if (elType != null)
                    {
                        _ElementsType.Add(elType);
                        _ElementsByType.Add(elTypeId, new List<ElementId>());
                    }
                }

                if (_ElementsByType.ContainsKey(elTypeId))
                    _ElementsByType[elTypeId].Add(item.Id);

            });



            ElementsCount = _Elements.Count;
            ElementsInfo = GetElemsInfo();




        }


        #region CurrentElement


        private string GetElemsInfo()
        {
            //group by category
            string elemsInfo = LocalizationProvider.GetString("Inserire un numero di elemento");
            string da = LocalizationProvider.GetString("da");
            string a = LocalizationProvider.GetString("a");


            if (_Elements.Count <= 0)
                return elemsInfo;

            string baseCategory = _Elements.First().Category.Name;
            int baseIndex = 0;

            for (int i = 0; i < _Elements.Count; i++)
            {
                Element el = _Elements[i];
                var cat = el.Category.Name;

                if (cat != baseCategory)
                {
                    elemsInfo += string.Format("\n{0} {1} {2} {3} {4}", da, baseIndex + 1, a, i, baseCategory);
                    baseIndex = i;
                    baseCategory = cat;
                }
            }

            elemsInfo += string.Format("\n{0} {1} {2} {3} {4}", da, baseIndex + 1, a, _Elements.Count, baseCategory);

            return elemsInfo;
        }

        public string FilterName
        {
            get => _FilterName;
            set => SetProperty(ref _FilterName, value);
        }

        public ICommand CurrentElementFirstCommand { get { return new CommandHandler(() => this.CurrentElementFirst()); } }
        void CurrentElementFirst()
        {
            CurrentElementNumber = 1;
        }

        public ICommand CurrentElementPrevCommand { get { return new CommandHandler(() => this.CurrentElementPrev()); } }
        void CurrentElementPrev()
        {
            if (_Elements.Count > 0)
            {
                if (CurrentElementNumber > 1)
                    CurrentElementNumber -= 1;
                else
                    CurrentElementLast();
            }
            else
            {
                CurrentElementNumber = 0;
            }
        }

        public ICommand CurrentElementNextCommand { get { return new CommandHandler(() => this.CurrentElementNext()); } }
        void CurrentElementNext()
        {
            if (_Elements.Count > 0)
            {
                if (CurrentElementNumber < ElementsCount)
                    CurrentElementNumber += 1;
                else
                    CurrentElementFirst();
            }
            else
            {
                CurrentElementNumber = 0;
            }
        }

        public ICommand CurrentElementLastCommand { get { return new CommandHandler(() => this.CurrentElementLast()); } }
        void CurrentElementLast()
        {
            CurrentElementNumber = ElementsCount;
        }

        int _CurrentElementNumber = 0;
        public int CurrentElementNumber
        {
            get => _CurrentElementNumber;
            set
            {
                if (0 <= value && value <= _Elements.Count) //lascio settare anche lo 0 ?
                {
                    if (SetProperty(ref _CurrentElementNumber, value))
                    {
                        if (0 <= CurrentElementIndex && CurrentElementIndex < _Elements.Count)
                        {
                            Element el = _Elements[CurrentElementIndex];
                            CurrentElementCategoria = el.Category.Name;

                            SelectElements(new List<ElementId> { el.Id });

                            var ruleErrorsCalc = CalculateFilterRules();
                            LoadRulesGrid(ruleErrorsCalc);


                            LoadParametersList();
                            LoadMaterialsQtaList();
                        }
                        else
                        {
                            CurrentElementCategoria = string.Empty;
                        }

                    }
                }
            }
        }

        public int CurrentElementIndex
        {
            get => _CurrentElementNumber - 1;
        }



        int _ElementsCount = 0;
        public int ElementsCount
        {
            get => _ElementsCount;
            set => SetProperty(ref _ElementsCount, value);
        }

        string _CurrentElementCategoria = string.Empty;
        public string CurrentElementCategoria
        {
            get => _CurrentElementCategoria;
            set => SetProperty(ref _CurrentElementCategoria, value);
        }

        string _ElementsInfo = string.Empty;
        public string ElementsInfo
        {
            get => _ElementsInfo;
            set => SetProperty(ref _ElementsInfo, value);
        }

        void SelectElements(ICollection<ElementId> elemsId)
        {
            var uiDoc = CmdInit.This.UIApplication.ActiveUIDocument;

            uiDoc.Selection.SetElementIds(elemsId);

        }

        #endregion


        #region Rules

        void LoadRules()
        {
            LoadRules(Guid.Empty);
        }

        void LoadRules(Guid currentRuleId)
        {
            //_FiltersData = CmdInit.This.JoinService.GetCurrentProjectModel3dFilters();
            if (_FiltersData == null)
                return;

            _AttributoQuantita = _FiltersData.AttributiRegola.FirstOrDefault(item => item.IsQuantita);
            if (_AttributoQuantita == null)
                return;


            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
            string? projectIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);

            if (projectIfcGuid == null)
                return;


            _FiltersDataItem = _FiltersData.Items.FirstOrDefault(item => item.RvtFilter?.RvtFilterUniqueId == _RvtFilterUniqueId);
            if (_FiltersDataItem == null)
            {
                _FiltersDataItem = new FiltersDataItem();
                _FiltersDataItem.ID = Guid.NewGuid();


                //var rvtFilter = FilterElementConverter.Convert(projectIfcGuid, _RvtFilterUniqueId);
                //if (rvtFilter == null)
                //    return;

                //_FiltersDataItem.RvtFilter = rvtFilter;

                //_FiltersDataItem.RvtFilter = new RvtFilterForIO()
                //{
                //    RvtFilterUniqueId = _RvtFilterUniqueId,
                //    ProjectIfcGuid = projectIfcGuid,
                //};
                _FiltersDataItem.Descri = _FilterName;
                _FiltersDataItem.RvtFilter = FilterElementConverter.Convert(projectIfcGuid, _RvtFilterUniqueId);

                _FiltersData.Items.Add(_FiltersDataItem);

            }
            else
            {
                _FiltersDataItem.Descri = _FilterName;
                _FiltersDataItem.RvtFilter = FilterElementConverter.Convert(projectIfcGuid, _RvtFilterUniqueId);
            }


            var rulesCalculator = new RulesCalculator();

            rulesCalculator.LoadFilter(projectIfcGuid, _RvtFilterUniqueId, _FilterName, _FiltersData, _Elements);
            _RuleErrorsPrezzo = rulesCalculator.UpdateFilterPrezzarioItems();
            if (_RuleErrorsPrezzo == null)
                return;

            var ruleErrorsCalc = rulesCalculator.CalculateFilterRules();


            LoadRulesGrid(currentRuleId, ruleErrorsCalc);
            LoadRulesQta();
            LoadRulesAttributiGrid();


        }

        void LoadRulesGrid(List<RuleError> ruleErrorsCalc = null)
        {
            LoadRulesGrid(Guid.Empty, ruleErrorsCalc);
        }

        void LoadRulesGrid(Guid currentRuleId, List<RuleError> ruleErrorsCalc = null)
        {
            if (_FiltersData == null)
                return;

            if (_FiltersDataItem == null)
                return;

            //Guid currentRuleId = Guid.Empty;
            if (CurrentRule != null)
                currentRuleId = CurrentRule.RuleId;


            List<RuleError> ruleErrors = JoinRuleErrors(ruleErrorsCalc);

            List<RulesItemView> rules = new List<RulesItemView>();

            String qtaFormat = "#,##0.00";


            foreach (var rule in _FiltersDataItem.RulesIO)
            {
                var ruleView = new RulesItemView(this, rule);

                double prezzo = RulesCalculator.PrezzoAsDouble(rule.Prezzo);


                ruleView.Id = rule.RuleId;
                ruleView.Prezzario = rule.Prezzario;
                ruleView.Codice = rule.Codice;
                ruleView.Descrizione = rule.Descrizione;
                ruleView.UM = rule.UM;
                ruleView.Prezzo = string.Format(rule.PrezzoFormat, prezzo);
                ruleView.QuantitaElemento = rule.SingleQta.ToString(qtaFormat);
                ruleView.ImportoElemento = string.Format(rule.PrezzoFormat, rule.SingleImporto);
                ruleView.QuantitaTotale = rule.QuanTot.ToString(qtaFormat);
                ruleView.ImportoTotale = rule.ImportoTotale;



                var ruleErrorsThisRule = ruleErrors.Where(item => item.RuleId == rule.RuleId).ToList();
                if (ruleErrorsThisRule?.Count > 0)
                {
                    string headerErr = string.Empty;
                    int nErrShowed = 50;
                    if (ruleErrorsThisRule?.Count  < nErrShowed)
                        headerErr = string.Format("{0} {1}", LocalizationProvider.GetString("Errori"), ruleErrorsThisRule?.Count);
                    else
                        headerErr = string.Format("{0} >{1}", LocalizationProvider.GetString("Errori"), nErrShowed);

                    ruleView.RuleInfo = headerErr;
                    ruleView.RuleInfo += "\n\n";

                    var errorLines = ruleErrorsThisRule.Take(nErrShowed).Select(item => item.AsString());//visualizza i primi n errori

                    ruleView.RuleInfo += string.Join("\n", errorLines);
                }

                rules.Add(ruleView);


            }

            RulesItem = new ObservableCollection<RulesItemView>(rules);

            if (currentRuleId == Guid.Empty && RulesItem.Count > 0)
                CurrentRulesItem = RulesItem.First();
            else
                CurrentRulesItem = RulesItem.FirstOrDefault(item => item.Rule.RuleId == currentRuleId);


        }

        private List<RuleError> JoinRuleErrors(List<RuleError> ruleErrorsCalc)
        {
            var rulesErrors = new List<RuleError>();
            if (_RuleErrorsConflict != null)
                rulesErrors.AddRange(_RuleErrorsConflict);

            if (_RuleErrorsPrezzo != null)
                rulesErrors.AddRange(_RuleErrorsPrezzo);

            if (ruleErrorsCalc != null)
                rulesErrors.AddRange(ruleErrorsCalc);

            return rulesErrors;
        }

        public void SetRuleErrorsConflict(List<RuleError> ruleErrorsConflict)
        {
            _RuleErrorsConflict = ruleErrorsConflict;
        }

        public void LoadRulesAttributiGrid()
        {
            if (_FiltersData == null)
                return;

            List<FormuleAttributiItemView> formuleAtt = new List<FormuleAttributiItemView>();

            foreach (AttributoRegola attReg in _FiltersData.AttributiRegola)
            {
                if (attReg.IsValoreLockedByDefault)
                    continue;

                if (attReg.IsQuantita)
                    continue;

                var formulaAtt = new FormuleAttributiItemView(this);

                formulaAtt.AttributoCodice = attReg.Codice;
                formulaAtt.AttributoEtichetta = attReg.Etichetta;

                if (CurrentRule == null)
                {
                    formulaAtt.AttributoValore = string.Empty;
                }
                else
                {
                    if (CurrentRule.FormuleByAttributoComputo.ContainsKey(attReg.Codice))
                        formulaAtt.AttributoValore = CurrentRule.FormuleByAttributoComputo[attReg.Codice];
                }

                formuleAtt.Add(formulaAtt);
            }

            FormuleAttributi = new ObservableCollection<FormuleAttributiItemView>(formuleAtt);
        }

        void LoadRulesQta()
        {
            if (!IsQtaEnabled)
            {
                FormulaQuantita = _AttributoQuantita.Formula;
            }
            else
            {

                if (CurrentRule == null)
                    FormulaQuantita = string.Empty;
                else
                    FormulaQuantita = CurrentRule.FormulaQta;
            }

        }

        public bool IsQtaEnabled
        {
            get
            {

                if (CurrentRule != null && !_AttributoQuantita.IsValoreLockedByDefault)
                    return true;

                return false;

            }
        }

        public bool IsAttributiGridEnabled
        {
            get
            {
                if (CurrentRule != null)
                    return true;

                return false;
            }
        }


        RulesItemView _CurrentRulesItem = null;
        public RulesItemView CurrentRulesItem
        {
            get => _CurrentRulesItem;
            set
            {
                if (SetProperty(ref _CurrentRulesItem, value))
                {
                    LoadRulesQta();
                    LoadRulesAttributiGrid();
                    UpdateUI();
                }
            }
        }

        public RegoleComputoForIO CurrentRule
        {
            get
            {
                if (_CurrentRulesItem != null)
                    return _CurrentRulesItem.Rule;

                return null;
            }
        }

        //public bool IsAttributoQuantita(string codiceAttributo)
        //{
        //    if (_FiltersData == null)
        //        return false;

        //    if (_AttributoQuantita != null && _AttributoQuantita.Codice == codiceAttributo)
        //        return true;

        //    return false;

        //}

        public ICommand CalculateRulesCommand { get { return new CommandHandler(() => this.CalculateRules()); } }
        void CalculateRules()
        {
            var ruleErrorsCalc = CalculateFilterRules();

            LoadRulesGrid(ruleErrorsCalc);
        }


        List<RuleError> CalculateFilterRules()
        {

            var rulesCalculator = new RulesCalculator();

            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
            string? projectIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);

            if (projectIfcGuid == null)
                return null;


            ElementId? currentElId= null;

            if (0 <= CurrentElementIndex && CurrentElementIndex < _ElementsId.Count)
                currentElId = _ElementsId[CurrentElementIndex];


            rulesCalculator.LoadFilter(projectIfcGuid, _RvtFilterUniqueId, _FilterName, _FiltersData, _Elements, currentElId);
            var ruleErrors = rulesCalculator.CalculateFilterRules();

            return ruleErrors;
        }



        public ICommand ArticoloAddCommand { get { return new CommandHandler(() => this.ArticoloAdd()); } }
        void ArticoloAdd()
        {

            Guid articoloId = Guid.Empty;
            List<Model3dPrezzarioItem> articoli = CmdInit.This.JoinService.SelectPrezzarioItems(LocalizationProvider.GetString("Inserisci articoli elenco prezzi"), articoloId, true);

            if (articoli == null)
                return;


            int? currentRuleIndex = _FiltersDataItem?.RulesIO.IndexOf(CurrentRule);

            var art = articoli.FirstOrDefault();

            //foreach (var art in articoli)
            if (art != null)
            { 
                var newRule = new RegoleComputoForIO();
                newRule.RuleId = Guid.NewGuid();
                newRule.Id = art.Id;
                newRule.Prezzario = art.Prezzario;
                newRule.Codice = art.Codice;
                newRule.Descrizione = art.Descrizione;
                newRule.UM = art.UM;
                newRule.Prezzo = art.Prezzo;
                newRule.PrezzoFormat = art.PrezzoFormat;


                //Per ora aggiungo in fondo

                //if (currentRuleIndex.HasValue && currentRuleIndex >= 0 && currentRuleIndex < _FiltersDataItem?.RulesIO.Count)
                //{
                //    _FiltersDataItem?.RulesIO.Insert(currentRuleIndex.Value, newRule);
                //}
                //else
                {
                    _FiltersDataItem?.RulesIO.Add(newRule);
                    currentRuleIndex = _FiltersDataItem?.RulesIO.Count - 1;
                }

                LoadRulesGrid();
                CurrentRulesItem = RulesItem.LastOrDefault();
            }
        }

        public ICommand ArticoloRemoveCommand { get => new CommandHandler(() => ArticoloRemove()); } 
        void ArticoloRemove()
        {
            if (CurrentRule == null)
                return;

            if (_FiltersDataItem == null)
                return;

            int? currentRuleIndex = _FiltersDataItem?.RulesIO.IndexOf(CurrentRule);

            if (currentRuleIndex.HasValue && currentRuleIndex >= 0 && currentRuleIndex < _FiltersDataItem?.RulesIO.Count)
                _FiltersDataItem?.RulesIO.RemoveAt(currentRuleIndex.Value);

            CurrentRulesItem = null;

            LoadRulesGrid();
        }

        public ICommand ArticoloMoveUpCommand { get => new CommandHandler(() => ArticoloMoveUp()); } 
        void ArticoloMoveUp()
        {
            if (CurrentRule == null)
                return;

            if (_FiltersDataItem == null)
                return;

            int? currentRuleIndex = _FiltersDataItem?.RulesIO.IndexOf(CurrentRule);

            if (currentRuleIndex.HasValue && currentRuleIndex > 0 && currentRuleIndex < _FiltersDataItem?.RulesIO.Count)
            {
                _FiltersDataItem?.RulesIO.MoveTo(currentRuleIndex.Value, currentRuleIndex.Value-1);
            }

            LoadRulesGrid();

        }

        public ICommand ArticoloMoveDownCommand { get => new CommandHandler(() => ArticoloMoveDown()); }
        void ArticoloMoveDown()
        {
            if (CurrentRule == null)
                return;

            if (_FiltersDataItem == null)
                return;

            int? currentRuleIndex = _FiltersDataItem?.RulesIO.IndexOf(CurrentRule);

            if (currentRuleIndex.HasValue && currentRuleIndex >= 0 && currentRuleIndex < _FiltersDataItem?.RulesIO.Count - 1)
            {
                _FiltersDataItem?.RulesIO.MoveTo(currentRuleIndex.Value, currentRuleIndex.Value + 1);
            }

            LoadRulesGrid();
        }

        public ICommand CurrentRuleReplaceCommand { get => new CommandHandler(() => CurrentRuleReplace()); }
        void CurrentRuleReplace()
        {
            if (CurrentRule == null)
                return;

            if (_FiltersDataItem == null)
                return;

            Guid articoloId = CurrentRule.Id;
            List<Model3dPrezzarioItem> articoli = CmdInit.This.JoinService.SelectPrezzarioItems(LocalizationProvider.GetString("Inserisci articoli elenco prezzi"), articoloId, true);

            if (articoli == null)
                return;

            int? currentRuleIndex = _FiltersDataItem?.RulesIO.IndexOf(CurrentRule);

            var art = articoli.FirstOrDefault();

            if (art != null && currentRuleIndex >= 0)
            {
                var newRule = new RegoleComputoForIO();
                newRule.RuleId = Guid.NewGuid();
                newRule.Id = art.Id;
                newRule.Prezzario = art.Prezzario;
                newRule.Codice = art.Codice;
                newRule.Descrizione = art.Descrizione;
                newRule.UM = art.UM;
                newRule.Prezzo = art.Prezzo;
                newRule.PrezzoFormat = art.PrezzoFormat;

                //dati copiati dalla vecchia regola
                newRule.FormulaQta = CurrentRule.FormulaQta;
                newRule.FormuleByAttributoComputo = new Dictionary<string, string>(CurrentRule.FormuleByAttributoComputo);

                _FiltersDataItem.RulesIO[currentRuleIndex.Value] = newRule;


                LoadRulesGrid();
                CurrentRulesItem = RulesItem[currentRuleIndex.Value];
            }


        }


        #endregion

        #region Parameters

        void LoadParameters()
        {

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            foreach (Element el in _Elements)
            {
                var parameters = el.GetOrderedParameters();

                foreach (Parameter par in parameters)
                {
                    if (!IsParameterTypeAdmitted(par))
                        continue;

                    //aggiungo i parametri dell'elemento
                    AddParamInfo(el, par, false, _ParamsElInfo);
                }

                //Aggiungo i parametri del tipo    
                ElementId elTypeId = el.GetTypeId();
                Element elType = doc.GetElement(elTypeId);

                if (elType != null)
                {
                    var parametersType = elType.GetOrderedParameters();
                    foreach (Parameter par in parametersType)
                    {
                        if (!IsParameterTypeAdmitted(par))
                            continue;

                        AddParamInfo(elType, par, true, _ParamsElTypeInfo);
                    }
                }
            }
        }

        private void AddParamInfo(Element el, Parameter par, bool isElementType, Dictionary<string, ParamInfo> paramsInfo)
        {
            if (el == null)
                return;

            if (par == null)
                return;

            Definition parDef = par.Definition;
            bool isValueValid = IsParameterValueValid(par);

            paramsInfo.TryAdd(parDef.Name, new ParamInfo());
            paramsInfo[parDef.Name].Name = parDef.Name;
            paramsInfo[parDef.Name].ElementsId.Add(el.Id);
            if (!isValueValid)
                paramsInfo[parDef.Name].InvalidValueElementsId.Add(el.Id);
            paramsInfo[parDef.Name].InElementType = isElementType;


            //if (paramsInfo[parDef.Name].IsAllValueValid == true)
            //    paramsInfo[parDef.Name].IsAllValueValid = isValueValid;
        }

        private bool IsParameterValueValid(Parameter par)
        {
            bool isValueValid = false;
            if (par.HasValue)
            {
                if (IsParameterNumeric(par))
                {
                    if (par.StorageType == StorageType.Double)
                    {
                        double d = par.AsDouble();
                        if (d != 0)
                            isValueValid = true;
                    }
                    else if (par.StorageType == StorageType.ElementId)
                    {
                        ElementId elId = par.AsElementId();
                        if (elId != null && elId.Value != 0)
                            isValueValid = true;
                    }
                    else
                    {
                        string val = par.AsValueString();
                        if (!string.IsNullOrEmpty(val))
                            isValueValid = true;
                    }


                }
                else
                    isValueValid = true;
            }

            return isValueValid;
        }

        void LoadParametersList()
        {

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            Element? currentElement = null;

            if (0 <= CurrentElementIndex && CurrentElementIndex < _Elements.Count)
            {
                currentElement = _Elements[CurrentElementIndex];
            }


            List<ParameterItemView> parsView = GetElementParameters(currentElement, _ParamsElInfo);

            if (currentElement != null)
            {
                ElementId elTypeId = currentElement.GetTypeId();

                Element currentElementType = doc.GetElement(elTypeId);

                List<ParameterItemView> parsElemsTypeView = GetElementParameters(currentElementType, _ParamsElTypeInfo);
                
                parsView.AddRange(parsElemsTypeView);
            }

            

            ParameterItems = new ObservableCollection<ParameterItemView>(parsView.OrderBy(item => item.ParameterName));

            UpdateUI();

        }

        private List<ParameterItemView> GetElementParameters(Element? element, Dictionary<string, ParamInfo> paramsInfo)  
        {

            List<ParameterItemView> parsView = new List<ParameterItemView>();
            Dictionary<string, ParameterItemView> parsViewByName = new Dictionary<string, ParameterItemView>();   
            
            
            if (element == null)
                return parsView;


            

            var parameters = element.GetOrderedParameters();

            foreach (Parameter par in parameters)
            {
                if (!IsParameterTypeAdmitted(par, true))
                    continue;

                if (!IsParamsFilterByTextAdmitted(par))
                    continue;

                Definition parDef = par.Definition;

                ParamInfo paramInfo = null;
                if (paramsInfo.TryGetValue(parDef.Name, out paramInfo))
                {
                    if (ParamsInAllElements)
                    {
                        if (IsParameterCommon(paramInfo) && IsParameterValueValid(paramInfo))
                        {
                            //parametro comune
                            if (parsViewByName.ContainsKey(parDef.Name))
                            {
                            }
                            else
                            {
                                var parView = new ParameterItemView(this, paramInfo);
                                parView.ParameterName = parDef.Name;
                                parView.InElementType = paramInfo.InElementType? typeFont : "";
                                parView.ParameterValue = GetFormattedValue(par);
                                parsView.Add(parView);
                                parsViewByName.Add(parDef.Name, parView);
                            }
                        }
                    }

                    if (ParamsAnyValueInvalid)
                    {
                        if (IsParameterCommon(paramInfo) && !IsParameterValueValid(paramInfo))
                        {
                            //parametro comune con valore non valido (0 o null)

                            if (parsViewByName.ContainsKey(parDef.Name))
                            {
                            }
                            else
                            {
                                var format = new FormatOptions();
                                

                                var parView = new ParameterItemView(this, paramInfo);
                                parView.ParameterName = parDef.Name;
                                parView.InElementType = paramInfo.InElementType ? typeFont : "";
                                parView.ParameterValue = GetFormattedValue(par);
                                parsView.Add(parView);
                                parsViewByName.Add(parDef.Name, parView);
                            }
                        }
                    }

                    if (ParamsInAnyElements)
                    {
                        if (!IsParameterCommon(paramInfo))
                        {
                            //parametro non comune


                            if (parsViewByName.ContainsKey(parDef.Name))
                            {
                            }
                            else
                            {
                                var parView = new ParameterItemView(this, paramInfo);
                                parView.ParameterName = parDef.Name;
                                parView.InElementType = paramInfo.InElementType ? typeFont : "";
                                parView.ParameterValue = GetFormattedValue(par);
                                parsView.Add(parView);
                                parsViewByName.Add(parDef.Name, parView);
                            }
                        }
                    }
                }
            }
            

            return parsView;
        }

        public static string GetFormattedValue(Parameter parameter)
        {
            string formattedValue = string.Empty;
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            // Verifica che il parametro non sia nullo
            if (parameter == null)
                return string.Empty;

            if (parameter.StorageType == StorageType.Double)
            {
                //double d = parameter.AsDouble();

                //// Ottieni l'unità di misura del parametro
                //ForgeTypeId unitTypeId = parameter.GetUnitTypeId();

                //if (UnitUtils.IsMeasurableSpec(unitTypeId))
                //{
                //    // Formatta il valore con 3 cifre decimali
                //    formattedValue = UnitFormatUtils.Format(doc.GetUnits(), unitTypeId, d, false, new FormatValueOptions() { AppendUnitSymbol = true });
                //}

                //double? d = Utils.GetParameterValueAsDouble(parameter);
                //formattedValue = d.Value.ToString(CultureInfo.InvariantCulture);


            }
            else if (parameter.StorageType == StorageType.Integer)
            {
                //int i = parameter.AsInteger();

                //// Ottieni l'unità di misura del parametro
                //ForgeTypeId unitTypeId = parameter.GetUnitTypeId();

                //if (UnitUtils.IsMeasurableSpec(unitTypeId))
                //{ 
                //    // Formatta il valore con 3 cifre decimali
                //    formattedValue = UnitFormatUtils.Format(doc.GetUnits(), unitTypeId, i, false, new FormatValueOptions() { AppendUnitSymbol = true });
                //}
            }



            if (string.IsNullOrEmpty(formattedValue))
            {
                // Ottieni il valore formattato come stringa
                formattedValue = parameter.AsValueString();
            }


            //// Se necessario, puoi ulteriormente formattare la stringa per assicurarti che abbia 3 cifre decimali
            //if (double.TryParse(parameter.AsDouble().ToString("F3"), out double value))
            //{
            //    formattedValue = value.ToString("F3") + " " + parameter.DisplayUnitType.ToString();
            //}

            return formattedValue;
        }

        public bool IsParameterCommon(ParamInfo paramInfo)
        {
            if (paramInfo.InElementType)
                return paramInfo.ElementsId.Count == _ElementsType.Count;
            else
                return paramInfo.ElementsId.Count == _Elements.Count;
        }

        public bool IsParameterValueValid(ParamInfo paramInfo)
        {
            //oss: se un parametro ha valore 0 o null per almeno un elemento ma non è presente in tutti gli elementi allora è considerato valido (non rosso)
            return paramInfo.InvalidValueElementsId.Count == 0;
        }

        bool IsParameterNumeric(Parameter par)
        {
            var parDef = par.Definition;

            if (parDef.GetDataType() == SpecTypeId.Int.Integer)
                return true;

            if (par.StorageType == StorageType.Double)
                return true;



            //PropInfos = null;

            //if (prm.StorageType == StorageType.Double && (strParamName == CSCache.PCostoElem))
            //    bHasCosto = true;

            //if (prm.StorageType == StorageType.String)
            //{
            //    //Verifica se l'attributo contiene dati di Mosaico:
            //    if (strParamNameNoGrp == CSCache.PNameElem || strParamNameNoGrp == CSCache.PNameFam)
            //    {
            //        //Trovato parametro dati di Mosaico
            //        strAux = prm.AsString();
            //        if (strAux != null && strAux.Length > 0 && strAux[0] == '[')
            //            strElementMosData += strAux;
            //    }
            //}
            //else
            //{
            //    //Add by Ale 09/11/2017
            //    double? dVal1 = null;
            //    if (prm.StorageType == StorageType.Double)
            //        dVal1 = prm.AsDouble();
            //    else if (prm.StorageType == StorageType.Integer)
            //        dVal1 = prm.AsInteger();

            //    if (dVal1.HasValue)
            //    {
            //        try
            //        {
            //            //by Ale 20/04/2021 x Revit2022
            //            if (prm.Ext_HasDisplayUnitType() && prm.StorageType == StorageType.Double)
            //            {
            //                try { prm.GetUnitTypeId(); }
            //                catch (Exception e) { }

            //                if (UnitUtils.IsUnit(prm.GetUnitTypeId()))
            //                {
            //                    double dValDisplay = UnitUtils.ConvertFromInternalUnits(dVal1.Value, prm.GetUnitTypeId());
            //                    PropInfos = new RevitPropInfo2(strParamName, dValDisplay);
            //                }
            //            }
            //            else if (prm.Definition.GetDataType() == SpecTypeId.Int.Integer)
            //            {
            //                PropInfos = new RevitPropInfo2(strParamName, dVal1.Value);
            //            }

            //        }
            //        catch (Exception e)
            //        {
            //        }
            //    }
            //}




            return false;
        }

        bool IsParameterText(Parameter par)
        {
            if (par.StorageType == StorageType.String)
                return true;

            if (par.StorageType == StorageType.ElementId)
                return true;

            return false;
        }

        bool IsParameterTypeAdmitted(Parameter par, bool byUser = false)
        {
            if (byUser)
            {
                if (IsParameterNumeric(par) && ParamsIsValoriNumericiChecked)
                    return true;

                if (IsParameterText(par) && ParamsIsTestiChecked)
                    return true;
            }
            else
            {
                if (IsParameterNumeric(par))
                    return true;

                if (IsParameterText(par))
                    return true;
            }
                 
            return false;
        }

        string filtroOnFont { get => "\ue149"; }//&#xe149;
        string filtroOffFont { get => "\ue14a"; }
        string typeFont { get => "\ue14c"; }

        bool _ParamsInAllElements = true;
        public bool ParamsInAllElements
        {
            get => _ParamsInAllElements;
            set
            {
                if (SetProperty(ref _ParamsInAllElements, value))
                    LoadParametersList();
            }
        }

        bool _ParamsInAnyElements = false;
        public bool ParamsInAnyElements
        {
            get => _ParamsInAnyElements;
            set
            {
                if (SetProperty(ref _ParamsInAnyElements, value))
                    LoadParametersList();
            }
        }

        bool _ParamsAnyValueInvalid = false;
        public bool ParamsAnyValueInvalid
        {
            get => _ParamsAnyValueInvalid;
            set
            {
                if (SetProperty(ref _ParamsAnyValueInvalid, value))
                    LoadParametersList();
            }
        }

        public string ParamsInAllElementsText
        {
            get
            {
                if (ParamsInAllElements)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        public string ParamsInAnyElementsText
        {
            get
            {
                if (ParamsInAnyElements)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        public string ParamsAnyValueInvalidText
        {
            get
            {
                if (ParamsAnyValueInvalid)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        bool _ParamsIsValoriNumericiChecked = true;
        public bool ParamsIsValoriNumericiChecked
        {
            get => _ParamsIsValoriNumericiChecked;
            set
            {
                if (SetProperty(ref _ParamsIsValoriNumericiChecked, value))
                    LoadParametersList();
            }
        }


        bool _ParamsIsTestiChecked = false;
        public bool ParamsIsTestiChecked
        {
            get => _ParamsIsTestiChecked;
            set
            {
                if (SetProperty(ref _ParamsIsTestiChecked, value))
                    LoadParametersList();
            }
        }

        string _ParamsFilterText = string.Empty;
        public string ParamsFilterText
        {
            get => _ParamsFilterText;
            set
            {
                if (SetProperty(ref _ParamsFilterText, value))
                    LoadParametersList();
            }
        }

        bool IsParamsFilterByTextAdmitted(Parameter par)
        {

            string[] strArray = _ParamsFilterText.Split(' ');
            foreach (string str in strArray)
            {
                var parDef = par.Definition;
                if (!parDef.Name.Contains(str.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }


            return true;
        }



        #endregion

        #region Materials

        void LoadMaterials()
        {

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            foreach (Element el in _Elements)
            {
                

                var materialsId = el.GetMaterialIds(false);
                foreach (ElementId matId in materialsId)
                {
                    Material? mat = doc.GetElement(matId) as Material;

                    //aggiungo i materiali dell'elemento
                    if (mat != null)
                    {
                        AddMaterialInfo(el, mat, MaterialQtaType.Area, _MatsElInfo);
                        AddMaterialInfo(el, mat, MaterialQtaType.Volume, _MatsElInfo);
                    }
                }

                //aggiungo i materiali paint
                var paintMaterialsId = el.GetMaterialIds(true);
                foreach (ElementId matId in paintMaterialsId)
                {
                    Material? mat = doc.GetElement(matId) as Material;

                    //aggiungo i materiali dell'elemento
                    if (mat != null)
                    {
                        AddMaterialInfo(el, mat, MaterialQtaType.PaintArea, _MatsElInfo);
                    }
                }
            }
        }

        static string GetMaterialQtaName(string materialName, MaterialQtaType matQtaType)
        {
            string qtaName = string.Empty;
            if (matQtaType == MaterialQtaType.Area)
                qtaName = "Area";//non tradurre perchè vengono salvati nella formula
            else if (matQtaType == MaterialQtaType.PaintArea)
                qtaName = "PaintArea";//non tradurre perchè vengono salvati nella formula
            else if (matQtaType == MaterialQtaType.Volume)
                qtaName = "Volume";//non tradurre perchè vengono salvati nella formula


            return string.Format("{0}.{1}", materialName, qtaName);
        }

        private void AddMaterialInfo(Element el, Material mat, MaterialQtaType matQtaType, Dictionary<string, MaterialQtaInfo> matsInfo)
        {
            if (el == null)
                return;

            if (mat == null)
                return;

            string matQtaName = GetMaterialQtaName(mat.Name, matQtaType);

            bool isValueValid = IsMaterialQtaValueValid(el, mat, matQtaType);

            matsInfo.TryAdd(matQtaName, new MaterialQtaInfo());
            matsInfo[matQtaName].Name = mat.Name;
            matsInfo[matQtaName].QtaType = matQtaType;
            matsInfo[matQtaName].ElementsId.Add(el.Id);
            if (!isValueValid)
                matsInfo[matQtaName].InvalidValueElementsId.Add(el.Id);
            //matsInfo[mat.Name].InElementType = isElementType;
        }



        private bool IsMaterialQtaValueValid(Element el, Material mat, MaterialQtaType matValueType)
        {
            bool isValueValid = false;

            double qta = Utils.GetMaterialQtaValue(el, mat.Id, matValueType, out _);

            if (qta > 0)
                isValueValid = true;

            return isValueValid;
        }

        void LoadMaterialsQtaList()
        {
            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            Element? currentElement = null;

            if (0 <= CurrentElementIndex && CurrentElementIndex < _Elements.Count)
            {
                currentElement = _Elements[CurrentElementIndex];
            }


            List<MaterialQtaItemView> matsView = GetElementMaterialsQta(currentElement, _MatsElInfo);


            MaterialItems = new ObservableCollection<MaterialQtaItemView>(matsView.OrderBy(item => item.MaterialQtaName));

            UpdateUI();

        }

        private List<MaterialQtaItemView> GetElementMaterialsQta(Element? element, Dictionary<string, MaterialQtaInfo> matsInfo)
        {

            List<MaterialQtaItemView> matsView = new List<MaterialQtaItemView>();

            if (element == null)
                return matsView;

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            Dictionary<string, MaterialQtaItemView> matsViewByName = new Dictionary<string, MaterialQtaItemView>();


            var paintMaterialsId = element.GetMaterialIds(true);
            var materialsId = element.GetMaterialIds(false);


            var allMaterialsId = materialsId.Union(paintMaterialsId);

            foreach (var materialId in allMaterialsId)
            {
                Material? mat = doc.GetElement(materialId) as Material;

                if (!IsMatsFilterByTextAdmitted(mat))
                    continue;

                List<string> matQtaNames = new List<string>() { GetMaterialQtaName(mat.Name, MaterialQtaType.Area), GetMaterialQtaName(mat.Name, MaterialQtaType.PaintArea), GetMaterialQtaName(mat.Name, MaterialQtaType.Volume) };

                foreach (var matQtaName in matQtaNames)
                { 

                    MaterialQtaInfo matInfo = null;
                    if (matsInfo.TryGetValue(matQtaName, out matInfo))
                    {

                        if (MatsInAllElements)
                        {
                            if (IsMaterialCommon(matInfo) && IsMaterialValueValid(matInfo))
                            {
                                //materialeQta comune
                                if (matsViewByName.ContainsKey(matQtaName))
                                {
                                }
                                else
                                {
                                    var matView = new MaterialQtaItemView(this, matInfo);
                                    matView.MaterialQtaName = matQtaName;

                                    double qta = Utils.GetMaterialQtaValue(element, mat.Id, matInfo.QtaType, out string formattedValue);
                                    matView.MaterialQtaValue = formattedValue;
                                    matsView.Add(matView);
                                    matsViewByName.Add(matQtaName, matView);
                                }
                            }
                        }

                        if (MatsAnyValueInvalid)
                        {
                            if (IsMaterialCommon(matInfo) && !IsMaterialValueValid(matInfo))
                            {
                                //materialeQta comune con valore non valido (0 o null)

                                if (matsViewByName.ContainsKey(matQtaName))
                                {
                                }
                                else
                                {
                                    var matView = new MaterialQtaItemView(this, matInfo);
                                    matView.MaterialQtaName = matQtaName;

                                    double qta = Utils.GetMaterialQtaValue(element, mat.Id, matInfo.QtaType, out string formattedValue);
                                    matView.MaterialQtaValue = formattedValue;
                                    matsView.Add(matView);
                                    matsViewByName.Add(matQtaName, matView);
                                }
                            }
                        }

                        if (MatsInAnyElements)
                        {
                            if (!IsMaterialCommon(matInfo))
                            {
                                //materialeQta non comune


                                if (matsViewByName.ContainsKey(matQtaName))
                                {
                                }
                                else
                                {
                                    var matView = new MaterialQtaItemView(this, matInfo);
                                    matView.MaterialQtaName= matQtaName;

                                    double qta = Utils.GetMaterialQtaValue(element, mat.Id, matInfo.QtaType, out string formattedValue);
                                    matView.MaterialQtaValue = formattedValue;
                                    matsView.Add(matView);
                                    matsViewByName.Add(matQtaName, matView);
                                }
                            }
                        }

                    }
                }
            }


            return matsView;
        }

        //public static string GetFormattedValue(double d, MaterialQtaType qtaType)
        //{
        //    string formattedValue = string.Empty;

        //    Units units = CmdInit.This.UIApplication.ActiveUIDocument.Document.GetUnits();


        //    if (qtaType == MaterialQtaType.Area || qtaType == MaterialQtaType.PaintArea)
        //    {
        //        formattedValue = UnitFormatUtils.Format(units, SpecTypeId.Area, d, false, new FormatValueOptions() { AppendUnitSymbol = true});
        //    }
        //    else if (qtaType == MaterialQtaType.Volume)
        //    {
        //        formattedValue = UnitFormatUtils.Format(units, SpecTypeId.Volume, d, false, new FormatValueOptions() { AppendUnitSymbol = true });
        //    }

        //    if (string.IsNullOrEmpty(formattedValue))
        //        formattedValue = d.ToString(CultureInfo.InvariantCulture);


        //    return formattedValue;
        
        //}

        public bool IsMaterialCommon(MaterialQtaInfo matInfo)
        {
            //if (matInfo.InElementType)
            //    return matInfo.ElementsId.Count == _ElementsType.Count;
            //else
                return matInfo.ElementsId.Count == _Elements.Count;
        }

        public bool IsMaterialValueValid(MaterialQtaInfo matInfo)
        {
            return matInfo.InvalidValueElementsId.Count == 0;
        }



        bool _MatsInAllElements = true;
        public bool MatsInAllElements
        {
            get => _MatsInAllElements;
            set
            {
                if (SetProperty(ref _MatsInAllElements, value))
                    LoadMaterialsQtaList();
            }
        }

        bool _MatsInAnyElements = false;
        public bool MatsInAnyElements
        {
            get => _MatsInAnyElements;
            set
            {
                if (SetProperty(ref _MatsInAnyElements, value))
                    LoadMaterialsQtaList();
            }
        }

        bool _MatsAnyValueInvalid = false;
        public bool MatsAnyValueInvalid
        {
            get => _MatsAnyValueInvalid;
            set
            {
                if (SetProperty(ref _MatsAnyValueInvalid, value))
                    LoadMaterialsQtaList();
            }
        }

        public string MatsInAllElementsText
        {
            get
            {
                if (MatsInAllElements)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        public string MatsInAnyElementsText
        {
            get
            {
                if (MatsInAnyElements)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        public string MatsAnyValueInvalidText
        {
            get
            {
                if (MatsAnyValueInvalid)
                    return filtroOnFont; //&#xe149;
                else
                    return filtroOffFont;

            }
        }

        //bool _MatsIsValoriNumericiChecked = true;
        //public bool MatsIsValoriNumericiChecked
        //{
        //    get => _MatsIsValoriNumericiChecked;
        //    set
        //    {
        //        if (SetProperty(ref _MatsIsValoriNumericiChecked, value))
        //            LoadMaterialsList();
        //    }
        //}


        //bool _MatsIsTestiChecked = false;
        //public bool MatsIsTestiChecked
        //{
        //    get => _MatsIsTestiChecked;
        //    set
        //    {
        //        if (SetProperty(ref _MatsIsTestiChecked, value))
        //            LoadMaterialsList();
        //    }
        //}

        string _MatsFilterText = string.Empty;
        public string MatsFilterText
        {
            get => _MatsFilterText;
            set
            {
                if (SetProperty(ref _MatsFilterText, value))
                    LoadMaterialsQtaList();
            }
        }

        bool IsMatsFilterByTextAdmitted(Material mat)
        {

            string[] strArray = _MatsFilterText.Split(' ');
            foreach (string str in strArray)
            {
                if (!mat.Name.Contains(str.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }


            return true;
        }



#endregion

        void UpdateUI()
        {

            RaisePropertyChanged(GetPropertyName(() => ParamsInAllElementsText));
            RaisePropertyChanged(GetPropertyName(() => ParamsInAnyElementsText));
            RaisePropertyChanged(GetPropertyName(() => ParamsAnyValueInvalidText));
            RaisePropertyChanged(GetPropertyName(() => MatsInAllElementsText));
            RaisePropertyChanged(GetPropertyName(() => MatsInAnyElementsText));
            RaisePropertyChanged(GetPropertyName(() => MatsAnyValueInvalidText));

            RaisePropertyChanged(GetPropertyName(() => IsQtaEnabled));
            RaisePropertyChanged(GetPropertyName(() => IsAttributiGridEnabled));


        }
        
    }

    public class ParameterItemView : NotificationBase
    {
        RulesView _owner = null;
        ParamInfo _paramInfo = null;

        public ParameterItemView(RulesView owner, ParamInfo paramInfo)
        {
            _owner = owner;
            _paramInfo = paramInfo;
        }



        string _InElementType = string.Empty;
        public string InElementType
        {
            get => _InElementType;
            set => SetProperty(ref _InElementType, value);
        }

        string _ParameterName = string.Empty;
        public string ParameterName
        {
            get => _ParameterName;
            set => SetProperty(ref _ParameterName, value);
        }

        string _ParameterValue = string.Empty;
        public string ParameterValue
        {
            get => _ParameterValue;
            set => SetProperty(ref _ParameterValue, value);
        }

        public string Formula { get => string.Format("rvt{{P>{0}}}", ParameterName); }


        public SolidColorBrush ParameterForeground
        {
            get
            {
                if (_owner.IsParameterCommon(_paramInfo))
                {
                    if (!_owner.IsParameterValueValid(_paramInfo))
                        return new SolidColorBrush(Colors.Red);
                    else
                        return new SolidColorBrush(Colors.Black);
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            
            }
            
        }

        public string ParameterInfo
        {
            get
            {
                string str = string.Empty;
                if (_owner.IsParameterCommon(_paramInfo))
                {
                    if (!_owner.IsParameterValueValid(_paramInfo))
                    {

                        List<ElementId> invalidValueElementsId = new List<ElementId>();

                        if (_paramInfo.InElementType)
                        {
                            foreach (var elTypeId in _paramInfo.InvalidValueElementsId)
                            {
                                List<ElementId>? elems = null;
                                if (_owner.ElementsByType.TryGetValue(elTypeId, out elems))
                                    invalidValueElementsId.AddRange(elems);
                            }
                        }
                        else
                        {
                            invalidValueElementsId = _paramInfo.InvalidValueElementsId.ToList();
                        }


                        str = string.Format("{0} {1}/{2}", LocalizationProvider.GetString("ParametriValore0Null"), invalidValueElementsId.Count, _owner.Elements.Count);

                        foreach (var elId in invalidValueElementsId)
                        {
                            int index = 0;
                            if (_owner.ElementIndexById.TryGetValue(elId, out index))
                            {

                                string row = string.Format("#{0} [{1}]", elId.Value, index + 1);
                                if (string.IsNullOrEmpty(str))
                                    str = row;
                                else
                                    str += string.Format("\n{0}", row);
                            }
                        }

                    }
                }
                else
                {
                    var notCommons = _owner.ElementsId.Where(item => !_paramInfo.ElementsId.Contains(item));

                    List<ElementId> notCommonsElementsId = new List<ElementId>();

                    if (_paramInfo.InElementType)
                    {
                        foreach (var elTypeId in _paramInfo.InvalidValueElementsId)
                        {
                            List<ElementId>? elems = null;
                            if (_owner.ElementsByType.TryGetValue(elTypeId, out elems))
                                notCommonsElementsId.AddRange(elems);
                        }
                    }
                    else
                    {
                        notCommonsElementsId = notCommons.ToList();
                    }


                    str = string.Format("{0} {1}/{2}", LocalizationProvider.GetString("ParametriValore0Null"), notCommonsElementsId.Count, _owner.Elements.Count);



                    foreach (var elId in notCommonsElementsId)
                    {
                        int index = 0;
                        if (_owner.ElementIndexById.TryGetValue(elId, out index))
                        {

                            string row = string.Format("#{0} [{1}]", elId.Value, index + 1);
                            if (string.IsNullOrEmpty(str))
                                str = row;
                            else
                                str += string.Format("\n{0}", row);
                        }
                    }

                }

                return str;
            }
        }

        public bool IsParameterInfoBtnVisible { get => !string.IsNullOrEmpty(ParameterInfo); }

        public ICommand ParameterInfoOpenCommand { get { return new CommandHandler(() => this.ParameterInfoOpen()); } }
        void ParameterInfoOpen()
        {
            IsParameterInfoOpen = false;
            IsParameterInfoOpen = true;
        }


        bool _IsParameterInfoOpen = false;
        public bool IsParameterInfoOpen
        {
            get => _IsParameterInfoOpen;
            set => SetProperty(ref _IsParameterInfoOpen, value);
        }


        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => ParameterForeground));
            RaisePropertyChanged(GetPropertyName(() => ParameterInfo));
            RaisePropertyChanged(GetPropertyName(() => IsParameterInfoBtnVisible));
            RaisePropertyChanged(GetPropertyName(() => Formula));
        }
    }

    public class FormuleAttributiItemView : NotificationBase
    {   
        private RulesView _owner;


        public FormuleAttributiItemView(RulesView rulesView)
        {
            _owner = rulesView;
        }

        public string AttributoCodice { get; set; } = string.Empty;
        
        string _AttributoEtichetta = string.Empty;
        public string AttributoEtichetta
        {
            get => _AttributoEtichetta;
            set => SetProperty(ref _AttributoEtichetta, value);
        }

        string _AttributoValore = string.Empty;


        public string AttributoValore
        {
            get => _AttributoValore;
            set 
            {
                if (SetProperty(ref _AttributoValore, value))
                {
                    if (_owner.CurrentRule != null)
                    {
                        if (!_owner.CurrentRule.FormuleByAttributoComputo.ContainsKey(AttributoCodice))
                            _owner.CurrentRule.FormuleByAttributoComputo.Add(AttributoCodice, string.Empty);

                        _owner.CurrentRule.FormuleByAttributoComputo[AttributoCodice] = AttributoValore;
                    }

                }
            }
        }
    }


    public class ParamInfo
    {
        public string Name { get; set; } = string.Empty;
        public HashSet<ElementId> ElementsId { get; set; } = new HashSet<ElementId>();
        public HashSet<ElementId> InvalidValueElementsId { get; set; } = new HashSet<ElementId>();
        public bool IsAllValueValid { get => InvalidValueElementsId.Count == 0; }
        public bool InElementType { get; set; } = false;
        
    
    }

    public class RulesItemView : NotificationBase
    {
        RulesView _owner = null;
        RegoleComputoForIO _rule = null;

        public RulesItemView(RulesView owner, RegoleComputoForIO rule)
        {
            _owner = owner;
            _rule = rule;
        }

        public RegoleComputoForIO Rule { get => _rule; }


        public Guid Id { get; set; }
        public string Prezzario { get; set; } = string.Empty;
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public string UM { get; set; } = string.Empty;
        public string Prezzo { get; set; } = string.Empty;
        public string QuantitaElemento { get; set; } = string.Empty;
        public string ImportoElemento { get; set; } = string.Empty;
        public string QuantitaTotale { get; set; } = string.Empty;
        public string ImportoTotale { get; set; } = string.Empty;

        string _RuleInfo = string.Empty;
        public string RuleInfo
        {
            get => _RuleInfo;
            set => SetProperty(ref _RuleInfo, value);

            //get
            //{
            //    string str = string.Empty;
            //    double prezzo = _owner.PrezzoAsDouble(_rule.Prezzo);

            //    if (prezzo == 0)
            //        str += string.Format("{0}", LocalizationProvider.GetString("Prezzo irregolare"));

                
              

            //    return str;
            //}
        }

        public bool IsRuleInfoBtnVisible { get => !string.IsNullOrEmpty(RuleInfo); }

        public ICommand RuleInfoOpenCommand { get { return new CommandHandler(() => this.RuleInfoOpen()); } }
        void RuleInfoOpen()
        {
            IsRuleInfoOpen = false;
            IsRuleInfoOpen = true;
        }


        bool _IsRuleInfoOpen = false;
        public bool IsRuleInfoOpen
        {
            get => _IsRuleInfoOpen;
            set => SetProperty(ref _IsRuleInfoOpen, value);
        }



    }

    public class AttFunctionsDependency
    {
        public string Etichetta { get; set; } = string.Empty;
        public string Codice { get; set; } = string.Empty;
        public string Function { get; set; } = string.Empty;
        public bool IsRiferimento { get; set; } = false;
        public string Formula { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public List<AttFunctionsDependency> DependBy { get; set; } = new List<AttFunctionsDependency>();

        public bool IsLoopLevel = false;


        public int GetLevel()
        {
            if (IsLoopLevel)
                return 0;

            IsLoopLevel = true;

            int level = 0;
            if (!DependBy.Any())
                level = 0;
            else
                level = DependBy.Max(item => item.GetLevel()) + 1;

            IsLoopLevel = false;
            return level;
        }


    }

    public class MaterialQtaInfo
    {
        public string Name { get; set; } = string.Empty;
        public MaterialQtaType QtaType { get; set; } = MaterialQtaType.Unknown;
        public HashSet<ElementId> ElementsId { get; set; } = new HashSet<ElementId>();
        public HashSet<ElementId> InvalidValueElementsId { get; set; } = new HashSet<ElementId>();
        public bool IsAllValueValid { get => InvalidValueElementsId.Count == 0; }
        //public bool InElementType { get; set; } = false;


    }

    public class MaterialQtaItemView : NotificationBase
    {
        RulesView _owner = null;
        MaterialQtaInfo _matInfo = null;

        public MaterialQtaItemView(RulesView owner, MaterialQtaInfo matInfo)
        {
            _owner = owner;
            _matInfo = matInfo;
        }

        string _MaterialQtaName = string.Empty;
        public string MaterialQtaName
        {
            get => _MaterialQtaName;
            set => SetProperty(ref _MaterialQtaName, value);
        }

        string _MaterialQtaValue = string.Empty;
        public string MaterialQtaValue
        {
            get => _MaterialQtaValue;
            set => SetProperty(ref _MaterialQtaValue, value);
        }

        public string Formula
        {
            get
            {
                string qta = string.Empty;
                if (_matInfo.QtaType == MaterialQtaType.Area)
                {
                    qta = Utils.AreaKey;
                }
                if (_matInfo.QtaType == MaterialQtaType.PaintArea)
                {
                    qta = Utils.PaintAreaKey;
                }
                if (_matInfo.QtaType == MaterialQtaType.Volume)
                {
                    qta = Utils.VolumeKey;
                }

                string f  =string.Format("rvt{{M>{0}{1}}}}}", qta, _matInfo.Name);
                return f;
            }
        }


        public SolidColorBrush MaterialForeground
        {
            get
            {
                if (_owner.IsMaterialCommon(_matInfo))
                {
                    if (!_owner.IsMaterialValueValid(_matInfo))
                        return new SolidColorBrush(Colors.Red);
                    else
                        return new SolidColorBrush(Colors.Black);
                }
                else
                {
                    return new SolidColorBrush(Colors.Gray);
                }

            }

        }

        public string MaterialInfo
        {
            get
            {
                string str = string.Empty;
                if (_owner.IsMaterialCommon(_matInfo))
                {
                    if (!_owner.IsMaterialValueValid(_matInfo))
                    {

                        List<ElementId> invalidValueElementsId = new List<ElementId>();

                        //if (_matInfo.InElementType)
                        //{
                        //    foreach (var elTypeId in _matInfo.InvalidValueElementsId)
                        //    {
                        //        List<ElementId>? elems = null;
                        //        if (_owner.ElementsByType.TryGetValue(elTypeId, out elems))
                        //            invalidValueElementsId.AddRange(elems);
                        //    }
                        //}
                        //else
                        //{
                            invalidValueElementsId = _matInfo.InvalidValueElementsId.ToList();
                        //}


                        str = string.Format("{0} {1}/{2}", LocalizationProvider.GetString("MaterialiValore0Null"), invalidValueElementsId.Count, _owner.Elements.Count);

                        foreach (var elId in invalidValueElementsId)
                        {
                            int index = 0;
                            if (_owner.ElementIndexById.TryGetValue(elId, out index))
                            {

                                string row = string.Format("#{0} [{1}]", elId.Value, index + 1);
                                if (string.IsNullOrEmpty(str))
                                    str = row;
                                else
                                    str += string.Format("\n{0}", row);
                            }
                        }

                    }
                }
                else
                {
                    var notCommons = _owner.ElementsId.Where(item => !_matInfo.ElementsId.Contains(item));

                    List<ElementId> notCommonsElementsId = new List<ElementId>();

                    //if (_matInfo.InElementType)
                    //{
                    //    foreach (var elTypeId in _matInfo.InvalidValueElementsId)
                    //    {
                    //        List<ElementId>? elems = null;
                    //        if (_owner.ElementsByType.TryGetValue(elTypeId, out elems))
                    //            notCommonsElementsId.AddRange(elems);
                    //    }
                    //}
                    //else
                    //{
                        notCommonsElementsId = notCommons.ToList();
                    //}


                    str = string.Format("{0} {1}/{2}", LocalizationProvider.GetString("MaterialiValore0Null"), notCommonsElementsId.Count, _owner.Elements.Count);



                    foreach (var elId in notCommonsElementsId)
                    {
                        int index = 0;
                        if (_owner.ElementIndexById.TryGetValue(elId, out index))
                        {

                            string row = string.Format("#{0} [{1}]", elId.Value, index + 1);
                            if (string.IsNullOrEmpty(str))
                                str = row;
                            else
                                str += string.Format("\n{0}", row);
                        }
                    }

                }

                return str;
            }
        }

        public bool IsMaterialInfoBtnVisible { get => !string.IsNullOrEmpty(MaterialInfo); }

        public ICommand MaterialInfoOpenCommand { get { return new CommandHandler(() => this.MaterialInfoOpen()); } }
        void MaterialInfoOpen()
        {
            IsMaterialInfoOpen = false;
            IsMaterialInfoOpen = true;
        }


        bool _IsMaterialInfoOpen = false;
        public bool IsMaterialInfoOpen
        {
            get => _IsMaterialInfoOpen;
            set => SetProperty(ref _IsMaterialInfoOpen, value);
        }


        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => MaterialForeground));
            RaisePropertyChanged(GetPropertyName(() => MaterialInfo));
            RaisePropertyChanged(GetPropertyName(() => IsMaterialInfoBtnVisible));
            RaisePropertyChanged(GetPropertyName(() => Formula));
        }
    }

    public enum MaterialQtaType
    {
        Unknown = 0,
        Area = 1,
        PaintArea = 2,
        Volume = 3,
    }


}
