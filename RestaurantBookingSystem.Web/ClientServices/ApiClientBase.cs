using System.Net.Http.Json;
using System.Text.Json;

namespace RestaurantBookingSystem.Web.ClientServices;

public abstract class ApiClientBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }

    protected Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default) =>
        SendForJsonAsync<T>(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);

    protected Task<TResponse> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        SendForJsonAsync<TResponse>(CreateJsonRequest(HttpMethod.Post, requestUri, request), cancellationToken);

    protected Task<TResponse> PutAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        SendForJsonAsync<TResponse>(CreateJsonRequest(HttpMethod.Put, requestUri, request), cancellationToken);

    protected Task PutAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(CreateJsonRequest(HttpMethod.Put, requestUri, request), cancellationToken);

    protected Task PutAsync(string requestUri, CancellationToken cancellationToken = default) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Put, requestUri), cancellationToken);

    protected Task DeleteAsync(string requestUri, CancellationToken cancellationToken = default) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);

    private static HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string uri, TRequest request) =>
        new(method, uri) { Content = JsonContent.Create(request, options: JsonOptions) };

    private async Task<T> SendForJsonAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using (request)
        {
            using var response = await SendRequestAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return result ?? throw new ApiClientException(
                response.StatusCode,
                "The backend returned an empty or invalid response.");
        }
    }

    private async Task SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            using var response = await SendRequestAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await HttpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ApiClientException(
                System.Net.HttpStatusCode.ServiceUnavailable,
                "The backend API is unavailable. Start the API project and try again.",
                innerException: exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiClientException(
                System.Net.HttpStatusCode.ServiceUnavailable,
                "The backend API did not respond in time. Please try again.",
                innerException: exception);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = ExtractMessage(body) ?? $"The backend API returned error {(int)response.StatusCode}.";
        throw new ApiClientException(response.StatusCode, message, body);
    }

    private static string? ExtractMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("message", out var message))
                return message.GetString();
            if (document.RootElement.TryGetProperty("title", out var title))
                return title.GetString();
        }
        catch (JsonException)
        {
            // Keep the fallback when the backend response is not JSON.
        }
        return null;
    }
}
