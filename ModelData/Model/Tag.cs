using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Model
{
    [ProtoContract]
    public class TagItem : Entity
    {
    }

    [ProtoContract]
    public class TagItemType : EntityType
    {
    }

    [ProtoContract]
    public class TagItemCollection : EntityCollection
    {
    }
}
