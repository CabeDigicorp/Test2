// SOPPIANTATO DALL'USO DI SERILOG/SEQ

//using Microsoft.Build.Logging;
//using Microsoft.Extensions.Options;
//using System.IO;
//using System.Security.Principal;

//namespace JoinApi.Service
//{
//    public static class FileLoggerExtensions
//    {
//        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, IConfiguration configuration)
//        {
//            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
//            builder.Services.Configure<FileLoggerOptions>(configuration.GetSection("Logging:File"));
//            return builder;
//        }
//    }

//    public class FileLoggerProvider : ILoggerProvider
//    {
//        private readonly FileLoggerOptions? _options;

//        public FileLoggerProvider(IOptions<FileLoggerOptions> options)
//        {
//            _options = options.Value;
//        }

//        public ILogger CreateLogger(string categoryName)
//        {
//            return new FileLogger(_options.Path);
//        }

//        public void Dispose() { }
//    }

//    // File logger
//    public class FileLoggerOptions
//    {
//        public string? Path { get; set; }
//    }

//    public class FileLogger : ILogger
//    {
//        private readonly string? _logDirectory;
//        private readonly string? _logFileFormat;

//        public FileLogger(string logPathFormat)
//        {
//            string[] directories = GetDirectoriesInPath(logPathFormat);

//            foreach (string directory in directories)
//            {
//                if (!Directory.Exists(directory))
//                {
//                    if (CreateDirectoryWithAdminPrivileges(logPathFormat))
//                        throw new ArgumentException();
//                }
//            }

//            _logDirectory = Path.GetDirectoryName(logPathFormat);
//            _logFileFormat = Path.GetFileName(logPathFormat);
//        }

//        public IDisposable? BeginScope<TState>(TState state) => null;

//        public bool IsEnabled(LogLevel logLevel) => true;

//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            if (!IsEnabled(logLevel))
//                return;

//            string message = formatter(state, exception);

//            string filename = String.Format(_logFileFormat ?? "", DateTime.UtcNow);
//            string logFilePath = Path.Combine(_logDirectory ?? "", filename);

//            try
//            {
//                System.IO.File.AppendAllText(logFilePath, message + Environment.NewLine);
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Failed to log message: {ex}");
//            }
//        }
//        public bool CreateDirectoryWithAdminPrivileges(string path)
//        {
//            try
//            {
//                if (!IsAdministrator())
//                {
//                    Console.WriteLine("Attenzione, non si dispone di privilegi amministratore! Potrebbero verificarsi errori nella creazione dei log.");
//                }

//                Directory.CreateDirectory(path);

//                // Controllo privilegi di amministratore e riavvio in tale modalità:
//                //if (!IsAdministrator())
//                //{
//                //    RunAsAdmin($"cmd /c mkdir \"{path}\"");
//                //}
//                //else
//                //{
//                //    // Se ha già i privilegi di amministratore, crea la cartella direttamente
//                //    Directory.CreateDirectory(path);
//                //}

//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Errore nella creazione dei log: {ex.Message}");
//                return false;
//            }
//        }

//        private bool IsAdministrator()
//        {
//            WindowsIdentity identity = WindowsIdentity.GetCurrent();
//            WindowsPrincipal principal = new WindowsPrincipal(identity);
//            return principal.IsInRole(WindowsBuiltInRole.Administrator);
//        }

//        private void RunAsAdmin(string command)
//        {
//            System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo
//            {
//                FileName = "cmd.exe",
//                Arguments = $"/c \"{command}\"",
//                Verb = "runas",
//                UseShellExecute = true,
//                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
//            };

//            try
//            {
//                System.Diagnostics.Process.Start(processInfo).WaitForExit();
//            }
//            catch (System.ComponentModel.Win32Exception ex)
//            {
//                Console.WriteLine("Errore: Impossibile eseguire il comando con privilegi di amministratore.");
//                Console.WriteLine(ex.Message);
//            }
//        }

//        static string[] GetDirectoriesInPath(string path)
//        {
//            var directories = new System.Collections.Generic.List<string>();

//            string directoryPath = Path.GetDirectoryName(path);

//            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
//            while (directoryInfo != null)
//            {
//                directories.Add(directoryInfo.FullName);
//                directoryInfo = directoryInfo.Parent;
//            }

//            directories.Reverse();
//            return directories.ToArray();
//        }
//    }
//}
