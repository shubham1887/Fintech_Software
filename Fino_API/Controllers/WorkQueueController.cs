using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// "What is waiting for me?" in one call, for the counts strip that sits on every screen.
    ///
    /// Identity comes from the decrypted, session-validated KeyData, so the counts are the
    /// caller's own - there is no UserID on the request to tamper with.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class WorkQueueController : ControllerBase
    {
        private readonly IWorkQueueService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger = new();

        public WorkQueueController(IWorkQueueService service, IConfiguration configuration)
        {
            _service = service;
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Counts")]
        public async Task<APIResponse> Counts([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? key;
            try { key = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); }

            if (key == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var counts = await _service.GetCounts(key);
                // All-zero is a valid, meaningful answer here ("nothing waiting"), so this is a
                // success with an empty-looking payload - never a "no record found" failure.
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(counts)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
