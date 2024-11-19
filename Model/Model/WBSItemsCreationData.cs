using Commons;
using MasterDetailModel;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class WBSItemsCreationData
    {
        public string EntityTypeKey { get; set; } = string.Empty;
        //public bool IsAddAttivita { get; set; } = false;
        public List<WBSLevel> Items { get; set; } = new List<WBSLevel>();

        public WBSItemsCreationData Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            WBSItemsCreationData clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }
    }

    [ProtoContract]
    public class WBSLevel
    {
        public bool IsValoriAuto { get; set; } = true;
        public bool IsTreeInLevel { get; set; } = false;
        public AttributoFilterData AttributoFilterData { get; set; } = new AttributoFilterData();
    }
}
