using System.Net.Http.Json;

namespace Velux.Client.ApiClients;

/// <summary>Thrown when the API rejects a request; carries the validation errors from the server.</summary>
public class ApiException(List<string> errors) : Exception(string.Join(" ", errors))
{
    public List<string> Errors { get; } = errors;
}

internal static class HttpResponseExtensions
{
    public static async Task EnsureSuccessOrThrowAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        List<string>? errors = null;
        try
        {
            errors = await response.Content.ReadFromJsonAsync<List<string>>(JsonDefaults.Options);
        }
        catch
        {
            // The body was not a list of messages (e.g. a ProblemDetails object)
        }

        throw new ApiException(errors ?? new List<string> { $"The server returned an error ({(int)response.StatusCode})." });
    }
}
