using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{

    [ProtoContract]
    public class FilterData
    {
        //filtro tradizionale con soli AND
        [ProtoMember(2)]
        public List<AttributoFilterData> Items { get; set; } = new List<AttributoFilterData>();

        //nuova base dati per filtro (da fare).
        //oss: per memorizzare il nuovo filtro generico posso usare Items con un solo item (codiceAttributo = null) e il filtro in ValoreCoditions di questo item.




        public FilterData Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            FilterData clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public bool Equals1(FilterData filterData)
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            string json1 = "";
            JsonSerializer.JsonSerialize(filterData, out json1);

            return json1 == json;
        }

        //static public string CreateCodiceValore(string codiceAttributo, int itemTextIndex)
        //{
        //    string code = codiceAttributo;
        //    if (itemTextIndex >= 0)
        //        code = string.Join(".", code, itemTextIndex.ToString());
        //    return code;
        //}

        public bool IsFilterApplied()
        {
            foreach (AttributoFilterData attFilter in Items)
            {
                if (attFilter.IsValid() && attFilter.IsFiltroAttivato)
                    return true;
            }

            return false;
        }

        public bool IsSearchApplied()
        {
            foreach (AttributoFilterData attFilter in Items)
            {
                if (attFilter.IsValid())
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void FromConditions(string entitytypeKey, ValoreConditions valConds)
        {

            Clear();

            if (valConds.MainGroup.Operator != ValoreConditionsGroupOperator.And)
                return;


            var condsByAtt = valConds.MainGroup.Conditions.Where(item => item is AttributoValoreConditionSingle)
                                                        .Select(item => item as AttributoValoreConditionSingle)
                                                        .GroupBy(item => item.CodiceAttributo);


            foreach (var conds in condsByAtt)
            {
                AttributoFilterData attFilterData = new AttributoFilterData();
                attFilterData.EntityTypeKey = entitytypeKey;
                attFilterData.CodiceAttributo = conds.Key;
                attFilterData.IsFiltroAttivato = true;
                attFilterData.FilterType = FilterTypeEnum.Conditions;

                attFilterData.ValoreConditions = new ValoreConditions();
                foreach (AttributoValoreConditionSingle cond in conds)
                {
                    attFilterData.ValoreConditions.MainGroup.Operator = ValoreConditionsGroupOperator.And;
                    attFilterData.ValoreConditions.MainGroup.Conditions.Add(cond.Clone());
                }
                Items.Add(attFilterData);
            }
            
        }

        public ValoreConditions ToConditions()
        {
            var conds = new ValoreConditions();
            conds.MainGroup.Operator = ValoreConditionsGroupOperator.And;

            foreach (var attributoFilterData in Items)
            {
                attributoFilterData.ValoreConditions.MainGroup.Conditions.ForEach(item => conds.MainGroup.Conditions.Add((item as AttributoValoreConditionSingle).Clone()));
            }

            return conds;
        }



        
    }


    [ProtoContract]
    public class AttributoFilterData
    {
        [ProtoMember(1)]
        public string EntityTypeKey { get; set; }

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; }

        [ProtoMember(3)]
        public string TextSearched { get; set; }

        [ProtoMember(4)]
        public bool? IsAllChecked
        {
            get;
            set;
        } = false;

        [ProtoMember(5)]
        public bool IsFiltroAttivato { get; set; } = false;

        [ProtoMember(6)]
        public HashSet<string> CheckedValori { get; set; } = new HashSet<string>();


        //Utilizzati nel caso di filtro per Attributo di tipo Riferimento (filtro sull'entità sorgente)
        #region solo x Attributo riferimento
        [ProtoMember(7)]
        public string SourceCodiceAttributo { get; set; }

        [ProtoMember(8)]
        public string SourceEntityTypeKey { get; set; }
        #endregion

        [ProtoMember(9)]
        public FilterTypeEnum FilterType { get; set; } = FilterTypeEnum.Result;

        [ProtoMember(10)]
        public ValoreConditions ValoreConditions { get; set; } = new ValoreConditions();

        public HashSet<Guid> FoundEntitiesId { get; set; } = null;

        public bool IgnoreCase { get; set; } = true;

        public AttributoFilterData()
        {
        }

        public bool IsValid()
        {

            if (IsAllChecked == true)
                return true;

            if (/*SourceCheckedValori.Any() || */CheckedValori.Any())
                return true;

            if (FilterType == FilterTypeEnum.Conditions && ValoreConditions.MainGroup.Conditions.Any())
                return true;

            return false;
        }

        public AttributoFilterData Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            AttributoFilterData clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }


    }

    public enum FilterTypeEnum
    {
        Nothing = 0,
        Formula = 1,
        Result = 2,
        Conditions = 3,

    }

    //public enum FilterDataItemsGroupOperator
    //{
    //    And = 0,
    //    Or = 1,
    //}




}
