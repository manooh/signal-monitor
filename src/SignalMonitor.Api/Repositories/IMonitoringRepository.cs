using SignalMonitor.Api.Models;

namespace SignalMonitor.Api.Repositories;

/// <summary>
/// Stores and retrieves servers, signal samples, and alarms.
/// </summary>
public interface IMonitoringRepository
{
    /// <summary>
    /// Gets servers, optionally filtered by environment or status.
    /// </summary>
    Task<IReadOnlyCollection<Server>> GetServersAsync(
        string? environment = null,
        ServerStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a server by its ID.
    /// </summary>
    Task<Server?> GetServerAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a server.
    /// </summary>
    Task<Server> AddServerAsync(CreateServerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a server and its related signal samples and alarms.
    /// </summary>
    Task<bool> DeleteServerAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets signal samples, optionally filtered by server or signal type.
    /// </summary>
    Task<IReadOnlyCollection<SignalSample>> GetSignalsAsync(
        Guid? serverId = null,
        SignalKind? kind = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a signal sample by its ID.
    /// </summary>
    Task<SignalSample?> GetSignalAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a signal sample and raises an alarm when thresholds are crossed.
    /// </summary>
    Task<SignalSample?> AddSignalAsync(
        Guid serverId,
        IngestSignalRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alarms, optionally filtered by server or status.
    /// </summary>
    Task<IReadOnlyCollection<Alarm>> GetAlarmsAsync(
        Guid? serverId = null,
        AlarmStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an alarm status.
    /// </summary>
    Task<Alarm?> UpdateAlarmStatusAsync(
        Guid id,
        UpdateAlarmStatusRequest request,
        CancellationToken cancellationToken = default);
}
