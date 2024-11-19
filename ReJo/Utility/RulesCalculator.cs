using _3DModelExchange;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using CommonResources;
using ReJo.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace ReJo.Utility
{
    public class RulesCalculator
    {
        string _FilterName = string.Empty;
        string _FilterUniqueId = string.Empty;

        FiltersData? _FiltersData = null;
        FiltersDataItem? _FiltersDataItem = null;
        AttributoRegola? _AttributoQuantita = null;

        
        List<ElementId>? _ElementsId = null;
        List<Element>? _Elements = null;
        ElementId? _CurrentElementId = null;
        
        RvtCalculator? _Calculator = null;

        public List<ElementId>? ElementsId { get => _ElementsId; }


        public RulesCalculator()
        {
            _Calculator = new RvtCalculator();
            _Calculator.RvtCalculatorFunction = new RvtCalculatorFunction();
            _Calculator.Clear();
        }

        public List<RuleError> CheckRules()
        {
            Document doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;
            string? projectIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);

            if (projectIfcGuid == null)
                return null;


            var filterList = Utils.GetFilters(CmdInit.This.UIApplication.ActiveUIDocument.Document)?.OrderBy(item => item.Name);


            Dictionary<string, RuleError> rulesMap = new Dictionary<string, RuleError>();

            List<RuleError> ruleErrors = new List<RuleError>();


            foreach (var filter in filterList)
            {
                LoadFilter(projectIfcGuid, filter.Id.ToString(), filter.Name);

                var ruleErrorsPrezzo = UpdateFilterPrezzarioItems();
                var ruleErrorsConf = CalculateFilterConflicts(rulesMap);
                var ruleErrorsCalc = CalculateFilterRules();

                if (ruleErrorsPrezzo?.Count > 0)
                    ruleErrors.AddRange(ruleErrorsPrezzo);

                if (ruleErrorsConf?.Count > 0)
                    ruleErrors.AddRange(ruleErrorsConf);

                if (ruleErrorsCalc?.Count > 0)
                    ruleErrors.AddRange(ruleErrorsCalc);


            }

            return ruleErrors;
        }



        public void LoadFilter(string projectIfcGuid, string filterUniqueId, string filterName, FiltersData? filtersData = null, List<Element>? elements = null, ElementId? currentElementId = null)
        {
            Clear();

            _FilterName = filterName;
            _FilterUniqueId = filterUniqueId;



            if (elements == null)
                LoadElements();
            else
                _Elements = elements;

            _ElementsId = _Elements?.Select(item => item.Id).ToList();
            _CurrentElementId = currentElementId;


            if (filtersData == null)
            {
                _FiltersData = CmdInit.This.JoinService.GetCurrentProjectModel3dFilters();
                if (_FiltersData == null)
                    return;
            }
            else
            {
                _FiltersData = filtersData;
            }

            _AttributoQuantita = _FiltersData.AttributiRegola.FirstOrDefault(item => item.IsQuantita);
            if (_AttributoQuantita == null)
                return;

            _FiltersDataItem = _FiltersData.Items.FirstOrDefault(item => item.RvtFilter?.RvtFilterUniqueId == _FilterUniqueId);
            if (_FiltersDataItem == null)
            {
                _FiltersDataItem = new FiltersDataItem();
                _FiltersDataItem.ID = Guid.NewGuid();
                _FiltersDataItem.RvtFilter = new RvtFilterForIO() { RvtFilterUniqueId = _FilterUniqueId, ProjectIfcGuid = projectIfcGuid };
                _FiltersData.Items.Add(_FiltersDataItem);

            }

        }

        public List<RuleError>? CalculateFilterConflicts(Dictionary<string, RuleError> rulesMap)
        {
            if (!IsFilterLoaded())
                return null;

            List<RuleError> ruleErrors = new List < RuleError >();

            foreach (var rule in _FiltersDataItem.RulesIO)
            {
                foreach (var el in _Elements)
                {

                    string ruleKey = string.Format("{0}|{1}", rule.Id, el.Id);

                    var ruleData = new RuleErrorConflict(this)
                    {
                        Text = LocalizationProvider.GetString("Conflitto"),
                        RuleId = rule.RuleId,
                        PrezzarioItemId = rule.Id,
                        ElementId = el.Id,
                        FilterUniqueId = _FilterUniqueId,
                        FilterName = _FilterName,

                    };

                    if (rulesMap.ContainsKey(ruleKey))
                    {
                        RuleErrorConflict ruleDataFound = (RuleErrorConflict) rulesMap[ruleKey];

                        ruleData.ConflictFilterId = rulesMap[ruleKey].FilterUniqueId;
                        ruleData.ConflictFilterName = rulesMap[ruleKey].FilterName;

                        ruleDataFound.ConflictFilterId = ruleData.FilterUniqueId;
                        ruleDataFound.ConflictFilterName = ruleData.FilterName;

                        ruleErrors.Add(ruleData);
                        ruleErrors.Add(ruleDataFound);
                    }
                    else
                        rulesMap.Add(ruleKey, ruleData);

                }
            }

            return ruleErrors;

        }



        public List<RuleError>? CalculateFilterRules()
        {
            //calcola filtersDataItem.RulesIO

            if (!IsFilterLoaded())
                return null;

            var ruleErrors = new List<RuleError>();

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;


            _Calculator.Clear();

            List<Element> elements = _Elements;


            string projectIfcGuid = CmdInit.This.GetProjectIfcGuid(doc);



            double result;
            string resultDescription;

            foreach (var rule in _FiltersDataItem.RulesIO)
            {
                double? singleQta = null;
                double? singleImporto = null;

                double qtaTotale = 0;
                double prezzo = PrezzoAsDouble(rule.Prezzo);

                foreach (Element el in elements)
                {
                    string elIfcGuid = CmdInit.This.GetElementIfcGuid(doc, el.Id);

                    if (_AttributoQuantita.IsValoreLockedByDefault)
                    {
                        //calcolo gli attributi che fanno variare la quantità
                        string formulaQta = _AttributoQuantita.Formula;

                        List<AttributoRegola> attsReg = OrderAttributiByFunctions(_FiltersData.AttributiRegola, rule);

                        foreach (var attReg in attsReg)
                        {
                            string formulaAtt = string.Format("{0}{{{1}}}", AttCalculatorFunction.FunctionName, attReg.Etichetta);

                            if (formulaQta.Contains(formulaAtt))
                            {
                                if (rule.FormuleByAttributoComputo.TryGetValue(attReg.Codice, out string formula))
                                {
                                    if (!string.IsNullOrEmpty(formula))
                                    {
                                        if (_Calculator.Calculate(projectIfcGuid, elIfcGuid, formula, out result, out resultDescription))
                                        {
                                            AddErrors(ruleErrors, rule, el, _AttributoQuantita.Etichetta);

                                            _Calculator.AttCalculatorFunction.AddResult(attReg.Etichetta, result);
                                        }
                                    }
                                }
                            }
                        }

                        //calcolo la quantità
                        double qtaEl;
                        string qtaResultDescription;

                        if (_Calculator.Calculate(projectIfcGuid, elIfcGuid, formulaQta, out qtaEl, out resultDescription))
                        {
                            AddErrors(ruleErrors, rule, el, _AttributoQuantita.Etichetta);

                            if (_CurrentElementId == el.Id)
                            {
                                singleQta = qtaEl;
                                singleImporto = singleQta * prezzo;
                            }

                            qtaTotale += qtaEl;
                        }

                    }
                    else
                    {
                        string formulaQta = rule.FormulaQta;
                        double qtaEl;
                        string qtaResultDescription;


                        if (!string.IsNullOrEmpty(formulaQta))
                        {

                            if (_Calculator.Calculate(projectIfcGuid, elIfcGuid, formulaQta, out qtaEl, out qtaResultDescription))
                            {
                                AddErrors(ruleErrors, rule, el, _AttributoQuantita.Etichetta);

                                if (_CurrentElementId == el.Id)
                                {
                                    singleQta = qtaEl;
                                    singleImporto = singleQta * prezzo;
                                }

                                qtaTotale += qtaEl;
                            }
                        }
                    }
                }

                if (singleQta.HasValue)
                    rule.SingleQta = singleQta.Value;
                else
                    rule.SingleQta = 0;

                if (singleImporto.HasValue)
                    rule.SingleImporto = singleImporto.Value;
                else
                    rule.SingleImporto = 0;

                rule.QuanTot = qtaTotale;
                rule.ImportoTotale = string.Format(rule.PrezzoFormat, (qtaTotale * prezzo));

            }

            return ruleErrors;

        }

        private bool IsFilterLoaded()
        {
            if (_FiltersData == null)
                return false;

            if (_FiltersDataItem == null)
                return false;

            if (_Elements == null)
                return false;

            if (_ElementsId == null)
                return false;

            if (_AttributoQuantita == null)
                return false;

            return true;
        }

        void Clear()
        {
            _FilterName = string.Empty;
            _FilterUniqueId = string.Empty;
            _FiltersData = null;
            _FiltersDataItem = null;
            _Calculator.RvtCalculatorFunction = new RvtCalculatorFunction();
            _Calculator.Clear();
            _ElementsId = new List<ElementId>();
            _Elements = new List<Element>();
            _AttributoQuantita = null;
        }

        void LoadElements()
        {
            FilterElement? filterElement = null;

            var doc = CmdInit.This.UIApplication.ActiveUIDocument.Document;

            //long longFilterId = 0;
            //if (long.TryParse(_FilteUniqueId, out longFilterId))
            //{
            //    ElementId filterId = new ElementId(longFilterId);

            //    filterElement = doc.GetElement(filterId) as FilterElement;
            //}

            filterElement = doc.GetElement(_FilterUniqueId) as FilterElement;

            if (filterElement == null)
                return;

            _FilterName = filterElement.Name;

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
                    //_ElementIndexById.TryAdd(el.Id, _Elements.Count - 1);
                }
            }

        }

        public List<RuleError> UpdateFilterPrezzarioItems()
        {
            if (!IsFilterLoaded())
                return null;


            List<RuleError> ruleErrors = new List<RuleError>();

            List<Model3dPrezzarioItem> prezzarioItemsToUpdate = new List<Model3dPrezzarioItem>();

            foreach (var rule in _FiltersDataItem.RulesIO)
            {
                var prezItem = new Model3dPrezzarioItem();
                prezItem.Id = rule.Id;
                prezItem.Prezzario = rule.Prezzario;
                prezItem.Codice = rule.Codice;
                prezItem.Descrizione = rule.Descrizione;
                prezItem.Prezzo = rule.Prezzo;
                prezItem.PrezzoFormat = rule.PrezzoFormat;
                prezItem.UM = rule.UM;

                prezzarioItemsToUpdate.Add(prezItem);
            }

            var prezzarioItems  = JoinService.This.GetUpdatedPrezzarioItems(prezzarioItemsToUpdate);

            if (prezzarioItems == null)
                return null;

            //foreach (var rule in _FiltersDataItem.RulesIO)
            for (int i = 0; i< _FiltersDataItem.RulesIO.Count; i++)
            {
                var rule = _FiltersDataItem.RulesIO[i];
                Model3dPrezzarioItem prezItem = null;

                if (i < prezzarioItems.Count)
                    prezItem = prezzarioItems[i];

                if (prezItem == null)
                    continue;

                if (prezItem.Id == Guid.Empty)
                {
                    //articolo non trovato
                    ruleErrors.Add(new RuleErrorPrezzo()
                    {
                        RuleId = rule.RuleId,
                        FilterUniqueId = _FilterUniqueId,
                        FilterName = _FilterName,
                        Text = LocalizationProvider.GetString("Articolo non trovato"),
                        PrezzarioItemCodice = rule.Codice,
                        PrezzarioItemPrezzario = rule.Prezzario,
                    });

                    rule.Descrizione = string.Empty;
                    rule.Prezzo = string.Empty;
                    rule.PrezzoFormat = string.Empty;
                    rule.UM = string.Empty;
                    continue;
                }

                double prezzo = PrezzoAsDouble(prezItem.Prezzo);
                if (prezzo == 0)
                {
                    ruleErrors.Add(new RuleErrorPrezzo()
                    {
                        RuleId = rule.RuleId,
                        FilterUniqueId = _FilterUniqueId,
                        FilterName = _FilterName,
                        Text = LocalizationProvider.GetString("Prezzo non valido"),
                        PrezzarioItemCodice = rule.Codice,
                        PrezzarioItemPrezzario = rule.Prezzario,
                    });
                }


                prezItem.Id = rule.Id;
                rule.Prezzario = prezItem.Prezzario;
                rule.Codice = prezItem.Codice;


                rule.Descrizione = prezItem.Descrizione;
                rule.Prezzo = prezItem.Prezzo;
                rule.PrezzoFormat = prezItem.PrezzoFormat;
                rule.UM = prezItem.UM;

                prezzarioItemsToUpdate.Add(prezItem);
            }


            return ruleErrors;
        }

        public static double PrezzoAsDouble(string prezzo)
        {
            prezzo = prezzo.Replace(",", ".");
            if (Double.TryParse(prezzo, CultureInfo.InvariantCulture, out double res))
            {
                return res;
            }
            return 0;
        }

        protected List<AttributoRegola> OrderAttributiByFunctions(List<AttributoRegola> atts, RegoleComputoForIO rule)
        {
            List<AttributoRegola> orderedList = new List<AttributoRegola>();


            List<AttFunctionsDependency> funcAtts = atts.Where(item => item.IsValoreLockedByDefault == false).Select(item => new AttFunctionsDependency()
            {
                Etichetta = item.Etichetta,
                Codice = item.Codice,
                Function = string.Format("att{{{0}}}", item.Etichetta),
            }).ToList();



            List<string> functions = funcAtts.Select(item => item.Function).ToList();

            foreach (AttFunctionsDependency funcAtt in funcAtts)
            {
                string formula = string.Empty;
                if (rule.FormuleByAttributoComputo.TryGetValue(funcAtt.Codice, out formula))
                {
                    funcAtt.Formula = formula;

                    foreach (AttFunctionsDependency funcAtt1 in funcAtts)
                    {
                        if (funcAtt.Formula != null && funcAtt.Formula.Contains(funcAtt1.Function))
                        {
                            //funcAtt dipende da funcAtt1
                            funcAtt.DependBy.Add(funcAtt1);
                        }
                    }
                }

            }

            //Calcolo i livelli (di dipendenza)
            foreach (AttFunctionsDependency funcAtt in funcAtts)
            {
                funcAtts.ForEach(item => item.IsLoopLevel = false);
                funcAtt.Level = funcAtt.GetLevel();
            }


            foreach (AttFunctionsDependency funcAtt in funcAtts.OrderBy(item => item.Level))
            {
                AttributoRegola entAtt = atts.FirstOrDefault(item => item.Codice == funcAtt.Codice);
                if (entAtt != null)
                    orderedList.Add(entAtt);
            }

            return orderedList;
        }


        private void AddErrors(List<RuleError> errors, RegoleComputoForIO? rule, Element el, string attributoEtichetta)
        {

            if (_Calculator.RvtCalculatorFunction.Error)
            {
                errors.Add(new RuleErrorCalculator(this)
                {
                    Text = LocalizationProvider.GetString("Calcolo rvt"),
                    RuleId = rule.RuleId,
                    FilterUniqueId = _FilterUniqueId,
                    FilterName = _FilterName,
                    ElementId = el.Id,
                    AttributoEtichetta = attributoEtichetta,
                    ResultDescription = _Calculator.RvtCalculatorFunction.GetResultsDescription(),
                });

            }

            if (_Calculator.AttCalculatorFunction.Error)
            {
                errors.Add(new RuleErrorCalculator(this)
                {
                    Text = LocalizationProvider.GetString("Calcolo att"),
                    RuleId = rule.RuleId,
                    FilterUniqueId = _FilterUniqueId,
                    FilterName = _FilterName,
                    ElementId = el.Id,
                    AttributoEtichetta = attributoEtichetta,
                    ResultDescription = _Calculator.AttCalculatorFunction.GetResultsDescription(),
                });

            }
        }
    }

    public class RuleError
    {
        public Guid RuleId { get; internal set; }
        public string FilterUniqueId { get; set; } = string.Empty;
        public string FilterName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public virtual string AsString() { return string.Empty; }
        

    }
    
    public class RuleErrorCalculator : RuleError
    {

        RulesCalculator _owner = null;
        public RuleErrorCalculator(RulesCalculator owner)
        {
            _owner = owner;
        }


        public ElementId ElementId { get; internal set; }
        public string AttributoEtichetta { get; internal set; }
        public string ResultDescription { get; internal set; }

        public override string AsString()
        {
            int nElem = _owner.ElementsId.IndexOf(ElementId) + 1;


            string str = string.Format("{0}: {1}: #{2} [{3}] {4}: {5} {6}: {7}",
                Text,
                LocalizationProvider.GetString("Element"), ElementId.ToString(), nElem,
                LocalizationProvider.GetString("Attributo1"), AttributoEtichetta,
                LocalizationProvider.GetString("Formula1"), ResultDescription);

            return str;
        }
    }

    public class RuleErrorPrezzo : RuleError
    {

        public Guid PrezzarioItemId { get; internal set; }  
        public string PrezzarioItemPrezzario { get; internal set; }
        public string PrezzarioItemCodice { get; internal set; }
        public string Text {  get; internal set; } 




        public override string AsString()
        {
            string str = Text;
            str += "\n";
            str = string.Format("{0}: {1} {2}",Text, PrezzarioItemPrezzario, PrezzarioItemCodice);

            return str;
        }
    }

    public class RuleErrorConflict : RuleError
    {
        RulesCalculator _owner = null;
        public RuleErrorConflict(RulesCalculator owner)
        {
            _owner = owner;
        }

        public ElementId ElementId { get; set; }

        public Guid PrezzarioItemId { get; set; }

        public string ConflictFilterId { get; set; } = string.Empty;
        public string ConflictFilterName { get; set; } = string.Empty;


        public override string AsString()
        {
            int nElem = _owner.ElementsId.IndexOf(ElementId) + 1;

            string str = string.Format("{0}: {1} #{2} [{3}]", Text, ConflictFilterName, ElementId.Value.ToString(), nElem);
            return str;
        }
    }



  
}
