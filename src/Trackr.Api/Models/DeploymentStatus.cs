namespace Trackr.Api.Models;

/// <summary>
/// Status values used to track deployment progress.
/// </summary>
public enum DeploymentStatus
{
    /// <summary>
    /// Deployment is waiting to start.
    /// </summary>
    Queued,

    /// <summary>
    /// Deployment is currently running.
    /// </summary>
    InProgress,

    /// <summary>
    /// Deployment completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Deployment failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Deployment was rolled back.
    /// </summary>
    RolledBack
}
