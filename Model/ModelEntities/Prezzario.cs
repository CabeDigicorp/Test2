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
    [KnownType(typeof(PrezzarioItemType))]
    [KnownType(typeof(PrezzarioItemParentType))]
    public class PrezzarioItem : TreeEntity
    {
        
        public PrezzarioItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Prezzario;
            ParentEntityTypeCodice = "PrezzarioItemParent";
        }

        public void CreaAttributiRandom(int i)
        {
            

            Attributi.Add("PrezzarioItem_Codice", new EntityAttributo(this, EntityType.Attributi["PrezzarioItem_Codice"]) { Valore = new ValoreTesto() { V = "COD" + i.ToString("D3") } });
            //Attributi.Add(new EntityAttributo(EntityType.Attributi["PrezzarioItem.Descrizione"]) { Valore = new ValoreTesto() { V = "Tubo" + i.ToString() } });

            string rtf = "";
            ValoreHelper.RtfFromPlainString("Tubo" + i.ToString(), out rtf);
            Attributi.Add("PrezzarioItem_DescrizioneRTF", new EntityAttributo(this, EntityType.Attributi["PrezzarioItem_DescrizioneRTF"]) { Valore = new ValoreTestoRtf() { V = rtf } });
            Attributi.Add("PrezzarioItem_UnitaMisura", new EntityAttributo(this, EntityType.Attributi["PrezzarioItem_UnitaMisura"]) { Valore = new ValoreTesto() { V = "metri" } });
            Attributi.Add("PrezzarioItem_Prezzo", new EntityAttributo(this, EntityType.Attributi["PrezzarioItem_Prezzo"]) { Valore = new ValoreContabilita() { V = "12.30" } }); //12.30m

        }

        public void CreaAttributiParentRandom(int i)
        {
            

            Attributi.Add("PrezzarioItem_Codice", new EntityAttributo(this, ParentEntityType.Attributi["PrezzarioItem_Codice"]) { Valore = new ValoreTesto() { V = "COD" + i.ToString("D3") } });
            //Attributi.Add(new EntityAttributo(ParentEntityType.Attributi["PrezzarioItem.Descrizione"]) { Valore = new ValoreTesto() { V = "Tubo" + i.ToString() } });

            string rtf = "";
            ValoreHelper.RtfFromPlainString("Tubo" + i.ToString(), out rtf);
            Attributi.Add("PrezzarioItem_DescrizioneRTF", new EntityAttributo(this, ParentEntityType.Attributi["PrezzarioItem_DescrizioneRTF"]) { Valore = new ValoreTestoRtf() { V = rtf } });

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
                //EntityAttributo entAtt = Attributi.First(item => item.Attributo.Codice == "PrezzarioItem_Codice");
                //entAtt.Valore = ent.Attributi.First(item => item.Attributo.Codice == "PrezzarioItem_Codice").Valore.Clone();

                EntityAttributo entAtt = Attributi["Codice"];/*PrezzarioItem_*/
                entAtt.Valore = ent.Attributi["Codice"].Valore.Clone();/*PrezzarioItem_*/
            }
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);

            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
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
    public class PrezzarioItemType : TreeEntityType
    {
        static string Separator = "_";

        public PrezzarioItemType() { }//protobuf

        //public PrezzarioItemParentType PrezzarioItemParentType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Prezzario;
            Name = LocalizationProvider.GetString("ElencoPrezzi");
            FunctionName = EPCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = true;
            Attributo att = null;
            
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);
            string referenceCodiceGuid = "";

            ValoreAttributoFormatoNumeroItem formatoDefault = new ValoreAttributoFormatoNumeroItem()
            {
                Id = Guid.NewGuid(),
                Format = "#,##0.00",
            };


            //Attributi.Clear();
            //AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupOperation = ValoreOperationType.Equivalent;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.DescrizioneRTF);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    AllowReplaceInText = true,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    Height = 20,
                    IsVisible = true,
                    ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.UnitaMisura);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("UnitaDiMisura"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.FormatoQuantita);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.FormatoNumero].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("FormatoQuantita"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    IsVisible = true,
                    ValoreDefault = new ValoreFormatoNumero() {ValoreAttributoFormatoNumeroId = formatoDefault.Id, V = formatoDefault.Format },
                    //ValoreAttributo = new ValoreAttributoFormatoNumero() { Items = new List<ValoreAttributoFormatoNumeroItem>() { formatoDefault } },
                });
                
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.ValoreAttributo = new ValoreAttributoFormatoNumero() { Items = new List<ValoreAttributoFormatoNumeroItem>() { formatoDefault } };
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Prezzo);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Contabilita].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Prezzo"),
                    IsBuiltIn = true,
                    //AllowSort = true,
                    //AllowValoriUnivoci = true,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    ValoreFormat = "#,##0.00 €",
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Origine);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("PrezzarioOrigine"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    //DetailViewOrder = viewOrder++,
                    //DetailViewOrder = Attributi.Count,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            att.DetailViewOrder = viewOrder++;




            //Divisione Capitoli
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.Id);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Capitolo"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Capitoli,
                    IsVisible = true,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.AllowValoriUnivoci = true;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("CodiceCapitolo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Capitoli,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.DescrizioneRTF);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("DescrizioneCapitolo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Capitoli,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.DescrizioneRTF,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    GroupName = LocalizationProvider.GetString("Capitoli"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTestoRtf() { V = emptyRtf },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Attivita);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.GuidCollection].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Attivita"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
                    GroupName = LocalizationProvider.GetString("Attivita"),
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowValoriUnivoci = true;
            att.AllowSort = false;
            att.AllowMasterGrouping = false;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.Height = 60;
            //


            UpdateEtichetteMap();
        }

        //static public EntityCollection NewPrezzarioItemCollection()
        //{
        //    PrezzarioItemCollection artColl = new PrezzarioItemCollection();
        //    return artColl;

        //}

        //public override EntityCollection NewEntityCollection()
        //{
        //    return NewPrezzarioItemCollection();
        //}

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            AssociedType = entityTypes["PrezzarioItemParent"] as PrezzarioItemParentType;
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
            return BuiltInCodes.EntityType.Prezzario;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Prezzario;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Prezzario;

        public override MasterType MasterType => MasterType.Tree;

        public override EntityComparer EntityComparer { get; set; } = new PrezzarioItemKeyComparer();

        public override bool IsCustomizable() { return true; }

    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(PrezzarioItem))]
    public class PrezzarioItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    [DataContract]
    public class PrezzarioItemParentType : TreeEntityType
    {
        

        public PrezzarioItemParentType() { }//protobuf

        //public PrezzarioItemType PrezzarioItemType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.PrezzarioParent;// "PrezzarioItemParent";
            Name = LocalizationProvider.GetString("Prezzario (Parent)");
            string codiceAttributo; //CodiceEntity + "_" + code
            int viewOrder = 0;
            FunctionName = EPCalculatorFunction.FunctionName;
            string pref = BuiltInCodes.EntityType.Prezzario;
            string sep = "_";
            Attributo att = null;

            //IsTreeMaster = true;

            //Attributi.Clear();
            //AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            //
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DescrizioneRTF);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Articolo2"),
                    Height = 20,
                    IsVisible = true,
                    ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault },
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            

            UpdateEtichetteMap();
        }



        //public override EntityCollection NewEntityCollection()
        //{
        //    return PrezzarioItemType.NewPrezzarioItemCollection();
        //}

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

    public class PrezzarioItemKeyComparer : EntityComparer
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

    public class PrezzarioCodiceItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Codice };

        public override bool Equals(string key1, string key2)
        {
            string[] key1s = key1.Split(KeySeparator.ToCharArray());
            string[] key2s = key2.Split(KeySeparator.ToCharArray());

            //Se è uguale il codice
            if (key1s[0] == key2s[0])
            {
                return true;
            }

            return false;
        }
    }
}