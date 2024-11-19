

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
    [KnownType(typeof(ElementiItemType))]
    public class ElementiItem : Entity
    {
        
        public ElementiItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Elementi;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
            //ValoreGuid valGuid = Attributi[codiceAttributoGuid].Valore as ValoreGuid;
            //return valGuid == null ? Guid.Empty : valGuid.V;
        }

        public override string ToUserIdentity(UserIdentityMode mode)
        {
            if (mode == UserIdentityMode.SingleLine1)
                return GetValoreAttributo(BuiltInCodes.Attributo.Nome, false, true).PlainText;

            return string.Empty;
        }
    }

    [ProtoContract]
    [DataContract]
    public class ElementiItemType : EntityType
    {
        private static string Separator = "_";
        
        public ElementiItemType() {}//protobuf

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Elementi;
            Name = LocalizationProvider.GetString("Elementi2");
            string codiceAttributo;
            int viewOrder = 0;
            FunctionName = ElmCalculatorFunction.FunctionName;
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);
            //IsTreeMaster = false;
            string referenceCodiceGuid = "";
            Attributo att = null;
            IEnumerable<DivisioneItemType> divisioniType = entityTypes.Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();
            
            bool createEmptyModel = true;

            codiceAttributo = string.Join(Separator, "IfcLabel");//es: #34453
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Etichetta Ifc"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{_Label}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }




            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IfcFileName);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("NomeFileIfc"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{_FileName}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.ProjectGlobalId);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("ProjectGlobalId"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Nothing,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.GlobalId);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("GlobalId"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Nothing,
                    //ValoreDefault = new ValoreTesto() { V = "ifc{GlobalId}" },
                    //IsValoreLockedByDefault = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Name"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Nothing,
                    ValoreDefault = new ValoreTesto() { V = "ifc{Name}" },
                    IsValoreLockedByDefault = false,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }

            //
            codiceAttributo = string.Join(Separator, "IfcClass");//es: IfcWallStandardCase
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Classe Ifc"),
                    IsBuiltIn = true,
                    AllowValoriUnivoci = true,
                    AllowSort = true,
                    AllowReplaceInText = true,
                    DetailViewOrder = viewOrder,
                    GroupName = LocalizationProvider.GetString("Elementi2"),
                    AllowMasterGrouping = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Nothing,
                    ValoreDefault = new ValoreTesto() { V = "ifc{_Class}" },
                    IsValoreLockedByDefault = false,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }


            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IfcGroup);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.GuidCollection].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Gruppi"),
            //        IsBuiltIn = true,
            //        GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.ElencoAttivita,
            //        GroupName = LocalizationProvider.GetString("Elementi2"),
            //        IsVisible = true,
            //        DetailViewOrder = viewOrder,
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.AllowValoriUnivoci = true;
            //att.AllowSort = false;
            //att.AllowMasterGrouping = false;
            //att.AllowReplaceInText = false;
            //att.Height = 60;
            //viewOrder++;
            ////


            //
            codiceAttributo = string.Join(Separator, "ObjectType");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("ObjectType"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{ObjectType}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }

            //
            codiceAttributo = string.Join(Separator, "Tag");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Tag"),
                        IsBuiltIn = true,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Elementi2"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{Tag}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }

            //
            //Tipo
            //DivisioneItemType divTipoType = entityTypes.Values.First(item => item.Name == "Tipi") as DivisioneItemType;
            DivisioneItemType divTipoType = divisioniType.FirstOrDefault(item => item.Model3dClassName == Model3dClassEnum.IfcElementType);
            if (divTipoType != null)
            {
                codiceAttributo = DivisioneAttributoCodice(divTipoType.GetKey(), BuiltInCodes.Attributo.Id);
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    if (!createEmptyModel)
                    {
                        Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                        {
                            Etichetta = LocalizationProvider.GetString("Tipo"),
                            //Etichetta = divTipoType.Name,
                            IsBuiltIn = true,
                            GuidReferenceEntityTypeKey = divTipoType.GetKey(),
                            DetailViewOrder = viewOrder,
                            IsVisible = true,
                            AllowValoriUnivoci = true,
                            GroupName = LocalizationProvider.GetString("Divisioni"),
                        });
                        viewOrder++;
                    }
                }
                else
                {
                    att = Attributi[codiceAttributo];
                    att.AllowValoriUnivoci = true;
                }


                codiceAttributo = string.Join(Separator, divTipoType.GetKey(), "Nome");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    if (!createEmptyModel)
                    {
                        Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                        {
                            Etichetta = LocalizationProvider.GetString("Nome tipo"),
                            IsBuiltIn = true,
                            ReferenceEntityTypeKey = divTipoType.GetKey(),
                            ReferenceCodiceAttributo = "Nome",
                            ReferenceCodiceGuid = referenceCodiceGuid,
                            DetailViewOrder = viewOrder,
                            AllowSort = true,
                            GroupName = LocalizationProvider.GetString("Divisioni"),
                            IsValoreReadOnly = true,
                            AllowMasterGrouping = true,
                            AllowValoriUnivoci = true,
                            IsVisible = true,
                            GroupOperation = ValoreOperationType.Nothing,

                        });
                        AttributiMasterCodes.Add(codiceAttributo);
                        viewOrder++;
                    }
                }

            }

            //Proprietà fase di creazione
            codiceAttributo = string.Join(Separator, "Fase di creazione");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Fase di creazione"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Proprietà"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{P>Fasi>Fase di creazione}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }


            //Quantità Area
            codiceAttributo = string.Join(Separator, "Contrassegno");
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                if (!createEmptyModel)
                {
                    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                    {
                        Etichetta = LocalizationProvider.GetString("Contrassegno"),
                        IsBuiltIn = false,
                        AllowValoriUnivoci = true,
                        AllowSort = true,
                        AllowReplaceInText = true,
                        DetailViewOrder = viewOrder,
                        GroupName = LocalizationProvider.GetString("Proprietà"),
                        AllowMasterGrouping = true,
                        IsVisible = true,
                        GroupOperation = ValoreOperationType.Nothing,
                        ValoreDefault = new ValoreTesto() { V = "ifc{P>Dati identità>Contrassegno}" },
                        IsValoreLockedByDefault = true,
                    });
                    AttributiMasterCodes.Add(codiceAttributo);
                    viewOrder++;
                }
            }




            //Livello
            //DivisioneItemType divLivType = entityTypes.Values.First(item => item.Name == "Livelli") as DivisioneItemType;
            DivisioneItemType divLivType = divisioniType.FirstOrDefault(item => item.Model3dClassName == Model3dClassEnum.IfcBuildingStorey);
            if (divLivType != null)
            {
                codiceAttributo = DivisioneAttributoCodice(divLivType.GetKey(), BuiltInCodes.Attributo.Id);
                referenceCodiceGuid = codiceAttributo;
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    if (!createEmptyModel)
                    {
                        Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                        {
                            IsBuiltIn = true,
                            Etichetta = LocalizationProvider.GetString("Livello"),
                            //Etichetta = divLivType.Name,
                            GuidReferenceEntityTypeKey = divLivType.GetKey(),
                            DetailViewOrder = viewOrder,
                            IsVisible = true,
                            GroupName = LocalizationProvider.GetString("Divisioni"),
                            AllowValoriUnivoci = true,
                        });
                        viewOrder++;
                    }
                }
                else
                {
                    att = Attributi[codiceAttributo];
                    att.AllowValoriUnivoci = true;
                }


                codiceAttributo = string.Join(Separator, divLivType.GetKey(), "Nome");
                if (!Attributi.ContainsKey(codiceAttributo))
                {
                    if (!createEmptyModel)
                    {
                        Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                        {
                            Etichetta = LocalizationProvider.GetString("Nome livello"),
                            IsBuiltIn = true,
                            ReferenceEntityTypeKey = divLivType.GetKey(),
                            ReferenceCodiceAttributo = "Nome",
                            ReferenceCodiceGuid = referenceCodiceGuid,
                            DetailViewOrder = viewOrder,
                            AllowSort = true,
                            GroupName = LocalizationProvider.GetString("Divisioni"),
                            IsValoreReadOnly = true,
                            AllowMasterGrouping = true,
                            AllowValoriUnivoci = true,
                            IsVisible = true,
                            GroupOperation = ValoreOperationType.Nothing,

                        });
                        AttributiMasterCodes.Add(codiceAttributo);
                        viewOrder++;
                    }
                }

            }
                //



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

        public static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        {
            string str = string.Join(Separator, divTypeKey, divAttCodice);
            return str;
        }

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Elementi;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Elementi;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Elementi;

        public override MasterType MasterType => MasterType.Grid;
        public override EntityComparer EntityComparer { get; set; } = new ElementiItemKeyComparer();

        public override bool IsCustomizable() { return true; }

    }

    [ProtoContract]
    [DataContract]
    public class ElementiItemCollection : EntityCollection
    {
    }

    public class ElementiItemKeyComparer : EntityComparer
    {
        public override List<string> AttributiCode { get; set; } = new List<string>()
        { BuiltInCodes.Attributo.ProjectGlobalId, BuiltInCodes.Attributo.GlobalId };

        public override bool Equals(string key1, string key2)
        {
            if (key1 == key2)
                return true;

            return false;
        }
    }

}