using Commons;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebServiceClient.Clients
{
    public class OpereWebClient
    {
        static string ApiPath { get => "/api/opere"; }
        static readonly HttpClient _httpClient = null;
        

        static OpereWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiCurrent);
        }

        public static async Task<IEnumerable<OperaDto>> GetOpere(GenericResponse gr)
        {
            IEnumerable<OperaDto> opere = null;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken ?? string.Empty);
            //_httpClient.DefaultRequestHeaders.Add("content-type", "application/json");

            try
            {

                var response = await _httpClient.GetAsync(ApiPath);

                if (response.IsSuccessStatusCode)
                {
                    opere = await response.Content.ReadFromJsonAsync<IEnumerable<OperaDto>>();
                    gr.Success = true;
                }
                else
                    gr.Message = response.ReasonPhrase;
            }
            catch (Exception ex)
            {
                gr.Message = ex.Message;
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.InnerException.Message);
            }


            return opere;
        }

        public static async Task<AddResponse> AddOpera(OperaCreateDto operaCreateDto)
        {

            var gr = new AddResponse(false);

            if (operaCreateDto == null)
                return gr;


            string operaCreateDtoStr = null;
            Commons.JsonSerializer.JsonSerialize(operaCreateDto, out operaCreateDtoStr);

            var requestContent = new StringContent(operaCreateDtoStr, Encoding.UTF8, "application/json");


            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {

                var response = await _httpClient.PostAsync(ApiPath, requestContent);

                gr.Message = response.ReasonPhrase;
                if (response.IsSuccessStatusCode)
                {
                    gr.Success = true;
                }
            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return gr;
        }
    }

}

