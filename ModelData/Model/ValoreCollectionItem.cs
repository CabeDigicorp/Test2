
using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class ValoreCollectionItem
    {
        public virtual void Replace(ValoreCollectionItem val) { }
        public virtual bool Removed { get; set; }
        public virtual Guid Id { get; set; }
        public virtual bool Equals1(ValoreCollectionItem val) { return false; }
    }


    /// <summary>
    /// Item di una Valore di tipo collezione.
    /// </summary>
    [ProtoContract]
    public class ValoreTestoCollectionItem : ValoreCollectionItem, Valore
    {

        [ProtoMember(1)]
        public override Guid Id { get; set; }

        [ProtoMember(2)]
        public string Testo1 { get; set; } = "";

        [ProtoMember(3)]
        public string Testo2 { get; set; } = "";

        [ProtoMember(4)]
        public string Testo3 { get; set; } = "";

        [ProtoMember(5)]
        public override bool Removed { get; set; } = false;

    }

    [ProtoContract]
    public class ValoreGuidCollectionItem : ValoreCollectionItem , Valore
    {

        [ProtoMember(1)]
        public override Guid Id { get; set; }

        [ProtoMember(2)]
        public Guid EntityId { get; set; } = Guid.Empty;

        [ProtoMember(3)]
        public override bool Removed { get; set; } = false;

    }
}
