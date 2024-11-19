using Auth0.ManagementApi.Models;
using Humanizer.Localisation;
using Humanizer;
using JoinApi.Extensions;
using JoinApi.Models;
using JoinApi.Settings;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Options;
using ModelData.Dto;
using ModelData.Model;
using ModelData.Utilities;
//using ModelData.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.GridFS;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using Org.BouncyCastle.Math;

namespace JoinApi.Service
{
    public class MongoDbService
    {
        private readonly MongoSettings _mongoSettings;

        static class CollectionsName
        {
            public static string DefinizioniAttributo { get => "definizioniAttributo"; }
            public static string EntityTypes { get => "entityTypes"; }
            public static string DivisioniItems { get => "divisioniItems"; }
            public static string ComputoItems { get => "computoItems"; }
            public static string PrezzarioItems { get => "prezzarioItems"; }
            public static string CapitoliItems { get => "capitoliItems"; }
            public static string ElementiItems { get => "elementiItems"; }
            public static string VariabiliItems { get => "variabiliItems"; }
            public static string Opere { get => "opere"; }
            public static string Progetti { get => "progetti"; }
            public static string Utenti { get => "utenti"; }
            public static string Ruoli { get => "ruoli"; }
            public static string Tags { get => "tag"; }
            public static string Model3dFilesInfo { get => "model3dFilesInfo"; }
            public static string AllegatiItems { get => "allegatiItems"; }
            public static string ContattiItems { get => "contattiItems"; }
            public static string InfoProgettoItems { get => "infoProgettoItems"; }
            public static string WBSItems { get => "wbsItems"; }
            public static string CalendariItems { get => "calendariItems"; }
            public static string ViewSettingsItems { get => "viewSettings"; }
            public static string Model3dFiltersData { get => "model3dFiltersData"; }
            public static string Model3dValuesData { get => "model3dValuesData"; }
            public static string Model3dTagsData { get => "model3dTagsData"; }
            public static string Model3dPreferencesData { get => "model3dPreferencesData"; }
            public static string DocumentiItems { get => "documentiItems"; }
            public static string ReportItems { get => "reportItems"; }
            public static string StiliItems { get => "stiliItems"; }
            public static string NumericFormats { get => "numericFormats"; }
            public static string Model3dUserViewList { get => "model3dUserViewList"; }
            public static string ElencoAttivitaItems { get => "elencoAttivitaItems"; }
            public static string Model3dUserRotoTranslation { get => "model3dUserRotoTranslation"; }
            public static string GanttData { get => "ganttData"; }
            public static string WBSItemsCreationData { get => "wbsItemsCreationData"; }
            public static string FogliDiCalcoloData { get => "fogliDiCalcoloData"; }
            public static string TagItems { get => "tagItems"; }
            public static string GruppiUtenti { get => "gruppiUtenti"; }
            public static string Clienti { get => "clienti"; }
            public static string Settori { get => "settori"; }
            public static string Teams { get => "teams"; }
            public static string Permessi { get => "permessi"; }

        }


        public async Task Init()
        {
            try
            {
                var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(_mongoSettings.ConnectionURI));
                if(_mongoSettings.UseTls)
                {
                    Log.Information($"Connessione al database con utilizzo di TLS per {_mongoSettings.ConnectionURI}.");
                    mongoClientSettings.UseTls = true;
                }
                else
                {
                    Log.Warning($"Connessione al database senza l'utilizzo di TLS!");
                }

                mongoClientSettings.ClusterConfigurator = cb =>
                {
                    cb.Subscribe<CommandStartedEvent>(e =>
                    {
                        Console.WriteLine($"MongoDB : {e.CommandName} - {e.Command.ToJson()}");
                        Log.Information($"MongoDB : {e.CommandName} - {e.Command.ToJson()}");
                    });
                };

                //switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                //{
                //    case "Development":
                //        {
                //            string appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                //            //string caFilePath = Path.Combine(appPath, _mongoSettings.CaClientPath);
                //            string keyFilePath = Path.Combine(appPath, _mongoSettings.KeyClientPath);
                //            bool isFileExist = File.Exists(keyFilePath);
                //            Log.Verbose("Caricamento configurazione Development per Mongodb, in corso...");
                //            //var certCa = new X509Certificate(fileName: caFilePath);
                //
                //            var client = new MongoClient($"mongodb://{Uri.EscapeDataString(userName)}@server:27017?authMechanism=MONGODB-X509&tls=true",
                //            new MongoClientSettings
                //            {
                //                SslSettings = new SslSettings
                //                {
                //                    ClientCertificates = new X509CertificateCollection
                //                    {
                //                        new X509Certificate2($"{AppDomain.CurrentDomain.BaseDirectory}/certs/x509/client.pem")
                //                    }
                //                }
                //            });
                //
                //            // Connect using the MONGODB-X509 authentication mechanism
                //            try
                //            {
                //                client.ListDatabaseNames(); // This will trigger the connection
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine(ex.Message);
                //            }
                //            finally
                //            {
                //                client = null; // Dispose of the client
                //            }
                //        }
                //        break;
                //    case "Staging":
                //        {
                //            Log.Verbose("Caricamento configurazione Staging per Mongodb, in corso...");
                //        }
                //        break;
                //    case "Production":
                //        {
                //            Log.Verbose("Caricamento configurazione production per Mongodb, in corso...");
                //        }
                //        break;
                //    default:
                //        break;
                //}
                //

                // Questa parte di codice viene eseguita in try-catch perchè al primo passaggio l'autenticazione deve essere DISABLED (vedi file config di mongo) in modo si possa
                // creare un utente e poi usarlo per accedere.
                // Quando risulta essere attiva non riuscirà a verificarne la presenza e quindi andrà in eccezione.

                try
                {
                    _client = new MongoClient(mongoClientSettings);
                    var adminDb = _client.GetDatabase("admin");
                    // Verifica se l'utente amministratore esiste
                    Log.Information($"MongoDB : Verifica presenza per utente {_mongoSettings.AdminUser}.");

                    var command = new BsonDocument { { "usersInfo", _mongoSettings.AdminUser } };
                    var usersResult = await adminDb.RunCommandAsync<BsonDocument>(command);
                    var usersArray = usersResult["users"].AsBsonArray;

                    if (_mongoSettings.CreateAdminUserOnStartup && usersArray.Count == 0)
                    {
                        // Se l'utente amministratore specificato non esiste, crearlo
                        Log.Information($"MongoDB : Avvio creazione utente {_mongoSettings.AdminUser}.");

                        await CreateAdminUserAsync(adminDb);
                    }
                    else
                    {
                        Log.Information($"MongoDB : Utente {_mongoSettings.AdminUser} già presente in admin.");
                    }
                }
                catch (Exception ex)
                {
                    Log.ForContext("Eccezione", ex).Warning($"MongoDB : Avviso di connessione a {_mongoSettings.DatabaseName}, risultato del tentativo di accesso a {_mongoSettings.DatabaseName} fallito perchè l'autenticazione di Mongo è attiva. NOTA: Comportamento previsto tranne al primo accesso. Dettagli {ex.Message}.");
                }
                

                Log.Information($"MongoDB : Connessione tramite utente {_mongoSettings.AdminUser}.");

                mongoClientSettings.Credential = MongoCredential.CreateCredential("admin", _mongoSettings.AdminUser, _mongoSettings.AdminPassword);
                _client = new MongoClient(mongoClientSettings);
                Log.Information($"MongoDB : Credenziali caricate per {_mongoSettings.AdminUser}.");

                Log.Information($"MongoDB : Accesso alla lista dei database in corso.");
                var db = _client.ListDatabases();

                _database = _client.GetDatabase(_mongoSettings.DatabaseName);
                ConnectionEstablished = _client.Cluster.Description.State == ClusterState.Connected && _database != null;
                Log.Information($"MongoDB : Risultato connessione al database {_mongoSettings.DatabaseName} a {ConnectionEstablished.ToString()}.");
            }
            catch (Exception ex) 
            {
                Log.ForContext("Eccezione", ex).Error($"MongoDB : Errore di connessione a {_mongoSettings.DatabaseName}, risultato del tentativo di accesso a {ConnectionEstablished.ToString()}. Dettagli {ex.Message}.");
                ConnectionEstablished = false;
            }
        }

        /// <summary>
        /// CREAZIONE IN MONGODB COSI COMPOSTA (console mongosh):
        /// db.createUser(
        ///     {
        ///         user: "AdminDigicorp",
        ///         pwd: "AdminDigicorp",
        ///         roles: [ { role: "userAdminAnyDatabase", db: "admin" },  { role: "readWriteAnyDatabase" , db: "admin" }]
        ///     }
        /// )
        /// </summary>
        /// <param name="adminDb"></param>
        /// <returns></returns>
        private async Task CreateAdminUserAsync(IMongoDatabase adminDb)
        {
            try
            {
                await adminDb.RunCommandAsync<BsonDocument>(new BsonDocument
                {
                    { "createUser", _mongoSettings.AdminUser },
                    { "pwd", _mongoSettings.AdminPassword },
                    { "roles", new BsonArray
                        {
                            new BsonDocument { { "role", "userAdminAnyDatabase" }, { "db", "admin" } },
                            new BsonDocument { { "role", "readWriteAnyDatabase" }, { "db", "admin" } }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log.ForContext("Eccezione", ex).Error($"MongoDB : Eccezione durante il tentativo di creazione di un utente su {_mongoSettings.DatabaseName}. Dettaglio {ex.Message}.");
            }
        }

        public bool? ConnectionEstablished = null;


        private IMongoClient? _client;
        private IMongoClient? Client
        {
            get
            {
                if (_client == null || !ConnectionEstablished.GetValueOrDefault()) Init();
                
                return _client;
            }
        }

        private IMongoDatabase? _database;
        private IMongoDatabase? Database { 
            get
            {
                if (_database == null || !ConnectionEstablished.GetValueOrDefault()) Init();

                return _database;
            }
        }

        private IMongoCollection<SettoreDoc>? _settoriCollection;
        public IMongoCollection<SettoreDoc> SettoriCollection
        {
            get
            {
                if (_settoriCollection == null)
                {
                    _settoriCollection = Database?.GetCollection<SettoreDoc>(CollectionsName.Settori);
                }
                return _settoriCollection;
            }
        }

        private IMongoCollection<OperaDoc>? _opereCollection;
        public IMongoCollection<OperaDoc> OpereCollection
        {
            get
            {
                if (_opereCollection == null)
                {
                    _opereCollection = Database.GetCollection<OperaDoc>(CollectionsName.Opere);
                }
                return _opereCollection;
            }
        }

        private IMongoCollection<ProgettoDoc>? _progettiCollection;
        public IMongoCollection<ProgettoDoc> ProgettiCollection
        {
            get
            {
                if (_progettiCollection == null)
                {
                    _progettiCollection = Database.GetCollection<ProgettoDoc>(CollectionsName.Progetti);
                }
                return _progettiCollection;
            }
        }
        private IMongoCollection<UtenteDoc>? _utentiCollection;
        public IMongoCollection<UtenteDoc> UtentiCollection
        {
            get
            {
                if (_utentiCollection == null)
                {
                    _utentiCollection = Database.GetCollection<UtenteDoc>(CollectionsName.Utenti);
                }
                return _utentiCollection;
            }
        }
        private IMongoCollection<RuoloDoc>? _ruoliCollection;
        public IMongoCollection<RuoloDoc> RuoliCollection
        {
            get
            {
                if (_ruoliCollection == null)
                {
                    _ruoliCollection = Database.GetCollection<RuoloDoc>(CollectionsName.Ruoli);
                }
                return _ruoliCollection;
            }
        }

        private IMongoCollection<DefinizioneAttributoDoc>? _definizioneAttributiCollection;
        public IMongoCollection<DefinizioneAttributoDoc> DefinizioniAttributoCollection
        {
            get
            {
                if (_definizioneAttributiCollection == null)
                {
                    _definizioneAttributiCollection = Database.GetCollection<DefinizioneAttributoDoc>(CollectionsName.DefinizioniAttributo);
                }
                return _definizioneAttributiCollection;
            }
        }

        private IMongoCollection<EntityTypeDoc>? _entityTypesCollection;
        public IMongoCollection<EntityTypeDoc> EntityTypesCollection
        {
            get
            {
                if (_entityTypesCollection == null)
                {
                    _entityTypesCollection = Database.GetCollection<EntityTypeDoc>(CollectionsName.EntityTypes);
                }
                return _entityTypesCollection;
            }
        }

        private IMongoCollection<DivisioneItemDoc>? _divisioniItemsCollection;
        public IMongoCollection<DivisioneItemDoc> DivisioniItemsCollection
        {
            get
            {
                if (_divisioniItemsCollection == null)
                {
                    _divisioniItemsCollection = Database.GetCollection<DivisioneItemDoc>(CollectionsName.DivisioniItems);
                }
                return _divisioniItemsCollection;
            }
        }

        private IMongoCollection<ComputoItemDoc>? _computoItemsCollection;
        public IMongoCollection<ComputoItemDoc> ComputoItemsCollection
        {
            get
            {
                if (_computoItemsCollection == null)
                {
                    //if (!CollectionExists(Database, CollectionsName.ComputoItems))
                    //{

                    //    var collectionOptions = new CreateCollectionOptions
                    //    {
                    //        StorageEngine = new BsonDocument
                    //        {
                    //            { "wiredTiger", new BsonDocument
                    //                {
                    //                    { "configString", "block_compressor=zstd" }
                    //                }
                    //            }
                    //        }
                    //    };

                    //    Database.CreateCollection(CollectionsName.ComputoItems, collectionOptions);
                    //}

                    _computoItemsCollection = Database.GetCollection<ComputoItemDoc>(CollectionsName.ComputoItems);
                }
                return _computoItemsCollection;
            }
        }
        private IMongoCollection<PrezzarioItemDoc>? _prezzarioItemsCollection;
        public IMongoCollection<PrezzarioItemDoc> PrezzarioItemsCollection
        {
            get
            {
                if (_prezzarioItemsCollection == null)
                {
                    _prezzarioItemsCollection = Database.GetCollection<PrezzarioItemDoc>(CollectionsName.PrezzarioItems);
                }
                return _prezzarioItemsCollection;
            }
        }

        private IMongoCollection<CapitoliItemDoc>? _capitoliItemsCollection;
        public IMongoCollection<CapitoliItemDoc> CapitoliItemsCollection
        {
            get
            {
                if (_capitoliItemsCollection == null)
                {
                    _capitoliItemsCollection = Database.GetCollection<CapitoliItemDoc>(CollectionsName.CapitoliItems);
                }
                return _capitoliItemsCollection;
            }
        }

        private IMongoCollection<ElementiItemDoc>? _elementiItemsCollection;
        public IMongoCollection<ElementiItemDoc> ElementiItemsCollection
        {
            get
            {
                if (_elementiItemsCollection == null)
                {
                    _elementiItemsCollection = Database.GetCollection<ElementiItemDoc>(CollectionsName.ElementiItems);
                }
                return _elementiItemsCollection;
            }
        }

        private IMongoCollection<VariabiliItemDoc>? _variabiliItemsCollection;
        public IMongoCollection<VariabiliItemDoc> VariabiliItemsCollection
        {
            get
            {
                if (_variabiliItemsCollection == null)
                {
                    _variabiliItemsCollection = Database.GetCollection<VariabiliItemDoc>(CollectionsName.VariabiliItems);
                }
                return _variabiliItemsCollection;
            }
        }

        private IMongoCollection<Models.TagDoc>? _TagsCollection;
        public IMongoCollection<Models.TagDoc> TagsCollection
        {
            get
            {
                if (_TagsCollection == null)
                {
                    _TagsCollection = Database.GetCollection<Models.TagDoc>(CollectionsName.Tags);
                }
                return _TagsCollection;
            }
        }

        private IMongoCollection<Model3dFilesInfoDoc>? _model3dFileCollection;
        public IMongoCollection<Model3dFilesInfoDoc> Model3dFileCollection
        {
            get
            {
                if (_model3dFileCollection == null)
                {
                    _model3dFileCollection = Database.GetCollection<Model3dFilesInfoDoc>(CollectionsName.Model3dFilesInfo);
                }
                return _model3dFileCollection;
            }
        }

        private IMongoCollection<AllegatiItemDoc>? _allegatiItemsCollection;
        public IMongoCollection<AllegatiItemDoc> AllegatiItemsCollection
        {
            get
            {
                if (_allegatiItemsCollection == null)
                {
                    _allegatiItemsCollection = Database.GetCollection<AllegatiItemDoc>(CollectionsName.AllegatiItems);
                }
                return _allegatiItemsCollection;
            }
        }

        private IMongoCollection<ContattiItemDoc>? _contattiItemsCollection;
        public IMongoCollection<ContattiItemDoc> ContattiItemsCollection
        {
            get
            {
                if (_contattiItemsCollection == null)
                {
                    _contattiItemsCollection = Database.GetCollection<ContattiItemDoc>(CollectionsName.ContattiItems);
                }
                return _contattiItemsCollection;
            }
        }

        private IMongoCollection<InfoProgettoItemDoc>? _infoProgettoItemsCollection;
        public IMongoCollection<InfoProgettoItemDoc> InfoProgettoItemsCollection
        {
            get
            {
                if (_infoProgettoItemsCollection == null)
                {
                    _infoProgettoItemsCollection = Database.GetCollection<InfoProgettoItemDoc>(CollectionsName.InfoProgettoItems);
                }
                return _infoProgettoItemsCollection;
            }
        }

        private IMongoCollection<WBSItemDoc>? _wbsItemsCollection;
        public IMongoCollection<WBSItemDoc> WBSItemsCollection
        {
            get
            {
                if (_wbsItemsCollection == null)
                {
                    _wbsItemsCollection = Database.GetCollection<WBSItemDoc>(CollectionsName.WBSItems);
                }
                return _wbsItemsCollection;
            }
        }

        private IMongoCollection<CalendariItemDoc>? _calendariItemsCollection;
        public IMongoCollection<CalendariItemDoc> CalendariItemsCollection
        {
            get
            {
                if (_calendariItemsCollection == null)
                {
                    _calendariItemsCollection = Database.GetCollection<CalendariItemDoc>(CollectionsName.CalendariItems);
                }
                return _calendariItemsCollection;
            }
        }

        private IMongoCollection<ViewSettingsDoc>? _viewSettingsCollection;
        public IMongoCollection<ViewSettingsDoc> ViewSettingsCollection
        {
            get
            {
                if (_viewSettingsCollection == null)
                {
                    _viewSettingsCollection = Database.GetCollection<ViewSettingsDoc>(CollectionsName.ViewSettingsItems);
                }
                return _viewSettingsCollection;
            }
        }

        private IMongoCollection<Model3dFiltersDataDoc>? _model3dFiltersDataCollection;
        public IMongoCollection<Model3dFiltersDataDoc> Model3dFiltersDataCollection
        {
            get
            {
                if (_model3dFiltersDataCollection == null)
                {
                    _model3dFiltersDataCollection = Database.GetCollection<Model3dFiltersDataDoc>(CollectionsName.Model3dFiltersData);
                }
                return _model3dFiltersDataCollection;
            }
        }

        private IMongoCollection<Model3dValuesDataDoc>? _model3dValuesDataCollection;
        public IMongoCollection<Model3dValuesDataDoc> Model3dValuesDataCollection
        {
            get
            {
                if (_model3dValuesDataCollection == null)
                {
                    _model3dValuesDataCollection = Database.GetCollection<Model3dValuesDataDoc>(CollectionsName.Model3dValuesData);
                }
                return _model3dValuesDataCollection;
            }
        }

        private IMongoCollection<Model3dTagsDataDoc>? _model3dTagsDataCollection;
        public IMongoCollection<Model3dTagsDataDoc> Model3dTagsDataCollection
        {
            get
            {
                if (_model3dTagsDataCollection == null)
                {
                    _model3dTagsDataCollection = Database.GetCollection<Model3dTagsDataDoc>(CollectionsName.Model3dTagsData);
                }
                return _model3dTagsDataCollection;
            }
        }

        private IMongoCollection<Model3dPreferencesDataDoc>? _model3dPreferencesDataCollection;
        public IMongoCollection<Model3dPreferencesDataDoc> Model3dPreferencesDataCollection
        {
            get
            {
                if (_model3dPreferencesDataCollection == null)
                {
                    _model3dPreferencesDataCollection = Database.GetCollection<Model3dPreferencesDataDoc>(CollectionsName.Model3dPreferencesData);
                }
                return _model3dPreferencesDataCollection;
            }
        }

        private IMongoCollection<DocumentiItemDoc>? _documentiItemsCollection;
        public IMongoCollection<DocumentiItemDoc> DocumentiItemsCollection
        {
            get
            {
                if (_documentiItemsCollection == null)
                {
                    _documentiItemsCollection = Database.GetCollection<DocumentiItemDoc>(CollectionsName.DocumentiItems);
                }
                return _documentiItemsCollection;
            }
        }

        private IMongoCollection<ReportItemDoc>? _reportItemsCollection;
        public IMongoCollection<ReportItemDoc> ReportItemsCollection
        {
            get
            {
                if (_reportItemsCollection == null)
                {
                    _reportItemsCollection = Database.GetCollection<ReportItemDoc>(CollectionsName.ReportItems);
                }
                return _reportItemsCollection;
            }
        }

        private IMongoCollection<StiliItemDoc>? _stiliItemsCollection;
        public IMongoCollection<StiliItemDoc> StiliItemsCollection
        {
            get
            {
                if (_stiliItemsCollection == null)
                {
                    _stiliItemsCollection = Database.GetCollection<StiliItemDoc>(CollectionsName.StiliItems);
                }
                return _stiliItemsCollection;
            }
        }

        private IMongoCollection<NumericFormatDoc>? _numericFormatsCollection;
        public IMongoCollection<NumericFormatDoc> NumericFormatsCollection
        {
            get
            {
                if (_numericFormatsCollection == null)
                {
                    _numericFormatsCollection = Database.GetCollection<NumericFormatDoc>(CollectionsName.NumericFormats);
                }
                return _numericFormatsCollection;
            }
        }

        private IMongoCollection<Model3dUserViewListDoc>? _model3dUserViewListCollection;
        public IMongoCollection<Model3dUserViewListDoc> Model3dUserViewListCollection
        {
            get
            {
                if (_model3dUserViewListCollection == null)
                {
                    _model3dUserViewListCollection = Database.GetCollection<Model3dUserViewListDoc>(CollectionsName.Model3dUserViewList);
                }
                return _model3dUserViewListCollection;
            }
        }

        private IMongoCollection<ElencoAttivitaItemDoc>? _elencoAttivitaItemsCollection;
        public IMongoCollection<ElencoAttivitaItemDoc> ElencoAttivitaItemsCollection
        {
            get
            {
                if (_elencoAttivitaItemsCollection == null)
                {
                    _elencoAttivitaItemsCollection = Database.GetCollection<ElencoAttivitaItemDoc>(CollectionsName.ElencoAttivitaItems);
                }
                return _elencoAttivitaItemsCollection;
            }
        }

        private IMongoCollection<Model3dUserRotoTranslationDoc>? _model3dUserRotoTranslationCollection;
        public IMongoCollection<Model3dUserRotoTranslationDoc> Model3dUserRotoTranslationCollection
        {
            get
            {
                if (_model3dUserRotoTranslationCollection == null)
                {
                    _model3dUserRotoTranslationCollection = Database.GetCollection<Model3dUserRotoTranslationDoc>(CollectionsName.Model3dUserRotoTranslation);
                }
                return _model3dUserRotoTranslationCollection;
            }
        }

        private IMongoCollection<GanttDataDoc>? _ganttDataCollection;
        public IMongoCollection<GanttDataDoc> GanttDataCollection
        {
            get
            {
                if (_ganttDataCollection == null)
                {
                    _ganttDataCollection = Database.GetCollection<GanttDataDoc>(CollectionsName.GanttData);
                }
                return _ganttDataCollection;
            }
        }

        private IMongoCollection<WBSItemsCreationDataDoc>? _wbsItemsCreationDataCollection;
        public IMongoCollection<WBSItemsCreationDataDoc> WBSItemsCreationDataCollection
        {
            get
            {
                if (_wbsItemsCreationDataCollection == null)
                {
                    _wbsItemsCreationDataCollection = Database.GetCollection<WBSItemsCreationDataDoc>(CollectionsName.WBSItemsCreationData);
                }
                return _wbsItemsCreationDataCollection;
            }
        }
        private IMongoCollection<FogliDiCalcoloDataDoc>? _fogliDiCalcoloDataCollection;
        public IMongoCollection<FogliDiCalcoloDataDoc> FogliDiCalcoloDataCollection
        {
            get
            {
                if (_fogliDiCalcoloDataCollection == null)
                {
                    _fogliDiCalcoloDataCollection = Database.GetCollection<FogliDiCalcoloDataDoc>(CollectionsName.FogliDiCalcoloData);
                }
                return _fogliDiCalcoloDataCollection;
            }
        }

        private IMongoCollection<TagItemDoc>? _tagItemsCollection;
        public IMongoCollection<TagItemDoc> TagItemsCollection
        {
            get
            {
                if (_tagItemsCollection == null)
                {
                    _tagItemsCollection = Database.GetCollection<TagItemDoc>(CollectionsName.TagItems);
                }
                return _tagItemsCollection;
            }
        }



        private IMongoCollection<Models.GruppoUtentiDoc>? _GruppiUtentiCollection;
        public IMongoCollection<Models.GruppoUtentiDoc> GruppiUtentiCollection
        {
            get
            {
                if (_GruppiUtentiCollection == null)
                {
                    _GruppiUtentiCollection = Database.GetCollection<Models.GruppoUtentiDoc>(CollectionsName.GruppiUtenti);
                }
                return _GruppiUtentiCollection;
            }
        }

        private IMongoCollection<ClienteDoc>? _clientiCollection;
        public IMongoCollection<ClienteDoc> ClientiCollection
        {
            get
            {
                if (_clientiCollection == null)
                {
                    _clientiCollection = Database.GetCollection<ClienteDoc>(CollectionsName.Clienti);
                }
                return _clientiCollection;
            }
        }

        private IMongoCollection<TeamDoc>? _teamsCollection;
        public IMongoCollection<TeamDoc> TeamsCollection
        {
            get
            {
                if (_teamsCollection == null)
                {
                    _teamsCollection = Database.GetCollection<TeamDoc>(CollectionsName.Teams);
                }
                return _teamsCollection;
            }
        }

        private IMongoCollection<PermessoDoc>? _permessiCollection;
        public IMongoCollection<PermessoDoc> PermessiCollection
        {
            get
            {
                if (_permessiCollection == null)
                {
                    _permessiCollection = Database.GetCollection<PermessoDoc>(CollectionsName.Permessi);
                }
                return _permessiCollection;
            }
        }

        private IMongoCollection<BsonDocument>? _gridFsFilesCollection;
        public IMongoCollection<BsonDocument> GridFsFilesCollection
        {
            get
            {
                if (_gridFsFilesCollection == null)
                {
                    _gridFsFilesCollection = Database.GetCollection<BsonDocument>("fs.files");
                }
                return _gridFsFilesCollection;
            }
        }


        public MongoDbService(IOptions<MongoSettings> mongoSettingsOptions)
        {
            _mongoSettings = mongoSettingsOptions.Value;
        }

        public async Task DeleteDatabaseAsync()
        {
            await Client.DropDatabaseAsync(_mongoSettings.DatabaseName);
        }

        public async Task AddProgetto(ProgettoDoc progetto, ProjectDocuments projectDocs)
        {
            if (projectDocs == null)
                return;

            using (var session = await Client.StartSessionAsync())
            {
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    session.StartTransaction();


                //sovrascrittura autoamtica? da valutare
                ProgettoDoc progOld = await ProgettiCollection.Find(p => 
                                                       p.OperaId == progetto.OperaId &&
                                                       p.Nome == progetto.Nome)
                                                       .FirstOrDefaultAsync();

                if (progOld == null)
                {
                    progetto.ContentCreationDate = DateTime.Now;
                    progetto.ContentLastWriteDate = progetto.ContentCreationDate;
                    await ProgettiCollection.InsertOneAsync(progetto);
                }
                else
                {
                    //aggiorno i field del documento
                    progetto.Id = progOld.Id;
                    progetto.ContentCreationDate = progOld.ContentCreationDate;
                    progetto.ContentLastWriteDate = DateTime.Now;
                    var up = await ProgettiCollection.FindOneAndReplaceAsync(p => p.Id == progetto.Id, progetto);
                }

                await DeleteProjectDocuments(progetto.Id);

                await AddProjectDocuments(progetto.Id, projectDocs);

                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    await session.CommitTransactionAsync();

            }


        }

        //public async Task<long> ReplaceProject(Guid progettoId, ProjectDocuments? project)
        //{
        //    if (project == null)
        //        return 0;

        //    long deletedCount = 0;

        //    using (var session = await Client.StartSessionAsync())
        //    {
        //        if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
        //            session.StartTransaction();

        //        deletedCount = await DeleteProject(progettoId);

        //        await AddProject(progettoId, project);

        //        if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
        //            await session.CommitTransactionAsync();

        //    }

        //    return deletedCount;
        //}

        public async Task<long> DeleteOpere(IEnumerable<Guid> opereId)
        {
            long deletedCount = -1;

            using (var session = await Client.StartSessionAsync())
            {
                //inizio transazione
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    session.StartTransaction();

                deletedCount = await DeleteOpere_internal(opereId);

                //fine transazione
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                {
                    if (deletedCount >= 0)
                        await session.CommitTransactionAsync();
                    else
                        await session.AbortTransactionAsync();
                }

            }

            return deletedCount;
        }

        public async Task<long> DeleteProgetti(IEnumerable<Guid> progettiId)
        {
            long deletedCount = -1;

            using (var session = await Client.StartSessionAsync())
            {
                //inizio transazione
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    session.StartTransaction();

                deletedCount = await DeleteProgetti_internal(progettiId);

                //fine transazione
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                {
                    if (deletedCount >= 0)
                        await session.CommitTransactionAsync();
                    else
                        await session.AbortTransactionAsync();
                }

            }

            return deletedCount;
        }






        private async Task<long> DeleteOpere_internal(IEnumerable<Guid> opereId)
        {
            long deletedCount = -1;
            HashSet<Guid> opereSet = new HashSet<Guid>(opereId);

            var progettiDoc = ProgettiCollection.Find(p => opereSet.Contains(p.OperaId)).ToList();

            //delete progetti + projects
            if (await DeleteProgetti_internal(progettiDoc.Select(p => p.Id)) < 0)
                return -1;


            //delete opere
            var res = await OpereCollection.DeleteManyAsync(o => opereSet.Contains(o.Id));
            if (res.IsAcknowledged)
                deletedCount = res.DeletedCount;
            else
                return -1;

            return deletedCount;
        }

        private async Task<long> DeleteProgetti_internal(IEnumerable<Guid> progettiId)
        {
            long deletedCount = -1;
            HashSet<Guid> progettiSet = new HashSet<Guid>(progettiId);

            //delete projects
            foreach (var progettoId in progettiId)
            {
                if (await DeleteProjectDocuments(progettoId) < 0)
                    return - 1;
            }

            //delete progetti
            var res = await ProgettiCollection.DeleteManyAsync(p => progettiSet.Contains(p.Id));
            if (res.IsAcknowledged)
                deletedCount = res.DeletedCount;
            else
                return -1;

            return deletedCount;
        }

        private async Task<long> DeleteProjectDocuments(Guid progettoId)
        {
            long deletedCount = 0;

            //Definizione attributi
            var res = await DefinizioniAttributoCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Entity types
            res = await EntityTypesCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //variabili
            res = await VariabiliItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Divisioni
            res = await DivisioniItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //capitoli
            res = await CapitoliItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //prezzario
            res = await PrezzarioItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //elementi
            res = await ElementiItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //computo
            res = await ComputoItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dFileCollection
            res = await Model3dFileCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //allegati
            res = await AllegatiItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //contatti
            res = await ContattiItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //infoProgetto
            res = await InfoProgettoItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;


            //wbs
            res = await WBSItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //calendari
            res = await CalendariItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;


            //ViewSettings
            res = await ViewSettingsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dFiltersData
            res = await Model3dFiltersDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dValuesData
            res = await Model3dValuesDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dTagsData
            res = await Model3dTagsDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dPreferencesData
            res = await Model3dPreferencesDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //DocumentiItems
            res = await DocumentiItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //ReportItems
            res = await ReportItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //StiliItems
            res = await StiliItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //NumericFormats
            res = await NumericFormatsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dUserViewList
            res = await Model3dUserViewListCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //ElencoAttivitaItems
            res = await ElencoAttivitaItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //Model3dUserRotoTranslation
            res = await Model3dUserRotoTranslationCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //GanttData
            res = await GanttDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //WBSItemsCreationData
            res = await WBSItemsCreationDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //FogliDiCalcoloData
            res = await FogliDiCalcoloDataCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;

            //TagItems
            res = await TagItemsCollection.DeleteManyAsync(p => p.ProgettoId == progettoId);
            if (res.IsAcknowledged)
                deletedCount += res.DeletedCount;
            else
                return -1;




            return deletedCount;


        }


        private async Task AddProjectDocuments(Guid progettoId, ProjectDocuments projectDocs)
        {
            
            if (projectDocs.DefinizioniAttributo?.Any() == true)
            {
                //assegnazione di progettoId
                projectDocs.DefinizioniAttributo.ForEach(item => item.ProgettoId = progettoId);
                //inserimento
                await DefinizioniAttributoCollection.InsertManyAsync(projectDocs.DefinizioniAttributo);
            }

            if (projectDocs.EntityTypes?.Any() == true)
            {
                //assegnazione di progettoId
                projectDocs.EntityTypes.ForEach(item => item.ProgettoId = progettoId);
                //inserimento
                await EntityTypesCollection.InsertManyAsync(projectDocs.EntityTypes);
            }

            if (projectDocs.VariabiliItem != null)
            {
                //assegnazione di progettoId
                projectDocs.VariabiliItem.ProgettoId = progettoId;
                //inserimento
                await VariabiliItemsCollection.InsertOneAsync(projectDocs.VariabiliItem);
            }

            if (projectDocs.DivisioniItems?.Any() == true)
            {
                //assegnazione di progettoId
                //project.DivisioniItems.ForEach(item => item.ProgettoId = progettoId);
                for (int i = 0; i < projectDocs.DivisioniItems?.Count; i++)
                {
                    projectDocs.DivisioniItems[i].ProgettoId = progettoId;
                    projectDocs.DivisioniItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await DivisioniItemsCollection.InsertManyAsync(projectDocs.DivisioniItems);
            }

            if (projectDocs.CapitoliItems?.Any() == true)
            {
                //assegnazione di progettoId
                //project.CapitoliItems.ForEach(item => item.ProgettoId = progettoId);
                for (int i = 0; i < projectDocs.CapitoliItems?.Count; i++)
                {
                    projectDocs.CapitoliItems[i].ProgettoId = progettoId;
                    projectDocs.CapitoliItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await CapitoliItemsCollection.InsertManyAsync(projectDocs.CapitoliItems);
            }

            if (projectDocs.PrezzarioItems?.Any() == true)
            {
                //assegnazione di progettoId
                //project.PrezzarioItems.ForEach(item => item.ProgettoId = progettoId);
                for (int i = 0; i < projectDocs.PrezzarioItems?.Count; i++)
                {
                    projectDocs.PrezzarioItems[i].ProgettoId = progettoId;
                    projectDocs.PrezzarioItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await PrezzarioItemsCollection.InsertManyAsync(projectDocs.PrezzarioItems);
            }

            if (projectDocs.ElementiItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.ElementiItems?.Count; i++)
                {
                    projectDocs.ElementiItems[i].ProgettoId = progettoId;
                    projectDocs.ElementiItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await ElementiItemsCollection.InsertManyAsync(projectDocs.ElementiItems);
            }

            if (projectDocs.ComputoItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.ComputoItems?.Count; i++)
                {
                    projectDocs.ComputoItems[i].ProgettoId = progettoId;
                    projectDocs.ComputoItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await ComputoItemsCollection.InsertManyAsync(projectDocs.ComputoItems);
            }

            if (projectDocs.Model3dFilesInfo != null)
            {
                //assegnazione di progettoId
                projectDocs.Model3dFilesInfo.ProgettoId = progettoId;
                //inserimento
                await Model3dFileCollection.InsertOneAsync(projectDocs.Model3dFilesInfo);
            }

            if (projectDocs.AllegatiItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.AllegatiItems?.Count; i++)
                {
                    projectDocs.AllegatiItems[i].ProgettoId = progettoId;
                    projectDocs.AllegatiItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await AllegatiItemsCollection.InsertManyAsync(projectDocs.AllegatiItems);
            }

            if (projectDocs.ContattiItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.ContattiItems?.Count; i++)
                {
                    projectDocs.ContattiItems[i].ProgettoId = progettoId;
                    projectDocs.ContattiItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await ContattiItemsCollection.InsertManyAsync(projectDocs.ContattiItems);
            }

            if (projectDocs.InfoProgettoItem != null)
            {
                //assegnazione di progettoId
                projectDocs.InfoProgettoItem.ProgettoId = progettoId;
                //inserimento
                await InfoProgettoItemsCollection.InsertOneAsync(projectDocs.InfoProgettoItem);
            }

            if (projectDocs.WBSItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.WBSItems?.Count; i++)
                {
                    projectDocs.WBSItems[i].ProgettoId = progettoId;
                    projectDocs.WBSItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await WBSItemsCollection.InsertManyAsync(projectDocs.WBSItems);
            }

            if (projectDocs.CalendariItems?.Any() == true)
            {
                //assegnazione di progettoId
                for (int i = 0; i < projectDocs.CalendariItems?.Count; i++)
                {
                    projectDocs.CalendariItems[i].ProgettoId = progettoId;
                    projectDocs.CalendariItems[i].Position = new OrderPosition(i, 1);
                }
                //inserimento
                await CalendariItemsCollection.InsertManyAsync(projectDocs.CalendariItems);
            }

            if (projectDocs.ViewSettings != null)
            {
                //assegnazione di progettoId
                projectDocs.ViewSettings.ProgettoId = progettoId;
                //inserimento
                await ViewSettingsCollection.InsertOneAsync(projectDocs.ViewSettings);
            }

            if (projectDocs.Model3dFiltersData != null)
            {
                //assegnazione di progettoId
                projectDocs.Model3dFiltersData.ProgettoId = progettoId;
                //inserimento
                await Model3dFiltersDataCollection.InsertOneAsync(projectDocs.Model3dFiltersData);
            }

            if (projectDocs.Model3dValuesData != null)
            {
                //assegnazione di progettoId
                projectDocs.Model3dValuesData.ProgettoId = progettoId;
                //inserimento
                await Model3dValuesDataCollection.InsertOneAsync(projectDocs.Model3dValuesData);
            }

            if (projectDocs.Model3dTagsData != null)
            {
                //assegnazione di progettoId
                projectDocs.Model3dTagsData.ProgettoId = progettoId;
                //inserimento
                await Model3dTagsDataCollection.InsertOneAsync(projectDocs.Model3dTagsData);
            }

            if (projectDocs.Model3dPreferencesData != null)
            {
                projectDocs.Model3dPreferencesData.ProgettoId = progettoId;
                await Model3dPreferencesDataCollection.InsertOneAsync(projectDocs.Model3dPreferencesData);
            }

            if (projectDocs.DocumentiItems?.Any() == true)
            {
                for (int i = 0; i < projectDocs.DocumentiItems?.Count; i++)
                {
                    projectDocs.DocumentiItems[i].ProgettoId = progettoId;
                    projectDocs.DocumentiItems[i].Position = new OrderPosition(i, 1);
                }
                await DocumentiItemsCollection.InsertManyAsync(projectDocs.DocumentiItems);
            }

            if (projectDocs.ReportItems?.Any() == true)
            {
                for (int i = 0; i < projectDocs.ReportItems?.Count; i++)
                {
                    projectDocs.ReportItems[i].ProgettoId = progettoId;
                    projectDocs.ReportItems[i].Position = new OrderPosition(i, 1);
                }
                await ReportItemsCollection.InsertManyAsync(projectDocs.ReportItems);
            }

            if (projectDocs.StiliItems?.Any() == true)
            {
                for (int i = 0; i < projectDocs.StiliItems?.Count; i++)
                {
                    projectDocs.StiliItems[i].ProgettoId = progettoId;
                    projectDocs.StiliItems[i].Position = new OrderPosition(i, 1);
                }
                await StiliItemsCollection.InsertManyAsync(projectDocs.StiliItems);
            }

            if (projectDocs.NumericFormats?.Any() == true)
            {
                for (int i = 0; i < projectDocs.NumericFormats?.Count; i++)
                {
                    projectDocs.NumericFormats[i].ProgettoId = progettoId;
                    projectDocs.NumericFormats[i].Position = new OrderPosition(i, 1);
                }
                await NumericFormatsCollection.InsertManyAsync(projectDocs.NumericFormats);
            }

            ////////////////////////////////////////////////////////////////////////////////////////
            /////
            ////test velocità
            //List<NumericFormatDoc> testDocs = new List<NumericFormatDoc>();
            //for (int i = 0; i< 10000; i++)
            //{
            //    testDocs.Add(new NumericFormatDoc()
            //    {
            //        ProgettoId = progettoId,
            //        Position = new OrderPosition(i, 1),
            //        Format = new string('x', 10000),

            //    });
            //}

            //await NumericFormatsCollection.InsertManyAsync(testDocs);
            ///////////////////////////////////////////////////////////////////////////////////////////



            if (projectDocs.Model3dUserViewList != null)
            {
                projectDocs.Model3dUserViewList.ProgettoId = progettoId;
                await Model3dUserViewListCollection.InsertOneAsync(projectDocs.Model3dUserViewList);
            }

            if (projectDocs.ElencoAttivitaItems?.Any() == true)
            {
                for (int i = 0; i < projectDocs.ElencoAttivitaItems?.Count; i++)
                {
                    projectDocs.ElencoAttivitaItems[i].ProgettoId = progettoId;
                    projectDocs.ElencoAttivitaItems[i].Position = new OrderPosition(i, 1);
                }
                await ElencoAttivitaItemsCollection.InsertManyAsync(projectDocs.ElencoAttivitaItems);
            }

            if (projectDocs.Model3dUserRotoTranslation != null)
            {
                projectDocs.Model3dUserRotoTranslation.ProgettoId = progettoId;
                await Model3dUserRotoTranslationCollection.InsertOneAsync(projectDocs.Model3dUserRotoTranslation);
            }

            if (projectDocs.GanttData != null)
            {
                projectDocs.GanttData.ProgettoId = progettoId;
                await GanttDataCollection.InsertOneAsync(projectDocs.GanttData);
            }

            if (projectDocs.WBSItemsCreationData != null)
            {
                projectDocs.WBSItemsCreationData.ProgettoId = progettoId;
                await WBSItemsCreationDataCollection.InsertOneAsync(projectDocs.WBSItemsCreationData);
            }

            if (projectDocs.FogliDiCalcoloData != null)
            {
                projectDocs.FogliDiCalcoloData.ProgettoId = progettoId;
                await FogliDiCalcoloDataCollection.InsertOneAsync(projectDocs.FogliDiCalcoloData);
            }

            if (projectDocs.TagItems?.Any() == true)
            {
                for (int i = 0; i < projectDocs.TagItems?.Count; i++)
                {
                    projectDocs.TagItems[i].ProgettoId = progettoId;
                    projectDocs.TagItems[i].Position = new OrderPosition(i, 1);
                }
                await TagItemsCollection.InsertManyAsync(projectDocs.TagItems);
            }


        }

        public async Task LoadProjectDocuments(Guid progettoId, ProjectDocuments projDocs)
        {
            projDocs.DefinizioniAttributo = await DefinizioniAttributoCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.EntityTypes = await EntityTypesCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.VariabiliItem = await VariabiliItemsCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.DivisioniItems = await DivisioniItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.CapitoliItems = await CapitoliItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.PrezzarioItems = await PrezzarioItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.ElementiItems = await ElementiItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.ComputoItems = await ComputoItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.Model3dFilesInfo = await Model3dFileCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.AllegatiItems = await AllegatiItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.ContattiItems = await ContattiItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.InfoProgettoItem = await InfoProgettoItemsCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.WBSItems = await WBSItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.CalendariItems = await CalendariItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.ViewSettings = await ViewSettingsCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.Model3dFiltersData = await Model3dFiltersDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.Model3dValuesData = await Model3dValuesDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.Model3dTagsData = await Model3dTagsDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.Model3dPreferencesData = await Model3dPreferencesDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.DocumentiItems = await DocumentiItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.ReportItems = await ReportItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.StiliItems = await StiliItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.NumericFormats = await NumericFormatsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.Model3dUserViewList = await Model3dUserViewListCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.ElencoAttivitaItems = await ElencoAttivitaItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();
            projDocs.Model3dUserRotoTranslation = await Model3dUserRotoTranslationCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.GanttData = await GanttDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.WBSItemsCreationData = await WBSItemsCreationDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.FogliDiCalcoloData = await FogliDiCalcoloDataCollection.Find(item => item.ProgettoId == progettoId).FirstOrDefaultAsync();
            projDocs.TagItems = await TagItemsCollection.Find(item => item.ProgettoId == progettoId).ToListAsync();

        }

        public UploadFileResult UploadFile (Stream fileContentStream, string? fileName = null, string? contentType = null, IEnumerable<KeyValuePair<string,object>>? additionalMetadata = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
            }
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/octet-stream";
            }
            if (additionalMetadata == null)
            {
                additionalMetadata = new Dictionary<string, object>();
            }

            var bucket = new GridFSBucket(_database);

            var uploadGuid = Guid.NewGuid();

            var objectId =  bucket.UploadFromStream(fileName, fileContentStream, new GridFSUploadOptions { Metadata = new BsonDocument {{ "contentType", contentType }, { "uploadGuid", uploadGuid.ToBsonBinaryData() } }.AddRange(additionalMetadata) });

            return new UploadFileResult
            {
                FileName = fileName,
                ObjectId = objectId,
                UploadGuid = uploadGuid
            };
        }

        public class UploadFileResult
        {
            public string FileName { get; set; }
            public ObjectId ObjectId { get; set; }
            public Guid UploadGuid { get; set; }
        }

        public DownloadFileResult DownloadFile(Expression<Func<GridFSFileInfo,bool>> filterExpr)
        {
            var bucket = new GridFSBucket(_database);
            var filter = Builders<GridFSFileInfo>.Filter.Where(filterExpr);
            var result = bucket.Find(filter);
            var findInfo = result.FirstOrDefault();
            if (findInfo != null)
            {
                return new DownloadFileResult
                {
                    Found = true,
                    Name = findInfo.Filename,
                    ContentType = findInfo.Metadata["contentType"].AsString,
                    ContentStream = bucket.OpenDownloadStream(findInfo.Id),
                    UploadGuid = findInfo.Metadata["uploadGuid"].AsGuid,
                    Size = findInfo.Length,
                    Compressed = findInfo.Metadata.Contains("compressed") ? findInfo.Metadata["compressed"].AsBoolean : false,
                    UploadDateTime = findInfo.UploadDateTime,
                    OperaId = findInfo.Metadata.Contains("operaId") ? findInfo.Metadata["operaId"].AsGuid : null,
                    ParentId = findInfo.Metadata.Contains("parentGuid") ? findInfo.Metadata["parentGuid"].AsGuid : null,
                    NumeroProgettiAllegato = null,
                    NumeroProgettiModello = null,
                    ConversionState = findInfo.Metadata.Contains("ConversionInProgress") && findInfo.Metadata["ConversionInProgress"].AsBoolean ? AllegatoConversionState.ConversionInProgress : AllegatoConversionState.NotConverted
                };
            }
            else
            {
                return new DownloadFileResult
                {
                    Found = false
                };
            }
        }

        public class DownloadFileResult
        {
            public bool Found { get; set; }
            public string? Name { get; set; }
            public string? ContentType { get; set; }
            public Stream? ContentStream { get; set; }
            public Guid? UploadGuid { get; set; }
            public long? Size { get; set; }
            public bool? Compressed { get; set; }
            public DateTime? UploadDateTime { get; set; }
            public Guid? OperaId { get; set; }
            public Guid? ParentId  { get; set; }
            public int? NumeroProgettiAllegato  { get; set; }
            public int? NumeroProgettiModello{ get; set; }
            public AllegatoConversionState? ConversionState{ get; set; }

        }

        public IEnumerable<GridFSFileInfo> GetFilesInfo(Expression<Func<GridFSFileInfo, bool>> filterExpr)
        {
            var bucket = new GridFSBucket(_database);
            var filter = Builders<GridFSFileInfo>.Filter.Where(filterExpr);
            var result = bucket.Find(filter);
            return result.ToList();
        }

        public int DeleteFiles(Expression<Func<GridFSFileInfo, bool>> filterExpr)
        {
            var files = GetFilesInfo(filterExpr);
            var bucket = new GridFSBucket(_database);
            foreach (var file in files)
            {
                bucket.Delete(file.Id);
            }
            return files.Count();
        }

        public bool DeleteFile(ObjectId id)
        {
            var bucket = new GridFSBucket(_database);
            try
            {
                bucket.Delete(id);
            }
            catch (Exception e)
            {

                throw;
            }
            return true;
        }

        public async void InitCliente(string codiceCliente, string nomeCliente, string adminEmail, string licenseCode)
        {
            //per il replicaSet modificare C:\Program Files\MongoDB\Server\7.0\bin\mongod.cfg
            //aggiungere

            //#replication:
            //replication:
            //  replSetName: "testrep"



            using (var session = await Client.StartSessionAsync())
            {
                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    session.StartTransaction();

                var foundCliente = await ClientiCollection.Find(item => item.CodiceCliente == codiceCliente).FirstOrDefaultAsync();
                if (foundCliente == null)
                {
                    ClienteDoc cliente = new ClienteDoc();
                    cliente.ChiaveLicenza = licenseCode;
                    cliente.CodiceCliente = codiceCliente;
                    cliente.Nome = nomeCliente;
                    await ClientiCollection.InsertOneAsync(cliente);

                    SettoreDoc settore = new SettoreDoc();
                    settore.Nome = "Settore 1";
                    settore.ClienteId = cliente.Id;
                    await SettoriCollection.InsertOneAsync(settore);

                    TeamDoc team = new TeamDoc();
                    team.Nome = "Amministratori";
                    team.ClienteId = cliente.Id;
                    team.IsAdmin = true;
                    await TeamsCollection.InsertOneAsync(team);

                    UtenteDoc utente = new UtenteDoc();
                    utente.Nome = "";
                    utente.TeamsIds.Add(team.Id);
                    utente.Email = adminEmail;
                    utente.EmailConfirmed = true;
                    await UtentiCollection.InsertOneAsync(utente);

                    RuoloDoc ruolo = new RuoloDoc();
                    ruolo.Name = "Amministratore";
                    ruolo.NormalizedName = "AMMINISTRATORE";
                    ruolo.Version = 1;
                    ruolo.Claims.Add(new AspNetCore.Identity.MongoDbCore.Models.MongoClaim() { Type = ApplicationClaimTypes.Azione, Value = Enum.GetName(typeof(Azioni), Azioni.DefaultAction) });
                    await RuoliCollection.InsertOneAsync(ruolo);

                    RuoloDoc ruoloVis = new RuoloDoc();
                    ruolo.Name = "Visibile";
                    ruolo.NormalizedName = "VISIBILE";
                    ruolo.Version = 1;
                    ruolo.Claims.Add(new AspNetCore.Identity.MongoDbCore.Models.MongoClaim() { Type = ApplicationClaimTypes.Azione, Value = Enum.GetName(typeof(Azioni), Azioni.Visibile) });
                    ruolo.Claims.Add(new AspNetCore.Identity.MongoDbCore.Models.MongoClaim() { Type = ApplicationClaimTypes.Ereditabile, Value = "false" });
                    await RuoliCollection.InsertOneAsync(ruolo);

                    PermessoDoc permesso = new PermessoDoc();
                    permesso.OggettoTipo = Enum.GetName<TipiOggettoPermessi>(TipiOggettoPermessi.Cliente)!;
                    permesso.OggettoId = cliente.Id;
                    permesso.SoggettoId = team.Id;
                    permesso.RuoliIds = new List<Guid> { ruolo.Id };
                    await PermessiCollection.InsertOneAsync(permesso);



                }
                else
                {
                    //cliente già esistente
                    if (licenseCode != foundCliente.ChiaveLicenza)
                    {
                        if (foundCliente.ChiaveLicenza != null)
                        {
                            foundCliente.ArchivioLicenze.Add(foundCliente.ChiaveLicenza);
                            foundCliente.ChiaveLicenza = licenseCode;
                        }
                    }

                    //aggiungo l'utente e lo associo al team amministratori
                    var foundUtente = await UtentiCollection.Find(item => item.Email == adminEmail).FirstOrDefaultAsync();
                    var foundAdminTeam = await TeamsCollection.Find(item => item.IsAdmin && item.ClienteId == foundCliente.Id).FirstOrDefaultAsync();

                    if (foundUtente == null && foundAdminTeam != null)
                    {
                        UtenteDoc utente = new UtenteDoc();
                        utente.Nome = adminEmail;
                        utente.TeamsIds.Add(foundAdminTeam.Id);
                        utente.Email = adminEmail;
                        utente.EmailConfirmed = true;
                        await UtentiCollection.InsertOneAsync(utente);
                    }
                }

                if (Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
                    await session.CommitTransactionAsync();
            }
        }

        private bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var options = new ListCollectionNamesOptions { Filter = filter };
            return database.ListCollectionNames(options).Any();
        }


    }
}
