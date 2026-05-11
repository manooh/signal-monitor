using Microsoft.AspNetCore.Mvc;
using SignalMonitor.Api.Models;
using SignalMonitor.Api.Repositories;

namespace SignalMonitor.Api.Controllers;

/// <summary>
/// Lists and manages alarms raised by unhealthy server signals.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AlarmsController : ControllerBase
{
    private readonly IMonitoringRepository _repository;

    /// <summary>
    /// Creates a controller for alarms.
    /// </summary>
    public AlarmsController(IMonitoringRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gets alarms, optionally filtered by server or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<Alarm>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<Alarm>>> GetAlarms(
        [FromQuery] Guid? serverId,
        [FromQuery] AlarmStatus? status,
        CancellationToken cancellationToken)
    {
        var alarms = await _repository.GetAlarmsAsync(serverId, status, cancellationToken);

        return Ok(alarms);
    }

    /// <summary>
    /// Updates an alarm status.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<Alarm>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Alarm>> UpdateAlarmStatus(
        Guid id,
        UpdateAlarmStatusRequest request,
        CancellationToken cancellationToken)
    {
        var alarm = await _repository.UpdateAlarmStatusAsync(id, request, cancellationToken);

        return alarm is null
            ? NotFound()
            : Ok(alarm);
    }
}
