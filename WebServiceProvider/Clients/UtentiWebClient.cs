
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Builders;
using Auth0.AuthenticationApi.Models;
using Commons;
using DevExpress.Mvvm.Native;
using DevExpress.Pdf.Native.BouncyCastle.Ocsp;
using Microsoft.Isam.Esent.Interop;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using Auth0.OidcClient;
using IdentityModel.OidcClient.Browser;
using CommonResources;
using System.Windows.Navigation;

namespace WebServiceClient.Clients
{
    public class UtentiWebClient
    {
        static string ApiPath { get => "/api/utenti"; }
        static readonly HttpClient _httpClient = null;

        static string _authorizationToken = string.Empty;
        static string _refreshToken = string.Empty;
        static UtenteDto _currentUser = null;
        public static string AuthorizationToken { get => _authorizationToken; }

        static readonly AuthenticationApiClient _authApiClient = null;//obsolete

        static Auth0Client _auth0Client = null;
        readonly static string[] _connectionNames = new string[]
        {
            "Username-Password-Authentication",
            "google-oauth2",
            "twitter",
            "facebook",
            "github",
            "windowslive"
        };


        static UtentiWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiCurrent);


            HttpClientAuthenticationConnection auth0Conn = new HttpClientAuthenticationConnection(_httpClient);
            _authApiClient = new AuthenticationApiClient(new Uri("https://digicorp-joinweb.eu.auth0.com"), auth0Conn);
        }

 
        public static async Task<GenericResponse> LoginByPassword(LoginDto loginDto)
        {


            GenericResponse gr = new GenericResponse(false);

            try
            {

                /////////////////////////////////////
                ///NB. Per far fuonzionare la chiamata ho apportato le seguenti modifiche da https://manage.auth0.com
                ///
                //API Authorization Settings->Default
                //Username - Password - Authentication

                //JoinApp->Settings->Advanced Settings->Grant types

                //JoinApp->Credentials->Authentication Methods->Client Secret(Basic)

                ////////////////////////////////////
                string clientId = "LbXFOeK1Z67yVaLuuZT73n59sLWUMKvD";
                string clientSecret = "S_W_MALf-3Vb-4sRKMSAbqG4igZ9eEE1z5vB2xD9hARgtw5MoUxtuxPuPh-eLc7J";
                string scope = "openid";
                string audience = "https://api.joinweb.digicorp.it";



                var authenticationResponse = await _authApiClient.GetTokenAsync(new ResourceOwnerTokenRequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Scope = scope,
                    Audience = audience,
                    Username = loginDto.Email,
                    Password = loginDto.Password,
                });


                if (authenticationResponse != null)
                {
                    string token = authenticationResponse.AccessToken;
                    if (!string.IsNullOrEmpty(token))
                    {
                        string tokenType = authenticationResponse.TokenType;
                        string IdToken = authenticationResponse.IdToken;
                        _authorizationToken = token;

                        var user = await _authApiClient.GetUserInfoAsync(token);


                        gr.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return gr;
        }


        public static async Task<GenericResponse> Login()
        {
            var gr = new GenericResponse(false);

            try
            {

                string domain = "digicorp-joinweb.eu.auth0.com";
                string clientId = "LbXFOeK1Z67yVaLuuZT73n59sLWUMKvD";
                string audience = "https://api.joinweb.digicorp.it";

                _auth0Client = new Auth0Client(new Auth0ClientOptions
                {
                    Domain = domain,
                    ClientId = clientId
                });

                var extraParameters = new Dictionary<string, string>();
                extraParameters.Add("connection", _connectionNames[0]);
                extraParameters.Add("audience", audience);

                var loginResult = await _auth0Client.LoginAsync(extraParameters: extraParameters);

                

                // Display error
                if (!loginResult.IsError)//ok
                {
                    gr.Success = true;

                    var user = loginResult.User;
                    var name = user.FindFirst(c => c.Type == "name")?.Value;
                    var email = user.FindFirst(c => c.Type == "email")?.Value;
                    var picture = user.FindFirst(c => c.Type == "picture")?.Value;

                    _authorizationToken = loginResult.AccessToken;
                    //_refreshToken = loginResult.RefreshToken;

                    _currentUser = await GetUtenteByEmail(email, gr);
                }
                else//error
                {
                    gr.Message = loginResult.Error;
                }

            }
            catch(Exception ex)
            {
                gr.Message = ex.Message;
            }

            return gr;
        }


        public static async Task<GenericResponse> Logout()
        {
            GenericResponse gr = new GenericResponse(false);
            BrowserResultType browserResult = await _auth0Client.LogoutAsync();

            if (browserResult == BrowserResultType.Success)
            {
                _authorizationToken = string.Empty;
                _refreshToken = string.Empty;
                _currentUser = null;

                gr.Success = true;
            }
            else
            {
                var resultText = browserResult.ToString();
                gr.Message = resultText;
            }
            return gr;
        }

        public static async Task<GenericResponse> RefreshToken()
        {
            GenericResponse gr = new GenericResponse(true);

            if (string.IsNullOrEmpty(_authorizationToken) && string.IsNullOrEmpty(_refreshToken))
                gr = await Login();

            //if (!string.IsNullOrEmpty(_refreshToken))
            //{
            //    var aaa = await client.RefreshTokenAsync(_refreshToken);
            //}

            return gr;
        }

        static async Task<UtenteDto> GetUtenteByEmail(string email, GenericResponse gr)
        {

            //[Route("get-utente-by-email/{email}")]


            UtenteDto utente = null;

            if (gr == null)
                return utente;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {
                //string requestString = string.Format("{0}/progetto-content?progettoId={1}", ApiPath, progettoId.ToString());
                string requestString = string.Format("{0}/get-utente-by-email/{1}", ApiPath, email);
                var response = await _httpClient.GetAsync(requestString);

                if (response.IsSuccessStatusCode)
                {
                    utente = await response.Content.ReadFromJsonAsync<UtenteDto>();
                    gr.Success = true;
                }
                else
                {
                    gr.Success = false;
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        gr.Message = LocalizationProvider.GetString("UtenteNonTrovato");
                    }
                }

            }
            catch (Exception ex)
            {
                gr.Success = false;
                gr.Message = ex.Message;
            }

            return utente;
        }

        public static string GetCurrentUserName()
        {
            return _currentUser?.Nome;
        }

        public static string GetCurrentUserSurname()
        {
            return _currentUser?.Cognome;
        }

        public static string GetCurrentUserInitials()
        {
            string initials = string.Empty;
            if (_currentUser != null)
            {
                if (_currentUser?.Nome != null)
                    initials += _currentUser?.Nome[0];

                if (_currentUser?.Cognome != null)
                    initials += _currentUser?.Cognome[0];
            }
            return initials;
        }

        public static string GetCurrentUserEmail()
        {
            return _currentUser?.Email;
        }
        public static string GetCurrentUserProfileWebUILink()
        {
            string userProfileLink = string.Empty;
            if (_currentUser != null)
            {
                userProfileLink = string.Format("{0}/utenti/{1}", ServerAddress.WebUICurrent, _currentUser?.Id);
                //userProfileLink = "https://192.168.0.95:5101/opere/ef2880ae-039f-4479-bbe6-fb42954073f8";
            }
            
            return userProfileLink;
        }


        #region Old

        public static async Task<GenericResponse> LoginByApi()
        {


            GenericResponse gr = new GenericResponse(false);

            try
            {

                /////////////////////////////////////
                ///NB. Per far fuonzionare la chiamata ho apportato le seguenti modifiche da https://manage.auth0.com
                ///
                //API Authorization Settings->Default
                //Username - Password - Authentication

                //JoinApp->Settings->Advanced Settings->Grant types

                //JoinApp->Credentials->Authentication Methods->Client Secret(Basic)

                ////////////////////////////////////
                string clientId = "LbXFOeK1Z67yVaLuuZT73n59sLWUMKvD";
                string clientSecret = "S_W_MALf-3Vb-4sRKMSAbqG4igZ9eEE1z5vB2xD9hARgtw5MoUxtuxPuPh-eLc7J";
                string scope = "openid profile email";// "openid";
                string audience = "https://api.joinweb.digicorp.it";



                // Generates state and PKCE values.
                string state = randomDataBase64url(32);
                string code_verifier = randomDataBase64url(32);
                string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));

                // Creates a redirect URI using an available port on the loopback address.
                //string redirectURI = string.Format("https://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
                string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetOpenPort());
                //string redirectURI = "https://localhost:5100/authentication/login-callback/";


                // Creates an HttpListener to listen for requests on that redirect URI.
                var http = new HttpListener();
                http.Prefixes.Add(redirectURI);
                http.Start();

                AuthorizationUrlBuilder authorizationUrlBuilder = new AuthorizationUrlBuilder(_authApiClient.BaseUri);
                authorizationUrlBuilder.WithClient(clientId);
                authorizationUrlBuilder.WithAudience(audience);
                authorizationUrlBuilder.WithScope(scope);
                authorizationUrlBuilder.WithResponseType(AuthorizationResponseType.Code);
                authorizationUrlBuilder.WithState(state);
                authorizationUrlBuilder.WithRedirectUrl(redirectURI);
                authorizationUrlBuilder.WithValue("code_challenge", code_challenge);
                authorizationUrlBuilder.WithValue("code_challenge_method", "S256");
                //authorizationUrlBuilder.WithValue("prompt", "none");


                Uri authorizationRequest = authorizationUrlBuilder.Build();

                var process = new Process();


                // Opens request in the browser.
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = authorizationRequest.OriginalString;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                // Waits for the OAuth authorization response.
                var context = await http.GetContextAsync();


                // Sends an HTTP response to the browser.
                var response = context.Response;
                //string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://www.digicorp.it'></head><body>Please return to the app.</body></html>");
                string responseString = string.Empty;
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
                {
                    responseOutput.Close();
                    http.Stop();
                    Console.WriteLine("HTTP server stopped.");
                });

                // Checks for errors.
                if (context.Request.QueryString.Get("error") != null)
                {
                    //output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                    return gr;
                }
                if (context.Request.QueryString.Get("code") == null
                    || context.Request.QueryString.Get("state") == null)
                {
                    //output("Malformed authorization response. " + context.Request.QueryString);
                    return gr;
                }

                // extracts the code
                var authorization_code = context.Request.QueryString.Get("code");
                var incoming_state = context.Request.QueryString.Get("state");

                // Compares the receieved state to the expected value, to ensure that
                // this app made the request which resulted in authorization.
                if (incoming_state != state)
                {
                    //output(String.Format("Received request with invalid state ({0})", incoming_state));
                    return gr;
                }


                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Exchange your authorization_code and code_verifier for tokens.

                var authenticationResponse = await _authApiClient.GetTokenAsync(new AuthorizationCodePkceTokenRequest
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    CodeVerifier = code_verifier,
                    RedirectUri = redirectURI,
                    Code = authorization_code,
                    SigningAlgorithm = JwtSignatureAlgorithm.RS256,
                });

                if (authenticationResponse != null)
                {
                    string token = authenticationResponse.AccessToken;
                    if (!string.IsNullOrEmpty(token))
                    {
                        string tokenType = authenticationResponse.TokenType;
                        string IdToken = authenticationResponse.IdToken;
                        _authorizationToken = token;

                        var user = await _authApiClient.GetUserInfoAsync(token);



                        gr.Success = true;
                    }
                    else
                    {

                    }
                }
                else
                {
                }

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return gr;
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        public static string GetOpenPort()
        {
            int PortStartIndex = 1230;
            int PortEndIndex = 1235;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = PortStartIndex; port < PortEndIndex; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    unusedPort = port;
                    break;
                }
            }
            return unusedPort.ToString();
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes);
        }

        #endregion
    }



}
