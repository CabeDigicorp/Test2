

using Commons;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MasterDetailModel
{
    [ProtoContract]
    [DataContract]
    public class DefinizioneAttributo
    {
        //[DataMember]
        //public Guid Id;
        //
        [ProtoMember(1)]
        [DataMember]
        public string Codice { get; set; }

        [ProtoMember(2)]
        [DataMember]
        public Valore ValoreDefault { get; set; }

        [ProtoMember(3)]
        [DataMember]
        public bool IsExpandable {get;set;}

        [ProtoMember(4)]
        [DataMember]
        public bool IsPreviewable { get; set; }

        [ProtoMember(5)]
        [DataMember]
        public bool AllowAttributoCustom { get; set; }


        public DefinizioneAttributo Clone()
        {
            return new DefinizioneAttributo()
            {
                Codice = this.Codice,
                ValoreDefault = this.ValoreDefault.Clone(),
                IsExpandable = this.IsExpandable,
                IsPreviewable = this.IsPreviewable,
                AllowAttributoCustom = this.AllowAttributoCustom,
                
            };
        }
    }

    [ProtoContract]
    [DataContract]
    public class Attributo
    {
        //[DataMember]
        //public Guid Id;
        public Attributo()
        {

        }

        [ProtoMember(1)]
        [DataMember]
        public bool IsBuiltIn { get; set; } = false;

        [ProtoMember(2)]
        [DataMember]
        public bool IsValoreReadOnly { get; set; } = false;

        [ProtoMember(3)]
        [DataMember]
        public bool IsVisible { get; set; } = false;

        [ProtoMember(4)]
        [DataMember]
        public bool IsInternal { get; set; } = false;

        [ProtoMember(5)]
        [DataMember]
        public string Etichetta { get; set; } = "";

        [ProtoMember(6)]
        [DataMember]
        public string Codice { get; set; } = ""; //CodiceEntity + "_" + code

        [ProtoMember(7)]
        [DataMember]
        public string SuggestionCode { get; set; } = "";

        [ProtoMember(8)]
        [DataMember]
        public bool AllowValoriUnivoci { get; set; } = false;

        [ProtoMember(9)]
        [DataMember]
        public bool AllowSort { get; set; } = false;

        [ProtoMember(10)]
        [DataMember]
        public bool AllowReplaceInText { get; set; } = false;

        [ProtoMember(11)]
        [DataMember]
        public string GroupName { get; set; } = "";

        [ProtoMember(12)]
        [DataMember]
        public bool AllowMasterGrouping { get; set; } = false;

        /// <summary>
        /// Ordine di visualizzazione nel detail
        /// </summary>
        [ProtoMember(13)]
        [DataMember]
        public int DetailViewOrder { get; set; } = -1;

        public bool IsExpanded { get; set; } = false;//viene usato per ValoreTestoRtf
        public bool IsPreviewMode { get; set; } = false;//viene usato per ValoreTestoRtf

        [ProtoMember(14)]
        [DataMember]
        public double Height { get; set; } = 20; //altezza dell'attributo in detail di default

        [ProtoMember(15)]
        [DataMember]
        public string DefinizioneAttributoCodice;
        public DefinizioneAttributo Definizione { get; protected set; }//ref

        [ProtoMember(16)]
        [DataMember]
        public string EntityTypeKey { get; set; }
        public EntityType EntityType { get; protected set; }//ref

        [ProtoMember(17)]
        [DataMember]
        public string ValoreFormat { get; set; } = "#,##0.00";

        [ProtoMember(18)]
        [DataMember]
        public Valore ValoreDefault { get; set; }

        [ProtoMember(19)]
        [DataMember]
        public ValoreOperationType GroupOperation { get; set; } = ValoreOperationType.Nothing;

        [ProtoMember(20)]
        [DataMember]
        public bool IsValoreLockedByDefault { get; set; } = false;

        [ProtoMember(21)]
        [DataMember]
        public string GuidReferenceEntityTypeKey { get; set; } = "";

        /// <summary>
        /// Dati aggiuntivi che riguardano specificatamente il valore di un attributo ma comuni per tutte le entità
        /// Vengono impostati nel setting degli attributi per EntityType
        /// </summary>
        [ProtoMember(22)]
        [DataMember]
        public ValoreAttributo ValoreAttributo { get; set; } = null;

        public bool IsExpandable { get => Definizione.IsExpandable; }

        public bool IsPreviewable {get => Definizione.IsPreviewable; }

        /// <summary>
        /// Attributo visualizzato solo in modalità avanzata
        /// </summary>
        public bool IsAdvanced { get; set; } = false;

        /// <summary>
        /// Il dato è il risultato di somma dei valori dei figli
        /// </summary>
        public bool IsSummary { get; set; } = false; 


        public Attributo(DefinizioneAttributo defAtt, EntityType contenitoreclassType, string codiceAttributo)
        {
            //Id = Guid.NewGuid();
            Definizione = defAtt;
            EntityType = contenitoreclassType;
            Codice = codiceAttributo;

            DefinizioneAttributoCodice = Definizione.Codice;
            EntityTypeKey = EntityType.GetKey();

            ValoreDefault = Definizione.ValoreDefault;
        }


        public void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            EntityType = entityTypes[EntityTypeKey];
            Definizione = definizioniAttributo[DefinizioneAttributoCodice];

        }

        

        

        public virtual Attributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            Attributo clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            //Resolve references
            clone.EntityType = EntityType;
            clone.Definizione = Definizione;

            clone.ValoreDefault = ValoreDefault.Clone();

            if (ValoreAttributo != null)
                clone.ValoreAttributo = ValoreAttributo.Clone();

            return clone;

        }
    }

    [ProtoContract]
    public class AttributoRiferimento : Attributo
    {

        public AttributoRiferimento() { }//protobuf

        public AttributoRiferimento(DefinizioneAttributo defAtt, EntityType contenitoreclassType, string codiceAttributo) : base(defAtt, contenitoreclassType, codiceAttributo)
        {

        }

        string _referenceCodiceGuid = string.Empty;
        [ProtoMember(1)]
        [DataMember]
        public string ReferenceCodiceGuid
        {
            get => _referenceCodiceGuid;
            set
            {
                if (value == null || value == string.Empty)
                {
                    int alt = 0;
                }
                _referenceCodiceGuid = value;
            }
        }

        [ProtoMember(2)]
        [DataMember]
        public string ReferenceEntityTypeKey { get; set; }

        [ProtoMember(3)]
        [DataMember]
        public string ReferenceCodiceAttributo { get; set; }

        public override Attributo Clone()
        {
            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            AttributoRiferimento clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            //Resolve references
            clone.EntityType = EntityType;
            clone.Definizione = Definizione;

            clone.ValoreDefault = ValoreDefault.Clone();

            if (ValoreAttributo != null)
                clone.ValoreAttributo = ValoreAttributo.Clone();

            return clone;

        }
    }


    public enum ValoreOperationType
    {
        Nothing = 0,
        Equivalent = 1,
        Sum = 2,
        Multiplication = 3,
        Max = 4,
        Min = 5,
        Average = 6,
        AppendWithSpace = 7,
        AppendNewLine = 8,
    }





}