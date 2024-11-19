using CommonResources;
using Commons;
using DevExpress.Xpf.Core;
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
    [KnownType(typeof(WBSItemType))]
    [KnownType(typeof(WBSItemParentType))]
    public class WBSItem : TreeEntity
    {

        public WBSItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.WBS;
            ParentEntityTypeCodice = "WBSItemParent";
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
                EntityAttributo entAtt = Attributi[BuiltInCodes.Attributo.Codice];
                entAtt.Valore = ent.Attributi[BuiltInCodes.Attributo.Codice].Valore.Clone();

                entAtt = Attributi[BuiltInCodes.Attributo.DataInizio];
                entAtt.Valore = ent.Attributi[BuiltInCodes.Attributo.DataInizio].Valore.Clone();
            }
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
        }

        public override bool CanBeParent()
        {
            return true;
        }

        public override bool IsParentDependsOnChild() { return true; }

        public override bool MakeLeaf()
        {
            base.MakeLeaf();

            Attributi[BuiltInCodes.Attributo.Lavoro].Valore = new ValoreReale();

            return true;
        }

        public DateTime? GetDataInizio()
        {
            ValoreData val = GetValoreAttributo(BuiltInCodes.Attributo.DataInizio, false, false) as ValoreData;
            if (val != null && val.V != null)
                return val.V.Value;

            return null;
        }

        public DateTime? GetDataFine()
        {
            ValoreData val = GetValoreAttributo(BuiltInCodes.Attributo.DataFine, false, false) as ValoreData;
            if (val != null && val.V != null)
                return val.V.Value;

            return null;
        }

        public double? GetGiorniDurata()
        {
            ValoreReale val = GetValoreAttributo(BuiltInCodes.Attributo.Durata, false, false) as ValoreReale;
            if (val != null)
                return val.RealResult;

            return null;
        }

        public double? GetGiorniDurataCalendario()
        {
            ValoreReale val = GetValoreAttributo(BuiltInCodes.Attributo.DurataCalendario, false, false) as ValoreReale;
            if (val != null)
                return val.RealResult;

            return null;
        }

        public double? GetOreLavoro()
        {
            ValoreReale val = GetValoreAttributo(BuiltInCodes.Attributo.Lavoro, false, false) as ValoreReale;
            if (val != null)
                return val.RealResult;

            return null;
        }

        public WBSPredecessors GetPredecessors()
        {
            WBSPredecessors preds = null;
            ValoreTesto valJson = GetValoreAttributo(BuiltInCodes.Attributo.Predecessor, false, false) as ValoreTesto;
            if (valJson != null)
            {
                string json = valJson.PlainText;
                if (JsonSerializer.JsonDeserialize(json, out preds, typeof(WBSPredecessors)))
                    return preds;
            }
            return new WBSPredecessors();
        }

        public double? GetTaskProgress()
        {
            ValoreReale val = GetValoreAttributo(BuiltInCodes.Attributo.TaskProgress, false, false) as ValoreReale;
            if (val != null && val.RealResult != null)
            {
                if (!double.IsNaN(val.RealResult.Value))
                    return val.RealResult;
            }

            return 0;
        }

        public Guid GetCalendarioId()
        {
            Guid calId = Guid.Empty;

            string codiceAttCalendario = string.Join(WBSItemType.AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            ValoreGuid calendarioId = (ValoreGuid) GetValoreAttributo(codiceAttCalendario, false, false);

            if (calendarioId != null)
                calId = calendarioId.V;

            return calId;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            if (mode == UserIdentityMode.SingleLine1)
                return GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, true).PlainText;

            if (mode == UserIdentityMode.SingleLine2)
                return GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;

            return string.Empty;
        }

    }

    [ProtoContract]
    [DataContract]
    public class WBSItemType : TreeEntityType
    {
        public static string AttributoCodiceSeparator { get => "_"; }

        public WBSItemType() { }//protobuf


        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.WBS;
            Name = LocalizationProvider.GetString("WBS");
            FunctionName = WBSCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = true;
            Attributo att = null;
            AttributiMasterCodes.Clear();
            string referenceCodiceGuid = string.Empty;

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Codice"),
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            AttributiMasterCodes.Add(codiceAttributo);

            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    Height = 20,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                });

            }
            att = Attributi[codiceAttributo];
            att.AllowSort = true;
            att.ValoreDefault = new ValoreTesto() { V = string.Empty };
            att.AllowValoriUnivoci = true;
            AttributiMasterCodes.Add(codiceAttributo);
            //
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.AttributoFilterText);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    Etichetta = LocalizationProvider.GetString("Filtro2"),
                    IsVisible = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.AllowValoriUnivoci = true;
            att.AllowReplaceInText = false;
            att.Height = 20;
            //
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.AttributoFilter);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    Etichetta = "AttributoFilter",
                    IsBuiltIn = true,
                    IsInternal = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = false;
            att.AllowReplaceInText = false;
            att.GroupName = LocalizationProvider.GetString("WBS");
            att.Height = 20;
            att.IsVisible = false;
            //
            //Lavoro (ore di lavoro)
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Lavoro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 h",
                    ValoreDefault = new ValoreReale() { V = "8" },
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Lavoro"),
                });
            }
            att = Attributi[codiceAttributo];
            

            //
            //Durata (giorni di lavoro)
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Durata);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 g",
                    ValoreDefault = new ValoreReale() { V = "1" },
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Durata"),
                });
            }
            att = Attributi[codiceAttributo];
            //
            //Durata (calendario) giorni effettivi di calendario
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.DurataCalendario);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 g",
                    ValoreDefault = new ValoreReale() { V = "1" },
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Durata (calendario)"),
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;

            //Data inizio
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.DataInizio);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("DataInizio"),
                });
            }
            att = Attributi[codiceAttributo];
            att.ValoreDefault = new ValoreData() { V = null };
            att.ValoreFormat = "dd/MM/yyyy HH:mm";

            //
            //Data fine
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.DataFine);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("DataFine"),
                });
            }
            att = Attributi[codiceAttributo];
            att.ValoreDefault = new ValoreData() { V = null };
            att.ValoreFormat = "dd/MM/yyyy HH:mm";

            //
            //Attività
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.ElencoAttivita, BuiltInCodes.Attributo.Id);
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    Etichetta = LocalizationProvider.GetString("Attivita riferita"),
                    IsVisible = false,
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowValoriUnivoci = true;
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.ElencoAttivita, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                    Etichetta = LocalizationProvider.GetString("Attivita"),

                });
            }
            att = Attributi[codiceAttributo];
            //att.Etichetta = LocalizationProvider.GetString("Attivita");
            //

            //Predecessori
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Predecessor);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    Etichetta = "Predecessor",
                    IsBuiltIn = true,
                    IsInternal = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = false;
            att.AllowReplaceInText = false;
            att.GroupName = LocalizationProvider.GetString("WBS");
            att.Height = 20;
            att.IsVisible = false;
            //
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.PredecessorText);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Predecessori"),
            });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.Height = 20;
            //att.Etichetta = LocalizationProvider.GetString("Predecessori");

            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.TaskNote);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowReplaceInText = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Note"),
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            //att.DetailViewOrder = viewOrder++;
            AttributiMasterCodes.Add(codiceAttributo);

            //ComputoItemIds
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.ComputoItemIds);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.GuidCollection].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    IsInternal = false,
                    GroupName = LocalizationProvider.GetString("Computo"),
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Computo,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("VociDiComputoRiferite"),
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.Height = 60;
            att.ValoreDefault = new ValoreGuidCollection();
            //

            //ComputoItemIds
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.TaskProgress);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    AllowReplaceInText = true,
                    ValoreFormat = "#.##0,00 %",//"P2",
                    GroupName = LocalizationProvider.GetString("Contabilita"),
                    IsVisible = true,
                    ValoreDefault = new ValoreReale() { V = "0" },
                    Etichetta = LocalizationProvider.GetString("percCompletamento"),
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowReplaceInText = false;
            att.Height = 20;
            //att.Etichetta = LocalizationProvider.GetString("percCompletamento");

            //

            //ElementiItemIds
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.ElementiItemIds);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.GuidCollection].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    IsInternal = false,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("VociDiElementiRiferite"),
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.Height = 20;
            att.ValoreDefault = new ValoreGuidCollection();
            //
            //Gruppo opzioni
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    GroupName = LocalizationProvider.GetString("Opzioni"),
                    IsVisible = false,
                    Etichetta = LocalizationProvider.GetString("CalendarioRiferito"),
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowValoriUnivoci = true;
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    DetailViewOrder = viewOrder++,
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Opzioni"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    Etichetta = LocalizationProvider.GetString("Calendario2"),
                });
            }
            att = Attributi[codiceAttributo];
            //att.Etichetta = LocalizationProvider.GetString("Calendario2");


            UpdateEtichetteMap();
        }

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            AssociedType = entityTypes["WBSItemParent"] as WBSItemParentType;
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
            string str = string.Join(AttributoCodiceSeparator, divTypeKey, divAttCodice);
            return str;
        }

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.WBS;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.WBS;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.WBS;

        public override MasterType MasterType => MasterType.Tree;

        public override EntityComparer EntityComparer { get; set; } = new WBSItemKeyComparer();

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(WBSItem))]
    public class WBSItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    [DataContract]
    public class WBSItemParentType : TreeEntityType
    {


        public WBSItemParentType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.WBSParent;// "WBSItemParent";
            Name = LocalizationProvider.GetString("WBS (Parent)");
            string codiceAttributo; //CodiceEntity + "_" + code
            int viewOrder = 0;
            FunctionName = EPCalculatorFunction.FunctionName;
            string pref = BuiltInCodes.EntityType.WBS;
            string sep = "_";
            Attributo att = null;
            AttributiMasterCodes.Clear();

            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            AttributiMasterCodes.Add(codiceAttributo);

            //
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    Height = 20,
                    IsVisible = true,
                });

            }
            att = Attributi[codiceAttributo];
            att.ValoreDefault = new ValoreTesto() { V = string.Empty };
            att.DetailViewOrder = viewOrder++;
            att.AllowValoriUnivoci = true;
            AttributiMasterCodes.Add(codiceAttributo);
            //
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.AttributoFilterText);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Filtro2"),
                    IsBuiltIn = true,

                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.IsValoreReadOnly = true;
            att.AllowReplaceInText = false;
            att.GroupName = LocalizationProvider.GetString("WBS");
            att.Height = 20;
            att.IsVisible = true;
            //
            //
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.AttributoFilter);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = "AttributoFilter",
                    IsBuiltIn = true,
                    IsInternal = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = false;
            att.AllowReplaceInText = false;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("WBS");
            att.Height = 20;
            att.IsVisible = false;
            //
            //Lavoro (ore di lavoro)
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Lavoro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 h",
                    ValoreDefault = new ValoreReale() { V = "1" },
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.DetailViewOrder = viewOrder++;
            att.IsSummary = true;
            att.Etichetta = LocalizationProvider.GetString("Lavoro");
            //
            //Durata (giorni di lavoro)
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.Durata);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 g",
                    ValoreDefault = new ValoreReale() { V = "1" },
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.DetailViewOrder = viewOrder++;
            att.IsSummary = true;
            att.Etichetta = LocalizationProvider.GetString("Durata");
            //
            //Durata (calendario) giorni effettivi di calendario
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DurataCalendario);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {

                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "0.00 g",
                    ValoreDefault = new ValoreReale() { V = "1" },
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.DetailViewOrder = viewOrder++;
            att.IsSummary = true;
            att.Etichetta = LocalizationProvider.GetString("Durata (calendario)");

            //Data inizio
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DataInizio);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.ValoreDefault = new ValoreData() { V = DateTime.Today };
            att.ValoreFormat = "dd/MM/yyyy HH:mm";
            att.DetailViewOrder = viewOrder++;
            att.IsSummary = true;
            att.Etichetta = LocalizationProvider.GetString("DataInizio");
            att.IsValoreReadOnly = true;

            //
            //Data fine
            codiceAttributo = string.Join(sep, BuiltInCodes.Attributo.DataFine);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    IsBuiltIn = true,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("WBS"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.IsValoreReadOnly = true;
            att.ValoreDefault = new ValoreData() { V = DateTime.Today };
            att.ValoreFormat = "dd/MM/yyyy HH:mm";
            att.DetailViewOrder = viewOrder++;
            att.IsSummary = true;
            att.Etichetta = LocalizationProvider.GetString("DataFine");





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

    public class WBSItemKeyComparer : EntityComparer
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