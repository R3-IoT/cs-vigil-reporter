using Microsoft.Extensions.Logging;

namespace CSVigilReporter.Processes;

public class SystemStatsWindows: ISystemStats
{
    private readonly ILogger<SystemStatsWindows> Logger;

    public SystemStatsWindows(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<SystemStatsWindows>();
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