using Microsoft.AspNetCore.Mvc;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Controllers;

/// <summary>
/// Lists recorded server signal samples.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SignalsController : ControllerBase
{
    private readonly IMonitoringRepository _repository;

    /// <summary>
    /// Creates a controller for signal samples.
    /// </summary>
    public SignalsController(IMonitoringRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets signal samples, optionally filtered by server or signal type.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<SignalSample>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SignalSample>>> GetSignals(
        [FromQuery] Guid? serverId,
        [FromQuery] SignalKind? kind,
        CancellationToken cancellationToken)
    {
        var signals = await _repository.GetSignalsAsync(serverId, kind, cancellationToken);

        return Ok(signals);
    }

    /// <summary>
    /// Gets a signal sample by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SignalSample>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignalSample>> GetSignal(
        Guid id,
        CancellationToken cancellationToken)
    {
        var signal = await _repository.GetSignalAsync(id, cancellationToken);

        return signal is null
            ? NotFound()
            : Ok(signal);
    }
}
