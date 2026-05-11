using SignalMonitor.Api.Models;

namespace SignalMonitor.Api.Repositories;

/// <summary>
/// Thread-safe in-memory monitoring repository seeded from configuration.
/// </summary>
public sealed class InMemoryMonitoringRepository : IMonitoringRepository
{
    private readonly List<Alarm> _alarms;
    private readonly List<Server> _servers;
    private readonly List<SignalSample> _signals;
    private readonly object _lock = new();

    /// <summary>
    /// Creates an in-memory repository and loads seed data from configuration.
    /// </summary>
    public InMemoryMonitoringRepository(IConfiguration configuration)
    {
        _servers = configuration
            .GetSection("SeedServers")
            .Get<List<Server>>() ?? [];
        _signals = configuration
            .GetSection("SeedSignals")
            .Get<List<SignalSample>>() ?? [];
        _alarms = configuration
            .GetSection("SeedAlarms")
            .Get<List<Alarm>>() ?? [];
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Server>> GetServersAsync(
        string? environment = null,
        ServerStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _servers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(server =>
                    string.Equals(server.Environment, environment, StringComparison.OrdinalIgnoreCase));
            }

            if (status is not null)
            {
                query = query.Where(server => server.Status == status);
            }

            return Task.FromResult<IReadOnlyCollection<Server>>(
                query
                    .OrderBy(server => server.Name)
                    .Select(Clone)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<Server?> GetServerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var server = _servers.FirstOrDefault(item => item.Id == id);
            return Task.FromResult(server is null ? null : Clone(server));
        }
    }

    /// <inheritdoc />
    public Task<Server> AddServerAsync(
        CreateServerRequest request,
        CancellationToken cancellationToken = default)
    {
        var server = new Server
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Environment = request.Environment.Trim().ToLowerInvariant(),
            Region = request.Region.Trim().ToLowerInvariant(),
            Status = ServerStatus.Healthy,
            RegisteredAt = DateTimeOffset.UtcNow
        };

        lock (_lock)
        {
            _servers.Add(server);
            return Task.FromResult(Clone(server));
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteServerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var removedCount = _servers.RemoveAll(server => server.Id == id);

            if (removedCount == 0)
            {
                return Task.FromResult(false);
            }

            _signals.RemoveAll(signal => signal.ServerId == id);
            _alarms.RemoveAll(alarm => alarm.ServerId == id);

            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<SignalSample>> GetSignalsAsync(
        Guid? serverId = null,
        SignalKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _signals.AsEnumerable();

            if (serverId is not null)
            {
                query = query.Where(signal => signal.ServerId == serverId);
            }

            if (kind is not null)
            {
                query = query.Where(signal => signal.Kind == kind);
            }

            return Task.FromResult<IReadOnlyCollection<SignalSample>>(
                query
                    .OrderByDescending(signal => signal.RecordedAt)
                    .Select(Clone)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<SignalSample?> AddSignalAsync(
        Guid serverId,
        IngestSignalRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var server = _servers.FirstOrDefault(item => item.Id == serverId);

            if (server is null)
            {
                return Task.FromResult<SignalSample?>(null);
            }

            var signal = new SignalSample
            {
                Id = Guid.NewGuid(),
                ServerId = server.Id,
                ServerName = server.Name,
                Kind = request.Kind,
                Value = request.Value,
                Unit = NormalizeUnit(request),
                RecordedAt = DateTimeOffset.UtcNow
            };

            _signals.Add(signal);
            AddAlarmWhenNeeded(server, signal);
            RefreshServerStatus(server.Id);

            return Task.FromResult<SignalSample?>(Clone(signal));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Alarm>> GetAlarmsAsync(
        Guid? serverId = null,
        AlarmStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _alarms.AsEnumerable();

            if (serverId is not null)
            {
                query = query.Where(alarm => alarm.ServerId == serverId);
            }

            if (status is not null)
            {
                query = query.Where(alarm => alarm.Status == status);
            }

            return Task.FromResult<IReadOnlyCollection<Alarm>>(
                query
                    .OrderByDescending(alarm => alarm.TriggeredAt)
                    .Select(Clone)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<Alarm?> UpdateAlarmStatusAsync(
        Guid id,
        UpdateAlarmStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var alarm = _alarms.FirstOrDefault(item => item.Id == id);

            if (alarm is null)
            {
                return Task.FromResult<Alarm?>(null);
            }

            alarm.Status = request.Status;
            alarm.UpdatedAt = DateTimeOffset.UtcNow;
            RefreshServerStatus(alarm.ServerId);

            return Task.FromResult<Alarm?>(Clone(alarm));
        }
    }

    private void AddAlarmWhenNeeded(Server server, SignalSample signal)
    {
        var severity = GetSeverity(signal);

        if (severity is null)
        {
            return;
        }

        _alarms.Add(new Alarm
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            ServerName = server.Name,
            SignalKind = signal.Kind,
            Severity = severity.Value,
            Status = AlarmStatus.Active,
            Message = BuildAlarmMessage(signal),
            Value = signal.Value,
            TriggeredAt = signal.RecordedAt
        });
    }

    private static AlarmSeverity? GetSeverity(SignalSample signal)
    {
        return signal.Kind switch
        {
            SignalKind.Heartbeat when signal.Value <= 0 => AlarmSeverity.Critical,
            SignalKind.Cpu when signal.Value >= 90 => AlarmSeverity.Critical,
            SignalKind.Cpu when signal.Value >= 80 => AlarmSeverity.Warning,
            SignalKind.Memory when signal.Value >= 90 => AlarmSeverity.Critical,
            SignalKind.Memory when signal.Value >= 80 => AlarmSeverity.Warning,
            _ => null
        };
    }

    private static string BuildAlarmMessage(SignalSample signal)
    {
        return signal.Kind switch
        {
            SignalKind.Heartbeat => "Heartbeat is missing",
            SignalKind.Cpu => $"CPU is high at {signal.Value:0.#}%",
            SignalKind.Memory => $"Memory is high at {signal.Value:0.#}%",
            _ => "Signal threshold crossed"
        };
    }

    private void RefreshServerStatus(Guid serverId)
    {
        var server = _servers.First(item => item.Id == serverId);
        var activeAlarms = _alarms.Where(alarm =>
            alarm.ServerId == serverId &&
            alarm.Status == AlarmStatus.Active);

        server.Status = activeAlarms.Any(alarm => alarm.Severity == AlarmSeverity.Critical)
            ? ServerStatus.Critical
            : activeAlarms.Any()
                ? ServerStatus.Warning
                : ServerStatus.Healthy;
    }

    private static string NormalizeUnit(IngestSignalRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Unit))
        {
            return request.Unit.Trim();
        }

        return request.Kind == SignalKind.Heartbeat ? "ok" : "percent";
    }

    private static Server Clone(Server server)
    {
        return new Server
        {
            Id = server.Id,
            Name = server.Name,
            Environment = server.Environment,
            Region = server.Region,
            Status = server.Status,
            RegisteredAt = server.RegisteredAt
        };
    }

    private static SignalSample Clone(SignalSample signal)
    {
        return new SignalSample
        {
            Id = signal.Id,
            ServerId = signal.ServerId,
            ServerName = signal.ServerName,
            Kind = signal.Kind,
            Value = signal.Value,
            Unit = signal.Unit,
            RecordedAt = signal.RecordedAt
        };
    }

    private static Alarm Clone(Alarm alarm)
    {
        return new Alarm
        {
            Id = alarm.Id,
            ServerId = alarm.ServerId,
            ServerName = alarm.ServerName,
            SignalKind = alarm.SignalKind,
            Severity = alarm.Severity,
            Status = alarm.Status,
            Message = alarm.Message,
            Value = alarm.Value,
            TriggeredAt = alarm.TriggeredAt,
            UpdatedAt = alarm.UpdatedAt
        };
    }
}
