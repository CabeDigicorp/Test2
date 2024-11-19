using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Model
{
    public class IfcCode
    {
        public IfcCode(string? key, string? value)
        {
            Key = key;
            Value = value;
        }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public struct GlobalIdPair
    {
        public string ModelGlobalID { get; set; }
        public string ObjectGlobalID { get; set; }

        public GlobalIdPair(string modelGlobalID, string objectGlobalID)
        {
            ModelGlobalID = modelGlobalID;
            ObjectGlobalID = objectGlobalID;
        }
    }
}
