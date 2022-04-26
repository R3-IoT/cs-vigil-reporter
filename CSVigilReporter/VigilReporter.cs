using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CSVigilReporter.Dto;
using CSVigilReporter.Processes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSVigilReporter;

public class VigilReporter: BackgroundService
{
    private readonly ILogger<VigilReporter> Logger;
    private readonly string ProbeId;
    private readonly string NodeId;
    private readonly string ReplicaId;
    private readonly int Interval;
    private HttpClient HttpClient { get; }
    private readonly ISystemStats SystemStats;

    public VigilReporter(
        string url, 
        string secretToken, 
        string probeId, 
        string nodeId, 
        string replicaId, 
        int interval,
        HttpClient httpClient,
        ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<VigilReporter>();
        ProbeId = probeId;
        NodeId = nodeId;
        ReplicaId = replicaId;
        Interval = interval;
        HttpClient = httpClient;
        var secret64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{secretToken}"));
        HttpClient.BaseAddress = new Uri($"http://{url}/");
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {secret64Token}");
        
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            SystemStats = new SystemStatsWindows(loggerFactory);
            Logger.LogError("System Stats collecting not yet configured for windows systems");
            throw new NotImplementedException();
        }
        else
        {
            SystemStats = new SystemStatsUnix(loggerFactory);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task PrepareAndSendReport()
    {
        var cpu = await GetCpuLoad();
        var ram = await GetRamLoad();
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

        await PostReport(payload);
    }

    /// <summary>
    /// Method to check the current percentage usage of CPU.
    /// </summary>
    /// <returns>A float value between 0.00 and 1.00</returns>
    private async Task<float> GetCpuLoad()
    {
        return await SystemStats.CpuUsage();
    }

    /// <summary>
    /// Method to check the percentage of total RAM that is in use.
    /// </summary>
    /// <returns>A float value between 0.00 and 1.00</returns>
    private async Task<float> GetRamLoad()
    {
        return await SystemStats.MemoryUsage();
    }

    /// <summary>
    /// Post the payload to the HTTP API endpoint
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    private async Task PostReport(ReportPacketDto packet)
    {
        // var jsonPayload = PayloadToJson(packet);
        //
        // var request = new HttpRequestMessage
        // {
        //     RequestUri = new Uri($"{Url}reporter/{ProbeId}/{NodeId}"),
        //     Method = HttpMethod.Post,
        //     Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        // };
        //
        //
        // var response = await HttpClient.SendAsync(request);
        var response = await HttpClient.PostAsJsonAsync($"reporter/{ProbeId}/{NodeId}", packet,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        Logger.LogInformation("Report Sent. CPU: {Cpu}, RAM: {Ram}, Server response: {StatusCode}", packet.Load.Cpu,
            packet.Load.Ram, response.StatusCode);
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
        return JsonSerializer.Serialize(packet).ToLower();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Initialising System Stats");
        var intervalMs = 1000 * Interval;
        await SystemStats.InitValues();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(intervalMs, stoppingToken);
            await PrepareAndSendReport();
        }
    }
}