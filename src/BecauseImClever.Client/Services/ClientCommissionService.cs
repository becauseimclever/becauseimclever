namespace BecauseImClever.Client.Services;

using System.Net;
using System.Net.Http.Json;
using BecauseImClever.Application.Commissions;

/// <summary>HTTP client for public and administrator commission workflows.</summary>
/// <param name="http">Same-origin HTTP client.</param>
public sealed class ClientCommissionService(HttpClient http)
{
    private const string PublicPath = "api/v1/commissions";
    private const string AdminPath = "api/v1/admin/commissions";

    public async Task<IReadOnlyList<CommissionSummary>> ListAsync(bool admin, int offset = 0, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"{(admin ? AdminPath : PublicPath)}?offset={offset}&limit=20", cancellationToken);
        return await ReadAsync<CommissionSummary[]>(response, cancellationToken);
    }

    public async Task<CommissionResponse?> GetAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"{PublicPath}/{Uri.EscapeDataString(slug)}", cancellationToken);
        return response.StatusCode == HttpStatusCode.NotFound ? null : await ReadAsync<CommissionResponse>(response, cancellationToken);
    }

    public async Task<CommissionResponse> GetForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"{AdminPath}/{id}", cancellationToken);
        return await ReadAsync<CommissionResponse>(response, cancellationToken);
    }

    public async Task<CommissionResponse> SaveAsync(CommissionResponse? current, CommissionWriteRequest request)
    {
        using var response = current is null
            ? await http.PostAsJsonAsync(AdminPath, request)
            : await http.PutAsJsonAsync(Path(current), request);
        return await ReadAsync<CommissionResponse>(response, CancellationToken.None);
    }

    public Task<CommissionResponse> PublishAsync(CommissionResponse current, bool published) =>
        this.PutAsync(Path(current, "/publication"), published);

    public Task<CommissionResponse> UpdateImageAsync(CommissionResponse current, Guid imageId, string altText) =>
        this.PutAsync(Path(current, $"/images/{imageId}"), altText);

    public Task<CommissionResponse> ReorderImagesAsync(CommissionResponse current, IReadOnlyList<Guid> imageIds) =>
        this.PutAsync(Path(current, "/images/order"), imageIds);

    public async Task<CommissionResponse> UploadImageAsync(CommissionResponse current, CommissionImageUpload image)
    {
        using var response = await http.PostAsJsonAsync(Path(current, "/images"), image);
        return await ReadAsync<CommissionResponse>(response, CancellationToken.None);
    }

    public async Task<CommissionResponse> DeleteImageAsync(CommissionResponse current, Guid imageId)
    {
        using var response = await http.DeleteAsync(Path(current, $"/images/{imageId}"));
        return await ReadAsync<CommissionResponse>(response, CancellationToken.None);
    }

    public async Task DeleteAsync(CommissionResponse current)
    {
        using var response = await http.DeleteAsync(Path(current));
        response.EnsureSuccessStatusCode();
    }

    private static string Path(CommissionResponse current, string suffix = "") =>
        $"{AdminPath}/{current.Id}{suffix}?expectedVersion={current.Version}";

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new System.Text.Json.JsonException("The server returned an empty commission response.");
    }

    private async Task<CommissionResponse> PutAsync<T>(string path, T value)
    {
        using var response = await http.PutAsJsonAsync(path, value);
        return await ReadAsync<CommissionResponse>(response, CancellationToken.None);
    }
}
