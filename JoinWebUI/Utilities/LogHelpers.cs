using JoinWebUI.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ModelData.Dto;
using Syncfusion.Blazor.FileManager.Internal;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace JoinWebUI.Utilities
{
    public class Log
    {
        private readonly JoinWebApiClient _apiClient;
        private readonly IAuthSyncService _authSync;
        private readonly IWebAssemblyHostEnvironment _env;
        private LogDto _logData;

        public string? Message { get; set; }
        public string? AssemblyName { get; set; }
        public string? AssemblyVersion { get; set; }
        public string? EnvironmentName { get; set; }
        public string? User { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestFunction { get; set; }
        public int RequestLine { get; set; }

        public Log(JoinWebApiClient apiClient, IAuthSyncService authSync, IWebAssemblyHostEnvironment env)
        {
            try
            {
                _apiClient = apiClient;
                _authSync = authSync;
                _env = env;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore inizializzazione oggetto di logging. Dettaglio eccezione: {ex.Message}.");
            }
        }

        private async void InitializeLogData()
        {
            try
            {
                LogDto logData = new LogDto();
                logData.Id = Guid.NewGuid();
                var assemblyData = Assembly.GetExecutingAssembly();
                logData.AssemblyName = assemblyData.GetName().Name;
                string version = assemblyData?.GetName()?.Version?.ToString() ?? "";
                logData.AssemblyVersion = version;
                logData.EnvironmentName = _env.Environment;
                var userData = _authSync.UtenteData;
                if(userData != null)
                {
                    logData.User = $"Cognome: {userData.Cognome ?? "already not logged"}, Nome: {userData.Nome ?? "already not logged"}, Id: {userData.Id}.";
                }
                else
                {
                    logData.User = $"Cognome: already not logged, Nome: already not logged, Id: already not logged.";
                }
                logData.RequestPath = this.RequestPath;
                logData.RequestFunction = this.RequestFunction;
                logData.RequestLine = this.RequestLine;

                _logData = logData;
            }
            catch (Exception ex)
            {
                _logData = new LogDto();
                Console.WriteLine($"Errore inizializzazione dei dati di log. Dettaglio eccezione: {ex.Message}.");
            }
        }       

        public async void Verbose(string message, [CallerFilePathAttribute] string callerName = "", [CallerMemberName] string callerFunctionName = "", [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                RequestPath = callerName;
                RequestFunction = callerFunctionName;
                RequestLine = callerLine;
                InitializeLogData();
                _logData.Message = message;
                _logData.Type = ModelData.Utilities.LogType.Information;
                Console.WriteLine(message);

                SendToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di scrittura del log, dettaglio eccezione: {ex.Message}");
            }
        }

        public async void Information(string message, [CallerFilePathAttribute] string callerName = "", [CallerMemberName] string callerFunctionName = "", [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                RequestPath = callerName;
                RequestFunction = callerFunctionName;
                RequestLine = callerLine;
                InitializeLogData();
                _logData.Message = message;
                _logData.Type = ModelData.Utilities.LogType.Information;
                Console.WriteLine(message);

                SendToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di scrittura del log, dettaglio eccezione: {ex.Message}");
            }
        }

        public async void Warning(string message, [CallerFilePathAttribute] string callerName = "", [CallerMemberName] string callerFunctionName = "", [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                RequestPath = callerName;
                RequestFunction = callerFunctionName;
                RequestLine = callerLine;
                InitializeLogData();
                _logData.Message = message;
                _logData.Type = ModelData.Utilities.LogType.Warning;
                Console.WriteLine(message);

                SendToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di scrittura del log, dettaglio eccezione: {ex.Message}");
            }
        }
        public async void Error(string message, [CallerFilePathAttribute] string callerName = "", [CallerMemberName] string callerFunctionName = "", [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                RequestPath = callerName;
                RequestFunction = callerFunctionName;
                RequestLine = callerLine;
                InitializeLogData();
                _logData.Message = message;
                _logData.Type = ModelData.Utilities.LogType.Error;
                Console.WriteLine(message);

                SendToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di scrittura del log, dettaglio eccezione: {ex.Message}");
            }
        }
        public void Fatal(string message, [CallerFilePathAttribute] string callerName = "", [CallerMemberName] string callerFunctionName = "", [CallerLineNumber] int callerLine = 0)
        {
            try
            {
                RequestPath = callerName;
                RequestFunction = callerFunctionName;
                RequestLine = callerLine;
                InitializeLogData();
                _logData.Type = ModelData.Utilities.LogType.Fatal;
                Console.WriteLine(message);

                SendToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di scrittura del log, dettaglio eccezione: {ex.Message}");
            }
        }

        private async void SendToServer()
        {
            try
            {
                var isComputoUpdated = await _apiClient.JsonPostAsync<LogDto, bool>($"log/post-log", _logData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio dei dati di log. Dettaglio eccezione: {ex.Message}.");
            }
        }
    }
}
