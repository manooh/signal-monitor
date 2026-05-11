using Trackr.Api.Models;
using Trackr.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Trackr.Api.Controllers;

/// <summary>
/// Manages deployment records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class DeploymentsController : ControllerBase
{
    private readonly IDeploymentRepository _repository;

    /// <summary>
    /// Creates a controller for deployment records.
    /// </summary>
    public DeploymentsController(IDeploymentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets deployment records, optionally filtered by environment, service name, or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<Deployment>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<Deployment>>> GetDeployments(
        [FromQuery] string? environment,
        [FromQuery] string? serviceName,
        [FromQuery] DeploymentStatus? status,
        CancellationToken cancellationToken)
    {
        var deployments = await _repository.GetAllAsync(
            environment,
            serviceName,
            status,
            cancellationToken);

        return Ok(deployments);
    }

    /// <summary>
    /// Gets a deployment record by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Deployment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Deployment>> GetDeployment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deployment = await _repository.GetByIdAsync(id, cancellationToken);

        return deployment is null
            ? NotFound()
            : Ok(deployment);
    }

    /// <summary>
    /// Gets the most recent deployment, optionally filtered by environment.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType<Deployment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Deployment>> GetLatestDeployment(
        [FromQuery] string? environment,
        CancellationToken cancellationToken)
    {
        var deployment = await _repository.GetLatestAsync(environment, cancellationToken);

        return deployment is null
            ? NotFound()
            : Ok(deployment);
    }

    /// <summary>
    /// Creates a deployment record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Deployment>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Deployment>> CreateDeployment(
        CreateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = await _repository.AddAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetDeployment),
            new { id = deployment.Id },
            deployment);
    }

    /// <summary>
    /// Updates the status and optional notes for an existing deployment.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<Deployment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Deployment>> UpdateDeploymentStatus(
        Guid id,
        UpdateDeploymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var deployment = await _repository.UpdateStatusAsync(id, request, cancellationToken);

        return deployment is null
            ? NotFound()
            : Ok(deployment);
    }

    /// <summary>
    /// Deletes a deployment record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeployment(  
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}
