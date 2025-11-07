
namespace FoundryLocal.Core;

/// <summary>
/// Intermediary result of the parsing operation.
/// </summary>
public record StudentProfileUpdate
{
    /// <summary>
    /// The text of this update
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// The final student profile. This will be null if the update is not yet finalized.
    /// </summary>
    public StudentProfile? StudentProfile { get; set; }
}
