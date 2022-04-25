using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using CSVigilReporter.Dto;
using CSVigilReporter.Processes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace CSVigilReporter;

public class VigilReporter: BackgroundService
{
    private readonly string Url;
    private readonly string SecretToken;
    private readonly string ProbeId;
    private readonly string NodeId;
    private readonly string ReplicaId;
    private readonly int Interval;
    private HttpClient HttpClient { get; }

    public VigilReporter(
        string url, 
        string secretToken, 
        string probeId, 
        string nodeId, 
        string replicaId, 
        int interval)
    {
        Url = url;
        SecretToken = secretToken;
        ProbeId = probeId;
        NodeId = nodeId;
        ReplicaId = replicaId;
        Interval = interval;
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(Url),

        };
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json");
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Basic: {SecretToken}");
    }
    
    /// <summary>
    /// Kick off a background thread that posts a report to vigil every Interval seconds.
    /// </summary>
    public void StartBackgroundReporting()
    {
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool SendReport()
    {
        var cpu = GetCpuLoad();
        var ram = GetRamLoad();
        var payload = new ReportPacketDto
        {
            Replica = ReplicaId,
            Interval = Interval,
            Load = new ReportLoadDto
            {
                Cpu = cpu,
                Ram = ram
            }
        };

        var posted = PostReport(payload);

        return posted;
    }

    /// <summary>
    /// Method to check the current percentage usage of CPU.
    /// </summary>
    /// <returns>A float value between 0.00 and 1.00</returns>
    private float GetCpuLoad()
    {
        float cpuLoad;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            cpuLoad = new SystemStatsWindows().CpuUsage();
        }
        else
        {
            cpuLoad = new SystemStatsUnix().CpuUsage();
        }

        return cpuLoad;
    }

    /// <summary>
    /// Method to check the percentage of total RAM that is in use.
    /// </summary>
    /// <returns>A float value between 0.00 and 1.00</returns>
    private float GetRamLoad()
    {
        float ramLoad;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            ramLoad = new SystemStatsWindows().MemoryUsage();
        }
        else
        {
            ramLoad = new SystemStatsUnix().MemoryUsage();
        }

        return ramLoad;
    }

    /// <summary>
    /// Post the payload to the HTTP API endpoint
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    private bool PostReport(ReportPacketDto packet)
    {
        var jsonPayload = PayloadToJson(packet);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{Url}reporter/{ProbeId}/{NodeId}"),
            Method = HttpMethod.Post,
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };
        
        
        var response = HttpClient.Send(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Convert payload object to the json format expected by the Vigil API.
    /// JSON should be formatted as:
    /// 
    /// {
    ///     "replica": payload.replicaId,
    ///     "interval": payload.interval,
    ///     "load": {
    ///         "cpu": payload.cpu,
    ///         "ram": payload.ram
    ///     }
    /// }
    /// </summary>
    /// <param name="packet"></param>
    /// <returns>A JSON string</returns>
    private string PayloadToJson(ReportPacketDto packet)
    {
        return JsonSerializer.Serialize(packet);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMs = 1000 * Interval;
        do
        {
            Task.Delay(intervalMs, stoppingToken);

            SendReport();

        } while (!stoppingToken.IsCancellationRequested);
    }
}