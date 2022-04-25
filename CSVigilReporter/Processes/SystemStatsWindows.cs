namespace CSVigilReporter.Processes;

public class SystemStatsWindows: ISystemStats
{
    public async Task<float> CpuUsage()
    {
        throw new NotImplementedException();
    }

    public async Task<float> MemoryUsage()
    {
        throw new NotImplementedException();
    }
}