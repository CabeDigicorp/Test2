using ProtoBuf;

namespace ModelData.Model
{
    [ProtoContract]
    public class Model3dFileInfo
    {
        [ProtoMember(1)]
        public string FilePath { get; set; }

        [ProtoMember(2)]
        public string Note { get; set; }

        [ProtoMember(4)]
        public string FileName { get; set; }

        [ProtoMember(5)]
        public bool IsChecked { get; set; }
    }

    [ProtoContract]
    public class Model3dFilesInfo
    {
        [ProtoMember(1)]
        public List<Model3dFileInfo> Items { get; set; } = new List<Model3dFileInfo>();

        /// <summary>
        /// Obsoleto! Nella sezione "Modello ifc" indica se debba essere aperta la finestra "Visualizza preferenze" prima del caricamento dei file ifc
        /// </summary>
        //[ProtoMember(2)]//obsoleto
        //public bool ViewPreferencesBeforeLoad { get; set; } = false;
    }
}
