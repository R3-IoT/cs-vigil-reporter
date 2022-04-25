namespace CSVigilReporter.Processes;

public interface ISystemStats
{
    public Task<float> CpuUsage();
    public Task<float> MemoryUsage();
    public Task InitValues();
}
