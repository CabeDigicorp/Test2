using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{
    [ProtoContract]
    public class AttributoFilter
    {
        [ProtoMember(1)]
        public string EntityTypeCodice { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; } = string.Empty;

        [ProtoMember(3)]
        public string Valore { get; set; } = null;
    }
}
