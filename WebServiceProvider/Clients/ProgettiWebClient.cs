using CommonResources;
using Commons;
using DevExpress.DXBinding.Native;
using Model;
using ModelData.Dto;
using ModelData.Dto.Error;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebServiceClient.Clients
{
    public class ProgettiWebClient
    {
        static string ApiPath { get => "/api/progetti"; }
        static readonly HttpClient _httpClient = null;
        private readonly string _heartbeatUrl;
        private static Timer _timer;


        static ProgettiWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiCurrent);
        }

        public static async Task<IEnumerable<ProgettoDto>> GetProgetti(Guid? operaId = null)
        {
            IEnumerable<ProgettoDto> progetti = null;


            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {
                // GET: api/Progetti?operaId=5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
                string requestString = string.Format("{0}?operaId={1}", ApiPath, operaId?.ToString());
                var response = await _httpClient.GetAsync(requestString);

                if (response.IsSuccessStatusCode)
                {
                    progetti = await response.Content.ReadFromJsonAsync<IEnumerable<ProgettoDto>>();
                }

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }

            return progetti;
        }

        public static async Task<AddResponse> AddProgetto(ProgettoContentDto progettoContentDto)
        {

            var gr = new AddResponse(false);

            if (progettoContentDto == null)
                return gr;


            string progettoCreateDtoStr = null;
            Commons.JsonSerializer.JsonSerialize(progettoContentDto, out progettoCreateDtoStr);

            var requestContent = new StringContent(progettoCreateDtoStr, Encoding.UTF8, "application/json");


            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {

                var response = await _httpClient.PostAsync(ApiPath, requestContent);

                gr.Message = response.ReasonPhrase;
                if (response.IsSuccessStatusCode)
                {
                    var progettoDto = await response.Content.ReadFromJsonAsync<ProgettoDto>();
                    gr.NewId = progettoDto.Id;
                    gr.Success = true;
                }
                else
                {
                    if (response.Content.Headers.ContentType?.MediaType == "application/json")
                    {
                        ErrorDto errorDto = await response.Content.ReadFromJsonAsync<ErrorDto>();
                        string msg = string.Empty;
                        if (errorDto.ErrorData.TryGetValue("Reason", out msg))
                        {
                            if (msg == "DuplicateKey")
                                gr.Message = LocalizationProvider.GetString("Chiave duplicata");
                        }
                    }
                    else if (response.Content.Headers.ContentType?.MediaType == "text/plain")
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), content);
                    }
                }
            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return gr;
        }

        public static async Task<ProgettoContentDto> GetProgetto(Guid progettoId, GenericResponse gr)
        {
            ProgettoContentDto progetto = new ProgettoContentDto();

            if (gr == null)
                return progetto;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {
                string requestString = string.Format("{0}/progetto-content?progettoId={1}", ApiPath, progettoId.ToString());
                var response = await _httpClient.GetAsync(requestString);

                if (response.IsSuccessStatusCode)
                {
                    progetto = await response.Content.ReadFromJsonAsync<ProgettoContentDto>();
                    gr.Success = true;
                }
                else
                {
                    gr.Success = false;
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        gr.Message = LocalizationProvider.GetString("ProgettoNonTrovato");
                    }
                }

            }
            catch (Exception ex)
            {
                gr.Success = false;
                gr.Message = ex.Message;
                //MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return progetto;
        }


        public static async Task<AddResponse> SaveProject(Project currentProject, Guid operaId, string nomeProgetto, int projectVersion)
        {
            if (string.IsNullOrEmpty(nomeProgetto))
                return new AddResponse(false);



            //if (!DeveloperVariables.IsDebug)
            //{
            //    if (ServerAddress.CurrentServer == ServerAddress.ServerName.PcCompile4)
            //    {
            //        return new AddResponse(false) { Message = "Aggiornare WebJoin in pc-compile4" };
            //    }
            //}


            string projectStr = ModelData.ModelSerializer.Serialize(currentProject);

            /////////////////////////////////////
            ////Test di deserializzazione
            //try
            //{

            //    ModelData.Model.Project testProject = ModelData.ModelSerializer.Deserialize<ModelData.Model.Project>(projectStr);

            //    Project testProject1 = ModelData.ModelSerializer.Deserialize<Project>(projectStr);
            //}
            //catch (Exception ex)
            //{

            //}
            /////////////////////////////////////

            ProgettoContentDto progettoContentDto = new ProgettoContentDto()
            {
                Nome = nomeProgetto,
                OperaId = operaId.ToString(),
                Descrizione = String.Empty,
                Content = projectStr,
                ContentVersion = projectVersion,
            };

            AddResponse res = await AddProgetto(progettoContentDto);
            return res;


        }
        public static async Task<ProjectData> LoadProject(Guid operaId, Guid progettoId, GenericResponse gr)
        {
            
            var projResponse = new ProjectData();

            ProgettoContentDto progettoDto = await GetProgetto(progettoId, gr);
            if (gr.Success)
            { 
                if (progettoDto != null && progettoDto.ContentVersion > 0 && !string.IsNullOrEmpty(progettoDto.Content))
                {
                    //ModelData.Model.Project project1 = ModelData.ModelSerializer.Deserialize<ModelData.Model.Project>(progettoDto.ProgettoContent);
                    projResponse.Project = ModelData.ModelSerializer.Deserialize<Project>(progettoDto.Content);
                    projResponse.ProjectVersion = progettoDto.ContentVersion;
                }
            }


            return projResponse;
        }

        public static void StartHeartbeat()
        {
            _timer = new Timer(async _ => await SendHeartbeatAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private static async Task SendHeartbeatAsync()
        {
            try
            {
                string heartbeatUrl = string.Format("{0}{1}", ApiPath, "/heartbeat");
                var response = await _httpClient.GetAsync(heartbeatUrl);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Heartbeat sent successfully.");
                }
                else
                {
                    Debug.WriteLine("Failed to send heartbeat.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while sending heartbeat: {ex.Message}");
            }
        }


    }





    
}
