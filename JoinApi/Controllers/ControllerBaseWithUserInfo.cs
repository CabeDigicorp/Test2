using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi.Models;
using AutoMapper;
using Humanizer.Localisation;
using JoinApi.Models;
using JoinApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelData.Utilities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Text.Json;


namespace JoinApi.Controllers
{
    public abstract class ControllerBaseWithUserInfo : ControllerBase
    {
        protected readonly MongoDbService _mongoDbService;
        protected IMapper _mapper;
        protected readonly HttpClient _httpClient;
        private readonly AuthSupportService _authSupport;
        private readonly Serilog.ILogger _logger;

        public ControllerBaseWithUserInfo(IMapper mapper,
                                MongoDbService mongoDbService,
                                HttpClient httpClient,
                                AuthSupportService authSupport, 
                                Serilog.ILogger logger)
        {
            _mapper = mapper;
            _mongoDbService = mongoDbService;
            _httpClient = httpClient;
            _authSupport = authSupport;
            _logger = logger;
        }


        protected bool IsInRole(string role)
        {
            return _authSupport.GetCurrentUserInfo(HttpContext)?.Auth0Roles?.Contains(role) ?? false;
        }


        protected UserFullInfo? CurrentUser
        {
            get {
                return _authSupport.GetCurrentUserInfo(HttpContext);
            }
        }

        protected string? CurrentUserEmail
        {
            get
            {
                return _authSupport.GetCurrentUserInfo(HttpContext)?.Auth0Info.Email;
            }
        }

        protected Guid CurrentUserId
        {
            get
            {
                return _authSupport.GetCurrentUserInfo(HttpContext)?.JoinInfo.Id ?? Guid.Empty;
            }
        }

        protected Dictionary<Guid, string> CurrentUserClientiIdNames
        {
            get
            {
                var clienti = _authSupport.GetCurrentUserInfo(HttpContext)?.JoinInfo.Clienti;
                Dictionary<Guid, string> result = clienti?.ToDictionary<ClienteDoc, Guid, string>(x => x.Id, y => y.Nome) ?? new Dictionary<Guid, string>();
                return result;
            }
        }

        protected List<Guid> CurrentUserClientiIds
        {
            get
            {
                var clienti = _authSupport.GetCurrentUserInfo(HttpContext)?.JoinInfo?.Clienti;
                List<Guid> result = clienti?.Select(x => x.Id).ToList() ?? new List<Guid>();
                return result;
            }
        }

        protected void CurrentUserLogout()
        {
            _authSupport.Logout(HttpContext);
        }

        [Authorize]
        protected bool CheckPermissions(IOggettoPermessiBase oggetto, Azioni? azione = null)
        {
            _logger.ForContext("Azione", azione).
                    Information("Accesso controller ControllerBaseWithUserInfo, richiesta CheckPermissions.");

            if (CurrentUser?.JoinInfo == null || CurrentUser.JoinInfo.Disabled) return false;

            List<Guid> antenati = new List<Guid>();
            OperaDoc operaPadre = null;

            //ricerco antenati
            if (oggetto == null)
            {
                _logger.ForContext("Azione", azione).
                       Error("Uscita controller ControllerBaseWithUserInfo, richiesta CheckPermissions. parametro \"oggetto\" null");
                return false;
            }

            antenati.Add(oggetto.Id);

            //if (azione != Azioni.Visibile)
            //{
            //l'azione "Visibile" è l'unica a non essere ereditabile.
            if (oggetto is ComputoItemDoc c)
            {
                antenati.Add(c.ProgettoId);
                oggetto = _mongoDbService.ProgettiCollection.Find(p => p.Id == c.ProgettoId).FirstOrDefault();
            }
            if (oggetto is ProgettoDoc p)
            {
                antenati.Add(p.OperaId);
                oggetto = _mongoDbService.OpereCollection.Find(o => o.Id == p.OperaId).FirstOrDefault();
            }
            if (oggetto is OperaDoc o)
            {
                //salvo anche a parte, per ricerca gruppi
                operaPadre = o;

                antenati.Add(o.SettoreId);
                oggetto = _mongoDbService.SettoriCollection.Find(s => s.Id == o.SettoreId).FirstOrDefault();
            }
            if (oggetto is SettoreDoc s)
            {
                antenati.Add(s.ClienteId);
                //mi fermo qui...
            }
            //}
            //Seleziono permessi

            var permessiTeams = (from up in CurrentUser.JoinInfo.Permessi
                                 join utg in CurrentUser.JoinInfo.TeamsUnionGruppi on up.SoggettoId equals utg.Id
                                 from ur in _mongoDbService.RuoliCollection.AsQueryable(null)
                                 where up.RuoliIds.Contains(ur.Id) && (!azione.HasValue || ur.Azioni.Contains(azione.Value))
                                       && ((ur.Inheritable && antenati.Contains(up.OggettoId)) || antenati[0].Equals(up.OggettoId))
                                 select up);

            var count = permessiTeams.Count();

            _logger.ForContext("Azione", azione).
                    Information("Uscita controller ControllerBaseWithUserInfo, richiesta CheckPermissions. Completata correttamente.");

            return count > 0;


        }

        [Authorize]
        protected bool DeleteCurrentUser()
        {
            var id = CurrentUser?.JoinInfo?.Id;
            var email = CurrentUser?.Auth0Info?.Email;

            return false;

            //TODO




        }

    }

    public class UserFullInfo
    {
        public string AuthToken { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public UserInfo Auth0Info { get; private set; }
        public IEnumerable<string?> Auth0Roles { get; private set; }
        public JoinUserInfo? JoinInfo { get; private set; }

        public bool ForceUpdate { get; set; } = false;

        public UserFullInfo(string authToken, DateTime timeStamp, UserInfo auth0Info, JoinUserInfo? joinInfo)
        {
            AuthToken = authToken;
            TimeStamp = timeStamp;
            Auth0Info = auth0Info;
            Auth0Roles = auth0Info.AdditionalClaims.Where(c => c.Key == RuoliAuth0.ROLEKEY).SelectMany(c => c.Value.Children().Values<string>()).ToList();
            JoinInfo = joinInfo;
        }

    }

    public class JoinUserInfo
    {
        public Guid Id { get; private set; }
        //public string FullName { get; private set; }
        public string Nome { get; private set; }
        public string Cognome { get; private set; }

        public bool PrivacyConsent { get; private set; }

        public bool Disabled { get; private set; }

        //public List<string> Roles { get; private set; }

        public List<TeamDoc> Teams { get; private set; }
        public List<GruppoUtentiDoc> Gruppi { get; private set; }
        public List<ClienteDoc> Clienti { get; private set; }

        public string? DomainManagerInfo { get; private set; }
        public IEnumerable<ISoggettoPermessiBase> TeamsUnionGruppi { get; private set; }

        public IEnumerable<PermessoDoc> Permessi { get; internal set; } = new List<PermessoDoc>();



        public JoinUserInfo(Guid id, string cognome, string nome, bool privacyConsent, bool disabled, string? domainManager, List<TeamDoc> teams, List<GruppoUtentiDoc> gruppi, List<ClienteDoc> clienti)
        {
            Id = id;
            Cognome = cognome;
            Nome = nome;
            PrivacyConsent = privacyConsent;
            Disabled = disabled;
            DomainManagerInfo = domainManager;
            //Roles = roles;
            Teams = teams;
            Gruppi = gruppi;
            Clienti = clienti;
            TeamsUnionGruppi = teams.Union<ISoggettoPermessiBase>(gruppi);

            Log.ForContext("Memory", $"Libera: {Utilities.MemoryMetricsClientHelpers.GetMetrics().Free} mb / Usata: {Utilities.MemoryMetricsClientHelpers.GetMetrics().Used} mb / Totale: {Utilities.MemoryMetricsClientHelpers.GetMetrics().Total} mb").Warning("Calcolata memoria disponibile/usata/totale all'accesso di un nuovo utente. Dettaglio nel context.");
            GlobalLogContext.PushProperty("User", $"Cognome: {Cognome}, Nome: {Nome}, Id: {Id}.");
        }
    }
}
