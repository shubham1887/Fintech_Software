using Fino.PlanApi.Responses;

namespace Fino.PlanApi.Interfaces
{
    // Data-access layer for the provider (the "repository" of this library): performs the raw
    // HTTP GET against planapi.in and wraps the outcome. Knows nothing about specific endpoints.
    public interface IPlanApiClient
    {
        Task<PlanApiResponse> Get(string url);
    }
}
