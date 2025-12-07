namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BecauseImClever.Client.Services;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="HostAuthenticationStateProvider"/> class.
/// </summary>
public class HostAuthenticationStateProviderTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new HostAuthenticationStateProvider(null!));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidHttpClient_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenUnauthorized_ReturnsUnauthenticatedState()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.Unauthorized);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.NotNull(state);
        Assert.NotNull(state.User);
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenAuthenticated_ReturnsAuthenticatedState()
    {
        // Arrange
        var userInfo = new
        {
            Name = "testuser",
            Email = "test@example.com",
            IsAdmin = true,
            Claims = new[] { new { Type = "sub", Value = "12345" } },
        };
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, userInfo);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.NotNull(state);
        Assert.NotNull(state.User);
        Assert.True(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenAuthenticated_SetsCorrectNameClaim()
    {
        // Arrange
        var userInfo = new
        {
            Name = "testuser",
            Email = "test@example.com",
            IsAdmin = false,
            Claims = Array.Empty<object>(),
        };
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, userInfo);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Equal("testuser", state.User.Identity?.Name);
        Assert.Equal("testuser", state.User.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenAuthenticated_SetsCorrectEmailClaim()
    {
        // Arrange
        var userInfo = new
        {
            Name = "testuser",
            Email = "test@example.com",
            IsAdmin = false,
            Claims = Array.Empty<object>(),
        };
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, userInfo);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Equal("test@example.com", state.User.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenAdmin_SetsAdminGroupClaim()
    {
        // Arrange
        var userInfo = new
        {
            Name = "testuser",
            Email = "test@example.com",
            IsAdmin = true,
            Claims = Array.Empty<object>(),
        };
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, userInfo);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Equal("becauseimclever-admins", state.User.FindFirst("groups")?.Value);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenNotAdmin_DoesNotSetAdminGroupClaim()
    {
        // Arrange
        var userInfo = new
        {
            Name = "testuser",
            Email = "test@example.com",
            IsAdmin = false,
            Claims = Array.Empty<object>(),
        };
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, userInfo);
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.Null(state.User.FindFirst("groups"));
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenNullResponse_ReturnsUnauthenticatedState()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, content: "null");
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenHttpRequestException_ReturnsUnauthenticatedState()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
        var provider = new HostAuthenticationStateProvider(httpClient);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object? responseContent = null)
    {
        var content = responseContent != null
            ? JsonSerializer.Serialize(responseContent)
            : string.Empty;
        return CreateMockHttpClient(statusCode, content);
    }

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content),
            });

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
    }
}
