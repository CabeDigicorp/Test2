using CommonResources;
using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailModel
{
    
    public class GroupData
    {
        public static char GroupKeySeparator { get; } = '¦'; //= ';';

        public string EntityTypeKey { get; set; }

        public List<AttributoGroupData> Items { get; set; } = new List<AttributoGroupData>();

        /// <summary>
        /// key: chiave del gruppo con ';' come separatore (es: "COD23;metri")
        /// </summary>
        public Dictionary<string, GroupRecordData> GroupRecords { get; set; } = new Dictionary<string, GroupRecordData>();

        public void UpdateGroupRecordsData()
        {
            if(GroupRecords != null)
                GroupRecords.Clear();
        }

        public string[] SplitGroupKey(string groupKey)
        {
            if (groupKey == null)
                return new string[0];

            return groupKey.Split(GroupKeySeparator);
        }

        public static string JoinGroupKeys(string[] groupKeys)
        {
            string jstr = string.Join(GroupKeySeparator.ToString(), groupKeys);
            return jstr;
        }
    }

    [ProtoContract]
    public class AttributoGroupData
    {
        [ProtoMember(1)]
        public string CodiceAttributo { get; set; }

        public AttributoGroupData Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            AttributoGroupData clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }
    }

    public class GroupRecordData
    {
        /// <summary>
        /// key: codice attributo
        /// </summary>
        public Dictionary<string, string> Attributi { get; set; } = new Dictionary<string, string>();

        public List<Guid> ChildsId { get; set; } = new List<Guid>();
        public string HighlighterColorName { get; set; } = MyColorsEnum.Transparent.ToString();
    }
}
