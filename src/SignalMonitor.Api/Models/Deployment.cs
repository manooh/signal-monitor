namespace SignalMonitor.Api.Models;

/// <summary>
/// A deployment tracked by the API.
/// </summary>
public sealed class Deployment
{
    /// <summary>
    /// Unique deployment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the deployed service.
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Target environment, for example dev, staging, or production.
    /// </summary>
    public required string Environment { get; set; }

    /// <summary>
    /// Version or release identifier.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Current deployment status.
    /// </summary>
    public DeploymentStatus Status { get; set; }

    /// <summary>
    /// Person or automation account that started the deployment.
    /// </summary>
    public required string DeployedBy { get; set; }

    /// <summary>
    /// Time when the deployment record was created.
    /// </summary>
    public DateTimeOffset DeployedAt { get; set; }

    /// <summary>
    /// Optional source commit SHA for the deployed version.
    /// </summary>
    public string? CommitSha { get; set; }

    /// <summary>
    /// Optional human-readable notes.
    /// </summary>
    public string? Notes { get; set; }
}
