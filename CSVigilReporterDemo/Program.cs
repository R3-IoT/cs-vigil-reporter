using CSVigilReporter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Vigil Reporter Service Configuration

string url;
string secretToken;
string probeId;
string nodeId;
string replicaId;
int interval;

if (builder.Environment.IsDevelopment())
{
    url = builder.Configuration.GetSection("VigilReporter").GetValue("Url", "http://localhost:8080");
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

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();