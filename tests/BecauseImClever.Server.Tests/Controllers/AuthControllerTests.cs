namespace BecauseImClever.Server.Tests.Controllers;

using System.Security.Claims;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="AuthController"/> class.
/// </summary>
public class AuthControllerTests
{
    private readonly AuthController controller;
    private readonly Mock<IAuthenticationService> mockAuthService;

    public AuthControllerTests()
    {
        this.mockAuthService = new Mock<IAuthenticationService>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(this.mockAuthService.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider.Object,
        };

        this.controller = new AuthController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },
        };
    }

    [Fact]
    public void Login_ReturnsChallenge()
    {
        // Arrange
        var returnUrl = "/admin";

        // Act
        var result = this.controller.Login(returnUrl);

        // Assert
        var challengeResult = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("OpenIdConnect", challengeResult.AuthenticationSchemes.Single());
        Assert.NotNull(challengeResult.Properties);
        Assert.Equal(returnUrl, challengeResult.Properties.RedirectUri);
    }

    [Fact]
    public void Login_WithNullReturnUrl_DefaultsToRoot()
    {
        // Act
        var result = this.controller.Login(null);

        // Assert
        var challengeResult = Assert.IsType<ChallengeResult>(result);
        Assert.Equal("/", challengeResult.Properties!.RedirectUri);
    }

    [Fact]
    public async Task Logout_SignsOutAndRedirects()
    {
        // Arrange
        this.mockAuthService
            .Setup(s => s.SignOutAsync(It.IsAny<HttpContext>(), "Cookies", It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.controller.Logout();

        // Assert
        var signOutResult = Assert.IsType<SignOutResult>(result);
        Assert.Contains("Cookies", signOutResult.AuthenticationSchemes);
        Assert.Contains("OpenIdConnect", signOutResult.AuthenticationSchemes);
    }

    [Fact]
    public void GetCurrentUser_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        this.controller.HttpContext.User = new ClaimsPrincipal(identity);

        // Act
        var result = this.controller.GetCurrentUser();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetCurrentUser_WhenAuthenticated_ReturnsUserInfo()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("groups", "becauseimclever-admins"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        this.controller.HttpContext.User = new ClaimsPrincipal(identity);

        // Act
        var result = this.controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void GetCurrentUser_WhenAuthenticated_ReturnsCorrectClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("groups", "becauseimclever-admins"),
            new Claim("groups", "other-group"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        this.controller.HttpContext.User = new ClaimsPrincipal(identity);

        // Act
        var result = this.controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userInfo = okResult.Value;
        Assert.NotNull(userInfo);

        // Use reflection to check the anonymous type properties
        var nameProperty = userInfo.GetType().GetProperty("Name");
        var emailProperty = userInfo.GetType().GetProperty("Email");
        var isAdminProperty = userInfo.GetType().GetProperty("IsAdmin");
        var claimsProperty = userInfo.GetType().GetProperty("Claims");

        Assert.Equal("testuser", nameProperty?.GetValue(userInfo));
        Assert.Equal("test@example.com", emailProperty?.GetValue(userInfo));
        Assert.True((bool?)isAdminProperty?.GetValue(userInfo));
        Assert.NotNull(claimsProperty?.GetValue(userInfo));
    }

    [Fact]
    public void GetCurrentUser_WhenNotInAdminGroup_IsAdminIsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("groups", "other-group"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        this.controller.HttpContext.User = new ClaimsPrincipal(identity);

        // Act
        var result = this.controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userInfo = okResult.Value;
        Assert.NotNull(userInfo);

        var isAdminProperty = userInfo.GetType().GetProperty("IsAdmin");
        Assert.False((bool?)isAdminProperty?.GetValue(userInfo));
    }
}
