using SignalMonitor.Api.Models;

namespace SignalMonitor.Api.Repositories;

/// <summary>
/// Stores and retrieves deployment records.
/// </summary>
public interface IDeploymentRepository
{
    /// <summary>
    /// Gets deployment records, optionally filtered by environment, service name, or status.
    /// </summary>
    Task<IReadOnlyCollection<Deployment>> GetAllAsync(
        string? environment = null,
        string? serviceName = null,
        DeploymentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a deployment record by its ID.
    /// </summary>
    Task<Deployment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent deployment, optionally filtered by environment.
    /// </summary>
    Task<Deployment?> GetLatestAsync(string? environment = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a deployment record.
    /// </summary>
    Task<Deployment> AddAsync(CreateDeploymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status and optional notes for an existing deployment.
    /// </summary>
    Task<Deployment?> UpdateStatusAsync(
        Guid id,
        UpdateDeploymentStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a deployment record.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
