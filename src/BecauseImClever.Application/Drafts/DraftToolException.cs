namespace BecauseImClever.Application.Drafts;

/// <summary>A known failure safe to return to a tool caller without internal details.</summary>
/// <param name="code">A safe error code.</param>
public sealed class DraftToolException(string code) : Exception(code)
{
    /// <summary>Gets the safe error code: validation, denied, not_found, or conflict.</summary>
    public string Code { get; } = code;
}
