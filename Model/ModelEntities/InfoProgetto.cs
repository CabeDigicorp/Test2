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
    [KnownType(typeof(InfoProgettoItemType))]
    public class InfoProgettoItem : Entity
    {
        
        public InfoProgettoItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.InfoProgetto;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);

            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

    }

    [ProtoContract]
    [DataContract]
    public class InfoProgettoItemType : EntityType
    {
        public static string AttributoCodiceSeparator { get => "_"; }

        public InfoProgettoItemType() { }//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.InfoProgetto;
            Name = LocalizationProvider.GetString("InfoProgetto");
            FunctionName = InfCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = false;
            Attributo att = null;
            string referenceCodiceGuid = "";

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //foreach (var att1 in Attributi.Values)
            //{
            //    if (att1.Codice == BuiltInCodes.Attributo.OggettoLavori)
            //        continue;

            //    if (att1.Codice == BuiltInCodes.Attributo.DataInizio)
            //        continue;

            //    if (att1.Codice == string.Join(Separator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id))
            //        continue;

            //    if (att1.Codice == string.Join(Separator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Codice))
            //        continue;

            //    att1.IsBuiltIn = false;

            //}


            //Gruppo Dati progetto
            codiceAttributo = BuiltInCodes.Attributo.OggettoLavori;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("LavoriDi"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("DatiProgetto"),
                    IsVisible = true,
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;

            //Gruppo date
            //Data Inizio Gantt
            //codiceAttributo = BuiltInCodes.Attributo.DataInizioGantt;
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("DataInizioGantt"),
            //        IsBuiltIn = true,
            //        GroupName = LocalizationProvider.GetString("Date"),
            //        IsVisible = true,
            //        ValoreFormat = "dd/MM/yyyy",
            //        ValoreDefault = new ValoreData() { V = DateTime.Today },
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.IsValoreReadOnly = true;
            //att.DetailViewOrder = viewOrder++;
            //
            //Data Inizio lavori
            codiceAttributo = BuiltInCodes.Attributo.DataInizio;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("DataInizioLavori"),
                    IsBuiltIn = true,
                    GroupName = LocalizationProvider.GetString("Date"),
                    IsVisible = true,
                    ValoreFormat = "dd/MM/yyyy",
                    ValoreDefault = new ValoreData() { V = DateTime.Today },
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //
            ////Data Fine Gantt
            //codiceAttributo = BuiltInCodes.Attributo.DataFineGantt;
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("DataFineGantt"),
            //        IsBuiltIn = true,
            //        GroupName = LocalizationProvider.GetString("Date"),
            //        IsVisible = true,
            //        ValoreFormat = "dd/MM/yyyy",
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.IsValoreReadOnly = true;
            //att.DetailViewOrder = viewOrder++;
            //
            //Data Fine lavori
            codiceAttributo = BuiltInCodes.Attributo.DataFine;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("DataFineLavori"),
                    IsBuiltIn = true,
                    GroupName = LocalizationProvider.GetString("Date"),
                    IsVisible = true,
                    ValoreFormat = "dd/MM/yyyy",
                    ValoreDefault = new ValoreData() { V = DateTime.Today },
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //
            //
            //calendario di progetto
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Id);
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("CalendarioRiferito"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    GroupName = LocalizationProvider.GetString("Date"),
                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.AllowValoriUnivoci = true;
            att.IsVisible = false;
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Calendari, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Calendario2"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Date"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,

                });
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;



            bool isEmptyModel = true;
            if (!isEmptyModel)
            {
                //
                codiceAttributo = "IndirizzoLavori";
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Indirizzo"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("DatiProgetto"),
                        IsVisible = true,
                    });
                }
                //

                codiceAttributo = "ImportoLavori";
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Contabilita].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ImportoLavori"),
                        IsBuiltIn = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("DatiProgetto"),
                        ValoreFormat = "#,##0.00 €",
                        IsVisible = true,
                    });
                }
                //
                codiceAttributo = "Autorizzazioni";
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Autorizzazioni"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("DatiProgetto"),
                        IsVisible = true,
                    });
                }

                //Gruppo Committenti

                //Committente (Contatti)
                codiceAttributo = "Committente";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("Committente")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Committenti"),
                    });
                }
                //
                codiceAttributo = string.Join(AttributoCodiceSeparator, "Committente", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Committente"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Committenti"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }
                //
                //Gruppo Progettisti

                //Progetto archittettonico (Contatti)
                codiceAttributo = "ProgettoArchitettonico";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("ProgettoArchitettonico")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                    });
                }
                //
                codiceAttributo = string.Join(AttributoCodiceSeparator, "ProgettoArchitettonico", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ProgettoArchitettonico"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }
                //
                //Progetto strutture (Contatti)
                codiceAttributo = "ProgettoStrutture";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("ProgettoStrutture")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                    });
                }
                //
                codiceAttributo = string.Join(AttributoCodiceSeparator, "ProgettoStrutture", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ProgettoStrutture"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }
                //
                //Progetto impianti (Contatti)
                codiceAttributo = "ProgettoImpianti";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("ProgettoImpianti")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                    });
                }
                //
                codiceAttributo = string.Join(AttributoCodiceSeparator, "ProgettoImpianti", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ProgettoImpianti"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }
                //
                //Direzione lavori (Contatti)
                codiceAttributo = "DirezioneLavori";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("DirezioneLavori")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                    });
                }
                //
                codiceAttributo = string.Join(AttributoCodiceSeparator, "DirezioneLavori", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("DirezioneLavori"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Progettisti"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }
                //
                //Gruppo date
                //Data Inizio lavori
                codiceAttributo = "DataInizioLavori";
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("DataInizioLavori"),
                        IsBuiltIn = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Date"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,
                    });
                }
                //
                //Data fine lavori
                codiceAttributo = "DataFineLavori";
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("DataFineLavori"),
                        IsBuiltIn = true,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Date"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,
                    });
                }
                //
                //Gruppo Imprese
                //Impresa esecutrice
                codiceAttributo = "ImpresaEsecutrice";
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                    {
                        Etichetta = string.Format("{0} >", LocalizationProvider.GetString("ImpresaEsecutrice")),
                        IsBuiltIn = true,
                        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        DetailViewOrder = viewOrder++,
                        IsVisible = false,
                        GroupName = LocalizationProvider.GetString("Imprese"),
                    });
                }
                //

                codiceAttributo = string.Join(AttributoCodiceSeparator, "ImpresaEsecutrice", BuiltInCodes.Attributo.Nome);
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ImpresaEsecutrice"),
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Contatti,
                        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        GroupName = LocalizationProvider.GetString("Imprese"),
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                }

            }

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
            return BuiltInCodes.EntityType.InfoProgetto;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.InfoProgetto;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.InfoProgetto;

        public override MasterType MasterType => MasterType.NoMaster;

        public override bool IsCustomizable() { return true; }
    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(InfoProgettoItem))]
    public class InfoProgettoItemCollection : EntityCollection
    {
    }


}