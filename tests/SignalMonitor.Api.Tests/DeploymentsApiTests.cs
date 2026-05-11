using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Tests;

public sealed class DeploymentsApiTests
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
    public async Task CreateDeployment_ReturnsCreatedDeploymentWithLocation()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/deployments", new
        {
            serviceName = "dispatch-api",
            environment = "Staging",
            version = "1.2.3",
            status = "InProgress",
            deployedBy = "ci-user",
            commitSha = "abc123",
            notes = "Canary rollout"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var deployment = await response.Content.ReadFromJsonAsync<Deployment>(JsonOptions);
        deployment.Should().NotBeNull();
        deployment!.Id.Should().NotBeEmpty();
        deployment.Environment.Should().Be("staging");
        deployment.Status.Should().Be(DeploymentStatus.InProgress);
    }

    [Fact]
    public async Task CreateDeployment_WithInvalidRequest_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/deployments", new
        {
            serviceName = "",
            environment = "staging",
            version = "1.2.3",
            deployedBy = "ci-user"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeploymentLifecycle_CanCreateReadUpdateAndDeleteDeployment()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/deployments", new
        {
            serviceName = "dispatch-api",
            environment = "production",
            version = "2.0.0",
            status = "Queued",
            deployedBy = "release-bot"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Deployment>(JsonOptions);

        var getResponse = await client.GetAsync($"/api/deployments/{created!.Id}");
        var updateResponse = await client.PutAsJsonAsync($"/api/deployments/{created.Id}/status", new
        {
            status = "Succeeded",
            notes = "Released cleanly"
        });
        var updated = await updateResponse.Content.ReadFromJsonAsync<Deployment>(JsonOptions);
        var deleteResponse = await client.DeleteAsync($"/api/deployments/{created.Id}");
        var getAfterDeleteResponse = await client.GetAsync($"/api/deployments/{created.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Status.Should().Be(DeploymentStatus.Succeeded);
        updated.Notes.Should().Be("Released cleanly");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLatestDeployment_WhenEnvironmentHasNoDeployments_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/deployments/latest?environment=does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var repositoryDescriptor = services.Single(
                        service => service.ServiceType == typeof(IDeploymentRepository));

                    services.Remove(repositoryDescriptor);
                    services.AddSingleton<IDeploymentRepository>(
                        new InMemoryDeploymentRepository(
                            new ConfigurationBuilder().Build()));
                });
            });
    }
}
