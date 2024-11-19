using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{

    /// <summary>
    /// Evidenziatori di entità
    /// </summary>
    [ProtoContract]
    public class EntityHighlighters
    {
        [ProtoMember(1)]
        public string EntityTypeKey { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string CodiceAttributo { get; set; } = string.Empty;

        [ProtoMember(3)]
        public Dictionary<string, ValoreHighlighter> Highlighters { get; set; } = new Dictionary<string, ValoreHighlighter>();

    }

    [ProtoContract]
    public class ValoreHighlighter
    {
        [ProtoMember(1)]
        public string Valore { get; set; } = string.Empty;

        [ProtoMember(2)]
        public ValoreColore Colore { get; set; } = null;
    }
}
