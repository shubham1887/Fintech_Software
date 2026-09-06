using Fino.JaiJuiApi.Responses;

namespace Fino.JaiJuiApi.Interfaces
{
    // Data-access layer for the provider (the "repository" of this library): performs the raw HTTP
    // POST against jaijuirecharge.in with the "KeyData" credential header and a JSON body, and wraps
    // the outcome. Knows nothing about specific endpoints or their body shapes.
    public interface IJaiJuiApiClient
    {
        Task<JaiJuiApiResponse> Post(string url, string keyData, string body);
    }
}
