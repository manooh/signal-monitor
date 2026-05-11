namespace SignalMonitor.Api.Models;

/// <summary>
/// Supported server signal types.
/// </summary>
public enum SignalKind
{
    /// <summary>
    /// Basic liveness signal. Use value 1 for healthy and 0 for missing.
    /// </summary>
    Heartbeat,

    /// <summary>
    /// CPU utilization percentage.
    /// </summary>
    Cpu,

    /// <summary>
    /// Memory utilization percentage.
    /// </summary>
    Memory
}
