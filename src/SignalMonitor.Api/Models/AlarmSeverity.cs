namespace SignalMonitor.Api.Models;

/// <summary>
/// Alarm severity levels.
/// </summary>
public enum AlarmSeverity
{
    /// <summary>
    /// Warning that should be reviewed.
    /// </summary>
    Warning,

    /// <summary>
    /// Critical problem that needs attention.
    /// </summary>
    Critical
}
