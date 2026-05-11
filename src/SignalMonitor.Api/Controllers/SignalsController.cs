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
}
