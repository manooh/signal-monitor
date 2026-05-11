using System.ComponentModel.DataAnnotations;

namespace SignalMonitor.Api.Models;

/// <summary>
/// Request body for registering a monitored server.
/// </summary>
public sealed class CreateServerRequest
{
    /// <summary>
    /// Human-readable server name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@".*\S.*")]
    public required string Name { get; init; }

    /// <summary>
    /// Environment, for example dev, staging, or production.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@".*\S.*")]
    public required string Environment { get; init; }

    /// <summary>
    /// Region or location where the server runs.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@".*\S.*")]
    public required string Region { get; init; }
}
