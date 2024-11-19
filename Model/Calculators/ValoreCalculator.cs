
using _3DModelExchange;
using CommonResources;
using Commons;
using DevExpress.Office.Utils;
using MasterDetailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Model
{


    public class ValoreCalculator : Model3dCalculator
    {
        IDataService _dataService;
        public virtual IDataService DataService { get => _dataService; }

        public CmpCalculatorFunction CmpCalculatorFunction { get; set; } = null;
        public EPCalculatorFunction EPCalculatorFunction { get; set; } = null;
        public ElmCalculatorFunction ElmCalculatorFunction { get; set; } = null;
        public CntCalculatorFunction CntCalculatorFunction { get; set; } = null;
        public InfCalculatorFunction InfCalculatorFunction { get; set; } = null;
        public DivCalculatorFunction DivCalculatorFunction { get; set; } = null;
        public EAtCalculatorFunction EAtCalculatorFunction { get; set; } = null;
        public WBSCalculatorFunction WBSCalculatorFunction { get; set; } = null;
        public CalendariCalculatorFunction CalendariCalculatorFunction { get; set; } = null;
        public VarCalculatorFunction VarCalculatorFunction { get; set; } = null;
        public CapCalculatorFunction CapCalculatorFunction { get; set; } = null;

        public ValoreCalculatorFunction CurrentCalculatorFunction { get; set; } = null;

        //key: FilterData
        protected Dictionary<string, GuidCollectionCalculated> GuidCollectionsCalculated { get; private set; } = null;


        public ValoreCalculator(IDataService dataService)
        {
            _dataService = dataService;
            CmpCalculatorFunction = new CmpCalculatorFunction(dataService);
            EPCalculatorFunction = new EPCalculatorFunction(dataService);
            ElmCalculatorFunction = new ElmCalculatorFunction(dataService);
            CntCalculatorFunction = new CntCalculatorFunction(dataService);
            InfCalculatorFunction = new InfCalculatorFunction(dataService);
            DivCalculatorFunction = new DivCalculatorFunction(dataService);
            EAtCalculatorFunction = new EAtCalculatorFunction(dataService);
            WBSCalculatorFunction = new WBSCalculatorFunction(dataService);
            VarCalculatorFunction = new VarCalculatorFunction(dataService);
            CalendariCalculatorFunction = new CalendariCalculatorFunction(dataService);
            CapCalculatorFunction = new CapCalculatorFunction(dataService);
            //
            GuidCollectionsCalculated = new Dictionary<string, GuidCollectionCalculated>();

        }


        public bool Calculate(Entity entity, Attributo att, Valore val)
        {

            try
            {

                if (entity is ComputoItem)
                {
                    ComputoItem computoItem = entity as ComputoItem;

                    CurrentCalculatorFunction = CmpCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = computoItem.EntityId;

                    Guid elementiItemId = computoItem.GetElementiItemId();

                    List<Entity> elems = DataService.GetEntitiesById(BuiltInCodes.EntityType.Elementi, new List<Guid>() { elementiItemId });
                    SetModel3dCalculatorFunction(elems.FirstOrDefault());
                }
                else if (entity is ElementiItem)
                {
                    CurrentCalculatorFunction = ElmCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                    SetModel3dCalculatorFunction(entity);
                }
                else if (entity is PrezzarioItem)
                {
                    CurrentCalculatorFunction = EPCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;

                }
                else if (entity is DivisioneItem)
                {
                    DivCalculatorFunction.SetEntityType(att.EntityTypeKey);
                    CurrentCalculatorFunction = DivCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;

                    SetModel3dCalculatorFunction(entity);
                }
                else if (entity is ContattiItem)
                {
                    CurrentCalculatorFunction = CntCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is InfoProgettoItem)
                {
                    CurrentCalculatorFunction = InfCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is ElencoAttivitaItem)
                {
                    CurrentCalculatorFunction = EAtCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is WBSItem)
                {
                    CurrentCalculatorFunction = WBSCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is CalendariItem)
                {
                    CurrentCalculatorFunction = CalendariCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is VariabiliItem)
                {
                    CurrentCalculatorFunction = VarCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }
                else if (entity is CapitoliItem)
                {
                    CurrentCalculatorFunction = CapCalculatorFunction;
                    CurrentCalculatorFunction.CurrentEntityId = entity.EntityId;
                }



                if (val is ValoreReale)
                {
                    ValoreReale valReale = val as ValoreReale;
                    string formula = valReale.ToPlainText();

                    //controllo se è valore null e prendo il default
                    ValoreReale valoreDefault = att.ValoreDefault as ValoreReale;
                    if (!formula.Trim().Any() && valoreDefault != null)
                        formula = valoreDefault.RealResult.ToString();


                    double real;
                    string resultDescription;

                    bool res = Calculate(formula, out real, out resultDescription);
                    if (res)
                    {
                        var valAttReale = att.ValoreAttributo as ValoreAttributoReale;
                        if (valAttReale != null && valAttReale.UseSignificantDigitsByFormat)
                        {
                            NumberFormat nf = NumericFormatHelper.DecomposeFormat(att.ValoreFormat);
                            double realRounded = Math.Round(real, nf.DecimalDigitCount, MidpointRounding.AwayFromZero);
                            valReale.RealResult = realRounded;
                        }
                        else
                            valReale.RealResult = real;

                        if (string.IsNullOrEmpty(valReale.PlainText))
                            valReale.ResultDescription = String.Empty;
                        else
                            valReale.ResultDescription = resultDescription;
                    }


                    return res;
                }
                else if (val is ValoreContabilita)
                {
                    ValoreContabilita valCont = val as ValoreContabilita;
                    string formula = valCont.ToPlainText();


                    //controllo se è valore null e prendo il default
                    ValoreContabilita valoreDefault = att.ValoreDefault as ValoreContabilita;
                    if (!formula.Trim().Any() && valoreDefault != null)
                        formula = valoreDefault.RealResult.ToString();



                    double real;
                    string resultDescription;

                    bool res = Calculate(formula, out real, out resultDescription);
                    if (res)
                    {
                        if (!Double.IsNaN(real))
                        {
                            var valAttCont = att.ValoreAttributo as ValoreAttributoContabilita;
                            if (valAttCont != null && valAttCont.UseSignificantDigitsByFormat)
                            {
                                NumberFormat nf = NumericFormatHelper.DecomposeFormat(att.ValoreFormat);
                                decimal realRounded = Math.Round((decimal) real, nf.DecimalDigitCount, MidpointRounding.AwayFromZero);
                                valCont.RealResult = realRounded;
                            }
                            else
                                valCont.RealResult = (decimal)real;
                        }
                        else
                            valCont.RealResult = null;

                        if (string.IsNullOrEmpty(valCont.PlainText))
                            valCont.ResultDescription = String.Empty;
                        else
                            valCont.ResultDescription = resultDescription;

                    }

                    return res;
                }
                else if (val is ValoreTesto)
                {
                    ValoreTesto valTxt = val as ValoreTesto;
                    string formula = valTxt.V;


                    //controllo se è valore null e prendo il default
                    ValoreTesto valoreDefault = att.ValoreDefault as ValoreTesto;
                    //if (string.IsNullOrEmpty(formula) && valoreDefault != null)
                    if (formula == null && valoreDefault != null)
                        formula = valoreDefault.ToPlainText();

                    if (formula == null)
                        formula = string.Empty;

                    string trimmedFormula = formula.Trim();

                    List<string> valuesPath = new List<string>();

                    List<string> funtionsName = new List<string>();
                    if (IfcCalculatorFunction != null)
                        funtionsName.Add(IfcCalculatorFunction.GetFunctionName());
                    if (RvtCalculatorFunction != null)
                        funtionsName.Add(RvtCalculatorFunction.GetFunctionName());
                    if (CurrentCalculatorFunction != null)
                        funtionsName.Add(CurrentCalculatorFunction.GetFunctionName());

                    string sFuncs = string.Join("|", funtionsName/*functions.Select(item => item.Name)*/);


                    //string pattern = string.Format("[^a-zA-Z0-9]" + //non precedute da lettera o numero
                    //"({0})" + //precedute da una delle funzioni
                    //"\\{1}(.*?)\\{2}", sFuncs, OpenBracket, CloseBracket);//elementi tra le parentesi graffe

                    string pattern = string.Format("({0})" + //precedute da una delle funzioni
                    "\\{1}(.*?)\\{2}", sFuncs, OpenBracket, CloseBracket);//elementi tra le parentesi graffe

                    string[] splittedStr = Regex.Split(" " + formula, pattern, RegexOptions.CultureInvariant); //aggiunto lo spazio iniziale altrimenti non riconosce le funzioni se sono all'inizio della stringa

                    //ciclo per gestire le parentesi nidificate
                    List<string> splittedList = new List<string>();
                    string temp = "";
                    foreach (string str in splittedStr)
                    {
                        if (temp.Any())
                        {
                            int charLocation = str.IndexOf(CloseBracket);
                            if (charLocation == -1)
                                return false;
                            string s = string.Format("{0}{1}{2}", temp, CloseBracket, str.Substring(0, charLocation));
                            temp = "";
                            splittedList.Add(s);
                        }
                        else
                        {
                            if (str.Contains(OpenBracket))
                            {
                                temp = str;
                            }
                            else
                            {
                                splittedList.Add(str);
                            }
                        }
                    }



                    splittedStr[0].TrimStart();
                    int index = -1;

                    string outStr = "";
                    string currentFunc = "";
                    bool isCalculated = false;
                    //foreach (string splittedStrItem in splittedStr)
                    foreach (string splittedStrItem in splittedList)
                    {
                        index++;
                        string str = splittedStrItem;

                        if (index == 0)//tolgo lo spazio appena aggiunto
                            str = str.TrimStart();

                        if (currentFunc.Any())
                        {
                            string value = string.Empty;
                            bool res = false;
                            if (CurrentCalculatorFunction != null && currentFunc == CurrentCalculatorFunction.GetFunctionName())
                            {
                                res = CurrentCalculatorFunction.Calculate(str, out value);
                            }
                            else if (IfcCalculatorFunction != null && currentFunc == IfcCalculatorFunction.GetFunctionName())
                            {
                                res = IfcCalculatorFunction.Calculate(IfcCalculatorFunction.ProjectGlobalId, IfcCalculatorFunction.GlobalId, Model3dClassEnum.Nothing, string.Empty, str, out value);
                            }
                            else if (RvtCalculatorFunction != null && currentFunc == RvtCalculatorFunction.GetFunctionName())
                            {
                                res = RvtCalculatorFunction.Calculate(RvtCalculatorFunction.ProjectGlobalId, RvtCalculatorFunction.GlobalId, Model3dClassEnum.Nothing, string.Empty, str, out value);
                            }

                            //if (outStr.Any())
                            //    outStr += " ";

                            if (res)
                            {
                                outStr += value;
                            }
                            else
                            {
                                //outStr += string.Format("{0}({1})", currentFunc, str);
                                outStr += CalculatorExpression.CreateFormula(currentFunc, str);
                            }
                            currentFunc = "";
                        }
                        else if (funtionsName.Contains(str))
                        {
                            isCalculated = true;
                            currentFunc = str;
                        }
                        else
                        {
                            currentFunc = "";
                            outStr += str;
                        }
                    }

                    if (isCalculated)
                    {
                        //valTxt.V = outStr;
                        valTxt.Result = outStr;
                    }
                    else
                    {
                        valTxt.Result = valTxt.V;
                    }


                    return true;

                }
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message);
            }

            return false;

        }

        private void SetModel3dCalculatorFunction(Entity ent)
        {
            //if (IfcCalculatorFunction == null)
            //    return;

            if (ent != null)
            {
                Valore val = ent.GetValoreAttributo(BuiltInCodes.Attributo.GlobalId, false, false);
                if (val != null)
                {
                    if (IfcCalculatorFunction != null)
                        IfcCalculatorFunction.GlobalId = val.ToPlainText();

                    if (RvtCalculatorFunction != null)
                        RvtCalculatorFunction.GlobalId = val.ToPlainText();


                }

                val = ent.GetValoreAttributo(BuiltInCodes.Attributo.ProjectGlobalId, false, false);
                if (val != null)
                {
                    if (IfcCalculatorFunction != null)
                        IfcCalculatorFunction.ProjectGlobalId = val.ToPlainText();

                    if (RvtCalculatorFunction != null)
                        RvtCalculatorFunction.ProjectGlobalId = val.ToPlainText();
                }
            }
        }

        public override List<CalculatorFunction> GetFunctions()
        {
            List<CalculatorFunction> functs = base.GetFunctions();
            if (CurrentCalculatorFunction != null)
                functs.Add(CurrentCalculatorFunction);

            return functs;
        }

        //internal void ClearCalculatedValue()
        //{
        //    List<CalculatorFunction> functs = GetFunctions();
        //    foreach (CalculatorFunction calcFunc in functs)
        //    {
        //        ValoreCalculatorFunction valCalcFunc = calcFunc as ValoreCalculatorFunction;
        //        if (valCalcFunc != null)
        //            valCalcFunc.ClearCalculatedValue();
        //    }
        //}

        internal void SetCurrentCalculatorFunction(string key)
        {

        }

        public static Valore ExecuteOperation(string definizioneAttributoCodice, IEnumerable<Valore> vals, ValoreOperationType operation, int? significantDigitsCount)
        {
            Valore result = null;

            if (vals == null)
                return null;


            //Valore first = vals.FirstOrDefault();
            //if (first == null)
            //    return null;


            //reale
            if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                double sum = 0;
                double multiplication = 1;
                foreach (Valore val in vals)
                {
                    ValoreReale valReale = val as ValoreReale;
                    if (valReale == null)
                        return null;

                    if (!valReale.RealResult.HasValue)
                        return null;

                    sum += valReale.RealResult.Value;
                    multiplication *= valReale.RealResult.Value;

                    if (valReale.RealResult.Value > max)
                        max = valReale.RealResult.Value;

                    if (valReale.RealResult.Value < min)
                        min = valReale.RealResult.Value;

                }

                ValoreReale valRealeRes = new ValoreReale();
                if (operation == ValoreOperationType.Sum)
                {
                    valRealeRes.V = sum.ToString();
                    valRealeRes.RealResult = sum;
                }
                else if (operation == ValoreOperationType.Min)
                {
                    valRealeRes.V = min.ToString();
                    valRealeRes.RealResult = min;
                }
                else if (operation == ValoreOperationType.Max)
                {
                    valRealeRes.V = max.ToString();
                    valRealeRes.RealResult = max;
                }
                else if (operation == ValoreOperationType.Average)
                {
                    double average = sum / vals.Count(); 
                    
                    if (significantDigitsCount.HasValue)
                    {
                        average = Math.Round(average, significantDigitsCount.Value, MidpointRounding.AwayFromZero);
                    }

                    valRealeRes.V = average.ToString();
                    valRealeRes.RealResult = average;
                }
                else if (operation == ValoreOperationType.Multiplication)
                {
                    if (double.IsInfinity(multiplication))
                    {
                        valRealeRes.V = "\u221e";//infinity symbol
                    }
                    else
                    {
                        if (significantDigitsCount.HasValue)
                        {
                            multiplication = Math.Round(multiplication, significantDigitsCount.Value, MidpointRounding.AwayFromZero);
                        }

                        valRealeRes.V = multiplication.ToString();
                        valRealeRes.RealResult = multiplication;
                    }
                }

                result = valRealeRes;
            }
            //contabilità
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
            {
                decimal max = Decimal.MinValue;
                decimal min = Decimal.MaxValue;
                decimal sum = 0;
                double multiplication = 1;
                foreach (Valore val in vals)
                {
                    ValoreContabilita valCont = val as ValoreContabilita;
                    if (valCont == null)
                        return null;

                    if (!valCont.RealResult.HasValue)
                        return null;

                    sum += valCont.RealResult.Value;
                    multiplication *= (double) valCont.RealResult.Value;

                    if (valCont.RealResult.Value > max)
                        max = valCont.RealResult.Value;

                    if (valCont.RealResult.Value < min)
                        min = valCont.RealResult.Value;

                }

                ValoreContabilita valContRes = new ValoreContabilita();
                if (operation == ValoreOperationType.Sum)
                {
                    valContRes.V = sum.ToString();
                    valContRes.RealResult = sum;
                }
                else if (operation == ValoreOperationType.Min)
                {
                    valContRes.V = min.ToString();
                    valContRes.RealResult = min;
                }
                else if (operation == ValoreOperationType.Max)
                {
                    valContRes.V = max.ToString();
                    valContRes.RealResult = max;
                }
                else if (operation == ValoreOperationType.Average)
                {
                    decimal average = sum / vals.Count();
                    valContRes.V = average.ToString();
                    valContRes.RealResult = average;
                }
                else if (operation == ValoreOperationType.Multiplication)
                {
                    if (double.IsInfinity(multiplication))
                    {
                        valContRes.V = "\u221e";//infinity symbol
                    }
                    else
                    {
                        valContRes.V = multiplication.ToString();
                        valContRes.RealResult = (decimal) multiplication;
                    }


                }
                result = valContRes;
            }
            //testo
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
            {
                string concatWithSpace = string.Empty;
                string concatNewLine = string.Empty;
                string equal = string.Empty;

                foreach (Valore val in vals)
                {
                    ValoreTesto valTesto = val as ValoreTesto;
                    if (valTesto == null)
                        return null;

                    if (concatWithSpace.Any())
                        concatWithSpace = string.Format("{0} {1}", concatWithSpace, valTesto.PlainText);
                    else
                        concatWithSpace = valTesto.PlainText;

                    if (concatNewLine.Any())
                        concatNewLine = string.Format("{0}\n{1}", concatNewLine, valTesto.PlainText);
                    else
                        concatNewLine = valTesto.PlainText;

                    if (!equal.Any())
                        equal = valTesto.PlainText;
                    else if (equal != valTesto.PlainText)
                        equal = LocalizationProvider.GetString(ValoreHelper.Multi);

                }
                ValoreTesto valTestoRes = new ValoreTesto();
                if (operation == ValoreOperationType.AppendWithSpace)
                    valTestoRes.V = concatWithSpace;
                else if (operation == ValoreOperationType.AppendNewLine)
                    valTestoRes.V = concatNewLine;
                else if (operation == ValoreOperationType.Equivalent)
                    valTestoRes.V = equal;

                result = valTestoRes;
            }
            //elenco
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
            {
                string concatWithSpace = string.Empty;
                string concatNewLine = string.Empty;
                string equal = string.Empty;

                foreach (Valore val in vals)
                {
                    ValoreElenco valEl = val as ValoreElenco;
                    if (valEl == null)
                        return null;

                    if (concatWithSpace.Any())
                        concatWithSpace = string.Format("{0} {1}", concatWithSpace, valEl.PlainText);
                    else
                        concatWithSpace = valEl.PlainText;

                    if (concatNewLine.Any())
                        concatNewLine = string.Format("{0}\n{1}", concatNewLine, valEl.PlainText);
                    else
                        concatNewLine = valEl.PlainText;

                    if (!equal.Any())
                        equal = valEl.PlainText;
                    else if (equal != valEl.PlainText)
                        equal = LocalizationProvider.GetString(ValoreHelper.Multi);

                }
                ValoreTesto valTestoRes = new ValoreTesto();
                if (operation == ValoreOperationType.AppendWithSpace)
                    valTestoRes.V = concatWithSpace;
                else if (operation == ValoreOperationType.AppendNewLine)
                    valTestoRes.V = concatNewLine;
                else if (operation == ValoreOperationType.Equivalent)
                    valTestoRes.V = equal;

                result = valTestoRes;
            }
            //booleano
            else if (definizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
            {
                bool? equal = null;

                foreach (Valore val in vals)
                {
                    ValoreBooleano valBool = val as ValoreBooleano;
                    if (valBool == null)
                        return null;

                    if (equal.HasValue)
                    {
                        if (equal.Value == true && valBool.V == false)
                            equal = null;

                        if (equal.Value == false && valBool.V == true)
                            equal = null;
                    }
                    else
                        equal = valBool.V;
                }
                ValoreBooleano valBoolRes = new ValoreBooleano();
                if (operation == ValoreOperationType.Equivalent)
                    valBoolRes.V = equal;

                result = valBoolRes;
            }

            return result;
        }

        public Valore GetCalculatedValue(string entityTypeKey, Guid entityId, string etichettaAttributo)
        {
            ValoreCalculatorFunction calcFunction = GetCalculatorFunction(entityTypeKey);
            if (calcFunction == null)
                return null;

            calcFunction.CurrentEntityId = entityId;

            Valore val = calcFunction.GetCurrentEntityCalculatedValue(etichettaAttributo);

            return val;
        }

        ValoreCalculatorFunction GetCalculatorFunction(string entityTypeKey)
        {
            if (entityTypeKey == BuiltInCodes.EntityType.Computo)
                return CmpCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.Prezzario)
                return EPCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.Contatti)
                return CntCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.InfoProgetto)
                return InfCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.Divisione)
                return DivCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.ElencoAttivita)
                return EAtCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.WBS)
                return WBSCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.Calendari)
                return CalendariCalculatorFunction;
            else if (entityTypeKey == BuiltInCodes.EntityType.Capitoli)
                return CapCalculatorFunction;

            return null;
        }

        //GuidCollectionsCalculated
        public void SetGuidCollectionsCalculatedFilterIds(FilterData filterData, List<Guid> filterIds)
        {
            string filterDataSerialized = "";
            JsonSerializer.JsonSerialize(filterData, out filterDataSerialized);

            if (!GuidCollectionsCalculated.ContainsKey(filterDataSerialized))
                GuidCollectionsCalculated.Add(filterDataSerialized, new GuidCollectionCalculated());

            GuidCollectionsCalculated[filterDataSerialized].FilterIdsCalculated = filterIds;

        }

        public void SetRiferimentoGuidCollectionsCalculatedValore(FilterData filterData, AttributoRiferimento attRif, Valore valore)
        {
            ValoreAttributoRiferimentoGuidCollection valAtt = attRif.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
            if (valAtt == null)
                return;

            ValoreOperationType valOpType = valAtt.Operation;
            string codiceAttRef = attRif.Codice;

            string filterDataSerialized = "";
            JsonSerializer.JsonSerialize(filterData, out filterDataSerialized);

            if (!GuidCollectionsCalculated.ContainsKey(filterDataSerialized))
                GuidCollectionsCalculated.Add(filterDataSerialized, new GuidCollectionCalculated());

            var guidCollCalc = GuidCollectionsCalculated[filterDataSerialized];

            if (!guidCollCalc.RiferimentoValoreCalculated.ContainsKey(attRif.ReferenceCodiceAttributo))
                guidCollCalc.RiferimentoValoreCalculated.Add(attRif.ReferenceCodiceAttributo, new RiferimentoValoreCalculated());

            var rifValCalc = guidCollCalc.RiferimentoValoreCalculated[attRif.ReferenceCodiceAttributo];

            if (!rifValCalc.ValoreCalculated.ContainsKey(valOpType))
                rifValCalc.ValoreCalculated.Add(valOpType, valore);
            else
                rifValCalc.ValoreCalculated[valOpType] = valore;

        }

        public List<Guid> GetGuidCollectionsCalculatedIds(FilterData filterData)
        {
            string filterDataSerialized = "";
            JsonSerializer.JsonSerialize(filterData, out filterDataSerialized);

            GuidCollectionCalculated guidCollCalc = null;
            if (GuidCollectionsCalculated.TryGetValue(filterDataSerialized, out guidCollCalc))
                return guidCollCalc.FilterIdsCalculated;

            return null;
        }

        public Valore GetRiferimentoGuidCollectionsCalculatedValore(FilterData filterData, AttributoRiferimento attRif)
        {
            ValoreAttributoRiferimentoGuidCollection valAtt = attRif.ValoreAttributo as ValoreAttributoRiferimentoGuidCollection;
            if (valAtt == null)
                return null;


            ValoreOperationType valOpType = valAtt.Operation;
            string codiceAttRef = attRif.Codice;


            string filterDataSerialized = "";
            JsonSerializer.JsonSerialize(filterData, out filterDataSerialized);


            Valore val = null;
            GuidCollectionCalculated guidCollCalc = null;
            if (GuidCollectionsCalculated.TryGetValue(filterDataSerialized, out guidCollCalc))
            {
                RiferimentoValoreCalculated rifValCalc = null;
                if (guidCollCalc.RiferimentoValoreCalculated.TryGetValue(attRif.ReferenceCodiceAttributo, out rifValCalc))
                {
                    rifValCalc.ValoreCalculated.TryGetValue(valOpType, out val);
                }
            }

            return val;
        }

        public void ClearGuidCollectionsCalculated()
        {
            GuidCollectionsCalculated.Clear();
        }



    }

    public abstract class ValoreCalculatorFunction : CalculatorFunction
    {
        public static string FunctionName { get; } = "att";
        protected IDataService _dataService = null;
        public Guid CurrentEntityId { get; set; }
        public string EntityTypeKey { get; protected set; } = string.Empty;
        protected EntitiesHelper _entitiesHelper = null;

        /// <summary>
        /// key1: CurrentEntityId, key2: etichetta attributo
        /// </summary>
        Dictionary<Guid, Dictionary<string, Valore>> CalculatedValue { get; set; } = new Dictionary<Guid, Dictionary<string, Valore>>();


        public void AddCalculatedValue(Guid currentEntityId, string function, Valore val)
        {
            //if (function == "Prezzo manodopera")
            //{
            //    int tp = 0;
            //}

            CurrentEntityId = currentEntityId;


            if (!CalculatedValue.ContainsKey(CurrentEntityId))
                CalculatedValue.Add(CurrentEntityId, new Dictionary<string, Valore>());

            if (!CalculatedValue[CurrentEntityId].ContainsKey(function))
                CalculatedValue[CurrentEntityId].Add(function, val);
        }

        public void ClearCurrentEntityCalculatedValue()
        {
            if (CalculatedValue.ContainsKey(CurrentEntityId))
            {
                CalculatedValue.Remove(CurrentEntityId);
            }
        }

        public void ClearCalculatedValue()
        {
            CalculatedValue.Clear();
        }

        public Valore GetCurrentEntityCalculatedValue(string function)
        {
            if (CalculatedValue.ContainsKey(CurrentEntityId))
            {
                if (CalculatedValue[CurrentEntityId].ContainsKey(function))
                    return CalculatedValue[CurrentEntityId][function];
            }
            return null;
        }

        public override double calculate()
        {
            double result = 0.0;
            string resultDescriptionFormula = string.Empty;

            try
            {
                int valuePathIndex = (int)x;
                string etichettaAttributo = ValuesPath[valuePathIndex];

                //Cerco la funzione precalcolata (ad esempio quando il valore arriva da un riferimento)
                Valore val = GetCurrentEntityCalculatedValue(etichettaAttributo);

                //recupero il valore 
                if (val == null)
                {
                    IEnumerable<Entity> ents = _dataService.GetEntitiesById(EntityTypeKey, new List<Guid>() { CurrentEntityId });
                    Entity entity = ents.FirstOrDefault();
                    if (entity == null)
                        return 0.0;

                    if (entity.EntityType.EtichetteMap.ContainsKey(etichettaAttributo))
                    {
                        string codiceAtt = entity.EntityType.EtichetteMap[etichettaAttributo];
                        //val = entity.GetValoreAttributo(codiceAtt, false, false);
                        val = _entitiesHelper.GetValoreAttributo(entity, codiceAtt, false, false);//Rev by Ale 18/06/2021

                    }
                }


                if (val != null)
                {
                    if (val is ValoreContabilita)
                    {
                        ValoreContabilita valCont = val as ValoreContabilita;
                        if (valCont.RealResult != null)
                        {
                            result = (double)valCont.RealResult;
                            resultDescriptionFormula = valCont.ResultDescriptionFormula;
                        }
                        else
                            result = double.NaN;

                    }
                    else if (val is ValoreReale)
                    {
                        ValoreReale valReale = val as ValoreReale;
                        if (valReale.RealResult != null)
                        {
                            result = (double)valReale.RealResult;
                            resultDescriptionFormula = valReale.ResultDescriptionFormula;
                        }
                    }
                    else if (val is ValoreBooleano)
                    {
                        ValoreBooleano valBool = val as ValoreBooleano;
                        if (valBool.V != null && valBool.V.HasValue)
                        {
                            if (valBool.V.Value)
                                result = 1.0;
                            else
                                result = 0.0;
                        }
                    }
                    else if (val is ValoreData)
                    {
                        ValoreData valData = val as ValoreData;
                        if (valData.V != null && valData.V.HasValue)
                        {
                            result = (double) valData.V.Value.Ticks;
                        }
                    }
                    else if (val is ValoreGuidCollection)
                    {
                        ValoreGuidCollection valGuidCol = val as ValoreGuidCollection;
                        result = valGuidCol.Items.Count;
                    }
                    if (val is ValoreElenco)
                    {
                        ValoreElenco valCont = val as ValoreElenco;
                        result = valCont.ValoreAttributoElencoId;
                    }


                    if (!_results.ContainsKey(etichettaAttributo))
                        _results.Add(etichettaAttributo, result);

                    if (!_resultsDescriptionFormula.ContainsKey(etichettaAttributo))
                        _resultsDescriptionFormula.Add(etichettaAttributo, resultDescriptionFormula);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return result;
        }

        /// <summary>
        /// Ritorna il valore dell'attributo avente etichetta etichettaAttributo
        /// </summary>
        /// <param name="etichettaAttributo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool Calculate(string etichettaAttributo, out string result)
        {
            result = string.Empty;

            Valore val = GetCurrentEntityCalculatedValue(etichettaAttributo);

            AttributoFormatHelper attFormatHelper = new AttributoFormatHelper(_dataService);
            
            string attFormat = null;


            //recupero il valore 
            if (val == null)
            {
                IEnumerable<Entity> ents = _dataService.GetEntitiesById(EntityTypeKey, new List<Guid>() { CurrentEntityId });
                Entity entity = ents.FirstOrDefault();
                if (entity == null)
                    return false;

                if (entity.EntityType.EtichetteMap.ContainsKey(etichettaAttributo))
                {
                    string codiceAtt = entity.EntityType.EtichetteMap[etichettaAttributo];
                    //val = entity.GetValoreAttributo(codiceAtt, false, false);
                    val = _entitiesHelper.GetValoreAttributo(entity, codiceAtt, false, false);//Rev by Ale 18/06/2021

                    attFormat = attFormatHelper.GetValorePaddedFormat(entity, codiceAtt);
                }
                else if (!etichettaAttributo.Any())
                {
                    //att{} restituiscce l'ID dell'entità
                    val = new ValoreTesto() { V = entity.EntityId.ToString() };

                }
            }
            else
            {
                IEnumerable<Entity> ents = _dataService.GetEntitiesById(EntityTypeKey, new List<Guid>() { CurrentEntityId });
                Entity entity = ents.FirstOrDefault();
                if (entity == null)
                    return false;

                string codiceAtt = entity.EntityType.EtichetteMap[etichettaAttributo];
                attFormat = attFormatHelper.GetValorePaddedFormat(entity, codiceAtt);
            }



            if (val is ValoreTesto)
            {
                ValoreTesto valTesto = val as ValoreTesto;
                if (valTesto.Result != null)
                    result = valTesto.Result;
                else
                    result = valTesto.V;
            }
            else if (val is ValoreContabilita)
            {
                ValoreContabilita valCont = val as ValoreContabilita;
                if (valCont.RealResult != null)
                    result = valCont.FormatRealResult(attFormat);

            }
            else if (val is ValoreReale)
            {
                ValoreReale valReale = val as ValoreReale;
                if (valReale.RealResult != null)
                    result = valReale.FormatRealResult(attFormat);
            }
            else if (val is ValoreBooleano)
            {
                ValoreBooleano valBool = val as ValoreBooleano;
                if (valBool.V != null && valBool.V.HasValue)
                {
                    if (valBool.V.Value)
                        result = "1";
                    else
                        result = "0";
                }
            }
            else if (val is ValoreData)
            {
                ValoreData valData = val as ValoreData;
                if (valData.V != null && valData.V.HasValue)
                {
                    result = valData.V.Value.Ticks.ToString();
                }
            }
            else if (val is ValoreGuid)
            {
                result = string.Empty;
            }
            else if (val != null)
                result = val.ToPlainText();

            return true;
        }

        protected override string ResultAsString(double res)
        {
            return base.ResultAsString(res);
        }
    }

    

    public class EPCalculatorFunction : ValoreCalculatorFunction
    {
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "ep";


        public EPCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Prezzario;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class CmpCalculatorFunction : ValoreCalculatorFunction
    {
        //public Guid ComputoItemId { get; set; }
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "cmp";
        


        public CmpCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Computo;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }


    }



    public class ElmCalculatorFunction : ValoreCalculatorFunction
    {
        //public Guid ElementiItemId { get; set; }
        //IDataService _dataService;
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "elm";

        public ElmCalculatorFunction(IDataService dataService)
        {
            EntityTypeKey = BuiltInCodes.EntityType.Elementi;
            _dataService = dataService;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class CntCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "cnt";


        public CntCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Contatti;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class InfCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "inf";


        public InfCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.InfoProgetto;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class DivCalculatorFunction : ValoreCalculatorFunction
    {
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "div";

        public DivCalculatorFunction(IDataService dataService)
        {
            //va settata al momento del calcolo per sapere in quale divisione siamo
            //EntityTypeKey = BuiltInCodes.EntityType.Divisione;
            _dataService = dataService;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public void SetEntityType(string entityTypeKey)
        {
            EntityTypeKey = entityTypeKey;
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class EAtCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "eat";


        public EAtCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }



        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class WBSCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "wbs";


        public WBSCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.WBS;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }



        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }
    public class CalendariCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "cal";


        public CalendariCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Calendari;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }



        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class VarCalculatorFunction : ValoreCalculatorFunction
    {

        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "var";


        public VarCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Variabili;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }

    public class CapCalculatorFunction : ValoreCalculatorFunction
    {
        public static string FunctionName { get; } = "att";
        public static string Name { get; } = "cap";


        public CapCalculatorFunction(IDataService dataService)
        {
            _dataService = dataService;
            EntityTypeKey = BuiltInCodes.EntityType.Capitoli;
            _entitiesHelper = new EntitiesHelper(_dataService);
        }

        public override string GetName() { return Name; }

        public override string GetFunctionName() { return FunctionName; }

    }


}
