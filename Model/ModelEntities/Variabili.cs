using CommonResources;
using Commons;
using MasterDetailModel;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Model
{
    [ProtoContract]
    [DataContract]
    [KnownType(typeof(VariabiliItemType))]
    public class VariabiliItem : Entity
    {
        
        public VariabiliItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Variabili;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
        }

    }

    [ProtoContract]
    [DataContract]
    public class VariabiliItemType : EntityType
    {
        public static string AttributoCodiceSeparator { get => "_"; }

        public VariabiliItemType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Variabili;
            Name = LocalizationProvider.GetString("Variabili");
            FunctionName = VarCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;
            string referenceCodiceGuid = "";

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            UpdateEtichetteMap();
        }


        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        //internal static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        //{
        //    string str = string.Join(Separator, divTypeKey, divAttCodice);
        //    return str;
        //}

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Variabili;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Variabili;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Variabili;

        public override MasterType MasterType => MasterType.NoMaster;

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(VariabiliItem))]
    public class VariabiliItemCollection : EntityCollection
    {
    }


}