

using Commons;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using MasterDetailModel;
using _3DModelExchange;
using CommonResources;

namespace Model
{

    [ProtoContract]
    [DataContract]
    //[KnownType(typeof(PrezzarioItemType))]
    //[KnownType(typeof(PrezzarioItemParentType))]
    [KnownType(typeof(ComputoItemType))]
    public class ComputoItem : Entity
    {
        
        public ComputoItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Computo;
        }

        public Guid GetPrezzarioItemId()
        {
            return GetAttributoGuidId(BuiltInCodes.Attributo.PrezzarioItem_Guid);

            //ValoreGuid valGuid = Attributi[BuiltInCodes.PrezzarioItem_Guid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        //public string GetModelItemId()
        //{
        //    return Attributi[BuiltInCodes.ElementiItem_Guid].Valore.ToPlainText();
        //}

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        public Guid GetElementiItemId()
        {
            return GetAttributoGuidId(BuiltInCodes.Attributo.ElementiItem_Guid);
            //ValoreGuid valGuid = Attributi[BuiltInCodes.ElementiItem_Guid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        public Guid GetRuleId()
        {
            return GetAttributoGuidId(BuiltInCodes.Attributo.Model3dRuleId);
            //ValoreGuid valGuid = Attributi[BuiltInCodes.Attributo.Model3dRuleId].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        /// <summary>
        /// Restituisce tutti gli elementi collegati alla voce di computo
        /// </summary>
        /// <returns></returns>
        public HashSet<Guid> GetElementiItemsId()
        {
            HashSet<Guid> elemsId = new HashSet<Guid>();

            //var entAttsRif = Attributi.Values.Where(item =>
            foreach (EntityAttributo entAtt in Attributi.Values)
            {
                if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid && entAtt.Attributo.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                {
                    elemsId.Add((entAtt.Valore as ValoreGuid).V);
                }

                if (entAtt.Attributo.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.GuidCollection && entAtt.Attributo.GuidReferenceEntityTypeKey == BuiltInCodes.EntityType.Elementi)
                {
                    ValoreGuidCollection guidColl = entAtt.Valore as ValoreGuidCollection;
                    elemsId.UnionWith(guidColl.GetEntitiesId());
                }
            }
            return elemsId;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            if (mode == UserIdentityMode.SingleLine1)
                return GetValoreAttributo(BuiltInCodes.Attributo.DescrizioneQta, false, true).PlainText;

            return string.Empty;
        }
    }

    [ProtoContract]
    [DataContract]
    public class ComputoItemType : EntityType
    {
        public static string AttributoCodiceSeparator { get => "_"; }

        public ComputoItemType() {}//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Computo;
            Name = LocalizationProvider.GetString("Computo");
            string codiceAttributo;
            int viewOrder = 0;
            FunctionName = CmpCalculatorFunction.FunctionName;

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);
            //IsTreeMaster = false;
            string referenceCodiceGuid = "";
            Attributo att = null;

            IEnumerable<DivisioneItemType> divisioniType = entityTypes.Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();


            bool isEmptyModel = true;

            //Gruppo Articolo
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Id); //BuiltInCodes.Attributo.PrezzarioItem_Guid;// string.Join(Separator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Guid);
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Articolo2"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    DetailViewOrder = viewOrder++,
                    IsVisible = true,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                });
            }
            att = Attributi[codiceAttributo];
            att.AllowValoriUnivoci = true;
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("CodiceArticolo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.DescrizioneRTF);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.DescrizioneRTF,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTestoRtf() { V = emptyRtf },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.UnitaMisura);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("UnitaDiMisura"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.UnitaMisura,// "UnitaMisura",/*PrezzarioItem_*/
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.FormatoQuantita);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("FormatoQuantita"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.FormatoQuantita,// "UnitaMisura",/*PrezzarioItem_*/
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }

            //

            codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Prezzo);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Prezzo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Prezzo,/*PrezzarioItem_*/
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    ValoreFormat = "#,##0.00 €",
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            //Capitoli
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("CodiceCapitolo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.Codice),
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.EntityType.Prezzario, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.DescrizioneRTF);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("DescrizioneCapitolo"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Capitoli, BuiltInCodes.Attributo.DescrizioneRTF),
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = false,
                    AllowValoriUnivoci = false,
                    Height = 20,
                    ValoreDefault = new ValoreTestoRtf() { V = emptyRtf },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            //Attività dell'articolo
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Attivita);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("ArticoloAttivita"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Prezzario,
                    ReferenceCodiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Attivita),
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("ElencoPrezzi"),
                    IsValoreReadOnly = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                att = Attributi[codiceAttributo];
                att.AllowValoriUnivoci = true;
                att.AllowSort = false;
                att.AllowMasterGrouping = false;
                att.AllowReplaceInText = false;
                att.Height = 60;
            }


            //Gruppo Elemento
            codiceAttributo = codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, BuiltInCodes.Attributo.Id); //BuiltInCodes.Attributo.ElementiItem_Guid;// string.Join(Separator, BuiltInCodes.EntityType.Elementi, BuiltInCodes.Attributo.Guid);
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Elemento"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                    DetailViewOrder = viewOrder++,
                    IsVisible = true,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                });
            }

            if (!isEmptyModel)
            {

                //IfcLabel elemento
                codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.EntityType.Elementi, "IfcLabel");
                if (!Attributi.ContainsKey(codiceAttributo))
                {

                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = "Etichetta Ifc",
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                        ReferenceCodiceAttributo = "IfcLabel",/*PrezzarioItem_*/
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        AllowSort = true,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        IsValoreReadOnly = true,
                        AllowMasterGrouping = true,
                        AllowValoriUnivoci = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                }
                //Classe Elemento
                codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, "IfcClass");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = "Classe Ifc",
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                        ReferenceCodiceAttributo = "IfcClass",
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        AllowSort = true,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        IsValoreReadOnly = true,
                        AllowMasterGrouping = true,
                        AllowValoriUnivoci = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                }
            }
            //Name elemento
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Nome"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    DetailViewOrder = viewOrder++,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            if (!isEmptyModel)
            {
                //Tag Elemento
                codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, "Tag");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                    {
                        Etichetta = "Tag",
                        IsBuiltIn = true,
                        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                        ReferenceCodiceAttributo = "Tag",
                        ReferenceCodiceGuid = referenceCodiceGuid,
                        DetailViewOrder = viewOrder++,
                        AllowSort = true,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        IsValoreReadOnly = true,
                        AllowMasterGrouping = true,
                        AllowValoriUnivoci = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Equivalent,

                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                }


                //Tipo Elemento
                //DivisioneItemType divTipoType = entityTypes.Values.First(item => item.Name == "Tipi") as DivisioneItemType;
                DivisioneItemType divTipoType = divisioniType.FirstOrDefault(item => item.Model3dClassName == Model3dClassEnum.IfcElementType);
                if (divTipoType != null)
                {
                    codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, divTipoType.GetKey(), "Nome");
                    if (!Attributi.ContainsKey(codiceAttributo))
                    {
                        Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                        {
                            Etichetta = "Tipo",
                            IsBuiltIn = true,
                            ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                            ReferenceCodiceAttributo = DivisioneAttributoCodice(divTipoType.GetKey(), "Nome"),
                            ReferenceCodiceGuid = referenceCodiceGuid,
                            DetailViewOrder = viewOrder++,
                            AllowSort = true,
                            GroupName = LocalizationProvider.GetString("Elementi2"),
                            IsValoreReadOnly = true,
                            AllowMasterGrouping = true,
                            AllowValoriUnivoci = true,
                            IsVisible = true,
                            GroupOperation = ValoreOperationType.Equivalent,

                        });
                        AttributiMasterCodes.Add(codiceAttributo);
                    }
                }
                //Livello Elemento
                //DivisioneItemType divLivelloType = entityTypes.Values.First(item => item.Name == "Livelli") as DivisioneItemType;
                DivisioneItemType divLivelloType = divisioniType.FirstOrDefault(item => item.Model3dClassName == Model3dClassEnum.IfcBuildingStorey);
                if (divLivelloType != null)
                {
                    codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Elementi, divLivelloType.GetKey(), "Nome");
                    if (!Attributi.ContainsKey(codiceAttributo))
                    {
                        Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                        {
                            Etichetta = "Livello",
                            IsBuiltIn = true,
                            ReferenceEntityTypeKey = BuiltInCodes.EntityType.Elementi,
                            ReferenceCodiceAttributo = DivisioneAttributoCodice(divLivelloType.GetKey(), "Nome"),
                            ReferenceCodiceGuid = referenceCodiceGuid,
                            DetailViewOrder = viewOrder++,
                            AllowSort = true,
                            GroupName = LocalizationProvider.GetString("Elementi2"),
                            IsValoreReadOnly = true,
                            AllowMasterGrouping = true,
                            AllowValoriUnivoci = true,
                            IsVisible = true,
                            GroupOperation = ValoreOperationType.Equivalent,
                        });
                        AttributiMasterCodes.Add(codiceAttributo);
                    }
                }

            }

            //Gruppo Quantità
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Data);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Data].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Data"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.DescrizioneQta);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("DescrizioneQuantita"),
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    IsVisible = true,
                    DetailViewOrder = viewOrder++,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.AllowMasterGrouping = true;
            att.AllowReplaceInText = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.IsBuiltIn = true;
            att.GroupOperation = ValoreOperationType.Equivalent;
            
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.PU);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("PartiUguali"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "#,##0.00",
                    //GroupOperation = GroupOperationType.Equivalent,
                    ValoreDefault = new ValoreReale() { V = null, RealResult = 1 },
                    IsVisible = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.GroupOperation = ValoreOperationType.Sum;

            //
            //Rules
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Model3dRule);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("FiltroRegola"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    IsVisible = true,
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    GroupOperation = ValoreOperationType.Equivalent,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Model3dRuleId);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = "Id Regola",
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = "",
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    IsVisible = false,
                    IsValoreReadOnly = true,
                    IsInternal = true,

                });
            }
            //AttributiMasterCodes.Add(codiceAttributo);
            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Model3dRuleElementiItemId);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    Etichetta = "Id Elemento Regola",
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = "",
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    IsVisible = false,
                    IsValoreReadOnly = true,
                    IsInternal = true,

                });
            }


            codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.Attributo.Quantita);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Quantita"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    AllowValoriUnivoci = true,
                    AllowMasterGrouping = true,
                    ValoreFormat = "#,##0.00",
                    //GroupOperation = GroupOperationType.Nothing,
                    ValoreDefault = new ValoreReale() { V = null, RealResult = 0 },
                    IsVisible = true,

                });
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.GroupOperation = ValoreOperationType.Nothing;
            //att.ValoreDefault = new ValoreReale() { V = null, RealResult = 0 };

            //
            codiceAttributo = string.Join(AttributoCodiceSeparator, /*Codice, */BuiltInCodes.Attributo.QuantitaTotale);

            if (!Attributi.ContainsKey(codiceAttributo))
            {       
                string valQtaTotDefault = string.Format("{0}{1}{2}{3}{4}", "att{", Attributi[BuiltInCodes.Attributo.PU].Etichetta, "} * att{", Attributi[BuiltInCodes.Attributo.Quantita].Etichetta, "}");

                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("QuantitaTotale"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    AllowValoriUnivoci = true,
                    IsValoreReadOnly = true,
                    ValoreFormat = "#,##0.00",
                    //ValoreDefault = new ValoreReale() { V = "att{Parti Uguali} * att{Quantità}", RealResult = 0 },
                    GroupOperation = ValoreOperationType.Sum,
                    IsVisible = true,
                    IsValoreLockedByDefault = true,
                    ValoreDefault = new ValoreReale() { V = valQtaTotDefault, RealResult = 0 },
                }) ;
                AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            //att.ValoreDefault = new ValoreReale() { V = "att{Parti uguali} * att{Quantità}", RealResult = 0 };

            //////////////////////////////////////////////////////////////////////////////////////
            codiceAttributo = string.Join(AttributoCodiceSeparator, BuiltInCodes.Attributo.Importo);
            if (!Attributi.ContainsKey(codiceAttributo))
            { 
                string codicePrezzo = string.Join(AttributoCodiceSeparator, BuiltInCodes.EntityType.Prezzario, BuiltInCodes.Attributo.Prezzo);
                string valImpDefault = string.Format("{0}{1}{2}{3}{4}", "att{", Attributi[codicePrezzo].Etichetta, "} * att{", Attributi[BuiltInCodes.Attributo.QuantitaTotale].Etichetta, "}");

                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Contabilita].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Importo"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Quantita"),
                    AllowValoriUnivoci = true,
                    IsValoreReadOnly = true,
                    ValoreFormat = "#,##0.00 €",
                    ValoreDefault = new ValoreContabilita() { V = valImpDefault, RealResult = 0 },
                    GroupOperation = ValoreOperationType.Sum,
                    IsVisible = true,
                    IsValoreLockedByDefault = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
            }

            UpdateEtichetteMap();
        }

        private void AddAttributo(DefinizioneAttributo definizioneAttributo, string codice, string etichetta, bool allowReplaceInText)
        {
            Attributi.Add(codice, new Attributo(definizioneAttributo.Clone(), this, codice) { Etichetta = etichetta, AllowReplaceInText = allowReplaceInText });
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
            return BuiltInCodes.EntityType.Computo;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Computo;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Computo;

        public override MasterType MasterType => MasterType.Grid;
        public override EntityComparer EntityComparer { get; set; } = new ComputoItemKeyComparer();
        public override bool IsCustomizable() { return true; }

        //public override HashSet<string> LimitedEntityTypesOnImport() { return new HashSet<string>() { BuiltInCodes.EntityType.Computo }; }
    }

    [ProtoContract]
    [DataContract]
    [KnownType(typeof(PrezzarioItem))]
    public class ComputoItemCollection : EntityCollection
    {
    }

    public class ComputoItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.Id };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }

}