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
    public class Model3dValuesData
    {
        [ProtoMember(1)]
        public List<Model3dValueData> Values { get; set; } = new List<Model3dValueData>();

    }


    /// <summary>
    /// Classe che rappresenta un valore del modello 
    /// </summary>
    [ProtoContract]    
    public class Model3dValueData
    {

        /// <summary>
        /// Nome del file del modello 3d
        /// </summary>
        [ProtoMember(1)]
        public string ProjectGlobalId { get; set; }

        /// <summary>
        /// GlobalId dell'elemento del modello (IfcElement o derivato)
        /// </summary>
        [ProtoMember(2)]
        public string GlobalId { get; set; }

        /// <summary>
        /// Classe su cui cercare il valore
        /// Per esempio IfcBuildingStorey, IfcWallType
        /// </summary>
        [ProtoMember(3)]
        public string ClassName { get; set; }

        /// <summary>
        /// Percorso per la ricerca del valore (attributo o proprietà)
        /// Esempio in caso di ricerca di attributo: "Nome"
        /// Esempio in caso di ricerca di proprietà: "P>Dimensions>Lenght"
        /// </summary>
        [ProtoMember(4)]
        public string ValuePath { get; set; }

        /// <summary>
        /// Valore
        /// </summary>
        [ProtoMember(5)]
        public string Value { get; set; }

        /// <summary>
        /// Eventuale messaggio di errore
        /// </summary>
        [ProtoMember(6)]
        public string ErrorMsg { get; set; }

        [ProtoMember(7)]
        public Model3dType Model3dType { get; set; }


        /// <summary>
        /// Percorso per l'item (oggetto) di tipo ClassName
        /// </summary>
        //[ProtoMember(8)]
        public string ItemPath { get; set; }

    }


    
}
