// <copyright file="WebServerFixture.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Shared fixture that starts the web server once for all E2E tests.
/// </summary>
public class WebServerFixture : IAsyncLifetime
{
    private Process? serverProcess;

    /// <summary>
    /// Gets the base URL where the server is running.
    /// </summary>
    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Starts the web server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        var port = GetAvailablePort();
        this.BaseUrl = $"http://localhost:{port}";

        // Get the path to the Server project
        var serverProjectPath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(WebServerFixture).Assembly.Location)!,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "BecauseImClever.Server"));

        this.serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-build --urls \"{this.BaseUrl}\"",
                WorkingDirectory = serverProjectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                },
            },
        };

        this.serverProcess.Start();

        // Wait for the server to be ready
        await this.WaitForServerReadyAsync();
    }

    /// <summary>
    /// Stops the web server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DisposeAsync()
    {
        if (this.serverProcess != null && !this.serverProcess.HasExited)
        {
            this.serverProcess.Kill(entireProcessTree: true);
            this.serverProcess.Dispose();
        }

        return Task.CompletedTask;
    }

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task WaitForServerReadyAsync()
    {
        using var httpClient = new HttpClient();
        var maxRetries = 30;
        var delayMs = 1000;

        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(this.BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet
            }

            await Task.Delay(delayMs);
        }

        throw new InvalidOperationException($"Server did not start within {maxRetries * delayMs / 1000} seconds.");
    }
}
