namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="PostEditor"/> component.
/// </summary>
public class PostEditorTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostEditorTests"/> class.
    /// </summary>
    public PostEditorTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("vscode");
        this.JSInterop.SetupVoid("document.documentElement.removeAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
    }

    /// <summary>
    /// Verifies that the component displays Create New Post title for new post mode.
    /// </summary>
    [Fact]
    public void PostEditor_WhenNewPost_DisplaysCreateTitle()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        Assert.Contains("Create New Post", cut.Markup);
        Assert.Contains("Write a new blog post", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays Edit Post title when editing.
    /// </summary>
    [Fact]
    public void PostEditor_WhenEditingPost_DisplaysEditTitle()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post Title",
            "Test summary",
            "# Test Content",
            DateTimeOffset.Now,
            new List<string> { "tag1", "tag2" },
            PostStatus.Draft,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            "creator@test.com",
            "updater@test.com");

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        Assert.Contains("Edit Post", cut.Markup);
        Assert.Contains("Test Post Title", cut.Markup);
    }

    /// <summary>
    /// Verifies that the form displays all required fields.
    /// </summary>
    [Fact]
    public void PostEditor_DisplaysAllRequiredFields()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        Assert.Contains("Title", cut.Markup);
        Assert.Contains("Slug", cut.Markup);
        Assert.Contains("Summary", cut.Markup);
        Assert.Contains("Content (Markdown)", cut.Markup);
        Assert.Contains("Status", cut.Markup);
        Assert.Contains("Published Date", cut.Markup);
        Assert.Contains("Tags", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows loading state for edit mode.
    /// </summary>
    [Fact]
    public void PostEditor_WhenLoadingPost_ShowsLoadingIndicator()
    {
        // Arrange
        var tcs = new TaskCompletionSource<HttpResponseMessage>();
        this.ConfigureServices(pendingTask: tcs.Task);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "loading-test"));

        // Assert
        Assert.Contains("Loading post...", cut.Markup);
    }

    /// <summary>
    /// Verifies that the slug field is disabled when editing.
    /// </summary>
    [Fact]
    public void PostEditor_WhenEditing_SlugFieldIsDisabled()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        var slugInput = cut.Find("#slug");
        Assert.True(slugInput.HasAttribute("disabled"));
    }

    /// <summary>
    /// Verifies that the component displays Create Post button for new posts.
    /// </summary>
    [Fact]
    public void PostEditor_WhenNewPost_ShowsCreateButton()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        Assert.Contains("Create Post", cut.Markup);
        Assert.DoesNotContain("Delete", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays Update Post and Delete buttons when editing.
    /// </summary>
    [Fact]
    public void PostEditor_WhenEditing_ShowsUpdateAndDeleteButtons()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        Assert.Contains("Update Post", cut.Markup);
        Assert.Contains("Delete", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays metadata when editing.
    /// </summary>
    [Fact]
    public void PostEditor_WhenEditing_ShowsMetadata()
    {
        // Arrange
        var createdAt = new DateTime(2025, 6, 15, 10, 30, 0);
        var updatedAt = new DateTime(2025, 6, 20, 14, 45, 0);

        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            createdAt,
            updatedAt,
            "creator@test.com",
            "updater@test.com");

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        Assert.Contains("Metadata", cut.Markup);
        Assert.Contains("Created:", cut.Markup);
        Assert.Contains("Updated:", cut.Markup);
        Assert.Contains("creator@test.com", cut.Markup);
        Assert.Contains("updater@test.com", cut.Markup);
    }

    /// <summary>
    /// Verifies that tags are displayed as badges.
    /// </summary>
    [Fact]
    public void PostEditor_WhenHasTags_DisplaysTagBadges()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string> { "blazor", "dotnet", "testing" },
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        var tags = cut.FindAll(".tag");
        Assert.Equal(3, tags.Count);
    }

    /// <summary>
    /// Verifies that the component shows error when post not found.
    /// </summary>
    [Fact]
    public void PostEditor_WhenPostNotFound_ShowsError()
    {
        // Arrange
        this.ConfigureServices(notFound: true);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "non-existent"));

        // Assert - When API returns 404, HttpRequestException is thrown
        Assert.Contains("Failed to load post", cut.Markup);
    }

    /// <summary>
    /// Verifies that status dropdown has all options.
    /// </summary>
    [Fact]
    public void PostEditor_StatusDropdown_HasAllOptions()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        Assert.Contains("Draft", cut.Markup);
        Assert.Contains("Published", cut.Markup);
        Assert.Contains("Debug", cut.Markup);
    }

    /// <summary>
    /// Verifies that clicking Cancel button is rendered correctly.
    /// </summary>
    [Fact]
    public void PostEditor_HasCancelButton()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        var cancelButton = cut.Find("button.btn-secondary");
        Assert.Contains("Cancel", cancelButton.TextContent);
    }

    /// <summary>
    /// Verifies that clicking Delete shows confirmation modal.
    /// </summary>
    [Fact]
    public void PostEditor_WhenDeleteClicked_ShowsConfirmationModal()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post Title",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Act
        var deleteButton = cut.Find("button.btn-danger");
        deleteButton.Click();

        // Assert
        Assert.Contains("Delete Post", cut.Markup);
        Assert.Contains("Are you sure you want to delete", cut.Markup);
        Assert.Contains("Test Post Title", cut.Markup);
        Assert.Contains("This action cannot be undone", cut.Markup);
    }

    /// <summary>
    /// Verifies that word count displays correctly with content.
    /// </summary>
    [Fact]
    public void PostEditor_WithContent_DisplaysWordCount()
    {
        // Arrange
        var content = "This is a test post with exactly ten words here."; // Exactly 10 words
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            content,
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert - 10 words, 1 min read (minimum)
        Assert.Contains("10 words", cut.Markup);
        Assert.Contains("1 min read", cut.Markup);
    }

    /// <summary>
    /// Verifies that word count shows zero for empty content.
    /// </summary>
    [Fact]
    public void PostEditor_WithEmptyContent_DisplaysZeroWordCount()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert
        Assert.Contains("0 words", cut.Markup);
        Assert.Contains("1 min read", cut.Markup); // Minimum 1 min
    }

    /// <summary>
    /// Verifies that reading time calculates correctly for longer content.
    /// </summary>
    [Fact]
    public void PostEditor_WithLongContent_CalculatesCorrectReadingTime()
    {
        // Arrange - 400 words = 2 min read (200 wpm)
        var words = string.Join(" ", Enumerable.Repeat("word", 400));
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            words,
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert - 400 words / 200 wpm = 2 min
        Assert.Contains("400 words", cut.Markup);
        Assert.Contains("2 min read", cut.Markup);
    }

    /// <summary>
    /// Verifies that the editor registers beforeunload handler.
    /// </summary>
    [Fact]
    public void PostEditor_OnFirstRender_RegistersBeforeUnloadHandler()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<PostEditor>();

        // Assert - verify JS interop was called
        var jsInvocations = this.JSInterop.Invocations
            .Where(i => i.Identifier == "postEditor.registerBeforeUnload")
            .ToList();
        Assert.Single(jsInvocations);
    }

    /// <summary>
    /// Verifies that unsaved changes indicator is not shown for unchanged content.
    /// </summary>
    [Fact]
    public void PostEditor_WhenNoChanges_DoesNotShowUnsavedIndicator()
    {
        // Arrange
        var postForEdit = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.Now,
            new List<string>(),
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        this.ConfigureServices(postForEdit);

        // Act
        var cut = this.Render<PostEditor>(parameters => parameters
            .Add(p => p.Slug, "test-post"));

        // Assert
        Assert.DoesNotContain("Unsaved changes", cut.Markup);
    }

    private static HttpClient CreateMockHttpClient(
        PostForEdit? postForEdit,
        bool notFound,
        Task<HttpResponseMessage>? pendingTask)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        if (pendingTask != null)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(pendingTask);
        }
        else if (notFound)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });
        }
        else if (postForEdit != null)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(postForEdit, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    })),
                });
        }
        else
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(string.Empty),
                });
        }

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
    }

    private void ConfigureServices(
        PostForEdit? postForEdit = null,
        bool notFound = false,
        Task<HttpResponseMessage>? pendingTask = null)
    {
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        var httpClient = CreateMockHttpClient(postForEdit, notFound, pendingTask);
        this.Services.AddSingleton(httpClient);

        // Setup authorization - mock the policy authorization
        this.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireAssertion(_ => true));
        });

        // Setup a mock authentication state provider
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin@test.com"),
            new Claim("groups", "becauseimclever-admins"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider.Setup(p => p.GetAuthenticationStateAsync()).Returns(authState);

        this.Services.AddSingleton<AuthenticationStateProvider>(mockAuthStateProvider.Object);
    }
}
