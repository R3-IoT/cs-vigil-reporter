# cs-vigil-reporter

#### Vigil Reporter for C#. Used in pair with [Vigil](https://github.com/valeriansaliou/vigil), the Microservices Status Page.


## Who uses it?

<table>
<tr>
<td align="center"><a href="https://r3-iot.com/"><img src="https://r3-iot.com/imgs/logo.svg" height="32" /></a></td>
</tr>
<tr>
<td align="center">R3 IoT Ltd.</td>
</tr>
</table>



# How to install
Install with nuget:

```sh
$ nuget install CSVigilReporter -OutputDirectory packages
```


# How to use
An example on how to use the `VigilReporter` service can be found in the demo web-app project "CSVigilReporterDemo". 

```c#
using VigilReporter;

var builder = WebApplication.CreateBuilder(args);

string url;
string secretToken;
string probeId;
string nodeId;
string replicaId;
int interval;

if (builder.Environment.IsDevelopment())
{
    url = builder.Configuration.GetSection("VigilReporter").GetValue("Url", "localhost:8080");
    secretToken = builder.Configuration.GetSection("VigilReporter").GetValue("SecretToken", "password");
    probeId = builder.Configuration.GetSection("VigilReporter").GetValue("ProbeId", "");
    nodeId = builder.Configuration.GetSection("VigilReporter").GetValue("NodeId", "");
    replicaId = builder.Configuration.GetSection("VigilReporter").GetValue("ReplicaId", "");
    interval = builder.Configuration.GetSection("VigilReporter").GetValue("Interval", 60);
}
else
{
    url = Environment.GetEnvironmentVariable("STATUS_PAGE_URL") ?? "";
    secretToken = Environment.GetEnvironmentVariable("REPORTER_SECRET_TOKEN") ?? "";
    probeId = Environment.GetEnvironmentVariable("REPORTER_PROBE_ID") ?? "";
    nodeId = Environment.GetEnvironmentVariable("REPORTER_NODE_ID") ?? "";
    replicaId = Environment.GetEnvironmentVariable("REPORTER_REPLICA_ID") ?? "";
    interval = int.Parse(Environment.GetEnvironmentVariable("REPORTER_STATUS_INTERVAL") ?? "60");
}

builder.Services.AddHttpClient();

builder.Services.AddHostedService<VigilReporter>(x => new VigilReporter(url, secretToken, probeId, nodeId, replicaId,
    interval,  x.GetRequiredService<HttpClient>(), x.GetRequiredService<ILoggerFactory>()));
    
// Finish building & running app here!
```

# What is Vigil?
ℹ️ **Wondering what Vigil is?** Check out **[valeriansaliou/vigil](https://github.com/valeriansaliou/vigil)**.