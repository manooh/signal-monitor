namespace Trackr.Api.Models;

/// <summary>
/// Request body for updating deployment status.
/// </summary>
public sealed class UpdateDeploymentStatusRequest
{
    /// <summary>
    /// New deployment status.
    /// </summary>
    public DeploymentStatus Status { get; init; }

    /// <summary>
    /// Optional notes. Blank values keep the existing notes.
    /// </summary>
    public string? Notes { get; init; }
}
