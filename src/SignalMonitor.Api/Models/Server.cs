namespace SignalMonitor.Api.Models;

/// <summary>
/// A server monitored by the API.
/// </summary>
public sealed class Server
{
    /// <summary>
    /// Unique server identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable server name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Environment, for example dev, staging, or production.
    /// </summary>
    public required string Environment { get; set; }

    /// <summary>
    /// Region or location where the server runs.
    /// </summary>
    public required string Region { get; set; }

    /// <summary>
    /// Current derived server status.
    /// </summary>
    public ServerStatus Status { get; set; }

    /// <summary>
    /// Time when the server was registered.
    /// </summary>
    public DateTimeOffset RegisteredAt { get; set; }
}
