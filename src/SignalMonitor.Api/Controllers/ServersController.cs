using Microsoft.AspNetCore.Mvc;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Controllers;

/// <summary>
/// Manages monitored servers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ServersController : ControllerBase
{
    private readonly IMonitoringRepository _repository;

    /// <summary>
    /// Creates a controller for monitored servers.
    /// </summary>
    public ServersController(IMonitoringRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets servers, optionally filtered by environment or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<Server>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<Server>>> GetServers(
        [FromQuery] string? environment,
        [FromQuery] ServerStatus? status,
        CancellationToken cancellationToken)
    {
        var servers = await _repository.GetServersAsync(environment, status, cancellationToken);

        return Ok(servers);
    }

    /// <summary>
    /// Gets a server by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Server>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Server>> GetServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var server = await _repository.GetServerAsync(id, cancellationToken);

        return server is null
            ? NotFound()
            : Ok(server);
    }

    /// <summary>
    /// Registers a monitored server.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<Server>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Server>> CreateServer(
        CreateServerRequest request,
        CancellationToken cancellationToken)
    {
        var server = await _repository.AddServerAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetServer),
            new { id = server.Id },
            server);
    }

    /// <summary>
    /// Records a signal sample for a server.
    /// </summary>
    [HttpPost("{id:guid}/signals")]
    [ProducesResponseType<SignalSample>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignalSample>> IngestSignal(
        Guid id,
        IngestSignalRequest request,
        CancellationToken cancellationToken)
    {
        var signal = await _repository.AddSignalAsync(id, request, cancellationToken);

        return signal is null
            ? NotFound()
            : CreatedAtAction(
                nameof(SignalsController.GetSignal),
                "Signals",
                new { id = signal.Id },
                signal);
    }

    /// <summary>
    /// Deletes a server and its related signal samples and alarms.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteServerAsync(id, cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}
