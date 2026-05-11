using Trackr.Api.Models;

namespace Trackr.Api.Repositories;

/// <summary>
/// Thread-safe in-memory deployment repository seeded from configuration.
/// </summary>
public sealed class InMemoryDeploymentRepository : IDeploymentRepository
{
    private readonly List<Deployment> _deployments;
    private readonly object _lock = new();

    /// <summary>
    /// Creates an in-memory repository and loads seed deployments from configuration.
    /// </summary>
    public InMemoryDeploymentRepository(IConfiguration configuration)
    {
        _deployments = configuration
            .GetSection("SeedDeployments")
            .Get<List<Deployment>>() ?? [];
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Deployment>> GetAllAsync(
        string? environment = null,
        string? serviceName = null,
        DeploymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _deployments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(deployment =>
                    string.Equals(deployment.Environment, environment, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                query = query.Where(deployment =>
                    string.Equals(deployment.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
            }

            if (status is not null)
            {
                query = query.Where(deployment => deployment.Status == status);
            }

            return Task.FromResult<IReadOnlyCollection<Deployment>>(
                query
                    .OrderByDescending(deployment => deployment.DeployedAt)
                    .Select(Clone)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<Deployment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var deployment = _deployments.FirstOrDefault(item => item.Id == id);
            return Task.FromResult(deployment is null ? null : Clone(deployment));
        }
    }

    /// <inheritdoc />
    public Task<Deployment?> GetLatestAsync(
        string? environment = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _deployments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(deployment =>
                    string.Equals(deployment.Environment, environment, StringComparison.OrdinalIgnoreCase));
            }

            var latest = query.OrderByDescending(deployment => deployment.DeployedAt).FirstOrDefault();
            return Task.FromResult(latest is null ? null : Clone(latest));
        }
    }

    /// <inheritdoc />
    public Task<Deployment> AddAsync(
        CreateDeploymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var deployment = new Deployment
        {
            Id = Guid.NewGuid(),
            ServiceName = request.ServiceName.Trim(),
            Environment = request.Environment.Trim().ToLowerInvariant(),
            Version = request.Version.Trim(),
            Status = request.Status,
            DeployedBy = request.DeployedBy.Trim(),
            DeployedAt = DateTimeOffset.UtcNow,
            CommitSha = string.IsNullOrWhiteSpace(request.CommitSha) ? null : request.CommitSha.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        lock (_lock)
        {
            _deployments.Add(deployment);
            return Task.FromResult(Clone(deployment));
        }
    }

    /// <inheritdoc />
    public Task<Deployment?> UpdateStatusAsync(
        Guid id,
        UpdateDeploymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var deployment = _deployments.FirstOrDefault(item => item.Id == id);

            if (deployment is null)
            {
                return Task.FromResult<Deployment?>(null);
            }

            deployment.Status = request.Status;
            deployment.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? deployment.Notes
                : request.Notes.Trim();

            return Task.FromResult<Deployment?>(Clone(deployment));
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var removedCount = _deployments.RemoveAll(deployment => deployment.Id == id);
            return Task.FromResult(removedCount > 0);
        }
    }

    private static Deployment Clone(Deployment deployment)
    {
        return new Deployment
        {
            Id = deployment.Id,
            ServiceName = deployment.ServiceName,
            Environment = deployment.Environment,
            Version = deployment.Version,
            Status = deployment.Status,
            DeployedBy = deployment.DeployedBy,
            DeployedAt = deployment.DeployedAt,
            CommitSha = deployment.CommitSha,
            Notes = deployment.Notes
        };
    }
}
