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
    [KnownType(typeof(TagItemType))]
    public class TagItem : Entity
    {
        
        public TagItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Tag;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            string nome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;

            if (mode == UserIdentityMode.SingleLine2)
                return String.Empty;

            return nome;
        }

    }

    [ProtoContract]
    [DataContract]
    public class TagItemType : EntityType
    {
        static string Separator = "_";

        public TagItemType() { }//protobuf


        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Tag;
            Name = LocalizationProvider.GetString("Tag");
            FunctionName = string.Empty;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;
            
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Nome"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.AllowMasterGrouping = true;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Descrizione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.GroupName = LocalizationProvider.GetString("Dettagli");
            att.AllowMasterGrouping = true;

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

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Tag;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Tag;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Tag;

        public override MasterType MasterType => MasterType.List;

        public override EntityComparer EntityComparer { get; set; } = new TagItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(TagItem))]
    public class TagItemCollection : EntityCollection
    {
    }

    public class TagItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Nome };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }


}