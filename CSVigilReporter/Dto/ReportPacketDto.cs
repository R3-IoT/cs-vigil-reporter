namespace CSVigilReporter.Dto;

public class ReportPacketDto
{
    public string Replica { get; set; } = null!;
    public int Interval { get; set; }
    public ReportLoadDto Load { get; set; } = null!;
}