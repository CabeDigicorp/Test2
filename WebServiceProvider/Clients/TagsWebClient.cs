using Commons;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceClient.Clients
{
    public class TagsWebClient
    {
        static string ApiPath { get => "/api/tags"; }
        static readonly HttpClient _httpClient = null;

        static TagsWebClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ServerAddress.ApiCurrent);
        }

        public static async Task<IEnumerable<TagDto>> GetTags()
        {
            IEnumerable<TagDto> tags = null;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UtentiWebClient.AuthorizationToken);

            try
            {

                var response = await _httpClient.GetAsync(ApiPath);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Commons.JsonSerializer.JsonDeserialize(content, out tags, typeof(IEnumerable<TagDto>));
                }

            }
            catch (Exception ex)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex.Message);
            }


            return tags;
        }


    }

}

