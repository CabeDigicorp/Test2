using _3DModelExchange;
using CommonResources;
using Commons;
using MasterDetailModel;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    [DataContract]
    [KnownType(typeof(DocumentiItemType))]
    [KnownType(typeof(DocumentiItemParentType))]
    public class DocumentiItem : TreeEntity
    {
        public DocumentiItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Documenti;
            ParentEntityTypeCodice = "DocumentiItemParent";
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
            {
                //base.CopyValoriAttributiFrom(ent);
            }
            else if (isEntParent)
            {
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

    }
    [ProtoContract]
    [DataContract]
    public class DocumentiItemType : TreeEntityType
    {
        static string Separator = "_";

        public DocumentiItemType() { }//protobuf

        //public PrezzarioItemParentType PrezzarioItemParentType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Documenti;
            Name = LocalizationProvider.GetString("Documenti");
            FunctionName = EPCalculatorFunction.FunctionName;
            string codiceAttributo;
            int viewOrder = 0;
            //IsTreeMaster = true;
            Attributo att = null;
            AttributoRiferimento attRef = null;
            string referenceCodiceGuid = "";
            List<ValoreAttributoElencoItem> Lista;

            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //Attributi.Clear();
            AttributiMasterCodes.Clear();


            //
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
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Documento"),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            AttributiMasterCodes.Add(codiceAttributo);

            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Nome");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.AllowReplaceInText = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Id);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Report");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Report;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Codice);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Codice"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.DescrizioneReport);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Descrizione report"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.DescrizioneReport,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Sezione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Sezione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Sezione,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    AllowSort = true,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsValoreReadOnly = true,
                    AllowMasterGrouping = true,
                    AllowValoriUnivoci = true,
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;


            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Intestazione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Intestazione");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Corpo);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Descrizione");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Firme);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Firme");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePagina);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Piè pagina");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;
            //


            //Tag
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Tag);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.GuidCollection].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Tag"),
                    IsBuiltIn = true,
                    GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Tag,
                    GroupName = LocalizationProvider.GetString("Documento"),
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
            att.ValoreAttributo = new ValoreAttributoGuidCollection() { ItemsSelectionType = ItemsSelectionTypeEnum.ByHand };
            //


            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.DimensioniPagina);
            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "A3 (cm 29,7 x cm 42,0)", "A4 (cm 21 x cm 29,7)", "A5 (cm 14,8 x cm 21,0)" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A3 (cm 29,7 x cm 42,0)", Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A4 (cm 21 x cm 29,7)", Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A5 (cm 14,8 x cm 21,0)", Id = 2 });


            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = Lista.Where(l => l.Text == "A4 (cm 21 x cm 29,7)").FirstOrDefault().Id },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Dimensioni");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("ImpostazioniPagina");
            att.IsVisible = true;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Orientamento);

            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "Verticale", "Orizzontale" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Verticale"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Orizzontale"), Id = 1 });
            
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = 0 },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Orientamento");
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("ImpostazioniPagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineSuperiore);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Superiore");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineInferiore);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Inferiore");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineSinistro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Sinistro");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineDestro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Destro");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.StampaNuovaPagina);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    IsBuiltIn = false,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("OpzioniDiStampa"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("StampaNuovaPagina");
            att.GroupName = LocalizationProvider.GetString("OpzioniDiStampa");
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.StampaVoci);
            Lista = new List<ValoreAttributoElencoItem>();
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Tutte"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Selezionate"), Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Filtrate"), Id = 2 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("StampaVoci");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("OpzioniDiStampa");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = true;
            att.ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = 0 };
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IniziaDaPag);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IniziaDaPag");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = true;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;


            //VISIBILITA SOLO OIN ADMIN PER QUESTI
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneSinistra);
            Lista = new List<ValoreAttributoElencoItem>();
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("_Nessuno"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina"), Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina/NtotalePagine"), Id = 2 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Intestazione"), Id = 3 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneSinistra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneCentrale);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneCentrale");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneDestra);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneDestra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaSinistra);
            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "<nessuno>", "N° pagina", "N° pagina / N totale pagine", "Piè pagina" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("_Nessuno"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina"), Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina/NtotalePagine"), Id = 2 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Piè pagina"), Id = 3 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaSinistra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaCentrale);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaCentrale");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaDestra);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaDestra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StileIntestazioneId);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Stile");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StileIntestazione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StileIntestazione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StilePiepaginaId);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Stile");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StilePiepagina);            
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StilePiePagina"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.Id);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("StileNumerazione");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.Codice);

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StileNumerazione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //att.Etichetta = LocalizationProvider.GetString("StileNumerazione");
            att.GroupName = LocalizationProvider.GetString("Stili");
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;

            UpdateEtichetteMap();
        }

        public override bool AttributoIsPreviewable(string referenceCodiceAttributo)
        {
            return AssociedType.Attributi.ContainsKey(referenceCodiceAttributo);
        }

        public override void ResolveReferences(Dictionary<string, EntityType> entityTypes, Dictionary<string, DefinizioneAttributo> definizioniAttributo)
        {
            AssociedType = entityTypes["DocumentiItemParent"] as DocumentiItemParentType;
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

        public static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        {
            string str = string.Join(Separator, divTypeKey, divAttCodice);
            return str;
        }

        static public string CreateKey()
        {
            return BuiltInCodes.EntityType.Documenti;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Documenti;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Documenti;

        public override MasterType MasterType => MasterType.Tree;

        public override EntityComparer EntityComparer { get; set; } = new DocumentiItemKeyComparer();

        public override bool IsCustomizable() { return false; }

    }


    [ProtoContract]
    [DataContract]
    [KnownType(typeof(DocumentiItem))]
    public class DocumentiItemCollection : TreeEntityCollection
    {
    }

    [ProtoContract]
    [DataContract]
    public class DocumentiItemParentType : TreeEntityType
    {

        private static string Separator = "_";
        public DocumentiItemParentType() { }//protobuf

        //public PrezzarioItemType PrezzarioItemType { get; set; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.DocumentiParent;// "DocumentiItemParent";
            Name = LocalizationProvider.GetString("Documenti (Parent)");
            string codiceAttributo; //CodiceEntity + "_" + code
            int viewOrder = 0;
            FunctionName = EPCalculatorFunction.FunctionName;
            string pref = BuiltInCodes.EntityType.Documenti;
            string Separator = "_";
            string referenceCodiceGuid = "";
            string emptyRtf;
            Attributo att = null;
            AttributoRiferimento attRif = null;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);

            //IsTreeMaster = true;
            AttributiMasterCodes.Clear();

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
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Documento"),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Nome);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            AttributiMasterCodes.Add(codiceAttributo);

            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Nome");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.AllowReplaceInText = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            //




            //codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Id);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            //referenceCodiceGuid = codiceAttributo;
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
            //    {
            //        //DetailViewOrder = viewOrder++,
            //    });
            //}
            //att = Attributi[codiceAttributo];
            //att.Etichetta = LocalizationProvider.GetString("Report");
            //att.IsBuiltIn = true;
            //att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Report;
            //att.IsVisible = false;
            //att.GroupName = LocalizationProvider.GetString("Report");
            //att.DetailViewOrder = viewOrder++;

            ////
            //codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Codice);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Codice"),
            //        IsBuiltIn = true,
            //        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
            //        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Codice,
            //        ReferenceCodiceGuid = referenceCodiceGuid,
            //        //DetailViewOrder = viewOrder++,
            //        GroupName = LocalizationProvider.GetString("Report"),
            //        IsValoreReadOnly = true,
            //        Height = 20,
            //        ValoreDefault = new ValoreTesto() { V = null },
            //        IsVisible = true,
            //        GroupOperation = ValoreOperationType.Equivalent,

            //    });
            //    //AttributiMasterCodes.Add(codiceAttributo);
            //}
            //att = Attributi[codiceAttributo];
            //att.DetailViewOrder = viewOrder++;

            ////
            //codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.DescrizioneReport);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Descrizione report"),
            //        IsBuiltIn = true,
            //        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
            //        ReferenceCodiceAttributo = BuiltInCodes.Attributo.DescrizioneReport,
            //        ReferenceCodiceGuid = referenceCodiceGuid,
            //        //DetailViewOrder = viewOrder++,
            //        GroupName = LocalizationProvider.GetString("Report"),
            //        IsValoreReadOnly = true,
            //        Height = 20,
            //        ValoreDefault = new ValoreTesto() { V = null },
            //        IsVisible = true,
            //        GroupOperation = ValoreOperationType.Equivalent,

            //    });
            //    //AttributiMasterCodes.Add(codiceAttributo);
            //}
            //att = Attributi[codiceAttributo];
            //att.DetailViewOrder = viewOrder++;
            ////
            //codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Report, BuiltInCodes.Attributo.Sezione);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
            //    {
            //        Etichetta = LocalizationProvider.GetString("Sezione"),
            //        IsBuiltIn = true,
            //        ReferenceEntityTypeKey = BuiltInCodes.EntityType.Report,
            //        ReferenceCodiceAttributo = BuiltInCodes.Attributo.Sezione,
            //        ReferenceCodiceGuid = referenceCodiceGuid,
            //        //DetailViewOrder = viewOrder++,
            //        AllowSort = true,
            //        GroupName = LocalizationProvider.GetString("Report"),
            //        IsValoreReadOnly = true,
            //        AllowMasterGrouping = true,
            //        AllowValoriUnivoci = true,
            //        IsVisible = true,
            //        GroupOperation = ValoreOperationType.Equivalent,

            //    });
            //    //AttributiMasterCodes.Add(codiceAttributo);
            //}
            //att = Attributi[codiceAttributo];
            //att.DetailViewOrder = viewOrder++;



            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Intestazione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Intestazione");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Corpo);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Descrizione");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Firme);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Firme");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePagina);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.TestoRTF].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Piè pagina");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Documento");
            att.IsVisible = true;
            att.ValoreDefault = new ValoreTestoRtf() { V = RtfHelperDevExpress.RtfDefault };
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.DimensioniPagina);
            List<ValoreAttributoElencoItem> Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "A3 (cm 29,7 x cm 42,0)", "A4 (cm 21 x cm 29,7)", "A5 (cm 14,8 x cm 21,0)" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A3 (cm 29,7 x cm 42,0)", Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A4 (cm 21 x cm 29,7)", Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = "A5 (cm 14,8 x cm 21,0)", Id = 2 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = Lista.Where(l => l.Text == "A4 (cm 21 x cm 29,7)").FirstOrDefault().Id },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Dimensioni");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("ImpostazioniPagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Orientamento);
            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "Verticale", "Orizzontale" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Verticale"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Orizzontale"), Id = 1 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                    ValoreDefault = new ValoreElenco() { ValoreAttributoElencoId = 0},
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Orientamento");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("ImpostazioniPagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineSuperiore);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Superiore");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineInferiore);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Inferiore");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineSinistro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Sinistro");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.MargineDestro);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    ValoreFormat = "#,##0.00 [cm]",
                    ValoreDefault = new ValoreReale() { V = "1" },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Destro");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("Margini");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneSinistra);
            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "<nessuno>", "N° pagina", "N° pagina / N totale pagine", "Intestazione" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("_Nessuno"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina"), Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina/NtotalePagine"), Id = 2 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Intestazione"), Id = 3 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneSinistra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneCentrale);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneCentrale");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IntestazioneDestra);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IntestazioneDestra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutIntestazione");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaSinistra);
            Lista = new List<ValoreAttributoElencoItem>();
            //foreach (var item in new List<string>() { "<nessuno>", "N° pagina", "N° pagina / N totale pagine", "Piè pagina" }) { Lista.Add(new ValoreAttributoElencoItem() { Text = item, Id = Guid.NewGuid() }); };
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("_Nessuno"), Id = 0 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina"), Id = 1 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Npagina/NtotalePagine"), Id = 2 });
            Lista.Add(new ValoreAttributoElencoItem() { Text = LocalizationProvider.GetString("Piè pagina"), Id = 3 });

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    //ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaSinistra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;
            att.ValoreAttributo = new ValoreAttributoElenco() { Items = Lista };
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaCentrale);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaCentrale");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.PiePaginaDestra);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = new ValoreAttributoElenco() { Items = Lista },
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("PiePaginaDestra");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.GroupName = LocalizationProvider.GetString("LayoutPiePagina");
            att.IsVisible = true;
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.StampaNuovaPagina);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    IsBuiltIn = false,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("OpzioniDiStampa"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("StampaNuovaPagina");
            att.GroupName = LocalizationProvider.GetString("OpzioniDiStampa");
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IniziaDaPag);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("IniziaDaPag");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = true;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StileIntestazioneId);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Stile");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StileIntestazione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StileIntestazione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StilePiepaginaId);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Stile");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.StilePiepagina);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StilePiePagina"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.Id);  //BuiltInCodes.Attributo.CapitoliItem_Guid;
            referenceCodiceGuid = codiceAttributo;
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Guid].Clone(), this, codiceAttributo)
                {
                    //DetailViewOrder = viewOrder++,
                });
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("StileNumerazione");
            att.IsBuiltIn = true;
            att.GuidReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili;
            att.IsVisible = false;
            att.GroupName = LocalizationProvider.GetString("NumerazionePagina");
            att.DetailViewOrder = viewOrder++;

            codiceAttributo = string.Join(Separator, BuiltInCodes.EntityType.Stili, BuiltInCodes.Attributo.Codice);

            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new AttributoRiferimento(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Riferimento].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("StileNumerazione"),
                    IsBuiltIn = true,
                    ReferenceEntityTypeKey = BuiltInCodes.EntityType.Stili,
                    ReferenceCodiceAttributo = BuiltInCodes.Attributo.Nome,
                    ReferenceCodiceGuid = referenceCodiceGuid,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Stili"),
                    IsValoreReadOnly = true,
                    Height = 20,
                    ValoreDefault = new ValoreTesto() { V = null },
                    IsVisible = true,
                    GroupOperation = ValoreOperationType.Equivalent,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.DetailViewOrder = viewOrder++;
            //att.Etichetta = LocalizationProvider.GetString("StileNumerazione");
            att.GroupName = LocalizationProvider.GetString("Stili");

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

        public static string DivisioneAttributoCodice(string divTypeKey, string divAttCodice)
        {
            string str = string.Join(Separator, divTypeKey, divAttCodice);
            return str;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Nothing;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Nothing;

        public override MasterType MasterType => MasterType.Tree;
        public override EntityComparer EntityComparer { get; set; } = new DocumentiItemKeyComparer();

        public override bool IsParentType() { return true; }

        public override bool IsCustomizable() { return false; }
    }

    public class DocumentiItemKeyComparer : EntityComparer
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
