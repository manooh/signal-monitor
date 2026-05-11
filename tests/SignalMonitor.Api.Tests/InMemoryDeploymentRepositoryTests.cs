using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Tests;

public sealed class InMemoryDeploymentRepositoryTests
{
    [Fact]
    public async Task AddAsync_NormalizesInputAndSetsServerOwnedValues()
    {
        var repository = CreateRepository();
        var before = DateTimeOffset.UtcNow;

        var deployment = await repository.AddAsync(new CreateDeploymentRequest
        {
            ServiceName = "  dispatch-api  ",
            Environment = "  STAGING  ",
            Version = "  1.2.3  ",
            Status = DeploymentStatus.InProgress,
            DeployedBy = "  ci-user  ",
            CommitSha = "  abc123  ",
            Notes = "   "
        });

        deployment.Id.Should().NotBeEmpty();
        deployment.ServiceName.Should().Be("dispatch-api");
        deployment.Environment.Should().Be("staging");
        deployment.Version.Should().Be("1.2.3");
        deployment.Status.Should().Be(DeploymentStatus.InProgress);
        deployment.DeployedBy.Should().Be("ci-user");
        deployment.CommitSha.Should().Be("abc123");
        deployment.Notes.Should().BeNull();
        deployment.DeployedAt.Should().BeOnOrAfter(before);
        deployment.DeployedAt.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetAllAsync_FiltersCaseInsensitivelyAndOrdersNewestFirst()
    {
        var olderMatch = Deployment(
            environment: "staging",
            serviceName: "dispatch-api",
            status: DeploymentStatus.Succeeded,
            deployedAt: DateTimeOffset.Parse("2026-05-07T08:00:00Z"));
        var newerMatch = Deployment(
            environment: "STAGING",
            serviceName: "dispatch-api",
            status: DeploymentStatus.Succeeded,
            deployedAt: DateTimeOffset.Parse("2026-05-07T09:00:00Z"));
        var wrongStatus = Deployment(
            environment: "staging",
            serviceName: "dispatch-api",
            status: DeploymentStatus.Failed,
            deployedAt: DateTimeOffset.Parse("2026-05-07T10:00:00Z"));

        var repository = CreateRepository(olderMatch, newerMatch, wrongStatus);

        var deployments = await repository.GetAllAsync(
            environment: "StAgInG",
            serviceName: "DISPATCH-API",
            status: DeploymentStatus.Succeeded);

        deployments.Select(deployment => deployment.Id)
            .Should()
            .ContainInOrder(newerMatch.Id, olderMatch.Id);
        deployments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsNewestDeploymentForEnvironment()
    {
        var demo = Deployment(
            environment: "demo",
            deployedAt: DateTimeOffset.Parse("2026-05-07T11:00:00Z"));
        var stagingOlder = Deployment(
            environment: "staging",
            deployedAt: DateTimeOffset.Parse("2026-05-07T09:00:00Z"));
        var stagingNewer = Deployment(
            environment: "staging",
            deployedAt: DateTimeOffset.Parse("2026-05-07T10:00:00Z"));

        var repository = CreateRepository(demo, stagingOlder, stagingNewer);

        var latest = await repository.GetLatestAsync("STAGING");

        latest.Should().BeEquivalentTo(stagingNewer);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatusAndPreservesNotesWhenBlank()
    {
        var existing = Deployment(
            status: DeploymentStatus.InProgress,
            notes: "Canary running");
        var repository = CreateRepository(existing);

        var updated = await repository.UpdateStatusAsync(existing.Id, new UpdateDeploymentStatusRequest
        {
            Status = DeploymentStatus.Succeeded,
            Notes = "   "
        });

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(DeploymentStatus.Succeeded);
        updated.Notes.Should().Be("Canary running");
    }

    [Fact]
    public async Task ReturnedDeployments_AreClonesOfRepositoryState()
    {
        var existing = Deployment(status: DeploymentStatus.Succeeded);
        var repository = CreateRepository(existing);

        var deployment = await repository.GetByIdAsync(existing.Id);
        deployment!.Status = DeploymentStatus.Failed;

        var deploymentAfterMutation = await repository.GetByIdAsync(existing.Id);

        deploymentAfterMutation!.Status.Should().Be(DeploymentStatus.Succeeded);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingDeployment()
    {
        var existing = Deployment();
        var repository = CreateRepository(existing);

        var deleted = await repository.DeleteAsync(existing.Id);
        var deletedAgain = await repository.DeleteAsync(existing.Id);

        deleted.Should().BeTrue();
        deletedAgain.Should().BeFalse();
        (await repository.GetByIdAsync(existing.Id)).Should().BeNull();
    }

    private static InMemoryDeploymentRepository CreateRepository(params Deployment[] deployments)
    {
        var values = new Dictionary<string, string?>();

        for (var index = 0; index < deployments.Length; index++)
        {
            var deployment = deployments[index];
            var prefix = $"SeedDeployments:{index}";

            values[$"{prefix}:Id"] = deployment.Id.ToString();
            values[$"{prefix}:ServiceName"] = deployment.ServiceName;
            values[$"{prefix}:Environment"] = deployment.Environment;
            values[$"{prefix}:Version"] = deployment.Version;
            values[$"{prefix}:Status"] = deployment.Status.ToString();
            values[$"{prefix}:DeployedBy"] = deployment.DeployedBy;
            values[$"{prefix}:DeployedAt"] = deployment.DeployedAt.ToString("O");
            values[$"{prefix}:CommitSha"] = deployment.CommitSha;
            values[$"{prefix}:Notes"] = deployment.Notes;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new InMemoryDeploymentRepository(configuration);
    }

    private static Deployment Deployment(
        string serviceName = "dispatch-api",
        string environment = "staging",
        string version = "1.0.0",
        DeploymentStatus status = DeploymentStatus.Queued,
        DateTimeOffset? deployedAt = null,
        string? notes = null)
    {
        return new Deployment
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Environment = environment,
            Version = version,
            Status = status,
            DeployedBy = "ci-user",
            DeployedAt = deployedAt ?? DateTimeOffset.Parse("2026-05-07T08:00:00Z"),
            CommitSha = "abc123",
            Notes = notes
        };
    }
}
