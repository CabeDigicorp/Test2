using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class Model3dFileInfo
    {
        [ProtoMember(1)]
        public string FilePath { get; set; }

        [ProtoMember(2)]
        public string Note { get; set; }

        //[ProtoMember(3)]
        //public Guid FileId { get; set; }//used by cache
        
        [ProtoMember(4)]
        public string FileName { get; set; }

        [ProtoMember(5)]
        public bool IsChecked { get; set; }

        public Model3dFileInfo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            Model3dFileInfo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

    }

    [ProtoContract]
    public class Model3dFilesInfo
    {
        [ProtoMember(1)]
        public List<Model3dFileInfo> Items { get; set; } = new List<Model3dFileInfo>();

        ///// <summary>
        ///// Obsoleto! Nella sezione "Modello ifc" indica se debba essere aperta la finestra "Visualizza preferenze" prima del caricamento dei file ifc
        ///// </summary>
        //[ProtoMember(2)]
        //public bool ViewPreferencesBeforeLoad { get; set; } = false;

        public Model3dFilesInfo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            Model3dFilesInfo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }
    }
}
