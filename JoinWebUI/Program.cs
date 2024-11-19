using Blazored.LocalStorage;
using Blazored.SessionStorage;
using BlazorPro.BlazorSize;
using Configuration;
using JoinWebUI;
using JoinWebUI.Services;
using JoinWebUI.Shared;
using JoinWebUI.Utilities;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.IdentityModel.Logging;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using System.Reflection;

[assembly: AssemblyVersion("1.0.*")]

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("it");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("it");

//26.x.x
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXhfcnRQRWJdVkN1XkU=");
//27.x.x
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH9fdnRcRmlZUUNyV0o=");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<SfDialogService>();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMediaQueryService();
builder.Services.AddResizeListener();

//builder.Services.AddScoped<JoinApiAuthMessageHandler>();

//Join API
builder.Services.AddSingleton(sp => new JoinWebApiHttpClient(new HttpClient { BaseAddress = new Uri(builder.Configuration["JoinApiSettings:ServerBaseUrl"]) }));
builder.Services.AddScoped<JoinWebApiClient>();

//Auth0
builder.Services.AddOidcAuthentication<RemoteAuthenticationState, RemoteUserAccount>(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);
    options.ProviderOptions.DefaultScopes.Add("email");
}).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, RemoteUserAccount, ArrayClaimsPrincipalFactory<RemoteUserAccount>>();

builder.Services.AddHttpClient("Auth0",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Auth0"));

Console.WriteLine($"Sono in modalita' {builder.HostEnvironment.Environment}!");

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IAuthSyncService, AuthSyncService>();
builder.Services.AddScoped<Log>();

builder.Services.AddAutoMapper(typeof(DtoMappingProfile));

var app = builder.Build();

await app.RunAsync();

