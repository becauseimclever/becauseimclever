// <copyright file="TestConfiguration.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Provides access to test configuration including user secrets.
/// </summary>
public static class TestConfiguration
{
    private static readonly Lazy<IConfiguration> LazyConfiguration = new(() =>
    {
        return new ConfigurationBuilder()
            .AddUserSecrets<PlaywrightTestBase>()
            .Build();
    });

    /// <summary>
    /// Gets the configuration instance.
    /// </summary>
    public static IConfiguration Configuration => LazyConfiguration.Value;

    /// <summary>
    /// Gets the guest writer test credentials.
    /// </summary>
    /// <returns>A tuple containing the username and password, or null values if not configured.</returns>
    public static (string? Username, string? Password) GetGuestWriterCredentials()
    {
        var username = Configuration["TestAccounts:GuestWriter:Username"];
        var password = Configuration["TestAccounts:GuestWriter:Password"];
        return (username, password);
    }

    /// <summary>
    /// Gets the admin test credentials.
    /// </summary>
    /// <returns>A tuple containing the username and password, or null values if not configured.</returns>
    public static (string? Username, string? Password) GetAdminCredentials()
    {
        var username = Configuration["TestAccounts:Admin:Username"];
        var password = Configuration["TestAccounts:Admin:Password"];
        return (username, password);
    }
}
