using Fino.PlanApi.Interfaces;
using Fino.PlanApi.Messages;
using Fino.PlanApi.Responses;
using Microsoft.Extensions.Logging;

namespace Fino.PlanApi.Repositories
{
    // Transport/data-access for planapi.in. Issues the HTTP GET and maps the outcome to a
    // PlanApiResponse. All failures collapse to a single user-facing message (Plan.Message).
    public class PlanApiClient : IPlanApiClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<PlanApiClient>? _logger;

        public PlanApiClient(IHttpClientFactory httpFactory, ILogger<PlanApiClient>? logger = null)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<PlanApiResponse> Get(string url)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                var resp = await client.GetAsync(url);
                string body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("PlanApi HTTP {Status} for {Url}: {Body}", (int)resp.StatusCode, url, body);
                    return PlanApiResponse.Fail(Plan.Message.ServiceUnavailable);
                }
                return PlanApiResponse.Ok(body);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PlanApi call failed for {Url}", url);
                return PlanApiResponse.Fail(Plan.Message.ServiceUnavailable);
            }
        }
    }
}
