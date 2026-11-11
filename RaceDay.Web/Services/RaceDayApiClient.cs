using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services;

public sealed class RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    private void AttachToken()
    {
        var token = accessor.HttpContext?.Session.GetString("RaceDay.Jwt");
        http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        AttachToken();
        using var response = await http.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string path, TRequest payload, CancellationToken cancellationToken = default)
    {
        AttachToken();
        using var response = await http.PostAsJsonAsync(path, payload, cancellationToken);
        if (!response.IsSuccessStatusCode) return default;
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<bool> PutAsync<TRequest>(
        string path, TRequest payload, CancellationToken cancellationToken = default)
    {
        AttachToken();
        using var response = await http.PutAsJsonAsync(path, payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        AttachToken();
        using var response = await http.DeleteAsync(path, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
