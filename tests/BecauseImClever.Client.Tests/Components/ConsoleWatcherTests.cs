namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ConsoleWatcher component.
/// </summary>
public class ConsoleWatcherTests : TestContext
{
    private readonly BunitJSInterop jsInterop;
    private readonly Mock<IConsentService> consentMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleWatcherTests"/> class.
    /// </summary>
    public ConsoleWatcherTests()
    {
        this.jsInterop = this.JSInterop;
        this.jsInterop.Mode = JSRuntimeMode.Loose;

        // Setup default JSInterop for sessionStorage
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherShown").SetResult(null);

        // Setup consent service - consented by default
        this.consentMock = new Mock<IConsentService>();
        this.consentMock.Setup(x => x.HasUserConsentedAsync()).ReturnsAsync(true);
        this.Services.AddSingleton(this.consentMock.Object);
    }

    /// <summary>
    /// Verifies that the component renders without any visible markup.
    /// </summary>
    [Fact]
    public void Render_Always_ProducesNoVisibleMarkup()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that the JS console message is invoked on first render.
    /// </summary>
    [Fact]
    public void OnAfterRender_FirstRender_InvokesConsoleWatcherShowMessage()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        var invocation = this.JSInterop.Invocations["consoleWatcher.showMessage"];
        Assert.Single(invocation);
    }

    /// <summary>
    /// Verifies that sessionStorage is checked on first render.
    /// </summary>
    [Fact]
    public void OnAfterRender_FirstRender_ChecksSessionStorage()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        var invocation = this.JSInterop.Invocations["sessionStorage.getItem"];
        Assert.Single(invocation);
        Assert.Equal("consoleWatcherShown", invocation[0].Arguments[0]);
    }

    /// <summary>
    /// Verifies that sessionStorage flag is set after showing the message.
    /// </summary>
    [Fact]
    public void OnAfterRender_FirstRender_SetsSessionStorageFlag()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var invocations = this.jsInterop.Invocations["sessionStorage.setItem"];
            Assert.Single(invocations);
            Assert.Equal("consoleWatcherShown", invocations[0].Arguments[0]);
            Assert.Equal("true", invocations[0].Arguments[1]);
        });
    }

    /// <summary>
    /// Verifies that the console message is not shown if the session flag is already set.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenAlreadyShown_DoesNotInvokeShowMessage()
    {
        // Arrange
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherShown").SetResult("true");

        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert - getItem was called, but showMessage should not have been
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["sessionStorage.getItem"]);
        });
        Assert.Empty(this.jsInterop.Invocations["consoleWatcher.showMessage"]);
    }

    /// <summary>
    /// Verifies that sessionStorage flag is not set again when already shown.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenAlreadyShown_DoesNotSetSessionStorageFlagAgain()
    {
        // Arrange
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherShown").SetResult("true");

        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert - getItem was called, but setItem should not have been
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["sessionStorage.getItem"]);
        });
        Assert.Empty(this.jsInterop.Invocations["sessionStorage.setItem"]);
    }

    /// <summary>
    /// Verifies that exceptions during JS interop are silently handled.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenJSInteropFails_DoesNotThrow()
    {
        // Arrange
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherShown")
            .SetException(new JSException("Test error"));
        this.jsInterop.SetupVoid("consoleWatcher.showMessage");

        // Act & Assert - should not throw
        var cut = this.Render<ConsoleWatcher>();
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that startDetection is called on first render to begin DevTools polling.
    /// </summary>
    [Fact]
    public void OnAfterRender_FirstRender_StartsDevToolsDetection()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var invocations = this.jsInterop.Invocations["consoleWatcher.startDetection"];
            Assert.Single(invocations);
        });
    }

    /// <summary>
    /// Verifies that startDetection passes a DotNetObjectReference to JS.
    /// </summary>
    [Fact]
    public void OnAfterRender_FirstRender_PassesDotNetReferenceToDetection()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var invocations = this.jsInterop.Invocations["consoleWatcher.startDetection"];
            Assert.Single(invocations);
            Assert.NotNull(invocations[0].Arguments[0]);
        });
    }

    /// <summary>
    /// Verifies that detection is still started even when the session flag is already set.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenAlreadyShown_StillStartsDetection()
    {
        // Arrange
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherShown").SetResult("true");

        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
        });
    }

    /// <summary>
    /// Verifies that the component exposes an OnDevToolsDetected EventCallback parameter.
    /// </summary>
    [Fact]
    public void Component_HasOnDevToolsDetectedParameter()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>(parameters => parameters
            .Add(p => p.OnDevToolsDetected, EventCallback.Factory.Create(this, () => { })));

        // Assert - component renders without error when parameter is provided
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that stopDetection is called when the component is disposed.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Dispose_Always_StopsDetection()
    {
        // Arrange
        var cut = this.Render<ConsoleWatcher>();
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
        });

        // Act
        await cut.InvokeAsync(async () => await ((IAsyncDisposable)cut.Instance).DisposeAsync());

        // Assert
        var invocations = this.jsInterop.Invocations["consoleWatcher.stopDetection"];
        Assert.Single(invocations);
    }

    /// <summary>
    /// Verifies that the toast is not visible by default.
    /// </summary>
    [Fact]
    public void Render_ByDefault_DoesNotShowToast()
    {
        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert
        Assert.DoesNotContain("console-watcher-toast", cut.Markup);
    }

    /// <summary>
    /// Verifies that the toast appears when OnDevToolsOpened is invoked.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task OnDevToolsOpened_WhenConsented_ShowsToast()
    {
        // Arrange
        var cut = this.Render<ConsoleWatcher>();
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
        });

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnDevToolsOpened());

        // Assert
        Assert.Contains("console-watcher-toast", cut.Markup);
        Assert.Contains("We see you", cut.Markup);
    }

    /// <summary>
    /// Verifies that the toast can be dismissed by clicking.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Toast_WhenClicked_IsDismissed()
    {
        // Arrange
        var cut = this.Render<ConsoleWatcher>();
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
        });
        await cut.InvokeAsync(() => cut.Instance.OnDevToolsOpened());
        Assert.Contains("console-watcher-toast", cut.Markup);

        // Act
        var toast = cut.Find(".console-watcher-toast");
        await cut.InvokeAsync(() => toast.Click());

        // Assert
        Assert.DoesNotContain("console-watcher-toast", cut.Markup);
    }

    /// <summary>
    /// Verifies that the console message still shows when consent is not granted.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenNotConsented_StillShowsConsoleMessage()
    {
        // Arrange
        this.consentMock.Setup(x => x.HasUserConsentedAsync()).ReturnsAsync(false);

        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert - console message should still be invoked
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.showMessage"]);
        });
    }

    /// <summary>
    /// Verifies that detection is not started when consent is not granted.
    /// </summary>
    [Fact]
    public void OnAfterRender_WhenNotConsented_DoesNotStartDetection()
    {
        // Arrange
        this.consentMock.Setup(x => x.HasUserConsentedAsync()).ReturnsAsync(false);

        // Act
        var cut = this.Render<ConsoleWatcher>();

        // Assert - wait for the async init to complete
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.showMessage"]);
        });
        Assert.Empty(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
    }

    /// <summary>
    /// Verifies that the toast is not shown when consent is not granted even if DevTools is detected.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task OnDevToolsOpened_WhenNotConsented_DoesNotShowToast()
    {
        // Arrange
        this.consentMock.Setup(x => x.HasUserConsentedAsync()).ReturnsAsync(false);
        var cut = this.Render<ConsoleWatcher>();
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.showMessage"]);
        });

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnDevToolsOpened());

        // Assert
        Assert.DoesNotContain("console-watcher-toast", cut.Markup);
    }

    /// <summary>
    /// Verifies that the toast is not shown when its session flag is already set.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task OnDevToolsOpened_WhenToastAlreadyShown_DoesNotShowToast()
    {
        // Arrange
        this.jsInterop.Setup<string?>("sessionStorage.getItem", "consoleWatcherToastShown").SetResult("true");
        var cut = this.Render<ConsoleWatcher>();
        cut.WaitForAssertion(() =>
        {
            Assert.Single(this.jsInterop.Invocations["consoleWatcher.startDetection"]);
        });

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnDevToolsOpened());

        // Assert
        Assert.DoesNotContain("console-watcher-toast", cut.Markup);
    }
}
