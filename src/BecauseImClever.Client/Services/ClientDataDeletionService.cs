// <copyright file="ClientDataDeletionService.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;

/// <summary>
/// Client-side service for requesting data deletion via the API.
/// </summary>
public class ClientDataDeletionService : IDataDeletionService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDataDeletionService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for API requests.</param>
    public ClientDataDeletionService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> DeleteMyDataAsync(string fingerprintHash)
    {
        try
        {
            var request = new DeleteDataRequest(fingerprintHash);
            var response = await this.httpClient.PostAsJsonAsync("api/extensiontracking/delete-my-data", request);

            if (!response.IsSuccessStatusCode)
            {
                return new DeletionResult(false, 0, "Failed to delete data. Please try again later.");
            }

            var result = await response.Content.ReadFromJsonAsync<DeleteDataResponse>();
            if (result == null)
            {
                return new DeletionResult(false, 0, "Unexpected response from server.");
            }

            return new DeletionResult(true, result.DeletedRecords, result.Message);
        }
        catch
        {
            return new DeletionResult(false, 0, "An error occurred while requesting data deletion.");
        }
    }
}
