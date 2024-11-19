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
    [KnownType(typeof(CapitoliItemType))]
    [KnownType(typeof(CapitoliItemParentType))]
    public class CapitoliItem : TreeEntity
    {
        
        public CapitoliItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Capitoli;
            ParentEntityTypeCodice = "CapitoliItemParent";
        }

        public override void CopyValoriAttributiFrom(Entity ent)
        {
            TreeEntity treeEnt = ent as TreeEntity;

            bool isParent = true;
            if (Attributi.Count > ParentEntityType.Attributi.Count)
                isParent = false;

            bool isEntParent = true;
            if (treeEnt.Attributi.Count > treeEnt.ParentEntityType.Attributi.Count)
                isEntParent = false;


            if (isParent == isEntParent)
                base.CopyValoriAttributiFrom(ent);
            else if (isEntParent)
            {
                EntityAttributo entAtt = Attributi["Codice"];
                entAtt.Valore = ent.Attributi["Codice"].Valore.Clone();
            }
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
        }

        //public override string AsString()
        //{
        //    string result = "";
        //    TreeEntity entIter = this;
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
    public class CapitoliItemType : TreeEntityType
    {
        static string Separator = "_";

        public CapitoliItemType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Capitoli;
            Name = LocalizationProvider.GetString("Capitoli");
            FunctionName = CapCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = true;
            Attributo att = null;
            
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

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
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupOperation = ValoreOperationType.Equivalent;
            //
            codiceAttributo = string.Join(Separator, "DescrizioneRTF");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    AllowReplaceInText = true,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    Height = 20,
                    IsVisible = true,
                    ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                });
                
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Origine);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("PrezzarioOrigine"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.DetailViewOrder = viewOrder++;


            UpdateEtichetteMap();
        }

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            AssociedType = entityTypes["CapitoliItemParent"] as CapitoliItemParentType;
            base.ResolveReferences(entityTypes, definizioniAttributo);
        }

        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        internal static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        {
            string str = string.Join(Separator, divTypeKey, divAttCodice);
            return str;
        }

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Capitoli;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Capitoli;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Capitoli;

        public override MasterType MasterType => MasterType.Tree;

        public override EntityComparer EntityComparer { get; set; } = new CapitoliItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(CapitoliItem))]
    public class CapitoliItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    [DataContract]
    public class CapitoliItemParentType : TreeEntityType
    {
        

        public CapitoliItemParentType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.CapitoliParent;
            Name = LocalizationProvider.GetString("Capitoli (Parent)");
            string codiceAttributo; //CodiceEntity + "_" + code
            int viewOrder = 0;
            FunctionName = EPCalculatorFunction.FunctionName;
            string pref = BuiltInCodes.EntityType.Capitoli;
            string sep = "_";
            Attributo att = null;

            codiceAttributo = string.Join(sep, "Codice");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            //
            codiceAttributo = string.Join(sep, "DescrizioneRTF");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    Height = 20,
                    IsVisible = true,
                    ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];

            UpdateEtichetteMap();
        }


        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override EntityType Clone()
        {

            string json = "";
            JsonSerializer.JsonSerialize(this, out json);

            EntityType clone = null;
            JsonSerializer.JsonDeserialize(json, out clone, GetType());

            return clone;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Nothing;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Nothing;

        public override MasterType MasterType => MasterType.Tree;

        public override bool IsParentType() { return true; }

        public override bool IsCustomizable() { return true; }
    }

    public class CapitoliItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Codice, BuiltInCodes.Attributo.Origine };

        public override bool Equals(string key1, string key2)
        {
            string[] key1s = key1.Split(KeySeparator.ToCharArray());
            string[] key2s = key2.Split(KeySeparator.ToCharArray());

            //Se è uguale il codice
            if (key1s[0] == key2s[0])
            {
                //if (!string.IsNullOrEmpty(key1s[1]) && string.IsNullOrEmpty(key2s[1]))
                //{
                    //se è uguale il prezzario origine
                    if (key1s[1] == key2s[1])
                        return true;
                //}
            }

            return false;
        }
    }

    public class CapitoliCodiceItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Codice };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }
}