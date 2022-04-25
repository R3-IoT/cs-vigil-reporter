namespace CSVigilReporter.Processes;

public interface ISystemStats
{
    public float CpuUsage();
    public float MemoryUsage();
}