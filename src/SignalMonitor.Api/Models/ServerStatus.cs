namespace SignalMonitor.Api.Models;

/// <summary>
/// Current server health status.
/// </summary>
public enum ServerStatus
{
    /// <summary>
    /// No active alarms are attached to the server.
    /// </summary>
    Healthy,

    /// <summary>
    /// Server has at least one active warning.
    /// </summary>
    Warning,

    /// <summary>
    /// Server has at least one active critical alarm.
    /// </summary>
    Critical
}
