namespace SignalMonitor.Api.Models;

/// <summary>
/// Request body for updating alarm status.
/// </summary>
public sealed class UpdateAlarmStatusRequest
{
    /// <summary>
    /// New alarm status.
    /// </summary>
    public AlarmStatus Status { get; init; }
}
