namespace SignalMonitor.Api.Models;

/// <summary>
/// Lifecycle status for alarms.
/// </summary>
public enum AlarmStatus
{
    /// <summary>
    /// Alarm is still active.
    /// </summary>
    Active,

    /// <summary>
    /// Alarm has been acknowledged.
    /// </summary>
    Acknowledged,

    /// <summary>
    /// Alarm has been resolved.
    /// </summary>
    Resolved
}
