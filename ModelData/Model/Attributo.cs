

using ProtoBuf;

namespace ModelData.Model
{

    [ProtoContract]
    public class DefinizioneAttributo
    {
        //[DataMember]
        //public Guid Id;
        //
        [ProtoMember(1)]
        public string Codice { get; set; }

        [ProtoMember(2)]
        public Valore ValoreDefault { get; set; }

        [ProtoMember(3)]
        public bool IsExpandable {get;set;}

        [ProtoMember(4)]
        public bool IsPreviewable { get; set; }

        [ProtoMember(5)]
        public bool AllowAttributoCustom { get; set; }



    }



    [ProtoContract]
    public class Attributo
    {

        [ProtoMember(1)]
        public bool IsBuiltIn { get; set; } = false;

        [ProtoMember(2)]
        public bool IsValoreReadOnly { get; set; } = false;

        [ProtoMember(3)]
        public bool IsVisible { get; set; } = false;

        [ProtoMember(4)]
        public bool IsInternal { get; set; } = false;

        [ProtoMember(5)]
        public string Etichetta { get; set; } = "";

        [ProtoMember(6)]
        public string Codice { get; set; } = ""; //CodiceEntity + "_" + code

        [ProtoMember(7)]
        public string SuggestionCode { get; set; } = "";

        [ProtoMember(8)]
        public bool AllowValoriUnivoci { get; set; } = false;

        [ProtoMember(9)]
        public bool AllowSort { get; set; } = false;

        [ProtoMember(10)]
        public bool AllowReplaceInText { get; set; } = false;

        [ProtoMember(11)]
        public string GroupName { get; set; } = "";

        [ProtoMember(12)]
        public bool AllowMasterGrouping { get; set; } = false;

        /// <summary>
        /// Ordine di visualizzazione nel detail
        /// </summary>
        [ProtoMember(13)]
        public int DetailViewOrder { get; set; } = -1;

        [ProtoMember(14)]
        public double Height { get; set; } = 20; //altezza dell'attributo in detail di default

        [ProtoMember(15)]
        public string DefinizioneAttributoCodice { get; set; }

        [ProtoMember(16)]
        public string EntityTypeKey { get; set; }

        [ProtoMember(17)]
        public string ValoreFormat { get; set; } = "#,##0.00";

        [ProtoMember(18)]
        public Valore ValoreDefault { get; set; }

        [ProtoMember(19)]
        public ValoreOperationType GroupOperation { get; set; } = ValoreOperationType.Nothing;

        [ProtoMember(20)]
        public bool IsValoreLockedByDefault { get; set; } = false;

        [ProtoMember(21)]
        public string GuidReferenceEntityTypeKey { get; set; } = "";

        [ProtoMember(22)]
        public ValoreAttributo ValoreAttributo { get; set; }


    }

    [ProtoContract]
    public class AttributoRiferimento : Attributo
    {


        [ProtoMember(1)]
        public string ReferenceCodiceGuid { get; set; }


        [ProtoMember(2)]
        public string ReferenceEntityTypeKey { get; set; }

        [ProtoMember(3)]
        public string ReferenceCodiceAttributo { get; set; }


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
        Append = 7,
    }





}