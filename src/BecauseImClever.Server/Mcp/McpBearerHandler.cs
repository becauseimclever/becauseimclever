namespace BecauseImClever.Server.Mcp;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

/// <summary>Isolated, per-request MCP credential authentication. Never issues cookies.</summary>
/// <param name="options">Authentication scheme options.</param>
/// <param name="logger">Authentication logger.</param>
/// <param name="encoder">URL encoder.</param>
/// <param name="draftOptions">Reloadable private MCP configuration.</param>
public sealed class McpBearerHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptionsMonitor<McpDraftOptions> draftOptions) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>The isolated scheme and authorization policy name.</summary>
    public const string SchemeName = "McpDraft";

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configured = draftOptions.CurrentValue;
        var header = this.Request.Headers.Authorization;
        if (!this.Request.IsHttps || !configured.IsUsable(true) || header.Count != 1)
        {
            return Task.FromResult(AuthenticateResult.Fail("MCP authentication denied."));
        }

        var value = header[0]!;
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) || value.Length is < 50 or > 263 ||
            value[7..].Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
        {
            return Task.FromResult(AuthenticateResult.Fail("MCP authentication denied."));
        }

        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(value[7..]));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configured.Token!));
        if (!CryptographicOperations.FixedTimeEquals(providedHash, configuredHash))
        {
            return Task.FromResult(AuthenticateResult.Fail("MCP authentication denied."));
        }

        var identity = new ClaimsIdentity(
            [new(ClaimTypes.NameIdentifier, configured.OwnerSubject!), new(ClaimTypes.Name, configured.OwnerName!)],
            SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
