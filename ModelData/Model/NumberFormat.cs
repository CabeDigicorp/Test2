
using ProtoBuf;


namespace ModelData.Model
{
    [ProtoContract]
    public class NumericFormat
    {
        [ProtoMember(1)]
        public string Format { get; set; } = "#,##0.00";

    }
}
