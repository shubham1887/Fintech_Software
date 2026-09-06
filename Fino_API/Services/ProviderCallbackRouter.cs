using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Providers;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IProviderCallbackRouter"/>
    /// <remarks>
    /// Fixed order, same for every provider:
    ///   1. resolve the provider from the key in the URL   (unknown key -> logged and dropped)
    ///   2. IP allowlist, when the provider row carries one (wrong source -> logged and dropped)
    ///   3. the provider's parser verifies and translates   (unverified -> logged and dropped)
    ///   4. the neutral result goes through the ONE settlement path, ApplyRechargeCallback
    ///
    /// Every outcome — including a drop — is written to ProviderCallBackLog with the raw push, so a
    /// dispute with an upstream is answered from data rather than memory. Nothing here throws: the
    /// caller answers 200 "OK" regardless, because a non-200 makes providers retry forever.
    /// </remarks>
    public class ProviderCallbackRouter : IProviderCallbackRouter
    {
        private readonly IProviderMasterRepository _providerRepository;
        private readonly IEnumerable<IProviderCallbackParser> _parsers;
        private readonly ITransactionProcessingService _txnProcessing;
        private readonly IProviderCallBackLogRepository _callbackLog;
        private readonly Logger _logger = new();

        public ProviderCallbackRouter(
            IProviderMasterRepository providerRepository,
            IEnumerable<IProviderCallbackParser> parsers,
            ITransactionProcessingService txnProcessing,
            IProviderCallBackLogRepository callbackLog)
        {
            _providerRepository = providerRepository;
            _parsers = parsers;
            _txnProcessing = txnProcessing;
            _callbackLog = callbackLog;
        }

        public async Task<string> Route(string callbackKey, HttpRequest request, string ipAddress)
        {
            string key = ProviderCallbackKey.Normalize(callbackKey);
            var provider = (await _providerRepository.GetAll())
                .FirstOrDefault(p => !p.IsDelete && !p.IsDeleted &&
                                     ProviderCallbackKey.Normalize(p.CallbackKey) == key && key.Length > 0);

            if (provider == null)
            {
                var (_, unknownBody) = await CallbackValues.Read(request);
                await Drop(0, $"UNKNOWN KEY '{callbackKey}' [{ipAddress}] {Trim(unknownBody, request)}");
                return "Unknown callback key.";
            }

            return await Route(provider, request, ipAddress);
        }

        public async Task<string> Route(ProviderMaster provider, HttpRequest request, string ipAddress)
        {
            var (values, rawBody) = await CallbackValues.Read(request);
            string push = Trim(rawBody, request);

            if (!IsAllowedSource(provider, ipAddress))
            {
                await Drop(provider.ProviderID, $"IP REJECTED [{ipAddress}] {provider.ProviderName}: {push}");
                return "Callback source not allowed.";
            }

            var parser = _parsers.FirstOrDefault(p => p.CanHandle(provider));
            if (parser == null)
            {
                await Drop(provider.ProviderID, $"NO PARSER [{ipAddress}] {provider.ProviderName}: {push}");
                return "No callback parser for this provider.";
            }

            var parsed = parser.Parse(provider, values, rawBody);
            if (!parsed.Verified)
            {
                await Drop(provider.ProviderID, $"REJECTED ({parsed.Message}) [{ipAddress}] {provider.ProviderName}: {push}");
                _logger.Write(new Exception(
                    $"Callback REJECTED for {provider.ProviderName} [{ipAddress}]: {parsed.Message}. Push={push}"));
                return parsed.Message;
            }

            // One settlement path for every provider: SUCCESS finalises, FAILURE refunds, a reversal
            // refunds even a SUCCESS, everything is idempotent and the API-user callback fires from
            // inside it. The router deliberately owns none of that.
            string result = await _txnProcessing.ApplyRechargeCallback(parsed.Callback, ipAddress);

            _logger.Write(new Exception(
                $"Callback [{ipAddress}] {provider.ProviderName} req={parsed.Callback.RequestID} " +
                $"status={parsed.Callback.Status} reversal={parsed.Callback.IsReversal} -> {result}"));

            return result;
        }

        /// <summary>
        /// The callback key alone is a bearer secret; an IP allowlist is the second lock, and the
        /// only real one for providers that sign nothing. Blank <c>IPAddress</c> on the provider row
        /// means "not configured" — allowed, because most rows predate this and locking them out
        /// would silently strand live settlements.
        /// </summary>
        private static bool IsAllowedSource(ProviderMaster provider, string ipAddress)
        {
            string allowed = (provider.IPAddress ?? string.Empty).Trim();
            if (allowed.Length == 0) return true;

            return allowed
                .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(entry => entry.Trim().Equals(ipAddress, StringComparison.OrdinalIgnoreCase));
        }

        private static string Trim(string body, HttpRequest request)
        {
            string push = body.Length > 0 ? body : request.QueryString.Value ?? string.Empty;
            return push.Length > 3500 ? push[..3500] : push;
        }

        private async Task Drop(int providerId, string reply)
        {
            try
            {
                await _callbackLog.Insert(new ProviderCallBackLog
                {
                    TransactionMasterID = 0,
                    ProviderID = providerId,
                    Type = "RECHARGE",
                    ProviderReply = reply.Length > 4000 ? reply[..4000] : reply,
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now,
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
