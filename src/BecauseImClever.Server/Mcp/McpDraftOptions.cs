namespace BecauseImClever.Server.Mcp;

/// <summary>Privately provisioned MCP-only credential and trusted owner attribution.</summary>
public sealed class McpDraftOptions
{
    /// <summary>Gets or sets whether the endpoint is enabled. Defaults to false.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets a private base64url token generated from at least 32 random bytes.</summary>
    public string? Token { get; set; }

    /// <summary>Gets or sets the human owner's trusted identity subject.</summary>
    public string? OwnerSubject { get; set; }

    /// <summary>Gets or sets the human owner's trusted display name.</summary>
    public string? OwnerName { get; set; }

    /// <summary>Gets or sets an optional UTC credential expiry.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    internal bool IsUsable(bool databaseConfigured) =>
        this.Enabled && databaseConfigured &&
        this.Token is { Length: >= 43 and <= 256 } &&
        this.Token.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') &&
        !string.IsNullOrWhiteSpace(this.OwnerSubject) && this.OwnerSubject.Length <= 100 &&
        !string.IsNullOrWhiteSpace(this.OwnerName) && this.OwnerName.Length <= 255 &&
        (this.ExpiresAt is null || this.ExpiresAt > DateTimeOffset.UtcNow);
}
