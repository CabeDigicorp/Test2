
using CommonResources;
using Commons;
using DevExpress.Data.Filtering;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows;

namespace MasterDetailModel
{
    [ProtoContract]
    [ProtoInclude(1001, typeof(ValoreTesto))]
    [ProtoInclude(1002, typeof(ValoreTestoRtf))]
    [ProtoInclude(1003, typeof(ValoreContabilita))]
    [ProtoInclude(1004, typeof(ValoreReale))]
    //[ProtoInclude(1005, typeof(ValoreTestoSuggest))]
    [ProtoInclude(1006, typeof(ValoreCollection))]
    [ProtoInclude(1007, typeof(ValoreData))]
    [ProtoInclude(1008, typeof(ValoreGuid))]
    [ProtoInclude(1009, typeof(ValoreBooleano))]
    [ProtoInclude(1010, typeof(ValoreElenco))]
    [ProtoInclude(1011, typeof(ValoreColore))]
    [ProtoInclude(1012, typeof(ValoreFormatoNumero))]
    //[ProtoInclude(1013, typeof(ValoreVariabile))]
    public interface Valore
    {
        string PlainText { get; }

        /// <summary>
        /// Deep copy
        /// </summary>
        /// <returns></returns>
        Valore Clone();

        /// <summary>
        /// Ritorna true se il valore risultato è uguale a val
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        bool ResultEquals(Valore val);

        /// <summary>
        /// Ritorna true se il valore è uguale a val
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        bool Equals(Valore val);

        /// <summary>
        /// Trova i valori comuni con val. Nel caso di ValoreSingle o val è uguale o diventa [Multi]
        /// </summary>
        /// <param name="val"></param>
        void Intersect(Valore val);

        /// <summary>
        /// Ritorna true se il valore rappresenta più valori ([Multi])
        /// </summary>
        /// <returns></returns>
        bool IsMultiValore(bool checkResult = false);

        /// <summary>
        /// Ritorna true se il valore non è vuoto
        /// </summary>
        /// <returns></returns>
        bool HasValue();

        /// <summary>
        /// Ritorna true se il valore contiene tutte le stringhe (separate da spazio) contenute in text.
        /// Per cercare la stringa tale e quale a text occorre racchiuderla nelle virgolette.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool ContainsTesto(string text);

        /// <summary>
        /// Come ContainsTesto tranne che cerca in Result formattato
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool ResultContainsTesto(string text, string resultPaddedFormat);

        /// <summary>
        /// Ritorna true se il valore è text (nel caso di ValoreSingle) o se il valore contiene text (nel caso di ValoreCollection)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool HasTesto(string text);

        /// <summary>
        /// Ritorna true se il result è text (nel caso di ValoreSingle) o se il valore contiene text (nel caso di ValoreCollection)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true);

        /// <summary>
        /// Confronta il valore con val e ne determina l'ordine. I valori di tipo collection non vengono ordinati
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        bool GreaterThan(Valore val);


        string ToPlainText();

        /// <summary>
        /// Restituisce una lista di ValoreSingle.Se il valore è già single restituisce la lista con un solo elemento clonato.
        /// </summary>
        /// <param name="itemTextIndex"></param>
        /// <returns></returns>
        List<ValoreSingle> ToValoreSingleSet();

        /// <summary>
        /// Numero di Text all'interno di un item di ValoreCollection. Esempio attributo e-mail ha 2 text per ogni item: Etichetta e e-mail. Per ValoreSingle restituisce 0;
        /// </summary>
        /// <returns></returns>
        int ItemTextCount();

        /// <summary>
        /// Cerca di settare il valore da un stringa
        /// </summary>
        /// <param name="str"></param>
        bool SetByString(string str);

        /// <summary>
        /// Sostituisce oldTxt in newTxt all'interno del testo del valore (vale solo per ValoreTesto, ValoreTestoSuggest e ValoreTestoRtf)
        /// </summary>
        /// <param name="oldTxt"></param>
        /// <param name="newTxt"></param>
        void ReplaceInText(string oldTxt, string newTxt);

        /// <summary>
        /// Aggiorna il valore il tutti i suoi campi a partire da val
        /// </summary>
        void Update(Valore val);

        /// <summary>
        /// Verifica il soddisfacimento del filtro
        /// </summary>
        /// <param name="valoreFilter"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditions valoreFilter, string format);

        /// <summary>
        /// Ritorna la formula nei valori testo, reale, contabilita
        /// </summary>
        /// <returns></returns>
        string GetFormula();

    }


    public interface ValoreSingle : Valore
    {
    }

    /// <summary>
    /// Valore che contiene un testo
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreTesto : ValoreSingle
    {
        string _V = string.Empty;
        [ProtoMember(1)]
        [DataMember]
        public string V
        {
            get => _V;
            set
            {
                _V = value;
                _result = null;
                _isResultMulti = false;
            }
        }
        public string PlainText { get => ToPlainText(); }

        string _result = null;
        [ProtoMember(2)]
        [DataMember]
        public string Result
        {
            get => _result != null ? _result : V;
            set { _result = value; }
        }

        bool _isResultMulti = false;

        public Valore Clone() { return new ValoreTesto() { V = this.V, Result = this.Result, _isResultMulti = this._isResultMulti }; }
        public bool ResultEquals(Valore val)
        {
            if (val == null && Result == null)
                return true;

            if ((val is ValoreTesto) && (val as ValoreTesto).Result == Result)
                return true;

            return false;
        }
        public bool Equals(Valore val)
        {
            if ((val is ValoreTesto) && (val as ValoreTesto).V == V)
                return true;
            else
                return false;
        }
        public void Intersect(Valore val)
        {
            if (!Equals(val))
                V = "[Multi]";

            if (!ResultEquals(val))
            {
                Result = null;
                _isResultMulti = true;
            }
        }
        public bool IsMultiValore(bool checkResult = false)
        {
            if (checkResult)
            {
                if (_isResultMulti)
                    return true;
                return false;
            }
            else
            {
                if (V != null && V.Trim() == "[Multi]")
                    return true;
                return false;
            }
        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(V, text))
                return true;

            return false;
        }
        public bool HasTesto(string text) { if (string.Equals(V, text, StringComparison.CurrentCultureIgnoreCase)) return true; else return false; }
        public bool ResultHasTesto(string text, string resultOPaddedFormat, bool ignoreCase = true)
        {
            if (ignoreCase)
            {
                if (string.Equals(Result, text, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else
                    return false;
            }
            else
            {
                if (string.Equals(Result, text, StringComparison.CurrentCulture))
                    return true;
                else
                    return false;
            }
        }
        public bool GreaterThan(Valore val)
        {
            if ((Result != null) && (val is ValoreTesto) && (val as ValoreTesto).Result != null && (val as ValoreTesto).Result.CompareTo(Result) < 0)
                return true;
            else return false;
        }
        public string ToPlainText()
        {
            return Result != null ? Result : V; 
            //return V != null ? V : string.Empty;
        }
        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
            //return 2;
        }

        public bool SetByString(string str)
        {
            str = str.Replace(ValoreHelper.ItselfResult, Result.ToString());

            str = str.Replace(ValoreHelper.ItselfFormula, V);

            V = str;
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
        
            V = Regex.Replace(V, Regex.Escape(oldTxt), newTxt, RegexOptions.IgnoreCase);

            ////Extension method per OrdinalIgnoreCase
            //V = V.Replace(oldTxt, newTxt, StringComparison.OrdinalIgnoreCase);
        }

        public void Update(Valore val)
        {
            ValoreTesto val1 = val as ValoreTesto;
            if (val1 == null)
                return;

            _V = val1.V;
            _result = val1._result;//no Result
        }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            if (text == null || text == "")
                return true;

            if (Result != null && ValoreHelper.ContainsTesto(Result, text))
                return true;

            return false;
        }

        public bool CheckFilterConditions(ValoreConditions filterConditions, string format)
        {
            return CheckFilterConditions(filterConditions.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {

                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreTesto singleVal = single.Valore as ValoreTesto;
                    if (singleVal == null)
                        return false;

                    string singleTesto = singleVal.V;
                    string testo = Result;

                    res = CheckFilterConditions(singleTesto, testo, single.Condition);
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }


        bool CheckFilterConditions(string compTesto, string testo, ValoreConditionEnum condition)
        {
            
            if (compTesto != null && testo != null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                        return testo.StartsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.EndsWith:
                        return testo.EndsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Contains:
                        return testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.NotContains:
                        return !testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Equal:
                        return testo.Equals(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Unequal:
                        return !testo.Equals(compTesto, StringComparison.CurrentCultureIgnoreCase);

                }
            }
            else if (compTesto == null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                    case ValoreConditionEnum.EndsWith:
                    case ValoreConditionEnum.Contains:
                        return true;
                    case ValoreConditionEnum.NotContains:
                        return false;
                    case ValoreConditionEnum.Equal:
                        return testo == null;
                }
            }

            return false;
        }

        public string GetFormula()
        {
            return V;
        }

    }

    /// <summary>
    /// Valore che contiene una data.
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreData : ValoreSingle
    {
        DateTime? _V =  null;
        [ProtoMember(1)]
        [DataMember]
        public DateTime? V
        {
            get => _V;
            set
            {
                _V = value;
                _isMultiValore = false;
            }
        }
        public string PlainText { get => ToPlainText(); }

        bool _isMultiValore = false;


        public Valore Clone() { return new ValoreData() { V = this.V, _isMultiValore = this._isMultiValore }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreData) && (val as ValoreData).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val)
        {
            if (!ResultEquals(val))
            {
                //V = null;
                _isMultiValore = true;
            }
            else
                _isMultiValore = false;
        }
        public bool IsMultiValore(bool checkResult = false) { if (_isMultiValore) return true; return false; }
        public bool ContainsTesto(string text) { if (text == null || text == "" || (V != null && V.Value.ToString().Contains(text))) return true; else return false; }
        public bool HasTesto(string text) { if (ToPlainText() == text) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        public bool GreaterThan(Valore val) { if ((val is ValoreData) && (val as ValoreData).V < V) return true; else return false; }
        public string ToPlainText()
        {
            if (V != null)
            {
                string temp = V.Value.ToShortDateString();
                return temp;
            }
            return "";
        }
        public bool HasValue() { if (V != null && V.HasValue) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            var dateTime = DateTime.Parse(str);
            V = dateTime;
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            throw new NotImplementedException();
        }

        public void Update(Valore val)
        {
            ValoreData val1 = val as ValoreData;
            if (val1 == null)
                return;

            bool isMultiValore = val1._isMultiValore;
            V = val1.V;
            _isMultiValore = isMultiValore;
        }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions filterConditions, string format)
        {
            return CheckFilterConditions(filterConditions.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {
                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreData singleVal = single.Valore as ValoreData;
                    if (singleVal == null)
                        return false;

                    DateTime? singleData = singleVal.V;
                    DateTime? roundSingleData = singleData;
                    if (singleData.HasValue)
                    {
                        roundSingleData = DateTime.Parse(singleData.Value.ToString(format));
                    }


                    DateTime? data = V;
                    DateTime? roundData = data;
                    if (data.HasValue)
                    {
                        roundData = DateTime.Parse(data.Value.ToString(format));
                    }
                    
                    res = CheckFilterConditions(singleData, roundData, single.Condition);
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }


        bool CheckFilterConditions(DateTime? singleData, DateTime? reale, ValoreConditionEnum condition)
        {
            if (singleData.HasValue && reale.HasValue)
            {


                switch (condition)
                {
                    case ValoreConditionEnum.Equal:
                        return reale == singleData;
                    case ValoreConditionEnum.GreaterOrEqualThan:
                        return reale >= singleData;
                    case ValoreConditionEnum.GreaterThan:
                        return reale > singleData;
                    case ValoreConditionEnum.LessOrEqualThan:
                        return reale <= singleData;
                    case ValoreConditionEnum.LessThan:
                        return reale < singleData;
                    case ValoreConditionEnum.Unequal:
                        return reale != singleData;
                }
            }
            else
            {
                if (singleData.HasValue || reale.HasValue) 
                {
                    switch (condition)
                    {
                        case ValoreConditionEnum.Equal:
                        case ValoreConditionEnum.GreaterOrEqualThan:
                        case ValoreConditionEnum.LessOrEqualThan:
                        case ValoreConditionEnum.GreaterThan:
                        case ValoreConditionEnum.LessThan:
                            return false;
                        case ValoreConditionEnum.Unequal:
                            return true;
                    }
                }
                else //null, null
                {
                    switch (condition)
                    {
                        case ValoreConditionEnum.Equal:
                        case ValoreConditionEnum.GreaterOrEqualThan:
                        case ValoreConditionEnum.LessOrEqualThan:
                            return true;
                        case ValoreConditionEnum.GreaterThan:
                        case ValoreConditionEnum.LessThan:
                        case ValoreConditionEnum.Unequal:
                            return false;
                    }
                }
            }
            return false;
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Valore che contiene una collezione di item. Ogni item può contenere fino a 3 testi.
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreCollection : Valore
    {
        [ProtoMember(1)]
        [DataMember]
        public List<ValoreCollectionItem> V
        {
            get;
            set;
        } = new List<ValoreCollectionItem>();

        protected string EmptyText = string.Empty;

        public ValoreCollection()
        {
            EmptyText = LocalizationProvider.GetString("_Nessuno");
        }

        public virtual List<ValoreCollectionItem> Items { get => V; }
         

        public string PlainText { get => ToPlainText(); }

        public bool HasValue() { if (Items != null && Items.Any()) return true; else return false; }

        public virtual bool Has(ValoreCollectionItem val)
        {
            foreach (ValoreCollectionItem item in Items)
            {
                if (!item.Removed && item.Equals1(val))
                {
                    item.Id = val.Id;//se il testo contenuto è lo stesso setto anche id uguale
                    return true;
                }
            }
            return false;
        }

        public bool Add(ValoreCollectionItem val)
        {
            if (Items == null)
                return false;

            if (!Has(val))
            {
                Items.Add(val);
                return true;
            }
            return false;
        }

        public virtual void Intersect(Valore val)
        {
            if (val == null)
                return;

            if (val.GetType() != this.GetType())
                return;

            if (ResultEquals(val))
                _isMultiValore = false;
            else
            {
                _isMultiValore = true;

                if (!val.HasValue())
                {
                    Items.Clear();
                    return;
                }

                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (!(val as ValoreCollection).Has(Items[i]))
                    {
                        Items.RemoveAt(i);
                    }
                }
            }
        }

        public void Clear()
        {
            if (Items == null)
                return;

            Items.Clear();
        }

        public bool ContainsTesto(string text)
        {
            if (Items == null)
                return false;

            if (text == null || !text.Any())
                return true;

            foreach (Valore item in Items)
            {
                if (item.ContainsTesto(text))
                    return true;

                //if (item.Testo1.Contains(text))
                //    return true;

                //if (item.Testo2.Contains(text))
                //    return true;

                //if (item.Testo3.Contains(text))
                //    return true;
            }
            return false;
        }

        public virtual bool HasTesto(string text)
        {

            foreach (Valore item in Items)
            {
                if (item.HasTesto(text))
                    return true;
            }
            return false;
        }




        public bool ResultEquals(Valore val)
        {
            if (val == null)
                return false;

            if (val.GetType() != this.GetType())
                return false;

            List<ValoreCollectionItem> sorted1 = Items.Where(item => !item.Removed).OrderBy(item => item.Id).ToList();
            List<ValoreCollectionItem> sorted2 = (val as ValoreCollection).Items.Where(item => !item.Removed).OrderBy(item => item.Id).ToList();

            if (sorted1.Count != sorted2.Count)
                return false;

            for (int i = 0; i < sorted1.Count; i++)
                if (!sorted1[i].Equals(sorted2[i]))
                    return false;


            return true;
        }
        public virtual bool Equals(Valore val) { return ResultEquals(val); }

        public bool GreaterThan(Valore val)
        {
            return true;
        }


        bool _isMultiValore = false;
        public bool IsMultiValore(bool checkResult = false)
        {
            return _isMultiValore;
        }

        public void Remove(Guid id)
        {
            if (IsMultiValore(false))
                return;

            ValoreCollectionItem valItem = Items.Where(item => item.Id == id).FirstOrDefault();
            if (valItem != null)
            {
                valItem.Removed = true;//da eliminare
                Items.Remove(valItem);//Add by Ale 01/02/2023
            }
        }

        public string ToPlainText()
        {
            return typeof(ValoreCollection).ToString();
        }

        public List<ValoreSingle> ToValoreSingleSet()
        {

            List<ValoreSingle> l = new List<ValoreSingle>();

            if (Items == null)
                return l;

            //if (!V.Any())
            //    l.Add(new ValoreTesto() { V = string.Empty });

            if (!Items.Any())
                l.Add(new ValoreTesto() { V = EmptyText });


            foreach (Valore item in Items)
            {
                l.AddRange(item.ToValoreSingleSet());

                //if (itemTextIndex == 0)
                //l.Add(new ValoreTesto() { V = (item.Testo1) });
                //else if (itemTextIndex == 1)
                //l.Add(new ValoreTesto() { V = (item.Testo2) });
                //else if (itemTextIndex == 2)
                //l.Add(new ValoreTesto() { V = (item.Testo3) });
            }
            return l;
        }

        public void Replace(Guid id, ValoreCollectionItem valItem)
        {
            ValoreCollectionItem itemToReplace = Items.Where(item => item.Id == id).FirstOrDefault();
            if (itemToReplace != null)
            {
                itemToReplace.Replace(valItem);

                //itemToReplace.Testo1 = valItem.Testo1;
                //itemToReplace.Testo2 = valItem.Testo2;
                //itemToReplace.Testo3 = valItem.Testo3;
            }
        }

        virtual public int ItemTextCount()
        {
            return 2;
        }

        public bool SetByString(string str)
        {
            return false;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            throw new NotImplementedException();
        }

        public void Update(Valore val) { }

        public virtual Valore Clone() { return null; }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            throw new NotImplementedException();
        }

        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true)
        {
            return HasTesto(text);
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    [ProtoContract]
    [DataContract]
    public class ValoreTestoCollection : ValoreCollection
    {
        public override Valore Clone()
        {
            if (V != null)
                return new ValoreTestoCollection() { V = new List<ValoreCollectionItem>(this.V) };
            else
                return new ValoreTestoCollection();
        }

        public override bool HasTesto(string text)
        {
            Items.RemoveAll(item => item.Removed);

            if (Items == null || !Items.Any())
            {
                if (text == EmptyText)
                    return true;

                return false;
            }

            return base.HasTesto(text);
        }
    }

    [ProtoContract]
    [DataContract]
    public class ValoreGuidCollection : ValoreCollection
    {
        //[ProtoMember(1)]
        //[DataMember]
        //string EntityTypeCodice { get; set; }

        [DataMember]
        [ProtoMember(2)]
        public FilterData Filter { get; set; } = new FilterData();

        /// <summary>
        /// non deve essere salvato
        /// </summary>
        [DataMember]
        public List<ValoreCollectionItem> FilterResult { get; set; } = null;

        public override List<ValoreCollectionItem> Items 
        {

            get
            {
                //NB: V viene salvato solo nel caso nel caso ValoreAttributoGuidCollection.ItemsSelectionTypeEnum = ItemsSelectionTypeEnum.ByHand
                //atrimenti V viene calcolato runtime e non viene salvato

                if (FilterResult != null)
                {
                    return FilterResult;
                }
                else
                {
                    return base.Items;
                }
                //return base.Items;
            }
        }

        public override Valore Clone()
        {
            if (V != null)
            {
                FilterData filter = Filter;
                List<ValoreCollectionItem> filterResult = FilterResult;
                return new ValoreGuidCollection() { V = new List<ValoreCollectionItem>(V), Filter = filter.Clone(), FilterResult = filterResult?.ToList() };
            }
            else
                return new ValoreGuidCollection();
        }

        public override bool HasTesto(string text)
        {
            Items.RemoveAll(item => item.Removed);

            if (Items == null || !Items.Any())
            {
                if (text == Guid.Empty.ToString())
                    return true;

                return false;
            }

            return base.HasTesto(text);
        }

        public List<Guid> GetEntitiesId()
        {
            Items.RemoveAll(item => item.Removed);

            return Items.Select(item => (item as ValoreGuidCollectionItem).EntityId).ToList();
        }

        public override bool Equals(Valore val)
        {
            ValoreGuidCollection valGuidColl = val as ValoreGuidCollection;
            if (valGuidColl == null)
                return false;

            bool isFilterEqual = valGuidColl.Filter.Equals1(Filter);
            bool isResultEqual = ResultEquals(val);

            return isFilterEqual && isResultEqual;
        }

        public void RemoveEntitiesId(HashSet<Guid> entsId)
        {
            int vCount = Items.Count;
            for (int i = vCount - 1; i>= 0; i--)
            {
                ValoreGuidCollectionItem item = Items[i] as ValoreGuidCollectionItem;
                if (item != null)
                {
                    if (entsId.Contains(item.EntityId))
                        Items.RemoveAt(i);
                }
            }
        }

    }


    /// <summary>
    /// Valore che contiene un testo Rtf
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreTestoRtf : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        string _V = "";
        public string V
        {
            get { return _V; }
            set
            {
                _V = value;
                _plainText = null;
            }
        }

        string _plainText = null;
        public string PlainText
        {
            get
            {
                if (_plainText == null)
                    ValoreHelper.RtfToPlainString(_V, out _plainText);
                return _plainText;
            }
        }

        public ValoreTestoRtf()
        {
            string rtf = null;
            ValoreHelper.RtfFromPlainString("", out rtf);
            V = rtf;
        }

        public Valore Clone() { return new ValoreTestoRtf() { V = this.V, _plainText = this._plainText }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreTestoRtf) && (val as ValoreTestoRtf).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val)
        {
            string v;
            if (!ResultEquals(val))
            {
                ValoreHelper.RtfFromPlainString("[Multi]", out v);
                V = v;
            }
        }
        public bool IsMultiValore(bool checkResult = false)
        {
            string plainText = ToPlainText();
            if (plainText.Trim() == "[Multi]") return true; return false;
        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null)
            {
                //string plainText = ToPlainString();
                //if (ValoreHelpers.ContainsTesto(plainText, text))
                //    return true;
                if (ValoreHelper.ContainsTesto(PlainText, text))
                    return true;

            }
            return false;
        }
        public bool HasTesto(string text) { if (V == text) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        public bool GreaterThan(Valore val)
        {
            if ((V != null) && (val is ValoreTestoRtf) && (val as ValoreTestoRtf).V != null && (val as ValoreTestoRtf).V.CompareTo(V) < 0)
                return true;
            else return false;
        }

        public string ToPlainText()
        {
            return PlainText == null ? "" : PlainText;
        }
        public bool HasValue()
        {
            if (V != null && V.Any())
            {
                if (PlainText.Any())
                    return true;
            }

            return false;
        }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            string rtf = null;
            ValoreHelper.RtfFromPlainString(str, out rtf);
            V = rtf;
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            string rtf = V;

            ValoreHelper.ReplaceInRtfText(ref rtf, oldTxt, newTxt);
            V = rtf;
        }

        public void Update(Valore val)
        {
            ValoreTestoRtf val1 = val as ValoreTestoRtf;
            if (val1 == null)
                return;
            V = val1.V;
        }

        public string BriefRtf
        {
            get
            {
                string briefRtf = null;



                ValoreHelper.FormattedTextHelper.GetBriefRtf(_V, out briefRtf);


                return briefRtf;
            }
        }
        public string BriefPlainText
        {
            get
            {

                StringReader strReader = new StringReader(PlainText);
                string line = strReader.ReadLine();
                return (!string.IsNullOrEmpty(line))? line : string.Empty;


//                //List<string> rtfSplitted = ValoreHelper.RtfSplit(V);

//                string result = "";
//                //foreach (string rtf in rtfSplitted)
//                //{
//                    //string plainText = null;
//                    //ValoreHelper.RtfToPlainString(V, out plainText);

//                    StringReader strReader = new StringReader(PlainText);
//                    string line;
//                    while ((line = strReader.ReadLine()) != null)
//                    {
//                        // Do something with the line
//                        if (result.Any())
//                            result += "\\";

//                        result += line;
//                    }
//                //}
//                return result;

            }
        }

        public bool IsEmpty()
        {
            return ValoreHelper.RtfIsEmpty(V);
        }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            throw new NotImplementedException();
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Valore che contiene una valuta
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreReale : ValoreSingle
    {

        string _V = "";
        [ProtoMember(1)]
        [DataMember]
        public string V
        {
            get { return _V; }
            set
            {
                _V = value;
                _realResult = null;
                _isRealResultMulti = false;
            }
        }

        double? _realResult = null;
        [ProtoMember(2)]
        [DataMember]
        public double? RealResult
        {
            get => _realResult;
            set { _realResult = value; }
        }

        [ProtoMember(3)]
        [DataMember]
        public string Format { get; set; } = null;

        [ProtoMember(4)]
        [DataMember]
        public string ResultDescription { get; set; }

        public string ResultDescriptionFormula
        {
            get
            {
                string res = string.Empty;
                if (ResultDescription != null)
                    res = ResultDescription.Split('\n').FirstOrDefault();

                return res;
            }
        }

        bool _isRealResultMulti = false;
        bool? _significantDigitsCount = null;

        public ValoreReale()
        {
        }


        public string PlainText { get => ToPlainText(); }

        //Methods
        public Valore Clone() { return new ValoreReale() { V = this.V, _realResult = this._realResult, ResultDescription = this.ResultDescription, _isRealResultMulti = this._isRealResultMulti}; }
        public bool ResultEquals(Valore val)
        {
            if (_realResult == null)
                return true;

            if (double.IsNaN(_realResult.Value))
                return true;

            if (double.IsInfinity(_realResult.Value))
                return true;

            if ((val is ValoreReale) && (val as ValoreReale).RealResult == RealResult)
                return true;

            return false;
        }

        public bool Equals(Valore val) { if ((val is ValoreReale) && (val as ValoreReale).V == V) return true; else return false; }

        public void Intersect(Valore val)
        {

            if (!Equals(val))
            {
                V = "[Multi]";
            }

            if (!ResultEquals(val))
            {
                RealResult = null;
                _isRealResultMulti = true;
            }
        }
        public bool IsMultiValore(bool checkResult = false)
        {
            if (checkResult)
            {
                if (_isRealResultMulti)
                    return true;
                return false;
            }
            else
            {
                if (V != null && V.Trim() == "[Multi]")
                    return true;
                return false;
            }
        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(ToPlainText(), text))
                return true;

            return false;
        }
        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            string resultStr = FormatRealResult(resultPaddedFormat);

            if (resultStr != null && ValoreHelper.ContainsTesto(resultStr, text))
                return true;

            return false;
        }
        public bool HasTesto(string text)
        {
            if (text == LocalizationProvider.GetString(ValoreHelper.ZeroRealResult) && RealResult == 0)
                return true;

            if (ToPlainText() == text)
                return true;
            else
                return false;

        }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true)
        {
            string resultStr = FormatRealResult(resultPaddedFormat);

            if (text == LocalizationProvider.GetString(ValoreHelper.ZeroRealResult) && RealResult == 0)
                return true;

            if (resultStr == text)
                return true;
            else
                return false;

        }
        public bool GreaterThan(Valore val)
        {
            if (!Validate(out _))
                return false;

            double? realResult = RealResult;
            if ((realResult != null) && (val is ValoreReale) && (val as ValoreReale).RealResult != null && (val as ValoreReale).RealResult < realResult)
                return true;
            else
                return false;
        }
        public string FormatRealResult(string paddedFormat)
        {
            if (paddedFormat == null || !paddedFormat.Any())
                return "";

            if (V == null || V == "")
                return "";

            if (IsMultiValore(true))
                return LocalizationProvider.GetString(ValoreHelper.Multi);

            try
            {
                //string fullFormat = string.Format("{0}0:{1}{2}", "{", format, "}");
                string str = string.Format(paddedFormat, _realResult);
                return str.Trim();
            }
            catch (FormatException)
            {
                return string.Empty;
            }
            return string.Empty;
        }
        public string ToPlainText()
        {
            return V != null ? V : "";
        }
        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);

            //if (itemTextIndex == 0 || itemTextIndex == -1)
            //{
            //    l.Add(this.Clone() as ValoreSingle);
            //}

            //if (itemTextIndex == 1 || itemTextIndex == -1)
            //{
            //    l.Add(new ValoreTesto() { V = RealResult.ToString() });
            //}
            return l;
        }

        public int ItemTextCount()
        {
            //return 2;
            return 0;
        }

        public bool SetByString(string str)
        {
            str = str.Replace(ValoreHelper.ItselfResult, RealResult.ToString());

            str = str.Replace(ValoreHelper.ItselfFormula, V);

            V = str;
            return true;
        }

        //static public bool TryParse(string str, NumberStyles style, CultureInfo cultureInfo, out double dec)
        //{
        //    bool res = Double.TryParse(str, style, cultureInfo, out dec);
        //    return res;
        //}

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            if (newTxt == null)
            {
                newTxt = RealResult.ToString();
            }

            V = Regex.Replace(V, Regex.Escape(oldTxt), newTxt, RegexOptions.IgnoreCase);
        }


        public void Update(Valore val)
        {
            ValoreReale val1 = val as ValoreReale;
            if (val1 == null)
                return;

            _V = val1.V;
            _realResult = val1.RealResult;
            ResultDescription = val1.ResultDescription;
            Format = val1.Format;
            
        }

        /// <summary>
        /// Formula senza la valuta e spazi
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string RemoveSymbols(string str, string format)
        {
            string format2 = Regex.Replace(format, "0|#|,|:|;|\\.|{|}|%|‰|\\\\", "");
            char[] chars = format2.ToCharArray();
            string format3 = string.Join("|", chars);
            string v1 = Regex.Replace(str, format3, "");
            return v1;
        }



        public bool CheckFilterConditions(ValoreConditions filterConditions, string format)
        {
            return CheckFilterConditions(filterConditions.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {
                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreReale singleVal = single.Valore as ValoreReale;
                    if (singleVal == null)
                        return false;

                    double singleReale = 0.0;
                    if (!Double.TryParse(singleVal.V, out singleReale))
                        return false;

                    double reale = 0.0;
                    if (!Validate(out reale))
                        return false;

                    double roundReale = reale;
                    NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);

                    if (nf.SymbolText.Trim() == "%")
                        roundReale = reale * 100;
                    else if (nf.SymbolText.Trim() == "‰")
                        roundReale = reale * 1000;

                    roundReale = Math.Round(roundReale, nf.DecimalDigitCount);


                    res = CheckFilterConditions(singleReale, roundReale, single.Condition);
                }  

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }

        bool Validate(out double reale)
        {
            reale = 0.0;

            if (_realResult == null)
                return false;

            if (double.IsNaN(_realResult.Value))
                return false;

            if (double.IsInfinity(_realResult.Value))
                return false;

            reale = _realResult.Value;
            return true;
        }

        bool CheckFilterConditions(double singleReale, double reale, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return reale == singleReale;
                case ValoreConditionEnum.GreaterOrEqualThan:
                    return reale >= singleReale;
                case ValoreConditionEnum.GreaterThan:
                    return reale > singleReale;
                case ValoreConditionEnum.LessOrEqualThan:
                    return reale <= singleReale;
                case ValoreConditionEnum.LessThan:
                    return reale < singleReale;
                case ValoreConditionEnum.Unequal:
                    return reale != singleReale;
            }
            return false;
        }

        public string GetFormula()
        {
            return V;
        }
    }

    /// <summary>
    /// Valore che contiene una valuta
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreContabilita : ValoreSingle
    {

        string _V = ""; //Formula
        [ProtoMember(1)]
        [DataMember]
        public string V
        {
            get { return _V; }
            set
            {
                _V = value;
                _realResult = null;
                _isRealResultMulti = false;
            }
        }

        decimal? _realResult = null;
        [ProtoMember(2)]
        [DataMember]
        public decimal? RealResult
        {
            get => _realResult;
            set { _realResult = value; }
        }

        [ProtoMember(3)]
        [DataMember]
        public string Format { get; set; } = null;

        [ProtoMember(4)]
        [DataMember]
        public string ResultDescription { get; set; }

        public string ResultDescriptionFormula
        {
            get
            {
                string res = string.Empty;
                if (ResultDescription != null)
                    res = ResultDescription.Split('\n').FirstOrDefault();

                return res;
            }
        }

        bool _isRealResultMulti = false;


        //[DataMember]
        //public CultureInfo cultureInfo = CultureInfo.CurrentUICulture;

        public ValoreContabilita()
        {
            //ZeroRealResult = LocalizationProvider.GetString("_ZeroRealResult");
        }

        public string PlainText { get => ToPlainText(); }

        //Methods
        public Valore Clone() { return new ValoreContabilita() { V = this.V, RealResult = this.RealResult, ResultDescription = this.ResultDescription, _isRealResultMulti = this._isRealResultMulti }; }
        public bool ResultEquals(Valore val)
        {
            if (val == null && RealResult == null)
                return true;

            //if ((val is ValoreContabilita) && (val as ValoreContabilita).V == V) return true; else return false;
            if ((val is ValoreContabilita) && (val as ValoreContabilita).RealResult == RealResult)
                return true;

            return false;
        }
               
        public bool Equals(Valore val) { if ((val is ValoreContabilita) && (val as ValoreContabilita).V == V) return true; else return false; }

        public void Intersect(Valore val)
        {
            if (!Equals(val))
            {
                V = "[Multi]";
            }

            if (!ResultEquals(val))
            {
                RealResult = null;
                _isRealResultMulti = true;
            }

        }
        //bool _isMultiValore = false;
        public bool IsMultiValore(bool checkResult = false)
        {
            if (checkResult)
            {
                if (_isRealResultMulti)
                    return true;
                return false;
            }
            else
            {
                if (V != null && V.Trim() == "[Multi]")
                    return true;
                return false;
            }


        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(ToPlainText(), text))
                return true;

            return false;
        }
        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            string resultStr = FormatRealResult(resultPaddedFormat);

            if (resultStr != null && ValoreHelper.ContainsTesto(resultStr, text))
                return true;

            return false;
        }
        public bool HasTesto(string text)
        {
            if (text == LocalizationProvider.GetString(ValoreHelper.ZeroRealResult) && RealResult == 0)
                return true;

            if (ToPlainText() == text)
                return true;
            else
                return false;
        }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true)
        {
            string resultStr = FormatRealResult(resultPaddedFormat);

            if (text == LocalizationProvider.GetString(ValoreHelper.ZeroRealResult) && RealResult == 0)
                return true;

            if (resultStr == text)
                return true;
            else
                return false;

        }
        public bool GreaterThan(Valore val)
        {
            decimal? realResult = RealResult;
            if ((realResult != null) && (val is ValoreContabilita) && (val as ValoreContabilita).RealResult != null && (val as ValoreContabilita).RealResult < realResult)
                return true;
            else return false;
        }
        public string ToPlainText()
        {
            return V != null ? V : "";
        }

        public string FormatRealResult(string paddedFormat)
        {
            if (paddedFormat == null || !paddedFormat.Any())
                return String.Empty;

            if (V == null || V == "")
                return String.Empty;

            if (IsMultiValore(true))
                return LocalizationProvider.GetString(ValoreHelper.Multi);

            if (_realResult == null)
                return BuiltInCodes.Others.NaN;

            string str = string.Format(paddedFormat, _realResult);
            return str.Trim();
        }
        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            //List<ValoreSingle> l = new List<ValoreSingle>();

            //if (RealResult == 0)
            //    l.Add(new ValoreTesto() { V = ZeroRealResult });

            //l.Add(this.Clone() as ValoreSingle);
            //return l;

            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            //return 2;
            return 0;
        }

        public bool SetByString(string str)
        {
            str = str.Replace(ValoreHelper.ItselfResult, RealResult.ToString());

            str = str.Replace(ValoreHelper.ItselfFormula, V);

            V = str;
            return true;

        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            if (newTxt == null)
            {
                newTxt = RealResult.ToString();
            }

            V = Regex.Replace(V, Regex.Escape(oldTxt), newTxt, RegexOptions.IgnoreCase);
        }

        //public bool IsFormulaEqualResult(string format)
        //{
        //    return false;
        //}


        /// <summary>
        /// Formula senza la valuta e spazi
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string RemoveSymbols(string str, string format)
        {
            string format2 = Regex.Replace(format, "0|#|,|:|;|\\.|{|}|%|‰|\\\\", "");
            char[] chars = format2.ToCharArray();
            string format3 = string.Join("|", chars);
            string v1 = Regex.Replace(str, format3, "");
            return v1;
        }

        public void Update(Valore val)
        {
            ValoreContabilita val1 = val as ValoreContabilita;
            if (val1 == null)
                return;

            
            _V = val1.V;
            _realResult = val1.RealResult;
            ResultDescription = val1.ResultDescription;
            Format = val1.Format;
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            return CheckFilterConditions(valoreFilter.MainGroup, format);
        }


        public string GetFormula()
        {
            return V;
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {
                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreContabilita singleVal = single.Valore as ValoreContabilita;
                    if (singleVal == null)
                        return false;

                    decimal singleDec = 0;
                    if (!Decimal.TryParse(singleVal.V, out singleDec))
                        return false;

                    decimal dec = 0;
                    if (!Validate(out dec))
                        return false;

                    decimal roundDec = dec;
                    NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);
                    if (nf.SymbolText.Trim() == "%")
                        roundDec = dec * 100;
                    else if (nf.SymbolText.Trim() == "‰")
                        roundDec = dec * 1000;

                    roundDec = Math.Round(roundDec, nf.DecimalDigitCount);

                    res = CheckFilterConditions(singleDec, dec, single.Condition);
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }

        bool Validate(out decimal dec)
        {
            dec = 0;

            if (_realResult == null)
                return false;

            dec = _realResult.Value;
            return true;
        }

        bool CheckFilterConditions(decimal singleDec, decimal dec, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    return dec == singleDec;
                case ValoreConditionEnum.GreaterOrEqualThan:
                    return dec >= singleDec;
                case ValoreConditionEnum.GreaterThan:
                    return dec > singleDec;
                case ValoreConditionEnum.LessOrEqualThan:
                    return dec <= singleDec;
                case ValoreConditionEnum.LessThan:
                    return dec < singleDec;
                case ValoreConditionEnum.Unequal:
                    return dec != singleDec;
            }
            return false;
        }
    }

    [ProtoContract]
    [DataContract]
    public class ValoreGuid : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        public Guid V { get; set; } = Guid.Empty;

        public string PlainText { get => ToPlainText(); }

        //Methods
        public Valore Clone() { return new ValoreGuid() { V = this.V }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreGuid) && (val as ValoreGuid).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val) { if (!ResultEquals(val)) V = Guid.Empty; }
        public bool IsMultiValore(bool checkResult = false) { if (V == null) return true; return false; }
        public bool ContainsTesto(string text)
        {
            Guid guidRes;
            if (Guid.TryParse(text, out guidRes))
            {
                if (V == guidRes)
                    return true;
            }
            return false;
        }

        public bool HasTesto(string text) { if (ToPlainText() == text) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        //public bool HasTesto(string text)
        //{
        //    string str = V.ToString();
        //    if (str == text)
        //        return true;

        //    return false;
        //}
        public bool GreaterThan(Valore val) { return false; }
        public string ToPlainText() { return V.ToString(); }
        public bool HasValue() { return V != Guid.Empty; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            //return false;
            V = new Guid(str);
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
        }

        public void Update(Valore val) { }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            return CheckFilterConditions(valoreFilter.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {

                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    string singleGuid = null;
                    //if (single.Valore == null)
                    //    singleGuid = ValoreHelper.ValoreNullAsText;
                    //else
                    //    singleGuid = single.Valore.PlainText;

                    if (single.Valore == null || single.Valore.ToPlainText() == ValoreHelper.ValoreNullAsText)
                        singleGuid = Guid.Empty.ToString();
                    else
                        singleGuid = single.Valore.PlainText;

                    string guid = V.ToString();

                    res = CheckFilterConditions(singleGuid, guid, single.Condition);

                    //if (singleGuid == ValoreHelper.ValoreNullAsText && V == Guid.Empty)
                    //    return true;
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }

        bool CheckFilterConditions(string singleGuid, string guid, ValoreConditionEnum condition)
        {
            switch (condition)
            {
                case ValoreConditionEnum.Equal:
                    {
                        return guid == singleGuid;
                    }
                case ValoreConditionEnum.Unequal:
                    {
                        return guid != singleGuid;
                    }
            }
            return false;
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }
    /// <summary>
    ///   /// Valore che contiene un booleano
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreBooleano : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        public bool? V { get; set; } = false;

        public string PlainText { get => ToPlainText(); }

        public Valore Clone() { return new ValoreBooleano() { V = this.V }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreBooleano) && (val as ValoreBooleano).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val) { if (!ResultEquals(val)) V = null; }
        public bool IsMultiValore(bool checkResult = false) { if (V == null) return true; return false; }
        public bool ContainsTesto(string text) { if (text == null || text == "" || ToPlainText() == text) return true; else return false; }
        public bool HasTesto(string text) { if (ToPlainText() == text) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        public bool GreaterThan(Valore val) { if ((val is ValoreBooleano) && (val as ValoreBooleano).V == true && V == false) return true; else return false; }
        public string ToPlainText()
        {
            if (V != null)
            {
                if (V == true)
                    return "true";
                else if (V == false)
                    return "false";
            }
            return "";
        }
        public bool HasValue() { if (V != null && V.HasValue) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            if (str == "true")
                V = true;
            else if (str == "false")
                V = false;

            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            throw new NotImplementedException();
        }

        public void Update(Valore val) { }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            return CheckFilterConditions(valoreFilter.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {

                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreBooleano singleVal = single.Valore as ValoreBooleano;
                    if (singleVal == null)
                        return false;

                    bool? singleBool = singleVal.V;
                    bool? Bool = V;

                    res = singleBool == Bool;
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Valore che contiene una scelta tra un elenco (combo box)
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreElenco : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        public string V { get; set; }

        //[ProtoMember(2)]
        //[DataMember]
        //public Guid ValoreAttributoElencoId { get; set; }

        [ProtoMember(3)]
        [DataMember]
        public int ValoreAttributoElencoId { get; set; }

        public string PlainText { get => ToPlainText(); }

        public Valore Clone() { return new ValoreElenco() { V = this.V, ValoreAttributoElencoId = this.ValoreAttributoElencoId }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreElenco) && (val as ValoreElenco).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val)
        {
            if (!ResultEquals(val))
            {
                ValoreAttributoElencoId = -1;
                V = "[Multi]";
            }
        }
        public bool IsMultiValore(bool checkResult = false)
        {
            if (V != null && V.Trim() == "[Multi]") return true; return false;
        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(V, text))
                return true;

            return false;
        }
        public bool HasTesto(string text) { if (string.Equals(ToPlainText(), text, StringComparison.CurrentCultureIgnoreCase)) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        public bool GreaterThan(Valore val)
        {
            if ((V != null) && (val is ValoreElenco) && (val as ValoreElenco).V != null && (val as ValoreElenco).V.CompareTo(V) < 0)
                return true;
            else return false;
        }
        public string ToPlainText() { return V != null ? V : ""; }
        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            //da vedere...
            V = str;
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            throw new NotImplementedException();
        }

        public void Update(Valore val) { }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            return CheckFilterConditions(valoreFilter.MainGroup, format);
        }

        /// <summary>
        /// recursive
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        bool CheckFilterConditions(ValoreConditionsGroup group, string format)
        {
            bool res = false;

            foreach (var cond in group.Conditions)
            {
                if (cond is ValoreConditionsGroup)
                {
                    res = CheckFilterConditions(cond as ValoreConditionsGroup, format);
                }
                else if (cond is AttributoValoreConditionSingle)
                {

                    AttributoValoreConditionSingle attSingle = cond as AttributoValoreConditionSingle;
                    ValoreConditionSingle single = attSingle.ValoreConditionSingle;

                    ValoreElenco singleVal = single.Valore as ValoreElenco;
                    if (singleVal == null)
                        return false;

                    string singleTesto = singleVal.V;
                    string testo = V;

                    res = CheckFilterConditions(singleTesto, testo, single.Condition);
                }

                if (group.Operator == ValoreConditionsGroupOperator.And)
                {
                    if (res == false)
                        return false;
                    else { }
                }
                else if (group.Operator == ValoreConditionsGroupOperator.Or)
                {
                    if (res == true)
                        return true;
                    else { }
                }

            }

            return res;
        }


        bool CheckFilterConditions(string compTesto, string testo, ValoreConditionEnum condition)
        {

            if (compTesto != null && testo != null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                        return testo.StartsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.EndsWith:
                        return testo.EndsWith(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Contains:
                        return testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.NotContains:
                        return !testo.Contains(compTesto, StringComparison.CurrentCultureIgnoreCase);
                    case ValoreConditionEnum.Equal:
                        return testo.Equals(compTesto, StringComparison.CurrentCultureIgnoreCase);

                }
            }
            else if (compTesto == null)
            {
                switch (condition)
                {
                    case ValoreConditionEnum.StartsWith:
                    case ValoreConditionEnum.EndsWith:
                    case ValoreConditionEnum.Contains:
                        return true;
                    case ValoreConditionEnum.NotContains:
                        return false;
                    case ValoreConditionEnum.Equal:
                        return testo == null;
                }
            }

            return false;
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Valore che contiene una scelta tra un elenco (combo box)
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreColore : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        public string V { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public string Hexadecimal { get; set; }
        public string PlainText { get => ToPlainText(); }
        public Valore Clone() { return new ValoreColore() { V = this.V, Hexadecimal = this.Hexadecimal }; }

        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(V, text))
                return true;

            return false;
        }

        public bool ResultEquals(Valore val) { if ((val is ValoreColore) && (val as ValoreColore).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }

        public bool GreaterThan(Valore val)
        {
            if ((V != null) && (val is ValoreColore) && (val as ValoreColore).V != null && (val as ValoreColore).V.CompareTo(V) < 0)
                return true;
            else return false;
        }

        public bool HasTesto(string text) { if (string.Equals(ToPlainText(), text, StringComparison.CurrentCultureIgnoreCase)) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }

        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public void Intersect(Valore val) { if (!ResultEquals(val)) V = "[Multi]"; }

        public bool IsMultiValore(bool checkResult = false)
        {
            if (V != null && V.Trim() == "[Multi]") return true; return false;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            //V = Regex.Replace(V, oldTxt, newTxt, RegexOptions.IgnoreCase);
            throw new NotImplementedException();
        }

        public bool SetByString(string str)
        {
            V = str;
            return true;
        }

        public string ToPlainText() { return V != null ? V : ""; }
        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public void Update(Valore val) { }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            throw new NotImplementedException();
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Valore che contiene una scelta tra un Unità di misura 
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreFormatoNumero : ValoreSingle
    {
        [ProtoMember(1)]
        [DataMember]
        public string V { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public Guid ValoreAttributoFormatoNumeroId { get; set; }

        public string PlainText { get => ToPlainText(); }

        public Valore Clone() { return new ValoreFormatoNumero() { V = this.V, ValoreAttributoFormatoNumeroId = this.ValoreAttributoFormatoNumeroId }; }
        public bool ResultEquals(Valore val) { if ((val is ValoreFormatoNumero) && (val as ValoreFormatoNumero).V == V) return true; else return false; }
        public bool Equals(Valore val) { return ResultEquals(val); }
        public void Intersect(Valore val) { if (!ResultEquals(val)) V = "[Multi]"; }
        public bool IsMultiValore(bool checkResult = false)
        {
            if (V != null && V.Trim() == "[Multi]") return true; return false;
        }
        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (V != null && ValoreHelper.ContainsTesto(V, text))
                return true;

            return false;
        }
        public bool HasTesto(string text) { if (string.Equals(ToPlainText(), text, StringComparison.CurrentCultureIgnoreCase)) return true; else return false; }
        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }
        public bool GreaterThan(Valore val)
        {
            if ((V != null) && (val is ValoreFormatoNumero) && (val as ValoreFormatoNumero).V != null && (val as ValoreFormatoNumero).V.CompareTo(V) < 0)
                return true;
            else return false;
        }
        public string ToPlainText() { return V != null ? V : ""; }
        public bool HasValue() { if (V != null && V.Any()) return true; else return false; }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            l.Add(this.Clone() as ValoreSingle);
            return l;
        }

        public int ItemTextCount()
        {
            return 0;
        }

        public bool SetByString(string str)
        {
            //da vedere...
            V = str;
            return true;
        }

        public void ReplaceInText(string oldTxt, string newTxt)
        {
            throw new NotImplementedException();
        }

        public void Update(Valore val) { }

        public bool ResultContainsTesto(string text, string resultPaddedFormat)
        {
            return ContainsTesto(text);
        }

        public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
        {
            throw new NotImplementedException();
        }

        public string GetFormula()
        {
            return string.Empty;
        }
    }


    //[ProtoContract]
    //[DataContract]
    //public class ValoreVariabile : ValoreSingle
    //{
    //    [ProtoMember(1)]
    //    [DataMember]
    //    public string V
    //    {
    //        get;
    //        set;
    //    } = string.Empty; //codice attributo sezione variabili

    //    public string PlainText { get => ToPlainText(); }

    //    //Methods
    //    public Valore Clone() { return new ValoreVariabile() { V = this.V }; }
    //    public bool ResultEquals(Valore val) { if ((val is ValoreVariabile) && (val as ValoreVariabile).V == V) return true; else return false; }
    //    public bool Equals(Valore val) { return ResultEquals(val); }
    //    public void Intersect(Valore val) { if (!ResultEquals(val)) V = string.Empty; }
    //    public bool IsMultiValore(bool checkResult = false) { if (V == null) return true; return false; }
    //    public bool ContainsTesto(string text)
    //    {
    //        if (V == text)
    //            return true;

    //        return false;
    //    }

    //    public bool HasTesto(string text) { if (ToPlainText() == text) return true; else return false; }
    //    public bool ResultHasTesto(string text, string resultPaddedFormat) { return HasTesto(text); }
    //    //public bool HasTesto(string text)
    //    //{
    //    //    string str = V.ToString();
    //    //    if (str == text)
    //    //        return true;

    //    //    return false;
    //    //}
    //    public bool GreaterThan(Valore val) { return false; }
    //    public string ToPlainText() { return V; }
    //    public bool HasValue() { return V != string.Empty; }

    //    public List<ValoreSingle> ToValoreSingleSet()
    //    {
    //        List<ValoreSingle> l = new List<ValoreSingle>();
    //        l.Add(this.Clone() as ValoreSingle);
    //        return l;
    //    }

    //    public int ItemTextCount()
    //    {
    //        return 0;
    //    }

    //    public bool SetByString(string str)
    //    {
    //        //return false;
    //        V = str;
    //        return true;
    //    }

    //    public void ReplaceInText(string oldTxt, string newTxt)
    //    {
    //    }

    //    public void Update(Valore val) { }

    //    public bool ResultContainsTesto(string text, string resultPaddedFormat)
    //    {
    //        return ContainsTesto(text);
    //    }

    //    public bool CheckFilterConditions(ValoreConditions valoreFilter, string format)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string GetFormula()
    //    {
    //        return string.Empty;
    //    }
    //}

    public class ValoreComparer : IComparer<Valore>
    {
        public int Compare(Valore x, Valore y)
        {
            try
            {

                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                if (x.GreaterThan(y))
                    return 1;
                else if (x.ResultEquals(y))
                    return 0;
                else
                    return -1;
            }
            catch (Exception exc)
            {
                return 0;
            }
        }
    }

    public enum ResultEnum
    {
        Result = 0,
    }

}
