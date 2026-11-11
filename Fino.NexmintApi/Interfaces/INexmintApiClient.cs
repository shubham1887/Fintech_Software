using Fino.NexmintApi.Responses;

namespace Fino.NexmintApi.Interfaces
{
    // Data-access layer for the provider (the "repository" of this library): performs the raw HTTP
    // call against api.nexmint.in with the client-id / Authorization headers, and wraps the outcome.
    // Knows nothing about specific endpoints or their payload shapes.
    public interface INexmintApiClient
    {
        // `body` is the JSON body for a POST, or null for a GET (the balance enquiry).
        Task<NexmintApiResponse> Send(HttpMethod method, string url, string clientID, string authToken, string? body, int timeoutSeconds);
    }
}
