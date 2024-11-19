// Global using
global using Serilog;
global using Serilog.Context;


using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;

using JoinApi.Configuration;
using JoinApi.Models;
using JoinApi.Service;
using JoinApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Net;
using MongoDB.Bson;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;
using System.Reflection;
using JoinApi.Utilities;
using Auth0.ManagementApi.Models;
using MongoDB.Driver.Core.Clusters;


[assembly: AssemblyVersion("1.0.*")]

IdentityModelEventSource.ShowPII = true;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<JoinWebUISettings>(builder.Configuration.GetSection("JoinWebUISettings"));
builder.Services.AddTransient<ErrorHandlerMiddleware>();
//MongoDB
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddHostedService<MongoDbSetup>();
var mongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = builder.Configuration["MongoSettings:ConnectionURI"],
        DatabaseName = builder.Configuration["MongoSettings:DatabaseName"]
    },
    IdentityOptionsAction = options =>
    {
        options.SignIn.RequireConfirmedEmail = builder.Configuration.GetValue<bool>("IdentitySettings:RequireConfirmedEmail");
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 1;
    }
};
builder.Services
    .ConfigureMongoDbIdentity<UtenteDoc, RuoloDoc, Guid>(mongoDbIdentityConfiguration)
    .AddSignInManager<SignInManager<UtenteDoc>>()
    .AddDefaultTokenProviders();

//API Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
    {
        //c.Authority = $"https://{builder.Configuration["Auth0:Authority"]}";
        c.Authority = builder.Configuration["Auth0:Authority"];
        c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudience = builder.Configuration["Auth0:Audience"],
            //ValidIssuer = $"https://{builder.Configuration["Auth0:Authority"]}"
            ValidIssuer = builder.Configuration["Auth0:Authority"]

        };

    });


//Auth0
//builder.Services.AddHttpClient<HttpClient>("Auth0",
//    client => client.BaseAddress = new Uri(builder.Environment.IsEnvironment("Development-IISExpress") ? "https://localhost:44300" : "https://localhost:5100"));
builder.Services.AddHttpClient<HttpClient>("Auth0", client =>
{
    var environment = builder.Environment.EnvironmentName;
    var baseAddress = environment switch
    {
        "Development-IISExpress" => "https://localhost:44300",
        "Development" => "https://localhost:5100",
        "Staging" => "https://localhost:5100",
        "Production" => "https://localhost:5100",
        _ => throw new InvalidOperationException($"Unknown environment: {environment}")
    };
    client.BaseAddress = new Uri(baseAddress);
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Auth0"));


builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthSupportService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
    {
        // Configurazione di swagger - rimozione eventuale "Dto" finale in nome schema
        c.CustomSchemaIds(x => Regex.Replace(x.Name, "Dto$", ""));
    });

builder.Services.AddAutoMapper(typeof(DtoMappingProfile));

// POLICY CORS - DA COMPLETARE IN TERMINI RESTRITTIVI -:
const string policyName = "corsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
                      policy =>
                      {
                          switch (builder.Environment.EnvironmentName)
                          {
                              case "Development":
                                  {
                                      policy.WithOrigins(new string[] {   "https://localhost:5101", "https://*./localhost:5101",
                                    "https://localhost:5100", "https://*./localhost:5100",
                                    "https://*.auth0.com" })
                                        .AllowAnyHeader()
                                        .WithExposedHeaders("*")
                                        .AllowAnyMethod()
                                        .AllowCredentials()
                                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                                        .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                                  }
                                  break;
                              case "Development-IISExpress":
                                  {
                                      policy.WithOrigins(new string[] {   "https://localhost:44301", "https://*./localhost:44301",
                                    "https://localhost:44300", "https://*./localhost:44300",
                                    "https://*.auth0.com" })
                                        .AllowAnyHeader()
                                        .WithExposedHeaders("*")
                                        .AllowAnyMethod()
                                        .AllowCredentials()
                                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                                        .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                                  }
                                  break;
                              case "Staging":
                                  {
                                      policy.WithOrigins("joinweb.digicorp.it", "https://joinapi", "https://joinwebui",
                                                  "https://joinweb.digicorp.it", "http://joinweb.digicorp.it")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod()
                                                  .WithExposedHeaders("*")
                                                  .AllowAnyMethod()
                                                  .AllowCredentials()
                                                  .SetIsOriginAllowedToAllowWildcardSubdomains()
                                                  .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                                  }
                                  break;
                              case "Production":
                                  {
                                      policy.WithOrigins("joincloud.digicorp.it", "https://joinapi", "https://joinwebui",
                                                  "https://joincloud.digicorp.it", "http://joincloud.digicorp.it")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod()
                                                  .WithExposedHeaders("*")
                                                  .AllowAnyMethod()
                                                  .AllowCredentials()
                                                  .SetIsOriginAllowedToAllowWildcardSubdomains()
                                                  .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                                  }
                                  break;
                              default:
                                  {
                                      policy.WithOrigins(new string[] {   "https://localhost:5101", "https://*./localhost:5101",
                                    "https://localhost:5100", "https://*./localhost:5100",
                                    "https://*.auth0.com" })
                                        .AllowAnyHeader()
                                        .WithExposedHeaders("*")
                                        .AllowAnyMethod()
                                        .AllowCredentials()
                                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                                        .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
                                  }
                                  break;
                          }
                      });
});

//CorsPolicyBuilder pb = new CorsPolicyBuilder();
//if (builder.Environment.IsEnvironment("Development-IISExpress"))
//{
//    pb.WithOrigins(new string[] {   "https://localhost:44301", "https://*./localhost:44301",
//                                    "https://localhost:44300", "https://*./localhost:44300",
//                                    "https://*.auth0.com" });
//}
//else
//{
//    pb.AllowAnyOrigin(); 
//}
//pb.AllowAnyHeader();
//pb.WithExposedHeaders("*");
//pb.AllowAnyMethod();
//pb.AllowCredentials();
//pb.SetIsOriginAllowedToAllowWildcardSubdomains();
//pb.SetPreflightMaxAge(TimeSpan.FromSeconds(1728000));
//CorsPolicy policy = pb.Build();


//builder.Services.AddCors(options => options.AddPolicy(policyName, policy));

builder.Services.TryAddScoped<IWebAssemblyHostEnvironment, ServerHostEnvironment>();

CustomizeMongoDbDriver();

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 5100;
    });
}
else if (builder.Environment.IsEnvironment("Development-IISExpress"))
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 44300;
    });
}
else
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
}

var logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
Log.Logger = logger;
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddSingleton(Log.Logger);
Serilog.Debugging.SelfLog.Enable(Console.Error);
//GlobalLogContext.PushProperty("OrarioEventoCES", DateTime.Now.ToString("HH:mm:ss.fff"));
logger.Information("Inizializzazione backend JoinApi in corso...");

var app = builder.Build();
app.UseMiddleware<ErrorHandlerMiddleware>();
//app.UseSerilogRequestLogging();
TestConnection(app);
if (builder.Environment.IsStaging()) { app.Logger.LogInformation("Attiva modalita' staging!"); }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Development-IISExpress") ||
    app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
if (builder.Environment.IsStaging())
{
    builder.WebHost.UseStaticWebAssets();
}
app.UseRouting();

app.UseCors();
//app.UseCors(policyName);
//app.UseEndpoints();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();


/// Funzione per verifica connettività componenti esterni (modulo database)
void TestConnection(WebApplication app)
{
    try
    {
        // test logs:
        string hostName = Dns.GetHostName();

        Log.Information($"Hostname di invocazione: {hostName}");

        IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
        foreach (IPAddress address in hostAddresses)
        {
            app.Logger.LogInformation($"IP Address: {address}");
        }

        string? connectionString = builder.Configuration["MongoSettings:ConnectionURI"];
        bool.TryParse(builder.Configuration["MongoSettings:UseTls"], out bool useTls);
        var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
        if (useTls)
        {
            Log.Information($"Test di connessione al database con utilizzo di TLS per {connectionString}.");
            mongoClientSettings.UseTls = false;
        }
        else
        {
            Log.Warning($"Connessione al database senza l'utilizzo di TLS!");
        }

        mongoClientSettings.Credential = MongoCredential.CreateCredential("admin", builder.Configuration["MongoSettings:AdminUser"], builder.Configuration["MongoSettings:AdminPassword"]);
        var client = new MongoClient(mongoClientSettings);
        var database = client.GetDatabase(builder.Configuration["MongoSettings:DatabaseName"]);
        bool ConnectionEstablished = client.Cluster.Description.State == ClusterState.Connected && database != null;

        if (ConnectionEstablished)
        {
            Log.Information("Connessione al database riuscita!");
        }
        else
        {
            Log.Warning("Connessione al database non riuscita o in attesa di risposta.");
        }
    }
    catch (Exception ex)
    {
        Log.ForContext("Eccezione", ex).Error($"Connessione al database non riuscita! Dettaglio eccezione: {ex}.");
    }
}


void CustomizeMongoDbDriver()
{
    MongoDefaults.GuidRepresentation = MongoDB.Bson.GuidRepresentation.Standard;

    var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
    ConventionRegistry.Register("camelCase", conventionPack, t => true);

    var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || type.FullName.StartsWith("ModelData.Model"));
    BsonSerializer.RegisterSerializer(objectSerializer);
}