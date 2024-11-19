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
    [KnownType(typeof(AllegatiItemType))]
    public class AllegatiItem : Entity
    {
        [ProtoMember(1)]
        public string FileName { get; set; } = string.Empty;
        
        public AllegatiItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Allegati;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            string nome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;

            if (mode == UserIdentityMode.SingleLine1)
                return nome;

            if (mode == UserIdentityMode.SingleLine2)
            {
                string link = GetValoreAttributo(BuiltInCodes.Attributo.Link, false, true).PlainText;
                return link;
            }

            return nome;
        }

    }

    [ProtoContract]
    [DataContract]
    public class AllegatiItemType : EntityType
    {
        static string Separator = "_";

        public AllegatiItemType() { }//protobuf


        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Allegati;
            Name = LocalizationProvider.GetString("Allegati");
            FunctionName = CntCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;
            
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //Attributi.Clear();
            //AttributiMasterCodes.Clear();

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
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Link);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Link"),
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


            ////////////////////////////////////////////////
            ///////Da cancellare
            ////Unique file Id
            //codiceAttributo = string.Join(Separator, "UniqueFileName");
            //if (Attributi.ContainsKey(codiceAttributo))
            //{
            //    att = Attributi[codiceAttributo];
            //    att.Etichetta = "UniqueFileName";
            //    att.IsValoreReadOnly = false;
            //    att.IsInternal = false;
            //    att.IsBuiltIn = false;
            //    att.DetailViewOrder = viewOrder++;
            //    att.AllowReplaceInText = false;
            //    att.GroupName = LocalizationProvider.GetString("Dettagli");
            //    att.IsVisible = false;
            //    att.IsAdvanced = true;
            //}
            ////////////////////////////////////////////////////




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
            return BuiltInCodes.EntityType.Allegati;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Allegati;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Allegati;

        public override MasterType MasterType => MasterType.List;

        public override EntityComparer EntityComparer { get; set; } = new AllegatiItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(AllegatiItem))]
    public class AllegatiItemCollection : EntityCollection
    {
    }

    public class AllegatiItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Link };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }


}