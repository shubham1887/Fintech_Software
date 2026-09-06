using Fino.Pay1Api.Interfaces;
using Fino.Pay1Api.Messages;
using Microsoft.Extensions.Logging;

namespace Fino.Pay1Api.Repositories
{
    // Transport/data-access for recharges.pay1.in. Issues the HTTP GET and reports whether the call
    // was actually completed. A timeout is deliberately reported as "reached" = false with its own
    // message: the payment may well have gone through, so the host must hold it, never fail it.
    public class Pay1ApiClient : IPay1ApiClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<Pay1ApiClient>? _logger;

        public Pay1ApiClient(IHttpClientFactory httpFactory, ILogger<Pay1ApiClient>? logger = null)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<(string body, bool reached, string failureMessage)> Get(string url, int timeoutSeconds)
        {
            try
            {
                var client = _httpFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : 60);

                var response = await client.GetAsync(url);
                string body = await response.Content.ReadAsStringAsync();

                // Pay1 answers 200 even for a declined transaction, so a non-200 is a real
                // transport/gateway problem rather than a payment outcome.
                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("Pay1 HTTP {Status}: {Body}", (int)response.StatusCode, body);
                    return (body, false, Pay1.Message.ServiceUnavailable);
                }
                return (body, true, string.Empty);
            }
            catch (TaskCanceledException ex)
            {
                _logger?.LogError(ex, "Pay1 call timed out");
                return (string.Empty, false, Pay1.Message.Timeout);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Pay1 call failed");
                return (string.Empty, false, Pay1.Message.ServiceUnavailable);
            }
        }
    }
}
