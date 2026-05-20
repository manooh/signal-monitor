using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Tests;

public sealed class InMemoryMonitoringRepositoryTests
{
    [Fact]
    public async Task AddServerAsync_NormalizesInputAndSetsServerOwnedValues()
    {
        var repository = CreateRepository();
        var before = DateTimeOffset.UtcNow;

        var server = await repository.AddServerAsync(new CreateServerRequest
        {
            Name = "  api-prod-01  ",
            Environment = "  PRODUCTION  ",
            Region = "  EU-CENTRAL  "
        });

        server.Id.Should().NotBeEmpty();
        server.Name.Should().Be("api-prod-01");
        server.Environment.Should().Be("production");
        server.Region.Should().Be("eu-central");
        server.Status.Should().Be(ServerStatus.Healthy);
        server.RegisteredAt.Should().BeOnOrAfter(before);
        server.RegisteredAt.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetServersAsync_FiltersCaseInsensitivelyAndOrdersByName()
    {
        var beta = Server(name: "worker-beta", environment: "production", status: ServerStatus.Healthy);
        var alpha = Server(name: "api-alpha", environment: "PRODUCTION", status: ServerStatus.Healthy);
        var warning = Server(name: "api-warning", environment: "production", status: ServerStatus.Warning);

        var repository = CreateRepository([beta, alpha, warning], [], [Alarm(warning, AlarmSeverity.Warning)]);

        var servers = await repository.GetServersAsync("PrOdUcTiOn", ServerStatus.Healthy);

        servers.Select(server => server.Id)
            .Should()
            .ContainInOrder(alpha.Id, beta.Id);
        servers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Constructor_DerivesSeededServerStatusesFromActiveAlarms()
    {
        var criticalServer = Server(status: ServerStatus.Healthy);
        var staleWarningServer = Server(name: "api-prod-02", status: ServerStatus.Warning);
        var alarm = Alarm(criticalServer, AlarmSeverity.Critical);
        var repository = CreateRepository([criticalServer, staleWarningServer], [], [alarm]);

        var critical = await repository.GetServerAsync(criticalServer.Id);
        var healthy = await repository.GetServerAsync(staleWarningServer.Id);

        critical!.Status.Should().Be(ServerStatus.Critical);
        healthy!.Status.Should().Be(ServerStatus.Healthy);
    }

    [Fact]
    public async Task AddSignalAsync_RaisesCriticalAlarmForMissingHeartbeat()
    {
        var server = Server();
        var repository = CreateRepository([server]);

        var signal = await repository.AddSignalAsync(server.Id, new IngestSignalRequest
        {
            Kind = SignalKind.Heartbeat,
            Value = 0
        });

        var alarms = await repository.GetAlarmsAsync(server.Id, AlarmStatus.Active);
        var updatedServer = await repository.GetServerAsync(server.Id);

        signal.Should().NotBeNull();
        signal!.Unit.Should().Be("ok");
        alarms.Should().ContainSingle();
        alarms.Single().Severity.Should().Be(AlarmSeverity.Critical);
        updatedServer!.Status.Should().Be(ServerStatus.Critical);
    }

    [Fact]
    public async Task AddSignalAsync_RaisesWarningAlarmForHighMemory()
    {
        var server = Server();
        var repository = CreateRepository([server]);

        await repository.AddSignalAsync(server.Id, new IngestSignalRequest
        {
            Kind = SignalKind.Memory,
            Value = 82
        });

        var alarms = await repository.GetAlarmsAsync(server.Id, AlarmStatus.Active);

        alarms.Should().ContainSingle();
        alarms.Single().Severity.Should().Be(AlarmSeverity.Warning);
    }

    [Fact]
    public async Task UpdateAlarmStatusAsync_RefreshesServerStatus()
    {
        var server = Server(status: ServerStatus.Warning);
        var alarm = Alarm(server, AlarmSeverity.Warning);
        var repository = CreateRepository([server], [], [alarm]);

        var updated = await repository.UpdateAlarmStatusAsync(alarm.Id, new UpdateAlarmStatusRequest
        {
            Status = AlarmStatus.Resolved
        });

        var updatedServer = await repository.GetServerAsync(server.Id);

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(AlarmStatus.Resolved);
        updated.UpdatedAt.Should().NotBeNull();
        updatedServer!.Status.Should().Be(ServerStatus.Healthy);
    }

    [Fact]
    public async Task ReturnedServers_AreClonesOfRepositoryState()
    {
        var existing = Server(status: ServerStatus.Healthy);
        var repository = CreateRepository([existing]);

        var server = await repository.GetServerAsync(existing.Id);
        server!.Status = ServerStatus.Critical;

        var serverAfterMutation = await repository.GetServerAsync(existing.Id);

        serverAfterMutation!.Status.Should().Be(ServerStatus.Healthy);
    }

    [Fact]
    public async Task DeleteServerAsync_RemovesRelatedSignalsAndAlarms()
    {
        var server = Server();
        var signal = Signal(server);
        var alarm = Alarm(server, AlarmSeverity.Warning);
        var repository = CreateRepository([server], [signal], [alarm]);

        var deleted = await repository.DeleteServerAsync(server.Id);
        var deletedAgain = await repository.DeleteServerAsync(server.Id);

        deleted.Should().BeTrue();
        deletedAgain.Should().BeFalse();
        (await repository.GetServerAsync(server.Id)).Should().BeNull();
        (await repository.GetSignalsAsync(server.Id)).Should().BeEmpty();
        (await repository.GetAlarmsAsync(server.Id)).Should().BeEmpty();
    }

    private static InMemoryMonitoringRepository CreateRepository(
        Server[]? servers = null,
        SignalSample[]? signals = null,
        Alarm[]? alarms = null)
    {
        var values = new Dictionary<string, string?>();

        AddServers(values, servers ?? []);
        AddSignals(values, signals ?? []);
        AddAlarms(values, alarms ?? []);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new InMemoryMonitoringRepository(configuration);
    }

    private static void AddServers(Dictionary<string, string?> values, Server[] servers)
    {
        for (var index = 0; index < servers.Length; index++)
        {
            var server = servers[index];
            var prefix = $"SeedServers:{index}";

            values[$"{prefix}:Id"] = server.Id.ToString();
            values[$"{prefix}:Name"] = server.Name;
            values[$"{prefix}:Environment"] = server.Environment;
            values[$"{prefix}:Region"] = server.Region;
            values[$"{prefix}:Status"] = server.Status.ToString();
            values[$"{prefix}:RegisteredAt"] = server.RegisteredAt.ToString("O");
        }
    }

    private static void AddSignals(Dictionary<string, string?> values, SignalSample[] signals)
    {
        for (var index = 0; index < signals.Length; index++)
        {
            var signal = signals[index];
            var prefix = $"SeedSignals:{index}";

            values[$"{prefix}:Id"] = signal.Id.ToString();
            values[$"{prefix}:ServerId"] = signal.ServerId.ToString();
            values[$"{prefix}:ServerName"] = signal.ServerName;
            values[$"{prefix}:Kind"] = signal.Kind.ToString();
            values[$"{prefix}:Value"] = signal.Value.ToString();
            values[$"{prefix}:Unit"] = signal.Unit;
            values[$"{prefix}:RecordedAt"] = signal.RecordedAt.ToString("O");
        }
    }

    private static void AddAlarms(Dictionary<string, string?> values, Alarm[] alarms)
    {
        for (var index = 0; index < alarms.Length; index++)
        {
            var alarm = alarms[index];
            var prefix = $"SeedAlarms:{index}";

            values[$"{prefix}:Id"] = alarm.Id.ToString();
            values[$"{prefix}:ServerId"] = alarm.ServerId.ToString();
            values[$"{prefix}:ServerName"] = alarm.ServerName;
            values[$"{prefix}:SignalKind"] = alarm.SignalKind.ToString();
            values[$"{prefix}:Severity"] = alarm.Severity.ToString();
            values[$"{prefix}:Status"] = alarm.Status.ToString();
            values[$"{prefix}:Message"] = alarm.Message;
            values[$"{prefix}:Value"] = alarm.Value.ToString();
            values[$"{prefix}:TriggeredAt"] = alarm.TriggeredAt.ToString("O");
            values[$"{prefix}:UpdatedAt"] = alarm.UpdatedAt?.ToString("O");
        }
    }

    private static Server Server(
        string name = "api-prod-01",
        string environment = "production",
        string region = "eu-central",
        ServerStatus status = ServerStatus.Healthy)
    {
        return new Server
        {
            Id = Guid.NewGuid(),
            Name = name,
            Environment = environment,
            Region = region,
            Status = status,
            RegisteredAt = DateTimeOffset.Parse("2026-05-07T08:00:00Z")
        };
    }

    private static SignalSample Signal(Server server)
    {
        return new SignalSample
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            ServerName = server.Name,
            Kind = SignalKind.Cpu,
            Value = 83,
            Unit = "percent",
            RecordedAt = DateTimeOffset.Parse("2026-05-07T08:10:00Z")
        };
    }

    private static Alarm Alarm(Server server, AlarmSeverity severity)
    {
        return new Alarm
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            ServerName = server.Name,
            SignalKind = SignalKind.Cpu,
            Severity = severity,
            Status = AlarmStatus.Active,
            Message = "CPU is high",
            Value = 83,
            TriggeredAt = DateTimeOffset.Parse("2026-05-07T08:10:00Z")
        };
    }
}
