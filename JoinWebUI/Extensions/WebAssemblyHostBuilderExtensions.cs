using JoinWebUI.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

namespace JoinWebUI.Extensions
{
    public static class WebAssemblyHostBuilderExtensions
    {
        public async static Task<EnvironmentModel> GetEnvironment(this WebAssemblyHostBuilder builder)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            };
            var environment = await httpClient.GetFromJsonAsync<EnvironmentModel>("environment/env.config.json");
            return environment;
        }
    }
}
