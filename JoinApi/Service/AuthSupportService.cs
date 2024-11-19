using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi.Models;
using JoinApi.Models;
using JoinApi.Controllers;
using MongoDB.Driver;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using ModelData.Utilities;
using MongoDB.Driver.Linq;
using System.Data;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Org.BouncyCastle.Asn1.Crmf;
using System.Collections.ObjectModel;

namespace JoinApi.Service
{
    public interface IAuthSupportService
    {
        public Task<string> GetCurrentUserEmail();
    }

    public class AuthSupportService
    {

        private const int VALIDITY_SECONDS = 60;

        //protected readonly MongoDbService _mongoDbService;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationApiClient _auth0Client;
        private readonly MongoDbService _mongoDbService;

        private static UserFullInfoList _activeUsers = new UserFullInfoList();

        private class UserFullInfoList : KeyedCollection<string, UserFullInfo>
        {
            protected override string GetKeyForItem(UserFullInfo item)
            {
                return item.AuthToken;
            }
        }

        private static Mutex _syncMutex = new Mutex();
        private static System.Timers.Timer _timer = new System.Timers.Timer(60000);
                

        public AuthSupportService(HttpClient httpClient, MongoDbService mongoDbService)
        {
            _httpClient = httpClient;
            HttpClientAuthenticationConnection auth0Conn = new HttpClientAuthenticationConnection(_httpClient);
            _auth0Client = new AuthenticationApiClient("digicorp-joinweb.eu.auth0.com", auth0Conn);
            if (!_timer.Enabled)
                SetTimer();

            _mongoDbService = mongoDbService;

        }
        private static void SetTimer()
        {
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnTimerExpired;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private static void OnTimerExpired(Object? source, System.Timers.ElapsedEventArgs e)
        {
            PurgeInactiveUsers();
        }

        private static void PurgeInactiveUsers()
        {
            Log.
                Verbose("Accesso in AuthSupportService, rilascio utenti non attivi in corso...");

            _syncMutex.WaitOne();
            DateTime now = DateTime.Now;
            for (int i = _activeUsers.Count - 1; i >= 0; i--)
            {
                if ((now - _activeUsers.ElementAt(i).TimeStamp).TotalSeconds > VALIDITY_SECONDS)
                {
                    _activeUsers.RemoveAt(i);
                }
            }
            _syncMutex.ReleaseMutex();

            Log.
                Verbose("Accesso in AuthSupportService, rilascio utenti non attivi completato correttamente.");
        }

        public UserFullInfo? GetCurrentUserInfo(HttpContext httpContext)
        {
            UserFullInfo? activeUser = null;
            if (httpContext.User != null)
            {
                string? authorization = httpContext?.Request.Headers.Authorization.First();
                string[]? authParsed = authorization?.Split(' ');
                if (authParsed?.Length == 2 && authParsed[0] == "Bearer")
                {
                    activeUser = GetUserInfo(authParsed[1]);

                }

            }

            return activeUser;

        }



        private UserFullInfo? GetUserInfo(string authToken)
        {
            Log.
                Information("Accesso in AuthSupportService, richiesta informazioni utente in corso...");

            UserFullInfo? activeUser = null;

            _syncMutex.WaitOne();

            //if (_activeUsers.TryGetValue(authToken, out activeUser) && (DateTime.Now - activeUser.TimeStamp).TotalSeconds > VALIDITY_SECONDS)
            //{
            //    activeUser = null;
            //    _activeUsers.Remove(authToken);
            //}

            var attempt = _activeUsers.TryGetValue(authToken, out activeUser);

            if (!attempt || activeUser!.ForceUpdate)
            {
                var res = _auth0Client.GetUserInfoAsync(authToken);
                res.Wait();
                UserInfo user = res.Result;

                //necessario in caso di forzatura
                _activeUsers.Remove(authToken);

                bool isAuthenticated = user?.EmailVerified ?? false;
                if (isAuthenticated)
                {
                    activeUser = new UserFullInfo(authToken,
                                                   DateTime.Now,
                                                   user!,
                                                   GetUserJoinInfo(user!.Email));                                                   
                    _activeUsers.Add(activeUser);
                }
            }

            activeUser!.ForceUpdate = false;

            _syncMutex.ReleaseMutex();

            Log.
                Information("Accesso in AuthSupportService, richiesta informazioni utente completata correttamente.");

            return activeUser;
        }

        private JoinUserInfo? GetUserJoinInfo(string email)
        {
            JoinUserInfo? result = null;
            
            if (!string.IsNullOrWhiteSpace(email))
            {
                UtenteDoc? utente = _mongoDbService.UtentiCollection.Find(u => u.Email == email).FirstOrDefault();
                if (utente != null)
                {
                    Guid Id = utente.Id;
                    
                    //var roles = new List<string>();
                    //foreach (Guid rid in utente.Roles)
                    //{
                    //    var ruolo = _mongoDbService.RuoliCollection.Find(r => r.Id == rid).FirstOrDefault();
                    //    if (ruolo != null)
                    //        roles.Add(ruolo.Name);
                    //}
                    
                    //var clients = new List<ClienteDoc>();
                    //foreach (Guid cid in utente.ClientiIds)
                    //{
                    //    var cliente = _mongoDbService.ClientiCollection.Find(r => r.Id == cid).FirstOrDefault();
                    //    if (cliente != null)
                    //        clients.Add(cliente);
                    //}
                                        
                    var teams = _mongoDbService.TeamsCollection.Find(t => utente.TeamsIds.Contains(t.Id)).ToList();

                    var gruppi = (from g in _mongoDbService.GruppiUtentiCollection.AsQueryable(null)
                                  from t in teams
                                  where t.GruppiIds.Contains(g.Id) || utente.GruppiIds.Contains(g.Id)
                                  select g).Distinct().ToList();

                    var clienti = (from c in _mongoDbService.ClientiCollection.AsQueryable() 
                                  from t in teams
                                  where c.Id == t.ClienteId
                                  select c).Distinct().ToList();

                    string[] parsedMail = (email ?? string.Empty).ToLowerInvariant().Split('@');
                    string domain = parsedMail.Length > 0 ? parsedMail[1] : string.Empty;
                    string? domainManager = null;
                    if (!string.IsNullOrEmpty(domain))
                    {
                        var dm = (from c in _mongoDbService.ClientiCollection.AsQueryable()
                                  where c.DominiAssociati.Contains(domain)
                                  select c.Nome).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(dm)) domainManager = dm;
                    }

                    result = new JoinUserInfo(utente.Id, utente.Cognome, utente.Nome, utente.PrivacyConsent, utente.Disabled, domainManager, teams, gruppi, clienti);

                    var permessi = _mongoDbService.PermessiCollection.AsQueryable(null).ToList();
                    
                    result.Permessi = (from p in permessi
                                       join tg in result.TeamsUnionGruppi on p.SoggettoId equals tg.Id
                                       select p).Distinct();

                }
            }

            return result;

        }

        internal void Logout(HttpContext httpContext)
        {
            Log.
                Warning("Accesso in AuthSupportService, richiesta logout in corso...");

            if (httpContext.User != null)
            {
                string? authorization = httpContext.Request.Headers.Authorization.First();
                string[]? authParsed = authorization?.Split(' ');

                if (authParsed?.Length == 2 && authParsed[0] == "Bearer")
                {
                    _syncMutex.WaitOne();

                    _activeUsers.Remove(authParsed[1]);

                    _syncMutex.ReleaseMutex();
                }
            }

            Log.
                Warning("Accesso in AuthSupportService, richiesta logout completata correttamente.");
        }



    }

}
