using _3DModelExchange;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class Model3dTagsData
    {
        [ProtoMember(1)]
        public List<Model3dTagsDataItem> Items { get; set; } = new List<Model3dTagsDataItem>();
    }

    [ProtoContract]
    public class Model3dTagsDataItem
    {
        [ProtoMember(1)]
        public bool IsSelected { get; set; }

        [ProtoMember(2)]
        public string TagDescr { get; set; }

        [ProtoMember(3)]
        public Guid TagId { get; set; }

        [ProtoMember(4)]
        public List<Guid> lstAssociatedFilters { get; set; } = new List<Guid>();

        [ProtoMember(5)]
        public List<string> RvtAssociatedFilters { get; set; } = new List<string>();//unique id
    }

    public class Model3dTagsDataConverter
    {

        public static Model3dTagsData ConvertFromTagsData(TagsData source)
        {

            Model3dTagsData target = new Model3dTagsData();
            foreach (TagsDataItem s in source.Items)
            {
                Model3dTagsDataItem t = new Model3dTagsDataItem();
                t.IsSelected = s.IsSelected;
                t.TagDescr = s.TagDescr;
                t.TagId = s.TagId;
                t.lstAssociatedFilters = new List<Guid>(s.lstAssociatedFilters);
                t.RvtAssociatedFilters = new List<string>(s.RvtAssociatedFilters);

                target.Items.Add(t);
            }
            return target;
        }


        public static TagsData ConvertToTagsData(Model3dTagsData source, Int32 sourceProjectFileVersion)
        {
            TagsData target = new TagsData();
            target.Items = new List<TagsDataItem>();


            //if (sourceProjectFileVersion >= x)
            //{

            //}
            //else
            if (sourceProjectFileVersion >= 0)
            {
                foreach (Model3dTagsDataItem s in source.Items)
                {
                    TagsDataItem t = new TagsDataItem();
                    t.IsSelected = s.IsSelected;
                    t.TagDescr = s.TagDescr;
                    t.TagId = s.TagId;
                    t.lstAssociatedFilters = new List<Guid>(s.lstAssociatedFilters);
                    t.RvtAssociatedFilters = new List<string>(s.RvtAssociatedFilters);

                    target.Items.Add(t);
                }

            }
            else
            {
                //versione non supportata
            }
            return target;


        }


    }

}
