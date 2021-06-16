using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Anno.EngineData.SysInfo
{
    public class MemoryMetricsClient
    {
        public MemoryMetrics GetMetrics()
        {
            if (IsUnix())
            {
                return GetUnixMetrics();
            }

            return GetWindowsMetrics();
        }

        private bool IsUnix()
        {
#if NETSTANDARD
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
            var isUnix = Environment.OSVersion.Platform.ToString() == "Unix" ||
                Environment.OSVersion.Platform.ToString() == "MacOSX";
#endif
            return isUnix;
        }

        private MemoryMetrics GetWindowsMetrics()
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

            var lines = output.Trim().Split("\n".ToCharArray());
            var freeMemoryParts = lines[0].Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private MemoryMetrics GetUnixMetrics()
        {
            var output = "";

            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Split("\n".ToCharArray());
            var memory = lines[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics();
            metrics.Total = double.Parse(memory[1]);
            //metrics.Used = double.Parse(memory[2]);
            metrics.Free = double.Parse(memory[3]);
            metrics.Used = metrics.Total - metrics.Free;//包括 buff/Caches

            return metrics;
        }
    }
}
