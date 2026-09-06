using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Providers;
using FintechApp.Requests.TransactionProcessing;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Inbound provider callbacks. ONE address serves every upstream:
    ///
    ///     https://&lt;our-host&gt;/api/Callback/p/{CallbackKey}
    ///
    /// The key is the provider's own id (ProviderMaster.CallbackKey) and is what identifies the
    /// caller — so a new integration is a row plus a key, never a new endpoint. Everything after
    /// that (source check, signature, translation, settlement, logging) is
    /// <see cref="IProviderCallbackRouter"/>, identical for all of them.
    ///
    /// The endpoints are necessarily anonymous — no provider carries our JWT — and every one of them
    /// answers 200 "OK" no matter what happened inside, because a non-200 makes an upstream retry
    /// forever. What actually happened lands in ProviderCallBackLog and the application log.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CORSProject")]
    public class CallbackController : ControllerBase
    {
        private readonly Logger _logger;
        private readonly ITransactionProcessingService _txnProcessing;
        private readonly IProviderMasterRepository _providerRepository;
        private readonly IProviderCallbackRouter _router;

        public CallbackController(ITransactionProcessingService txnProcessing,
            IProviderMasterRepository providerRepository,
            IProviderCallbackRouter router)
        {
            _logger = new Logger();
            _txnProcessing = txnProcessing;
            _providerRepository = providerRepository;
            _router = router;
        }

        // ── The shared address ───────────────────────────────────────────────────────
        // Give a provider https://<our-host>/api/Callback/p/<their key> and nothing else. GET, form
        // post and JSON post all work, so a change of transport on their side cannot break us.
        [HttpGet("p/{key}")]
        [HttpPost("p/{key}")]
        public async Task<string> Common(string key)
        {
            string ip = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                await _router.Route(key, Request, ip);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
            return "OK";
        }

        // ── Legacy addresses ─────────────────────────────────────────────────────────
        // Published before the shared address existed and still registered with live upstreams, so
        // they stay. Both now run the same router, so there is only one settlement path to reason
        // about; new integrations should be given /p/{key} instead.

        // Recharge status callback (Payin & compatible upstreams). The provider calls this URL as a
        // GET when a recharge changes from PROCESS to SUCCESS/FAILURE:
        //   /api/Callback/recharge?RequestID=..&ReferenceNo=..&Status=..&Number=..&Amount=..&Balance=..
        // We match RequestID back to the transaction and settle it (SUCCESS finalises, FAILURE refunds).
        [HttpGet("recharge")]
        [HttpPost("recharge")]
        public async Task<string> Recharge([FromQuery] RechargeCallbackRequest request)
        {
            string ip = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _txnProcessing.ApplyRechargeCallback(request, ip);
                _logger.Write(new Exception(
                    $"Callback.recharge [{ip}] RequestID={request.RequestID} Status={request.Status} Ref={request.ReferenceNo} -> {result}"));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
            return "OK";
        }

        // Pay1 "Auto Status Update Url", registered with Pay1 before the shared address existed.
        // The push authenticates itself with sha1(partner_id + client_req_id + api_key); verification
        // happens in Fino.Pay1Api against the credentials on the Pay1 provider row, and a body that
        // does not verify is logged and dropped, never settled.
        [HttpGet("pay1")]
        [HttpPost("pay1")]
        public async Task<string> Pay1()
        {
            string ip = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                var provider = (await _providerRepository.GetAll())
                    .FirstOrDefault(x => !x.IsDelete && Pay1ProviderResolver.IsPay1(x.ProviderName));

                if (provider == null)
                {
                    _logger.Write(new Exception($"Callback.pay1 [{ip}] dropped: no Pay1 provider configured."));
                    return "OK";
                }

                await _router.Route(provider, Request, ip);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
            return "OK";
        }

        [HttpPost("postResponse")] public async Task<string> postResponse() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"Callback.postResponse: {body}")); return "OK"; }

        [HttpPost("post_yesbank_upi_collection_response")] public async Task<string> post_yesbank_upi_collection_response() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"Callback.post_yesbank_upi_collection_response: {body}")); return "OK"; }

        [HttpPost("post_response_yes_bank")] public async Task<string> post_response_yes_bank() { string body = ""; using (var reader = new System.IO.StreamReader(Request.Body)) { body = await reader.ReadToEndAsync(); } _logger.Write(new Exception($"Callback.post_response_yes_bank: {body}")); return "OK"; }
    }
}
