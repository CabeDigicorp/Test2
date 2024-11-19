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
    [KnownType(typeof(ReportItemType))]
    public class ReportItem : Entity
    {

        public ReportItem()
        {
            EntityId = Guid.NewGuid();
            EntityTypeCodice = BuiltInCodes.EntityType.Report;
        }

        public Guid GetDivisioneItemId(string codiceAttributoGuid)
        {
            return GetAttributoGuidId(codiceAttributoGuid);
        }

        public override void Validate()
        {
            ValoreElenco val = GetValoreAttributo(BuiltInCodes.Attributo.Sezione, false, false) as ValoreElenco;

            ValoreAttributoElenco elenco = Attributi[BuiltInCodes.Attributo.Sezione].Attributo.ValoreAttributo as ValoreAttributoElenco;

            if (val != null && elenco != null)
            {
                var found = elenco.Items.FirstOrDefault(item => item.Id == val.ValoreAttributoElencoId);
                if (found == null)
                {
                    //to validate
                    Attributi[BuiltInCodes.Attributo.Sezione].Valore = new ValoreElenco() { ValoreAttributoElencoId = 0};
                }
            }
        }
    }

    [ProtoContract]
    [DataContract]
    public class ReportItemType : EntityType
    {
        private static string Separator = "_";
        public List<string> ListEntityReportToEsclude = new List<string>() { BuiltInCodes.EntityType.Report, BuiltInCodes.EntityType.Documenti };
        public ReportItemType() { }//protobuf

        private string DivisioneCategory { get => LocalizationProvider.GetString("Divisione"); }
        private string SezioniCategory { get => LocalizationProvider.GetString("Sezioni"); }
        private string CategoryIndentSpace { get => "   "; }

        public override void CreaAttributi(Dictionary<string, DefinizioneAttributo> definizioniAttributo, Dictionary<string, EntityType> entityTypes)
        {
            Codice = BuiltInCodes.EntityType.Report;
            Name = LocalizationProvider.GetString("Report");
            string codiceAttributo;
            int viewOrder = 0;
            FunctionName = ElmCalculatorFunction.FunctionName;
            string emptyRtf;
            ValoreHelper.RtfFromPlainString("", out emptyRtf);
            //IsTreeMaster = false;
            string referenceCodiceGuid = "";
            Attributo att = null;

            //Attributi.Clear();
            //AttributiMasterCodes.Clear();


            IEnumerable<DivisioneItemType> divisioniType = entityTypes.Values.Where(item => item is DivisioneItemType).Cast<DivisioneItemType>();

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.IsDigicorpOwner);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("IsDigicorpOwner"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.IsVisible = MainViewStatus.IsAdvancedMode;
            att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;


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
                    GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = false,
                    //IsValoreReadOnly = true,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }   
            att = Attributi[codiceAttributo];
            //att.IsVisible = MainViewStatus.IsAdvancedMode;
            //att.IsAdvanced = MainViewStatus.IsAdvancedMode;
            att.IsBuiltIn = false;
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.DescrizioneReport);//es: IfcWallStandardCase
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Descrizione report"),
                    //IsBuiltIn = true,
                    //AllowSort = true,
                    //AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = true,
                    //IsValoreReadOnly = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Descrizione report");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowValoriUnivoci = true;
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = true;
            att.IsValoreReadOnly = false;
            //

            //
            ValoreAttributoElenco elencoSezioni = new ValoreAttributoElenco();
            List<ValoreAttributoElencoItem> Lista = new List<ValoreAttributoElencoItem>();
            LoadEntityTypesNameForCombo(entityTypes, elencoSezioni);

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Sezione);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Elenco].Clone(), this, codiceAttributo)
                {
                    ValoreAttributo = elencoSezioni,
                    //Etichetta = LocalizationProvider.GetString("Sezione"),
                    //IsBuiltIn = true,
                    //AllowSort = true,
                    //AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = true,
                    //IsValoreReadOnly = true,
                });
                AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.ValoreAttributo = elencoSezioni;
            att.Etichetta = LocalizationProvider.GetString("Sezione");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowMasterGrouping = true;
            att.AllowValoriUnivoci = true;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = true;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.NumeroColonne);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Reale].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Sezione"),
                    //IsBuiltIn = true,
                    //AllowSort = true,
                    //AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = true,
                    //IsValoreReadOnly = true,
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("NumeroColonne");
            att.IsBuiltIn = true;
            att.AllowSort = true;
            att.AllowMasterGrouping = true;
            att.AllowValoriUnivoci = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = false;
            att.IsValoreReadOnly = false;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.UsaRftAttributi);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("UsaRftAttributi"),
                    IsBuiltIn = false,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("UsaRftAttributi");
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.TabellaBordata);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("TabellaBordata"),
                    IsBuiltIn = false,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsVisible = true,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("TabellaBordata");
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = true;
            att.IsAdvanced = false;
            att.IsValoreReadOnly = false;
            att.DetailViewOrder = viewOrder++;

            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Orientamento);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("Orientamento"),
                    //IsBuiltIn = true,
                    //AllowValoriUnivoci = true,
                    //AllowSort = true,
                    //DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = true,
                    //IsValoreReadOnly = true,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.Etichetta = LocalizationProvider.GetString("Orientamento");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.AllowMasterGrouping = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = false;
            att.IsValoreReadOnly = true;
            //
            //codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Report);
            //if (!Attributi.ContainsKey(codiceAttributo))
            //{
            //    Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
            //    {
            //        //Etichetta = LocalizationProvider.GetString("Struttura"),
            //        //IsBuiltIn = true,
            //        //AllowValoriUnivoci = true,
            //        //AllowSort = true,
            //        DetailViewOrder = viewOrder++,
            //        //GroupName = LocalizationProvider.GetString("Report"),
            //        //IsVisible = false,
            //        //IsValoreReadOnly = true,

            //    });
            //    //AttributiMasterCodes.Add(codiceAttributo);
            //}
            //att = Attributi[codiceAttributo];
            //att.Etichetta = LocalizationProvider.GetString("Struttura");
            //att.IsBuiltIn = true;
            //att.AllowValoriUnivoci = true;
            //att.AllowSort = true;
            ////att.DetailViewOrder = viewOrder++;
            //att.GroupName = LocalizationProvider.GetString("Report");
            //att.IsVisible = false;
            //att.IsValoreReadOnly = false;
            //
            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.ReportWizardSetting);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Testo].Clone(), this, codiceAttributo)
                {
                    //Etichetta = LocalizationProvider.GetString("ReportWizardSetting"),
                    //IsBuiltIn = true,
                    //AllowValoriUnivoci = true,
                    //AllowSort = true,
                    //DetailViewOrder = viewOrder++,
                    //GroupName = LocalizationProvider.GetString("Report"),
                    //IsVisible = false,
                    //IsValoreReadOnly = true,

                });
                //AttributiMasterCodes.Add(codiceAttributo);
            }
            att = Attributi[codiceAttributo];
            att.IsInternal = true;
            att.Etichetta = LocalizationProvider.GetString("ReportWizardSetting");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = false;
            att.IsValoreReadOnly = false;
            //

            codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Compilato);
            if (!Attributi.ContainsKey(codiceAttributo))
            {
                Attributi.Add(codiceAttributo, new Attributo(definizioniAttributo[BuiltInCodes.DefinizioneAttributo.Booleano].Clone(), this, codiceAttributo)
                {
                    Etichetta = LocalizationProvider.GetString("Compilato"),
                    IsBuiltIn = true,
                    AllowSort = true,
                    AllowValoriUnivoci = true,
                    //DetailViewOrder = viewOrder++,
                    GroupName = LocalizationProvider.GetString("Report"),
                    IsVisible = false,
                    IsValoreReadOnly = false,
                    ValoreDefault = new ValoreBooleano(),
                });
                //AttributiMasterCodes.Add(codiceAttributo);
                //viewOrder++;
            }
            att.IsInternal = true;
            att.Etichetta = LocalizationProvider.GetString("Compilato");
            att.IsBuiltIn = true;
            att.AllowValoriUnivoci = true;
            att.AllowSort = true;
            att.DetailViewOrder = viewOrder++;
            att.GroupName = LocalizationProvider.GetString("Report");
            att.IsVisible = false;
            att.IsValoreReadOnly = false;

            UpdateEtichetteMap();
        }

        private void LoadEntityTypesNameForCombo(Dictionary<string, EntityType> entityTypes, ValoreAttributoElenco elencoSezioni)
        {
            //List<ValoreAttributoElencoItem> lista = new List<ValoreAttributoElencoItem>();

            string Category = null;

            elencoSezioni.Items.Clear();

            foreach (EntityType entType in entityTypes.Values)
            {
                int dependencyEnum = (int)entType.DependencyEnum;

                if (!entType.IsParentType())
                {
                    if (!(entType is DivisioneItemType))
                    {
                        string entTypeKey = entType.GetKey();
                        Category = SezioniCategory;
                        //_entityTypesKeyOnInit.Add(entTypeKey);
                        if (!ListEntityReportToEsclude.Contains(entTypeKey))
                        {
                            if (elencoSezioni.Items.Where(e => e.Text == Category).FirstOrDefault() == null)
                            {
                                elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = Category, Id = elencoSezioni.NewIdProgressive() });                                
                            }

                            int IdEnt = 0;

                            if (entTypeKey == BuiltInCodes.EntityType.Contatti) IdEnt = BuiltInCodes.SectionItemsId.Contatti;
                            if (entTypeKey == BuiltInCodes.EntityType.InfoProgetto) IdEnt = BuiltInCodes.SectionItemsId.InfoProgetto;
                            if (entTypeKey == BuiltInCodes.EntityType.Stili) IdEnt = BuiltInCodes.SectionItemsId.Stili;
                            if (entTypeKey == BuiltInCodes.EntityType.Prezzario) IdEnt = BuiltInCodes.SectionItemsId.Prezzario;
                            if (entTypeKey == BuiltInCodes.EntityType.Capitoli) IdEnt = BuiltInCodes.SectionItemsId.Capitoli;
                            if (entTypeKey == BuiltInCodes.EntityType.Elementi) IdEnt = BuiltInCodes.SectionItemsId.Elementi;
                            if (entTypeKey == BuiltInCodes.EntityType.Computo) IdEnt = BuiltInCodes.SectionItemsId.Computo;
                            if (entTypeKey == BuiltInCodes.EntityType.ElencoAttivita) IdEnt = BuiltInCodes.SectionItemsId.ElencoAttivita;
                            if (entTypeKey == BuiltInCodes.EntityType.WBS) IdEnt = BuiltInCodes.SectionItemsId.WBS;
                            if (entTypeKey == BuiltInCodes.EntityType.Calendari) IdEnt = BuiltInCodes.SectionItemsId.Calendari;

                            if (IdEnt != 0)
                                elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = CategoryIndentSpace + entityTypes[entTypeKey].Name, Id = elencoSezioni.NewIdProgressive() });

                            if (entTypeKey == BuiltInCodes.EntityType.WBS)
                            {
                                elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = CategoryIndentSpace +  "Gantt", Id = BuiltInCodes.SectionItemsId.Gantt });
                            }
                            if (entTypeKey == BuiltInCodes.EntityType.Calendari)
                            {
                                elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = CategoryIndentSpace + LocalizationProvider.GetString("Fogli di calcolo"), Id = BuiltInCodes.SectionItemsId.FogliDiCalcolo });
                            }
                            //elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = CategoryIndentSpace + "FogliDiCalcolo", Id = 12 });
                        }
                    }
                }
            }

            int Contatore = 1001;
            foreach (EntityType entType in entityTypes.Values)
            {
                int dependencyEnum = (int)entType.DependencyEnum;

                if (!entType.IsParentType())
                {
                    if ((entType is DivisioneItemType))
                    {
                        string entTypeKey = entType.GetKey();
                        Category = DivisioneCategory;
                        //_entityTypesKeyOnInit.Add(entTypeKey);
                        if (!ListEntityReportToEsclude.Contains(entTypeKey))
                        {
                            if (elencoSezioni.Items.Where(e => e.Text == Category).FirstOrDefault() == null)
                            {
                                elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = Category, Id = 1000 });
                            }
                            elencoSezioni.Items.Add(new ValoreAttributoElencoItem() { Text = CategoryIndentSpace + entityTypes[entTypeKey].Name, Id = Contatore });
                        }
                    }
                    Contatore++;
                }
            }

            //elencoSezioni.Items = lista;
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
            return BuiltInCodes.EntityType.Report;
        }

        public override EntityTypeDependencyEnum DependencyEnum => EntityTypeDependencyEnum.Report;
        public override DependentEntityTypesEnum DependentTypesEnum => DependentEntityTypesEnum.Report;

        public override MasterType MasterType => MasterType.Grid;
        public override EntityComparer EntityComparer { get; set; } = new ReportItemKeyComparer();

        public override bool IsCustomizable() { return false; }


        public override bool UpdateAttributi(Dictionary<string, EntityType> entityTypes, EntityType entTypeOld, bool removed = false)
        {
            //Aggiorno l'elenco degli entityType dell'attributo sezioni nei seguenti casi:
            //1.Rimozione di una divisione.
            //2.Rinomina di una divisione
            //3.Aggiunta di una divisione

            bool res = false;

            string codiceAttributo = string.Join(Separator, BuiltInCodes.Attributo.Sezione);
            if (!Attributi.ContainsKey(codiceAttributo))
                return false;

            ValoreAttributoElenco elencoSezioni = Attributi[codiceAttributo].ValoreAttributo as ValoreAttributoElenco;
            if (elencoSezioni == null)
                return false;

            string entTypeKey = entTypeOld.GetKey();

            EntityType entTypeNew = null;
            if (removed)
            {
                //Rimuovo
                elencoSezioni.Items.RemoveAll(item => item.Text.Trim() == entTypeOld.Name);
                res = true;
            }
            else if (entityTypes.TryGetValue(entTypeKey, out entTypeNew))
            {
                //Rinomino
                ValoreAttributoElencoItem elencoItem = elencoSezioni.Items.FirstOrDefault(item => item.Text.Trim() == entTypeOld.Name);
                if (elencoItem != null)
                {
                    elencoItem.Text = string.Format("{0}{1}", CategoryIndentSpace, entTypeNew.Name);
                }
            }
            else
            {
                //Aggiungo
                elencoSezioni.Items.Add(new ValoreAttributoElencoItem()
                {
                    Id = elencoSezioni.NewId(),
                    Text = entTypeNew.Name,
                });
            }

            return res;

        }
    }

    [ProtoContract]
    [DataContract]
    public class ReportItemCollection : EntityCollection
    {
    }

    public class ReportItemKeyComparer : EntityComparer
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
