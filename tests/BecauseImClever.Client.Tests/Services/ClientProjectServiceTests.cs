namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;

/// <summary>
/// Unit tests for the <see cref="ClientProjectService"/> class.
/// </summary>
public class ClientProjectServiceTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientProjectService(null!));
        Assert.Equal("http", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidHttpClient_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new ClientProjectService(httpClient);

        // Assert
        Assert.NotNull(service);
    }
}
