using System.Reflection.Metadata;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CSVigilReporter.Processes;

public class SystemStatsUnix: ISystemStats
{
    // Based on methods set out in https://github.com/dotnet/orleans/blob/main/src/TelemetryConsumers/Orleans.TelemetryConsumers.Linux/LinuxEnvironmentStatistics.cs
    // unless better implementation can be found.
    private readonly ILogger<SystemStatsUnix> _logger;

    private const string MEMINFO_FILEPATH = "/proc/meminfo";
    private const string CPUSTAT_FILEPATH = "/proc/stat";

    private long PrevIdleTime { get; set; }
    private long PrevTotalTime { get; set; }
    private long? TotalPhysicalMemory { get; set; }
    private long? FreeMemory { get; set; }
    private float? CurrentCpuUsage { get; set; }

    public SystemStatsUnix(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SystemStatsUnix>();
    }

    public async Task InitValues()
    {
        await UpdateTotalMemory();
        await UpdateFreeMemory();
        await UpdateCpuUsage();
    }
    public async Task<float> CpuUsage()
    {
        await UpdateCpuUsage();
        return CurrentCpuUsage ?? 1.0f;
    }

    public async Task<float> MemoryUsage()
    {
        await UpdateTotalMemory();
        await UpdateFreeMemory();
        var memUsage = 1.0f - (float)(FreeMemory ?? 0.0f) / (TotalPhysicalMemory ?? 1.0f);
        return memUsage;
    }

    private async Task UpdateTotalMemory()
    {

        var memTotalLine = await ReadLineStartingWithAsync(MEMINFO_FILEPATH, "MemTotal");
        if (string.IsNullOrWhiteSpace(memTotalLine))
        {
            _logger.LogWarning($"Couldn't read 'MemTotal' line from '{MEMINFO_FILEPATH}'");
            return;
        }
        if (!long.TryParse(new string(memTotalLine.Where(char.IsDigit).ToArray()), out var totalMemKb))
        {
            _logger.LogWarning($"Couldn't parse meminfo output");
            return;
        }
        
        TotalPhysicalMemory = totalMemKb;
    }

    private async Task UpdateFreeMemory()
    {
        var memFreeLine = await ReadLineStartingWithAsync(MEMINFO_FILEPATH, "MemAvailable");
        if (string.IsNullOrWhiteSpace(memFreeLine))
        {
            memFreeLine = await ReadLineStartingWithAsync(MEMINFO_FILEPATH, "MemFree");
            if (string.IsNullOrWhiteSpace(memFreeLine))
            {
                _logger.LogWarning($"Couldn't read 'MemAvailable' or 'MemFree' line from '{MEMINFO_FILEPATH}'");
                return;
            }
        }
        if (!long.TryParse(new string(memFreeLine.Where(char.IsDigit).ToArray()), out var freeMemKb))
        {
            _logger.LogWarning($"Couldn't parse meminfo output");
            return;
        }

        FreeMemory = freeMemKb;
    }

    private async Task UpdateCpuUsage()
    {
        var cpuUsageLine = await ReadLineStartingWithAsync(CPUSTAT_FILEPATH, "cpu ");
        if (string.IsNullOrWhiteSpace(cpuUsageLine))
        {
            _logger.LogWarning($"Couldn't read 'MemTotal' line from '{CPUSTAT_FILEPATH}'");
            return;
        }
        var cpuNumberStrings = cpuUsageLine.Split(' ').Skip(2);

        if (cpuNumberStrings.Any(n => !long.TryParse(n, out _)))
        {
            _logger.LogWarning($"Failed to parse '{CPUSTAT_FILEPATH}' output correctly. Line: {cpuUsageLine}");
            return;
        }

        var cpuNumbers = cpuNumberStrings.Select(long.Parse).ToArray();
        var idleTime = cpuNumbers[3];
        var iowait = cpuNumbers[4]; // Iowait is not real cpu time
        var totalTime = cpuNumbers.Sum() - iowait;

        var deltaIdleTime = idleTime - PrevIdleTime;
        var deltaTotalTime = totalTime - PrevTotalTime;

        // When running in gVisor, /proc/stat returns all zeros, so check here and leave CpuUsage unset.
        // see: https://github.com/google/gvisor/blob/master/pkg/sentry/fs/proc/stat.go#L88-L95
        if (deltaTotalTime == 0f)
        {
            return;
        }

        var currentCpuUsage = (1.0f - deltaIdleTime / ((float)deltaTotalTime)) * 100f;

        var previousCpuUsage = CurrentCpuUsage ?? 0f;
        CurrentCpuUsage = (previousCpuUsage + 2 * currentCpuUsage) / 3;
        

        PrevIdleTime = idleTime;
        PrevTotalTime = totalTime;
    }

    private static async Task<string?> ReadLineStartingWithAsync(string path, string lineStartsWith)
    {
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 512, FileOptions.SequentialScan | FileOptions.Asynchronous))
        using (var r = new StreamReader(fs, Encoding.ASCII))
        {
            string? line;
            while ((line = await r.ReadLineAsync()) != null)
            {
                if (line.StartsWith(lineStartsWith, StringComparison.Ordinal))
                    return line;
            }
        }

        return null;
    }
}