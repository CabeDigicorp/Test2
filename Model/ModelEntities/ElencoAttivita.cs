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
    [KnownType(typeof(ElencoAttivitaItemType))]
    public class ElencoAttivitaItem : Entity
    {

        public ElencoAttivitaItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.ElencoAttivita;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);

            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {   
            string codice = GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, true).PlainText;
            string nome = GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;
            
            
            if (mode == UserIdentityMode.SingleLine1)
                return codice;


            if (mode == UserIdentityMode.SingleLine2)
                return nome;



            if (string.IsNullOrEmpty(codice))
                return string.Format("{0}", nome);
            else
                return string.Format("{0} - {1}",codice,  nome);
        }

        //public override string AsString()
        //{
        //    string result = "";
        //    Entity entIter = this;
        //    while (entIter != null)
        //    {
        //        string str = "";
        //        Valore valCodice = entIter.GetValoreAttributo("Codice", false, true);//codice
        //        Valore valDesc = entIter.GetValoreAttributo("DescrizioneRTF", false, true);

        //        if (valCodice != null)
        //            str = valCodice.ToPlainText();

        //        if (valDesc != null)
        //            str += " " + valDesc.ToPlainText();

        //        if (result.Any())
        //            result = string.Join(HierarchySeparator, str, result);
        //        else
        //            result = str;

        //        entIter = entIter.Parent;
        //    }
        //    return result;
        //}

    }

    [ProtoContract]
    [DataContract]
    public class ElencoAttivitaItemType : EntityType
    {
        static string Separator = "_";

        public ElencoAttivitaItemType() { }//protobuf

        //public ElencoAttivitaItemParentType ElencoAttivitaItemParentType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.ElencoAttivita;
            Name = LocalizationProvider.GetString("ElencoAttivita");
            FunctionName = EAtCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //Attributi.Clear();
            //AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(Separator, "Codice");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Identificazione"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupName = LocalizationProvider.GetString("Identificazione");
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, "Nome");
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
                    GroupName = LocalizationProvider.GetString("Identificazione"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;

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
            return BuiltInCodes.EntityType.ElencoAttivita;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.ElencoAttivita;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.ElencoAttivita;

        public override MasterType MasterType => MasterType.List;

        public override EntityComparer EntityComparer { get; set; } = new ElencoAttivitaItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(ElencoAttivitaItem))]
    public class ElencoAttivitaItemCollection : EntityCollection
    {
    }

    public class ElencoAttivitaItemKeyComparer : EntityComparer
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