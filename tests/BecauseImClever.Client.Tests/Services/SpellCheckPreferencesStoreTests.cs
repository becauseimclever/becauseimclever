namespace BecauseImClever.Client.Tests.Services;

using System.Security.Claims;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

/// <summary>
/// Unit tests for the <see cref="SpellCheckPreferencesStore"/> class.
/// </summary>
public class SpellCheckPreferencesStoreTests : BunitContext
{
    private readonly AuthenticationStateProvider authStateProvider = CreateAuthenticationStateProvider("user-123");

    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SpellCheckPreferencesStore(null!, this.authStateProvider));
        Assert.Equal("jsRuntime", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullAuthenticationStateProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, null!));
        Assert.Equal("authenticationStateProvider", exception.ParamName);
    }

    [Fact]
    public async Task GetCustomSpellCheckEnabledAsync_WithSavedTrue_ReturnsTrue()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "spellcheck.custom.enabled.user-123").SetResult("True");
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        var enabled = await store.GetCustomSpellCheckEnabledAsync();

        // Assert
        Assert.True(enabled);
    }

    [Fact]
    public async Task GetCustomSpellCheckEnabledAsync_WhenInteropFails_ReturnsFalse()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "spellcheck.custom.enabled.user-123")
            .SetException(new JSException("storage unavailable"));
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        var enabled = await store.GetCustomSpellCheckEnabledAsync();

        // Assert
        Assert.False(enabled);
    }

    [Fact]
    public async Task SetCustomSpellCheckEnabledAsync_StoresValue()
    {
        // Arrange
        this.JSInterop.SetupVoid("localStorage.setItem", "spellcheck.custom.enabled.user-123", "True").SetVoidResult();
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        await store.SetCustomSpellCheckEnabledAsync(true);

        // Assert
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public async Task GetIgnoredWordsAsync_WithSavedJson_ReturnsWords()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "spellcheck.custom.ignoredWords.user-123")
            .SetResult("[\"teh\",\"wierd\"]");
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        var words = await store.GetIgnoredWordsAsync();

        // Assert
        Assert.Equal(2, words.Count);
        Assert.Contains("teh", words);
        Assert.Contains("wierd", words);
    }

    [Fact]
    public async Task GetIgnoredWordsAsync_WithInvalidJson_ReturnsEmpty()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "spellcheck.custom.ignoredWords.user-123")
            .SetResult("not-json");
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        var words = await store.GetIgnoredWordsAsync();

        // Assert
        Assert.Empty(words);
    }

    [Fact]
    public async Task SetIgnoredWordsAsync_NormalizesAndStoresJson()
    {
        // Arrange
        this.JSInterop.SetupVoid(
            "localStorage.setItem",
            "spellcheck.custom.ignoredWords.user-123",
            "[\"teh\",\"wierd\"]").SetVoidResult();
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        await store.SetIgnoredWordsAsync(new[] { "teh", "  wierd ", string.Empty, "TEH" });

        // Assert
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public async Task SetIgnoredWordsAsync_WhenInteropFails_DoesNotThrow()
    {
        // Arrange
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true)
            .SetException(new JSException("storage unavailable"));
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, this.authStateProvider);

        // Act
        var exception = await Record.ExceptionAsync(() => store.SetIgnoredWordsAsync(new[] { "teh" }));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetCustomSpellCheckEnabledAsync_WhenAnonymous_UsesAnonymousScopeKey()
    {
        // Arrange
        var anonymousProvider = CreateAuthenticationStateProvider(userId: null);
        this.JSInterop.Setup<string?>("localStorage.getItem", "spellcheck.custom.enabled.anonymous").SetResult("True");
        var store = new SpellCheckPreferencesStore(this.JSInterop.JSRuntime, anonymousProvider);

        // Act
        var enabled = await store.GetCustomSpellCheckEnabledAsync();

        // Assert
        Assert.True(enabled);
    }

    private static AuthenticationStateProvider CreateAuthenticationStateProvider(string? userId)
    {
        ClaimsIdentity identity = string.IsNullOrWhiteSpace(userId)
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim("sub", userId)], authenticationType: "TestAuth");

        var user = new ClaimsPrincipal(identity);
        return new TestAuthenticationStateProvider(new AuthenticationState(user));
    }

    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationState authenticationState;

        public TestAuthenticationStateProvider(AuthenticationState authenticationState)
        {
            this.authenticationState = authenticationState;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(this.authenticationState);
        }
    }
}