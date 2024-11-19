using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{
    public class SortData
    {
        public string EntityTypeKey { get; set; }
        public List<AttributoSortData> Items { get; set; } = new List<AttributoSortData>();

        
    }

    [ProtoContract]
    public class AttributoSortData
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; }

        [ProtoMember(2)]
        public bool IsOrdinamentoInverso { get; set; }

        public AttributoSortData Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            AttributoSortData clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

    }
}
