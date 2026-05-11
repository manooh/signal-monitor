namespace SignalMonitor.Api.Models;

/// <summary>
/// Alarm raised when a server signal crosses a simple threshold.
/// </summary>
public sealed class Alarm
{
    /// <summary>
    /// Unique alarm identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Server associated with the alarm.
    /// </summary>
    public Guid ServerId { get; set; }

    /// <summary>
    /// Human-readable server name.
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// Signal type that triggered the alarm.
    /// </summary>
    public SignalKind SignalKind { get; set; }

    /// <summary>
    /// Severity of the alarm.
    /// </summary>
    public AlarmSeverity Severity { get; set; }

    /// <summary>
    /// Current alarm status.
    /// </summary>
    public AlarmStatus Status { get; set; }

    /// <summary>
    /// Short alarm message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Signal value that triggered the alarm.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Time when the alarm was raised.
    /// </summary>
    public DateTimeOffset TriggeredAt { get; set; }

    /// <summary>
    /// Time when the alarm was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
