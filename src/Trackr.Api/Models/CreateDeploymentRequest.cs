using System.ComponentModel.DataAnnotations;

namespace Trackr.Api.Models;

/// <summary>
/// Request body for creating a deployment record.
/// </summary>
public sealed class CreateDeploymentRequest
{
    /// <summary>
    /// Name of the deployed service.
    /// </summary>
    [Required]
    [MinLength(2)]
    public required string ServiceName { get; init; }

    /// <summary>
    /// Target environment, for example dev, staging, or production.
    /// </summary>
    [Required]
    [MinLength(2)]
    public required string Environment { get; init; }

    /// <summary>
    /// Version or release identifier.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Version { get; init; }

    /// <summary>
    /// Initial deployment status. Defaults to queued.
    /// </summary>
    public DeploymentStatus Status { get; init; } = DeploymentStatus.Queued;

    /// <summary>
    /// Person or automation account that started the deployment.
    /// </summary>
    [Required]
    [MinLength(2)]
    public required string DeployedBy { get; init; }

    /// <summary>
    /// Optional source commit SHA for the deployed version.
    /// </summary>
    public string? CommitSha { get; init; }

    /// <summary>
    /// Optional human-readable notes.
    /// </summary>
    public string? Notes { get; init; }
}
