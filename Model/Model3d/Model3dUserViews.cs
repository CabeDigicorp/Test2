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
    public class Model3dUserViewList
    {
        [ProtoMember(1)]
        public List<Model3dSingleView> lstUserViews { get; set; } = new List<Model3dSingleView>();
    }

    [ProtoContract]
    public class Model3dSingleView
    {
        [ProtoMember(1)]
        public HashSet<string> lstGlobalID { get; set; } = new HashSet<string>();

        [ProtoMember(2)]
        public byte[] byteImage { get; set; }

        [ProtoMember(3)]
        public string ViewDescriz { get; set; }
    }

    public class Model3dUserViewsConverter
    {

        public static Model3dUserViewList ConvertFromUserViews(UserViewList source)
        {

            Model3dUserViewList target = new Model3dUserViewList();
            foreach (SingleView s in source.lstUserViews)
            {
                Model3dSingleView t = new Model3dSingleView();
                t.lstGlobalID = s.lstGlobalID;
                t.ViewDescriz = s.ViewDescriz;
                t.byteImage = (byte[]) s.byteImage.Clone();

                target.lstUserViews.Add(t);
            }
            return target;
        }


        public static UserViewList ConvertToUserViews(Model3dUserViewList source, Int32 projectFileVersion)
        {
            UserViewList target = new UserViewList();
            target.lstUserViews = new List<SingleView>();


            //if (projectFileVersion >= x)
            //{

            //}
            //else
            if (projectFileVersion >= 0)
            {
                foreach (Model3dSingleView s in source.lstUserViews)
                {
                    SingleView t = new SingleView();
                    t.lstGlobalID = s.lstGlobalID;
                    t.ViewDescriz = s.ViewDescriz;
                    t.byteImage = (byte[]) s.byteImage.Clone();

                    target.lstUserViews.Add(t);
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
