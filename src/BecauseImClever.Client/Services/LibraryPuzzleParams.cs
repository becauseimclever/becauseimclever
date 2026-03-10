namespace BecauseImClever.Client.Services;

/// <summary>Library puzzle: decode a cipher message.</summary>
/// <param name="CipherKey">26-character substitution key (A→key[0], B→key[1], …).</param>
/// <param name="EncodedMessage">The encoded message the player sees.</param>
/// <param name="PlainMessage">The decoded answer.</param>
public record LibraryPuzzleParams(string CipherKey, string EncodedMessage, string PlainMessage);
