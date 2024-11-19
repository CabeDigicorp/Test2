using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JoinApi.Utilities
{
    public static class NumericHelpers
    {
        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class UtilityHelpers
    {
        public static string GetOraCorrenteCES()
        {
            try
            {
                TimeZoneInfo europeCentralTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                DateTime oraCorrente = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, europeCentralTimeZone);
                return oraCorrente.ToString("HH:mm:ss.fff");
            }
            catch (Exception)
            {
                Log.Error("Connessione al database non riuscita!");
            }
            return string.Empty;
        }
    }

    public class MemoryMetricsHelpers
    {
        public double Total;
        public double Used;
        public double Free;
    }

    public class MemoryMetricsClientHelpers
    {
        public static MemoryMetricsHelpers GetMetrics()
        {
            MemoryMetricsHelpers metrics = new();

            try
            {
                if (IsUnix())
                {
                    metrics = GetUnixMetrics();
                }
                else
                {
                    metrics = GetWindowsMetrics();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Errore nel reperimento delle metriche di sistema. Dettaglio: {ex.Message}.");
            }
            
            return metrics;
        }

        private static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private static MemoryMetricsHelpers GetWindowsMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetricsHelpers();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private static MemoryMetricsHelpers GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetricsHelpers();
            metrics.Total = double.Parse(memory[1]);
            metrics.Used = double.Parse(memory[2]);
            metrics.Free = double.Parse(memory[3]);

            return metrics;
        }
    }
}
