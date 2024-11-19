using MasterDetailModel;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FogliDiCalcoloDataSourceGenerator
    {
        private List<dynamic> UnknowDatasource;
        private FilterData filter;
        private EntitiesHelper entitiesHelper;
        private AttributoFormatHelper attributoFormatHelper;
        private ClientDataService DataService;

        public DataTable DataTable { get; set; }

        public static string HeaderTreeLevelSymbol { get => "\\"; }

        public FogliDiCalcoloDataSourceGenerator(ClientDataService dataService)
        {
            UnknowDatasource = new List<dynamic>();
            DataService = dataService;
        }
        public void CreateGenericDataSource(string SezioneItem, IMainOperation MainOperation)
        {

            ViewSettings viewSettings = DataService.GetViewSettings();
            if (!viewSettings.EntityTypes.ContainsKey(SezioneItem))
                return;

            EntityTypeViewSettings EntityViewSettings = viewSettings.EntityTypes[SezioneItem];

            //creo un filtro uguale al filtro corrente della vista
            filter = new FilterData();
            EntityViewSettings.Filters.ForEach(item =>
            {
                AttributoFilterData attFilter = new AttributoFilterData()
                {
                    CodiceAttributo = item.CodiceAttributo,
                    CheckedValori = new HashSet<string>(item.CheckedValori),
                    EntityTypeKey = SezioneItem,
                    IsFiltroAttivato = true,
                };
                filter.Items.Add(attFilter);
            });

            entitiesHelper = new EntitiesHelper(DataService);
            attributoFormatHelper = new AttributoFormatHelper(DataService);

            UnknowDatasource.Clear();

            HashSet<Guid> FilteredEntitiesIds = new HashSet<Guid>();

            GenerateDataToManage(SezioneItem, FilteredEntitiesIds, null);

            DataTable = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(UnknowDatasource));
        }

        private void GenerateDataToManage(string SezioneItem, HashSet<Guid> filteredEntitiesIds, dynamic unknowObject = null)
        {
            List<Guid> entitiesFound = null;
            List<TreeEntity> TreeEntities = new List<TreeEntity>();
            List<Entity> Entities = new List<Entity>();

            Dictionary<string, EntityType> EntitiesList = DataService.GetEntityTypes();
            EntityType EntitySelected = EntitiesList[SezioneItem];

            if (EntitySelected.IsTreeMaster == false)
            {
                List<EntityMasterInfo> MasterInfo = MasterInfo = DataService.GetFilteredEntities(SezioneItem, null, null, null, out entitiesFound);
                Entities = DataService.GetEntitiesById(SezioneItem, entitiesFound);
            }
            else
            {
                List<TreeEntityMasterInfo> TreeInfo = DataService.GetFilteredTreeEntities(SezioneItem, null, null, out entitiesFound);
                TreeInfo = EntitiesHelper.TreeFilterById(TreeInfo, entitiesFound);
                TreeEntities = DataService.GetTreeEntitiesById(SezioneItem, TreeInfo.Select(item => item.Id));
            }

            if ((TreeEntities.Count == 0 && Entities.Count == 0)) { return; }

            int contatore = 0;
            dynamic UnknowObjectLocal = null;

            //CREATE A DYMAMIC OBJECT FROM EVERY TYPE OF SUORCE BASED ON TREE OR NOT
            if (EntitySelected.IsTreeMaster)
            {
                foreach (var ent in TreeEntities)
                {
                    UnknowObjectLocal = new System.Dynamic.ExpandoObject();
                    AddAttributeToExpandoObject((Entity)ent, SezioneItem, UnknowObjectLocal);
                    contatore++;
                    UnknowDatasource.Add(UnknowObjectLocal);
                }
            }
            else
            {
                foreach (var ent in Entities)
                {
                    UnknowObjectLocal = new System.Dynamic.ExpandoObject();
                    AddAttributeToExpandoObject((Entity)ent, SezioneItem, UnknowObjectLocal);
                    contatore++;
                    UnknowDatasource.Add(UnknowObjectLocal);
                }
            }
        }

        private void AddAttributeToExpandoObject(Entity ent, string sezioneItem, dynamic unknowObject)
        {
            //foreach (var item in ent.Attributi)
            foreach (var item in entitiesHelper.GetAttributi(ent))
            {
                //EntityAttributo entAtt = ent.Attributi[item.Key];
                //Attributo att = entAtt.Attributo;

                Attributo att = item.Value;
                Entity entRef = null;
                Attributo attref = null;

                string etichetta = att.Etichetta;
                string codice = att.Codice;

                if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Guid)
                {
                    continue;
                }

                if (att.IsInternal) { continue; }

                bool HasValue = false;

                Valore val = null;

                Attributo sourceatt = entitiesHelper.GetSourceAttributo(att);


                //Imposto i valori delle colonne dei padri
                if (entitiesHelper.IsAttributoDeep(sourceatt.EntityTypeKey, sourceatt.Codice))
                {
                    var res = entitiesHelper.GetValorePlainTextForLevel(ent, att.Codice, true, true);

                    if (res != null)
                    {
                        string prefixLevel = string.Empty;
                        for (int i = 0; i < res.Count; i++)
                        {
                            string columnName = string.Format("{0}{1}", prefixLevel, att.Etichetta);
                            AddProperty(unknowObject, columnName, res[i]);
                            HasValue = true;

                            prefixLevel += HeaderTreeLevelSymbol;
                        }
                    }
                    else
                    {
                        AddProperty(unknowObject, att.Etichetta, string.Empty);
                    }

                }
                else
                {

                    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, true);


                    //if (sourceatt.ValoreDefault is ValoreTestoRtf)
                    //    val = new ValoreTesto() { V = entitiesHelper.GetValorePlainText(ent, att.Codice, true, true) };
                    //else
                    //    val = entitiesHelper.GetValoreAttributo(ent, att.Codice, false, true);


                    if (val is ValoreData)
                    {
                        ValoreData valData = val as ValoreData;
                        DateTime? data = valData.V;
                        AddProperty(unknowObject, etichetta, data);
                        HasValue = true;
                    }
                    else if (val is ValoreTesto)
                    {
                        ValoreTesto valTesto = val as ValoreTesto;
                        string plainText = valTesto.Result;
                        AddProperty(unknowObject, etichetta, plainText);
                        HasValue = true;
                    }
                    else if (val is ValoreFormatoNumero)
                    {
                        ValoreFormatoNumero valTesto = val as ValoreFormatoNumero;
                        string plainText = valTesto.V;
                        AddProperty(unknowObject, etichetta, plainText);
                        HasValue = true;
                    }
                    else if (val is ValoreContabilita)
                    {
                        ValoreContabilita valContabilita = val as ValoreContabilita;
                        double? res = null;
                        if (valContabilita.RealResult != null)
                            res = (double)valContabilita.RealResult;
                        //double? res = (double)valContabilita.RealResult;
                        AddProperty(unknowObject, etichetta, res);
                        HasValue = true;
                    }
                    else if (val is ValoreReale)
                    {
                        ValoreReale valReale = val as ValoreReale;
                        double? res = valReale.RealResult;
                        AddProperty(unknowObject, etichetta, res);
                        HasValue = true;
                    }
                    else if (val is ValoreTestoRtf)
                    {
                        ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;
                        string plainText = valTestoRtf.PlainText;
                        AddProperty(unknowObject, etichetta, plainText);
                        HasValue = true;
                    }
                    else if (val is ValoreElenco)
                    {
                        ValoreElenco valTesto = val as ValoreElenco;
                        string plainText = valTesto.V;
                        AddProperty(unknowObject, etichetta, plainText);
                        HasValue = true;
                    }
                    else if (val is ValoreBooleano)
                    {
                        ValoreBooleano valTesto = val as ValoreBooleano;
                        string plainText = valTesto.PlainText;
                        AddProperty(unknowObject, etichetta, plainText);
                        HasValue = true;
                    }

                    //if (val is ValoreElenco)
                    //{
                    //    ValoreElenco valTesto = val as ValoreElenco;
                    //    string plainText = valTesto.V;
                    //    AddProperty(unknowObject, etichetta, plainText);
                    //    HasValue = true;
                    //}

                    if (!HasValue)
                    {
                        //if (entAtt.Attributo is AttributoRiferimento)
                        if (att is AttributoRiferimento)
                        {
                            if (sourceatt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale || sourceatt.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                                AddProperty(unknowObject, etichetta, 0.0);
                            else
                                AddProperty(unknowObject, etichetta, "");
                        }
                        else
                        {
                            AddProperty(unknowObject, etichetta, "");
                        }
                    }
                }
            }
        }
        private void AddAttributeReferenceToExpandoObject(Entity entRef, string etichetta, Attributo attref, dynamic unknowObject)
        {
            Valore val = null;
            if (attref.ValoreDefault is ValoreTestoRtf)
                val = entitiesHelper.GetValoreAttributo(entRef, attref.Codice, false, false);
            else
                val = entitiesHelper.GetValoreAttributo(entRef, attref.Codice, false, true);
            if (val is ValoreData)
            {
                ValoreData valData = val as ValoreData;
                DateTime? data = valData.V;
                AddProperty(unknowObject, etichetta, data);
            }
            if (val is ValoreTesto)
            {
                ValoreTesto valTesto = val as ValoreTesto;
                string plainText = valTesto.Result;
                AddProperty(unknowObject, etichetta, plainText);
            }
            if (val is ValoreFormatoNumero)
            {
                ValoreFormatoNumero valTesto = val as ValoreFormatoNumero;
                string plainText = valTesto.V;
                AddProperty(unknowObject, etichetta, plainText);
            }
            if (val is ValoreContabilita)
            {
                ValoreContabilita valContabilita = val as ValoreContabilita;
                double? res = (double)valContabilita.RealResult;
                AddProperty(unknowObject, etichetta, res);
            }
            if (val is ValoreReale)
            {
                ValoreReale valReale = val as ValoreReale;
                double? res = valReale.RealResult;
                AddProperty(unknowObject, etichetta, res);
            }

            if (val is ValoreTestoRtf)
            {
                ValoreTestoRtf valTestoRtf = val as ValoreTestoRtf;
                string plainText = valTestoRtf.PlainText;
                AddProperty(unknowObject, etichetta, plainText);
            }
            if (val is ValoreElenco)
            {
                ValoreElenco valTesto = val as ValoreElenco;
                string plainText = valTesto.V;
                AddProperty(unknowObject, etichetta, plainText);
            }
        }

        //private int ContatoreEtichette;
        private void AddProperty(System.Dynamic.ExpandoObject expando, string propertyName, object propertyValue)
        {
            //rev by ale 13/11/2023
            /////////////////////////
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
                expandoDict.Add(propertyName, propertyValue);


            //var expandoDict = expando as IDictionary<string, object>;
            //if (expandoDict.ContainsKey(propertyName))
            //{
            //    ContatoreEtichette++;
            //    expandoDict.Add(propertyName + ContatoreEtichette, propertyValue);
            //}
            //else
            //    expandoDict.Add(propertyName, propertyValue);


            ////////////////////////////
        }

        public DataTable GetDataTable()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(UnknowDatasource);
            DataTable = JsonConvert.DeserializeObject<DataTable>(json);
            return DataTable;
        }
    }
}
