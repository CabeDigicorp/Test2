using _3DModelExchange;
using ProtoBuf;
using System;
using System.Collections.Generic;
using _3DModelExchange;
using ProtoBuf;


namespace Model
{
    [ProtoContract]
    public class Model3dUserRotoTranslation
    {
        [ProtoMember(1)]
        public List<Model3dUserRotoTranslationItem> Items = new List<Model3dUserRotoTranslationItem>();
    }

    [ProtoContract]
    public class Model3dUserRotoTranslationItem
    {
        [ProtoMember(1)]
        public string ProjGuid { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string ProjFilename { get; set; } = string.Empty;
        //
        [ProtoMember(3)]
        public double TranslX { get; set; } = 0;

        [ProtoMember(4)]
        public double TranslY { get; set; } = 0;

        [ProtoMember(5)]
        public double TranslZ { get; set; } = 0;

    }

    public class Model3dUserRotoTranslationConverter
    {

        public static Model3dUserRotoTranslation ConvertToUserRotoTranslation(Dictionary<UserTranslKey, UserTranslData> source)
        {
            if (source == null)
                return null;

            Model3dUserRotoTranslation result = new Model3dUserRotoTranslation();

            foreach (UserTranslKey key in source.Keys)
            {
                result.Items.Add(new Model3dUserRotoTranslationItem()
                {
                    ProjGuid = key.ProjGuid,
                    ProjFilename = key.ProjFilename,
                    TranslX = source[key].TranslX,
                    TranslY = source[key].TranslY,
                    TranslZ = source[key].TranslZ,

                });

            }

            return result;
        }

        public static Dictionary<UserTranslKey, UserTranslData> ConvertFromUserRotoTranslation(Model3dUserRotoTranslation source, Int32 projectFileVersion)
        {
            if (source == null)
                return null;

            if (source.Items == null)
                return null;

            Dictionary<UserTranslKey, UserTranslData> result = new Dictionary<UserTranslKey, UserTranslData>();

            foreach (Model3dUserRotoTranslationItem item in source.Items)
            {
                UserTranslKey key = new UserTranslKey()
                {
                    ProjGuid = item.ProjGuid,
                    ProjFilename = item.ProjFilename,
                };

                result.Add(key, new UserTranslData()
                {
                    TranslX = item.TranslX,
                    TranslY = item.TranslY,
                    TranslZ = item.TranslZ,
                });
            }

            return result;

        }
    }
}
