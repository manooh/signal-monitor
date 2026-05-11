namespace SignalMonitor.Api.Models;

/// <summary>
/// A single signal sample received from a server.
/// </summary>
public sealed class SignalSample
{
    /// <summary>
    /// Unique signal sample identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Server that sent the signal.
    /// </summary>
    public Guid ServerId { get; set; }

    /// <summary>
    /// Human-readable server name.
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// Type of signal recorded.
    /// </summary>
    public SignalKind Kind { get; set; }

    /// <summary>
    /// Numeric signal value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Unit for the value.
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Time when the signal was recorded by the API.
    /// </summary>
    public DateTimeOffset RecordedAt { get; set; }
}
