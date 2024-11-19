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
    public class ValoreCollectionItem
    {
        public virtual void Replace(ValoreCollectionItem val) { }
        public virtual bool Removed { get; set; }
        public virtual Guid Id { get; set; }
        public virtual bool Equals1(ValoreCollectionItem val) { return false; }
        //public virtual string Testo1 { get; set; }
        //public virtual string Testo2 { get; set; }
        //public virtual string Testo3 { get; set; }
    }


    /// <summary>
    /// Item di una Valore di tipo collezione.
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreTestoCollectionItem : ValoreCollectionItem, Valore
    {

        [ProtoMember(1)]
        [DataMember]
        public override Guid Id { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public string Testo1 { get; set; } = "";

        [ProtoMember(3)]
        [DataMember]
        public string Testo2 { get; set; } = "";

        [ProtoMember(4)]
        [DataMember]
        public string Testo3 { get; set; } = "";

        [ProtoMember(5)]
        [DataMember]
        public override bool Removed { get; set; } = false;



        //public bool TestoEquals(ValoreCollectionItem val)
        //{
        //    if (Testo1 == val.Testo1 && Testo2 == val.Testo2 && Testo3 == val.Testo3)
        //        return true;

        //    return false;
        //}

        public override bool Equals1(ValoreCollectionItem val)
        {
            ValoreTestoCollectionItem v = val as ValoreTestoCollectionItem;

            return Equals(v);
        }

        bool Equals(ValoreTestoCollectionItem v)
        {
            if (Testo1 == v.Testo1 && Testo2 == v.Testo2/* && Testo3 == v.Testo3*/)
                return true;

            return false;
        }

        public bool ResultEquals(Valore val)
        {
            ValoreTestoCollectionItem v = val as ValoreTestoCollectionItem;

            return Equals(v);
        }
        public bool Equals(Valore val) { return ResultEquals(val); }

        public Valore Clone()
        {
            return new ValoreTestoCollectionItem() { Id = this.Id, Testo1 = this.Testo1, Testo2 = this.Testo2, /*Testo3 = this.Testo3, */Removed = this.Removed };
        }

        public bool ContainsTesto(string text)
        {
            if (text == null || text == "")
                return true;

            if (Testo1 != null && ValoreHelper.ContainsTesto(Testo1, text))
                return true;

            if (Testo2 != null && ValoreHelper.ContainsTesto(Testo2, text))
                return true;

            //if (Testo1.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            //    return true;

            //if (Testo2.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            //    return true;

            //if (Testo3.Contains(text))
            //    return true;

            return false;
        }

        public bool HasTesto(string text)
        {
            if (Testo1.Equals(text, StringComparison.CurrentCultureIgnoreCase))
                return true;

            if (Testo2.Equals(text, StringComparison.CurrentCultureIgnoreCase))
                return true;

            //if (Testo3 == text)
            //    return true;

            return false;

        }

        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> vals = new List<ValoreSingle>();

            vals.Add(new ValoreTesto() { V = Testo1 });
            vals.Add(new ValoreTesto() { V = Testo2 });
            //vals.Add(new ValoreTesto() { V = Testo1 });

            return vals;
        }

        public override void Replace(ValoreCollectionItem val)
        {
            ValoreTestoCollectionItem v = val as ValoreTestoCollectionItem;

            Testo1 = v.Testo1;
            Testo2 = v.Testo2;
        }

        public string PlainText => throw new NotImplementedException();
        public void Intersect(Valore val) { throw new NotImplementedException();}
        public bool IsMultiValore(bool checkResult = false) { throw new NotImplementedException(); }
        public bool HasValue(){ throw new NotImplementedException(); }
        public int ItemTextCount() { throw new NotImplementedException(); }
        public bool GreaterThan(Valore val) { throw new NotImplementedException(); }
        public string ToPlainText() { throw new NotImplementedException(); }
        public bool SetByString(string str) {throw new NotImplementedException(); }
        public void ReplaceInText(string oldTxt, string newTxt) { throw new NotImplementedException(); }
        public void Update(Valore val) { throw new NotImplementedException(); }

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
    /// Item di una Valore di tipo collezione.
    /// </summary>
    [ProtoContract]
    [DataContract]
    public class ValoreGuidCollectionItem : ValoreCollectionItem , Valore
    {

        [ProtoMember(1)]
        [DataMember]
        public override Guid Id { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public Guid EntityId { get; set; } = Guid.Empty;

        [ProtoMember(3)]
        [DataMember]
        public override bool Removed { get; set; } = false;

        public Valore Clone()
        {
            return new ValoreGuidCollectionItem() { Id = this.Id, EntityId = this.EntityId, Removed = this.Removed };
        }

        public string PlainText => throw new NotImplementedException();


        public override bool Equals1(ValoreCollectionItem val)
        {
            ValoreGuidCollectionItem v = val as ValoreGuidCollectionItem;

            return Equals(v);
        }

        bool Equals(ValoreGuidCollectionItem v)
        {
            if (EntityId == v.EntityId)
                return true;

            return false;
        }

        public bool ResultEquals(Valore val)
        {
            ValoreGuidCollectionItem v = val as ValoreGuidCollectionItem;

            return Equals(v);
        }

        public bool Equals(Valore val) { return ResultEquals(val); }


        //public bool Equals(Valore val)
        //{
        //    ValoreGuidCollectionItem v = val as ValoreGuidCollectionItem;

        //    if (EntityId == v.EntityId)
        //        return true;

        //    return false;
        //}



        public bool ContainsTesto(string text)
        {
            return HasTesto(text);
        }

        public bool HasTesto(string text)
        {
            return text == EntityId.ToString();
        }

        public bool ResultHasTesto(string text, string resultPaddedFormat, bool ignoreCase = true) { return HasTesto(text); }

        public override void Replace(ValoreCollectionItem val)
        {
            ValoreGuidCollectionItem v = val as ValoreGuidCollectionItem;
            EntityId = v.EntityId;
        }

        public List<ValoreSingle> ToValoreSingleSet()
        {
            List<ValoreSingle> l = new List<ValoreSingle>();
            //l.Add(this.Clone() as ValoreSingle);
            l.Add(new ValoreGuid() { V = EntityId });
            return l;
        }


        public void Intersect(Valore val) { throw new NotImplementedException();}
        public bool IsMultiValore(bool checkResult = false) {  throw new NotImplementedException(); }
        public bool HasValue() { throw new NotImplementedException(); }
        public bool GreaterThan(Valore val) { throw new NotImplementedException(); }
        public string ToPlainText() { throw new NotImplementedException(); }
        public int ItemTextCount(){ throw new NotImplementedException(); }
        public bool SetByString(string str){throw new NotImplementedException(); }
        public void ReplaceInText(string oldTxt, string newTxt){ throw new NotImplementedException();}
        public void Update(Valore val)  { throw new NotImplementedException(); }

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
}
