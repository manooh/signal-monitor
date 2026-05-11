using System.ComponentModel.DataAnnotations;

namespace SignalMonitor.Api.Models;

/// <summary>
/// Request body for updating alarm status.
/// </summary>
public sealed class UpdateAlarmStatusRequest
{
    /// <summary>
    /// New alarm status.
    /// </summary>
    [Required]
    public AlarmStatus? Status { get; init; }
}
