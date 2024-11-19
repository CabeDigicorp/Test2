using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class EntityTypeCodiceRevision
    {
        public string OldEntityTypeCodice { get; set; } = string.Empty;
        public string NewEntityTypeCodice { get; set; } = string.Empty;
    }

    public class AttributoCodiceRevision : EntityTypeCodiceRevision
    {
        public string OldAttributoCodice { get; set; } = string.Empty;
        public string NewAttributoCodice { get; set; } = string.Empty;
    }
}
