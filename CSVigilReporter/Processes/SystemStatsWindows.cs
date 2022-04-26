using Microsoft.Extensions.Logging;

namespace CSVigilReporter.Processes;

public class SystemStatsWindows: ISystemStats
{
    private readonly ILogger<SystemStatsWindows> _logger;

    public SystemStatsWindows(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SystemStatsWindows>();
    }
    public Task<float> CpuUsage()
    {
        throw new NotImplementedException();
    }

    public Task<float> MemoryUsage()
    {
        throw new NotImplementedException();
    }

    public Task InitValues()
    {
        throw new NotImplementedException();
    }
}