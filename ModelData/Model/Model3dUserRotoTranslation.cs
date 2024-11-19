using ProtoBuf;


namespace ModelData.Model
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


}
