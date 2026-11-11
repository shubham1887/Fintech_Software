using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Live;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The one endpoint the app chrome polls: work-queue counts and wallet balances together.
    ///
    /// Replaces three separate polls (WorkQueue/Counts, UserWalletMaster/GetByUserID,
    /// Wallet/Select) that every signed-in user ran on every screen. Those still exist for the
    /// screens that manage that data; this is the cheap read-only view behind the header.
    ///
    /// Identity comes from the decrypted, session-validated KeyData — cross-checked against the
    /// signed token by <see cref="JwtAuthorizationAttribute"/> — so the answer is always the
    /// caller's own. There is no user id on the request, which is the point: this is the most
    /// frequently called endpoint in the product, so it is the one that least deserves a
    /// parameter somebody could change to somebody else's id.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class LiveController : ControllerBase
    {
        private readonly ILiveSummaryService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger = new();

        public LiveController(ILiveSummaryService service, IConfiguration configuration)
        {
            _service = service;
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Summary")]
        public async Task<APIResponse> Summary([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? key;
            LiveSummaryRequest? request;
            try
            {
                key = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                // The version is optional: a first poll, or a client that does not track it, simply
                // sends nothing and gets the full payload.
                request = string.IsNullOrEmpty(apiRequest.Request)
                    ? new LiveSummaryRequest()
                    : JsonConvert.DeserializeObject<LiveSummaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            }

            if (key == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var summary = await _service.GetSummary(key, request?.Version ?? string.Empty);

                // All-zero counts and an empty wallet list are both valid answers here, so this is
                // always a success — a header that renders on every screen must never report a
                // quiet account as a failure.
                return ResponseProcess.GetSuccessResponse(
                    _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(summary)));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
