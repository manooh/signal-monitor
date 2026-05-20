using System.ComponentModel.DataAnnotations;

namespace SignalMonitor.Api.Models;

/// <summary>
/// Request body for recording a server signal sample.
/// </summary>
public sealed class IngestSignalRequest : IValidatableObject
{
    /// <summary>
    /// Type of signal being recorded.
    /// </summary>
    [Required]
    public SignalKind? Kind { get; init; }

    /// <summary>
    /// Numeric signal value. Heartbeat uses 1 for healthy and 0 for missing.
    /// </summary>
    [Required]
    [Range(0, 100)]
    public double? Value { get; init; }

    /// <summary>
    /// Optional unit for the value, for example percent or ok.
    /// </summary>
    [StringLength(32)]
    [RegularExpression(@".*\S.*")]
    public string? Unit { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Kind == SignalKind.Heartbeat &&
            Value is double heartbeatValue &&
            heartbeatValue is not 0 and not 1)
        {
            yield return new ValidationResult(
                "Heartbeat value must be 0 or 1.",
                [nameof(Value)]);
        }
    }
}
