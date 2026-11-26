namespace BecauseImClever.Application.Commissions;

/// <summary>A known failure safe to display without disclosing internal details.</summary>
/// <param name="code">Safe validation, not_found, conflict, or denied code.</param>
public sealed class CommissionException(string code) : Exception(code)
{
    public string Code { get; } = code;
}
