using System.ComponentModel.DataAnnotations;

namespace SignalMonitor.Api.Models;

/// <summary>
/// Request body for recording a server signal sample.
/// </summary>
public sealed class IngestSignalRequest
{
    /// <summary>
    /// Type of signal being recorded.
    /// </summary>
    public SignalKind Kind { get; init; }

    /// <summary>
    /// Numeric signal value. Heartbeat uses 1 for healthy and 0 for missing.
    /// </summary>
    [Range(0, 100)]
    public double Value { get; init; }

    /// <summary>
    /// Optional unit for the value, for example percent or ok.
    /// </summary>
    public string? Unit { get; init; }
}
