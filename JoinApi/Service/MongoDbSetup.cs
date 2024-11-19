using ModelData.Utilities;
using JoinApi.Models;
using JoinApi.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TagDoc = JoinApi.Models.TagDoc;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace JoinApi.Service
{
    public class MongoDbSetup : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MongoDbService _mongoDbService;
        private readonly MongoSettings _mongoSettings;

        private readonly MongoClient _client;

        public MongoDbSetup(IOptions<MongoSettings> mongoSettingsOptions, IServiceScopeFactory serviceScopeFactory, MongoDbService mongoDbService)
        {
            _mongoSettings = mongoSettingsOptions.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _mongoDbService = mongoDbService;
            _client = new MongoClient(_mongoSettings.ConnectionURI);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!_mongoDbService.ConnectionEstablished.HasValue)
                    await _mongoDbService.Init();

                if (_mongoDbService.ConnectionEstablished.GetValueOrDefault())
                {
                    if (_mongoSettings.ResetDatabase)
                    {
                        await _mongoDbService.DeleteDatabaseAsync();
                    }

                    SetupTables();

                    RegisterClassMap();

                    SetupUsersAsync();

                }
                Log.Information($"MongoDB : Procedura di connessione ed inizializzazione a database {_mongoSettings.DatabaseName} completata correttamente.");

            }
            catch (Exception ex)
            {
                Log.ForContext("Eccezione", ex).Error($"MongoDB : Eccezione durante il tentativo di inizializzazione del database {_mongoSettings.DatabaseName}. Dettaglio {ex.Message}.");
            }
            await Task.CompletedTask;
        }

        private void SetupTables()
        {
            _mongoDbService.SettoriCollection.Indexes.CreateOne(
                new CreateIndexModel<SettoreDoc>(Builders<SettoreDoc>.IndexKeys.Ascending(s => s.ClienteId).Ascending(s => s.Nome),
                new CreateIndexOptions { Unique = true, Name = "settori_clienteId_nome" }));

            _mongoDbService.OpereCollection.Indexes.CreateOne(
                new CreateIndexModel<OperaDoc>(Builders<OperaDoc>.IndexKeys.Ascending(o => o.SettoreId).Ascending(o => o.Nome),
                new CreateIndexOptions { Unique = true, Name = "opere_settoreId_nome" }));

            _mongoDbService.ProgettiCollection.Indexes.CreateOne(
                new CreateIndexModel<ProgettoDoc>(Builders<ProgettoDoc>.IndexKeys.Ascending(p => p.OperaId).Ascending(p => p.Nome),
                new CreateIndexOptions { Unique = true, Name = "progetti_operaId_nome" }));

            _mongoDbService.TagsCollection.Indexes.CreateOne(
                new CreateIndexModel<TagDoc>(Builders<TagDoc>.IndexKeys.Ascending(p => p.ClienteId).Ascending(p => p.Nome),
                new CreateIndexOptions { Unique = true, Name = "tag_clienteId_nome" }));

            _mongoDbService.ComputoItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<ComputoItemDoc>(Builders<ComputoItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "computoItem_progettoId_entityId" }));

            _mongoDbService.ElementiItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<ElementiItemDoc>(Builders<ElementiItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "elementiItem_progettoId_entityId" }));

            _mongoDbService.PrezzarioItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<PrezzarioItemDoc>(Builders<PrezzarioItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "prezzarioItem_progettoId_entityId" }));

            _mongoDbService.CapitoliItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<CapitoliItemDoc>(Builders<CapitoliItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "capitoliItem_progettoId_entityId" }));

            _mongoDbService.DefinizioniAttributoCollection.Indexes.CreateOne(
                new CreateIndexModel<DefinizioneAttributoDoc>(Builders<DefinizioneAttributoDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.Codice),
                new CreateIndexOptions { Unique = true, Name = "definizioneAttributo_progettoId_codice" }));

            _mongoDbService.EntityTypesCollection.Indexes.CreateOne(
                new CreateIndexModel<EntityTypeDoc>(Builders<EntityTypeDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = false, Name = "entityTypes_progettoId" }));

            _mongoDbService.DivisioniItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<DivisioneItemDoc>(Builders<DivisioneItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.DivisioneId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "divisioneItem_progettoId_divisioneId_entityId" }));

            _mongoDbService.VariabiliItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<VariabiliItemDoc>(Builders<VariabiliItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "variabiliItem_progettoId_entityId" }));

            _mongoDbService.Model3dFileCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dFilesInfoDoc>(Builders<Model3dFilesInfoDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dFilesInfo_progettoId" }));

            _mongoDbService.AllegatiItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<AllegatiItemDoc>(Builders<AllegatiItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "allegatiItem_progettoId_entityId" }));

            _mongoDbService.ContattiItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<ContattiItemDoc>(Builders<ContattiItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "contattiItem_progettoId_entityId" }));

            _mongoDbService.InfoProgettoItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<InfoProgettoItemDoc>(Builders<InfoProgettoItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "infoProgettoItem_progettoId_entityId" }));

            _mongoDbService.WBSItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<WBSItemDoc>(Builders<WBSItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "wbsItem_progettoId_entityId" }));

            _mongoDbService.CalendariItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<CalendariItemDoc>(Builders<CalendariItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "calendariItem_progettoId_entityId" }));

            _mongoDbService.ViewSettingsCollection.Indexes.CreateOne(
                new CreateIndexModel<ViewSettingsDoc>(Builders<ViewSettingsDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "viewSettings_progettoId" }));

            _mongoDbService.Model3dFiltersDataCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dFiltersDataDoc>(Builders<Model3dFiltersDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dFiltersData_progettoId" }));

            _mongoDbService.Model3dValuesDataCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dValuesDataDoc>(Builders<Model3dValuesDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dValuesData_progettoId" }));

            _mongoDbService.Model3dTagsDataCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dTagsDataDoc>(Builders<Model3dTagsDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dTagsData_progettoId" }));

            _mongoDbService.Model3dPreferencesDataCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dPreferencesDataDoc>(Builders<Model3dPreferencesDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dPreferencesData_progettoId" }));

            _mongoDbService.DocumentiItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<DocumentiItemDoc>(Builders<DocumentiItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "documentiItem_progettoId_entityId" }));

            _mongoDbService.ReportItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<ReportItemDoc>(Builders<ReportItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "reportItem_progettoId_entityId" }));

            _mongoDbService.StiliItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<StiliItemDoc>(Builders<StiliItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "stiliItem_progettoId_entityId" }));

            _mongoDbService.NumericFormatsCollection.Indexes.CreateOne(
                new CreateIndexModel<NumericFormatDoc>(Builders<NumericFormatDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = false, Name = "numericFormat_progettoId" }));

            _mongoDbService.Model3dUserViewListCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dUserViewListDoc>(Builders<Model3dUserViewListDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dUserViewList_progettoId" }));

            _mongoDbService.ElencoAttivitaItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<ElencoAttivitaItemDoc>(Builders<ElencoAttivitaItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "elencoAttivitaItems_progettoId_entityId" }));

            _mongoDbService.Model3dUserRotoTranslationCollection.Indexes.CreateOne(
                new CreateIndexModel<Model3dUserRotoTranslationDoc>(Builders<Model3dUserRotoTranslationDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "model3dUserRotoTranslation_progettoId" }));

            _mongoDbService.GanttDataCollection.Indexes.CreateOne(
                new CreateIndexModel<GanttDataDoc>(Builders<GanttDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "ganttData_progettoId" }));

            _mongoDbService.WBSItemsCreationDataCollection.Indexes.CreateOne(
                new CreateIndexModel<WBSItemsCreationDataDoc>(Builders<WBSItemsCreationDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "wbsItemsCreationData_progettoId" }));

            _mongoDbService.FogliDiCalcoloDataCollection.Indexes.CreateOne(
                new CreateIndexModel<FogliDiCalcoloDataDoc>(Builders<FogliDiCalcoloDataDoc>.IndexKeys.Ascending(p => p.ProgettoId),
                new CreateIndexOptions { Unique = true, Name = "fogliDiCalcoloData_progettoId" }));

            _mongoDbService.TagItemsCollection.Indexes.CreateOne(
                new CreateIndexModel<TagItemDoc>(Builders<TagItemDoc>.IndexKeys.Ascending(p => p.ProgettoId).Ascending(p => p.EntityId),
                new CreateIndexOptions { Unique = true, Name = "tagItems_progettoId_entityId" }));

            _mongoDbService.GruppiUtentiCollection.Indexes.CreateOne(
                new CreateIndexModel<GruppoUtentiDoc>(Builders<GruppoUtentiDoc>.IndexKeys.Ascending(p => p.OperaId).Ascending(p => p.Nome),
                new CreateIndexOptions { Unique = true, Name = "gruppoUtenti_operaId_nome" }));

            _mongoDbService.ClientiCollection.Indexes.CreateOne(
                new CreateIndexModel<ClienteDoc>(Builders<ClienteDoc>.IndexKeys.Ascending(c => c.CodiceCliente).Ascending(c => c.Nome),
                new CreateIndexOptions { Unique = true, Name = "clienti_codiceCliente_nome" }));

            _mongoDbService.TeamsCollection.Indexes.CreateOne(
                new CreateIndexModel<TeamDoc>(Builders<TeamDoc>.IndexKeys.Ascending(t => t.ClienteId).Ascending(t => t.Nome),
                new CreateIndexOptions { Unique = true, Name = "teams_clienteId_nome" }));

            //_mongoDbService.PermessiCollection.Indexes.CreateOne(
            //    new CreateIndexModel<PermessoDoc>(Builders<PermessoDoc>.IndexKeys.Ascending(p => p.RuoloId).Ascending(p => p.Nome),
            //    new CreateIndexOptions { Unique = true, Name = "permessi_ruoloId_nome" }));
            _mongoDbService.PermessiCollection.Indexes.CreateOne(
                new CreateIndexModel<PermessoDoc>(Builders<PermessoDoc>.IndexKeys.Ascending(p => p.OggettoId).Ascending(p => p.SoggettoId),
                new CreateIndexOptions { Unique = true, Name = "permessi_oggettoId_soggettoId" }));
            _mongoDbService.PermessiCollection.Indexes.CreateOne(
                new CreateIndexModel<PermessoDoc>(Builders<PermessoDoc>.IndexKeys.Ascending(p => p.SoggettoId).Ascending(p => p.OggettoId),
                new CreateIndexOptions { Unique = true, Name = "permessi_soggettoId_oggettoId" }));
        }

        private void RegisterClassMap()
        {
            //base class Attributo
            BsonClassMap.RegisterClassMap<ModelData.Model.AttributoRiferimento>();

            //base class Valore
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreGuid>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreTesto>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreTestoRtf>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreReale>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreContabilita>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreData>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreBooleano>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreElenco>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreColore>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreFormatoNumero>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreGuidCollection>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreTestoCollection>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreCollectionItem>();

            //base class ValoreAttributo
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoElenco>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoColore>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoFormatoNumero>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoRiferimentoGuidCollection>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoVariabili>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoGuidCollection>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoReale>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoContabilita>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoGuid>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreAttributoTesto>();

            //base class ValoreCollectionItem
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreTestoCollectionItem>();
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreGuidCollectionItem>();

            //base class ValoreCondition
            BsonClassMap.RegisterClassMap<ModelData.Model.ValoreConditionsGroup>();
            BsonClassMap.RegisterClassMap<ModelData.Model.AttributoValoreConditionSingle>();


        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
        //private async Task SetupRolesAsync()
        //{
        //    using var scope = _serviceScopeFactory.CreateScope();
        //    using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RuoloDoc>>();

        //    if (!await roleManager.RoleExistsAsync(RuoliUtente.UTENTE))
        //    {
        //        await roleManager.CreateAsync(new RuoloDoc(RuoliUtente.AMMINISTRATORE));
        //    }

        //    if (!await roleManager.RoleExistsAsync(RuoliUtente.UTENTE))
        //    {
        //        await roleManager.CreateAsync(new RuoloDoc(RuoliUtente.UTENTE));
        //    }

        //    await Task.CompletedTask;
        //}

        private async Task SetupUsersAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UtenteDoc>>();
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RuoloDoc>>();

            foreach (var utente in await userManager.GetUsersInRoleAsync(RuoliAuth0.DIGICORP))
            {
                await userManager.AddToRoleAsync(utente, RuoliAuth0.REGISTERED);
            }
        }
    }
}
