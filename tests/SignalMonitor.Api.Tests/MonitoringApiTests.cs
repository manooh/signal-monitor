using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Tests;

public sealed class MonitoringApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateServer_ReturnsCreatedServerWithLocation()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/servers", new
        {
            name = "api-prod-01",
            environment = "Production",
            region = "EU-Central"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var server = await response.Content.ReadFromJsonAsync<Server>(JsonOptions);
        server.Should().NotBeNull();
        server!.Id.Should().NotBeEmpty();
        server.Environment.Should().Be("production");
        server.Region.Should().Be("eu-central");
        server.Status.Should().Be(ServerStatus.Healthy);
    }

    [Fact]
    public async Task CreateServer_WithInvalidRequest_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/servers", new
        {
            name = "",
            environment = "prod",
            region = "eu"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("""{"name":null,"environment":"prod","region":"eu"}""")]
    [InlineData("""{"name":123,"environment":"prod","region":"eu"}""")]
    [InlineData("""{"name":"  ","environment":"prod","region":"eu"}""")]
    public async Task CreateServer_WithMalformedBody_ReturnsBadRequest(string json)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/servers",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ServerLifecycle_CanCreateSignalAlarmAndDeleteServer()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/servers", new
        {
            name = "worker-prod-01",
            environment = "production",
            region = "eu-central"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Server>(JsonOptions);

        var signalResponse = await client.PostAsJsonAsync($"/api/servers/{created!.Id}/signals", new
        {
            kind = "Cpu",
            value = 93
        });
        var signal = await signalResponse.Content.ReadFromJsonAsync<SignalSample>(JsonOptions);
        var getSignalResponse = await client.GetAsync(signalResponse.Headers.Location);
        var fetchedSignal = await getSignalResponse.Content.ReadFromJsonAsync<SignalSample>(JsonOptions);

        var alarmsResponse = await client.GetAsync($"/api/alarms?serverId={created.Id}&status=Active");
        var alarms = await alarmsResponse.Content.ReadFromJsonAsync<Alarm[]>(JsonOptions);

        var serverResponse = await client.GetAsync($"/api/servers/{created.Id}");
        var updatedServer = await serverResponse.Content.ReadFromJsonAsync<Server>(JsonOptions);

        var deleteResponse = await client.DeleteAsync($"/api/servers/{created.Id}");
        var getAfterDeleteResponse = await client.GetAsync($"/api/servers/{created.Id}");

        signalResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        signalResponse.Headers.Location.Should().NotBeNull();
        signal!.Kind.Should().Be(SignalKind.Cpu);
        signal.Unit.Should().Be("percent");
        getSignalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        fetchedSignal!.Id.Should().Be(signal.Id);
        alarmsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        alarms.Should().ContainSingle();
        alarms![0].Severity.Should().Be(AlarmSeverity.Critical);
        updatedServer!.Status.Should().Be(ServerStatus.Critical);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAlarmStatus_ResolvesAlarm()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/servers", new
        {
            name = "api-prod-02",
            environment = "production",
            region = "eu-central"
        });
        var server = await createResponse.Content.ReadFromJsonAsync<Server>(JsonOptions);

        await client.PostAsJsonAsync($"/api/servers/{server!.Id}/signals", new
        {
            kind = "Memory",
            value = 86
        });
        var alarms = await (await client.GetAsync($"/api/alarms?serverId={server.Id}&status=Active"))
            .Content
            .ReadFromJsonAsync<Alarm[]>(JsonOptions);

        var updateResponse = await client.PutAsJsonAsync($"/api/alarms/{alarms![0].Id}/status", new
        {
            status = "Resolved"
        });
        var updated = await updateResponse.Content.ReadFromJsonAsync<Alarm>(JsonOptions);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Status.Should().Be(AlarmStatus.Resolved);
    }

    [Fact]
    public async Task IngestSignal_ForMissingServer_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/servers/{Guid.NewGuid()}/signals", new
        {
            kind = "Heartbeat",
            value = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSignal_ForMissingSignal_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/signals/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("""{"kind":null,"value":93}""")]
    [InlineData("""{"value":93}""")]
    [InlineData("""{"kind":"Cpu"}""")]
    [InlineData("""{"kind":1,"value":93}""")]
    [InlineData("""{"kind":"Cpu","value":"ninety-three"}""")]
    [InlineData("""{"kind":"Cpu","value":101}""")]
    [InlineData("""{"kind":"Cpu","value":93,"unit":"  "}""")]
    [InlineData("""{"kind":"Heartbeat","value":2}""")]
    public async Task IngestSignal_WithMalformedBody_ReturnsBadRequest(string json)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var server = await CreateServerAsync(client);

        var response = await client.PostAsync(
            $"/api/servers/{server.Id}/signals",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("""{"status":null}""")]
    [InlineData("""{}""")]
    [InlineData("""{"status":1}""")]
    [InlineData("""{"status":"not-real"}""")]
    public async Task UpdateAlarmStatus_WithMalformedBody_ReturnsBadRequest(string json)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var server = await CreateServerAsync(client);

        await client.PostAsJsonAsync($"/api/servers/{server.Id}/signals", new
        {
            kind = "Memory",
            value = 86
        });
        var alarms = await (await client.GetAsync($"/api/alarms?serverId={server.Id}&status=Active"))
            .Content
            .ReadFromJsonAsync<Alarm[]>(JsonOptions);

        var response = await client.PutAsync(
            $"/api/alarms/{alarms![0].Id}/status",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<Server> CreateServerAsync(HttpClient client)
    {
        var createResponse = await client.PostAsJsonAsync("/api/servers", new
        {
            name = "api-prod-01",
            environment = "production",
            region = "eu-central"
        });

        return (await createResponse.Content.ReadFromJsonAsync<Server>(JsonOptions))!;
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var repositoryDescriptor = services.Single(
                        service => service.ServiceType == typeof(IMonitoringRepository));

                    services.Remove(repositoryDescriptor);
                    services.AddSingleton<IMonitoringRepository>(
                        new InMemoryMonitoringRepository(
                            new ConfigurationBuilder().Build()));
                });
            });
    }
}
