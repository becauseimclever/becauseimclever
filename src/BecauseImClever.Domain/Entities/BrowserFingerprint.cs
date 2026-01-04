namespace BecauseImClever.Domain.Entities;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Represents a browser fingerprint composed of various browser and device attributes.
/// This is a value object that can compute a stable hash for visitor identification.
/// </summary>
/// <param name="CanvasHash">Hash of canvas rendering output.</param>
/// <param name="WebGLRenderer">The WebGL renderer string (GPU info).</param>
/// <param name="ScreenResolution">Screen resolution in format "WIDTHxHEIGHT".</param>
/// <param name="ColorDepth">Color depth in bits.</param>
/// <param name="Timezone">IANA timezone identifier.</param>
/// <param name="Language">Browser language setting.</param>
/// <param name="Platform">Platform/OS identifier.</param>
/// <param name="HardwareConcurrency">Number of logical processors.</param>
public record BrowserFingerprint(
    string CanvasHash,
    string WebGLRenderer,
    string ScreenResolution,
    int ColorDepth,
    string Timezone,
    string Language,
    string Platform,
    int HardwareConcurrency)
{
    /// <summary>
    /// Computes a SHA256 hash of all fingerprint components.
    /// </summary>
    /// <returns>A hexadecimal string representation of the hash.</returns>
    public string ComputeHash()
    {
        var combined = string.Join(
            "|",
            this.CanvasHash,
            this.WebGLRenderer,
            this.ScreenResolution,
            this.ColorDepth.ToString(),
            this.Timezone,
            this.Language,
            this.Platform,
            this.HardwareConcurrency.ToString());

        var bytes = Encoding.UTF8.GetBytes(combined);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
