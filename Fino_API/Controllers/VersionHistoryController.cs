using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class VersionHistoryController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _versionHistoryUrl;

        public VersionHistoryController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _httpClientFactory = httpClientFactory;
            _versionHistoryUrl = configuration["VersionHistory:URL"] ?? string.Empty;
        }

        [HttpPost("VersionHistorySelect")] public async Task<APIResponse> VersionHistorySelect([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            if (string.IsNullOrWhiteSpace(_versionHistoryUrl))
                return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var raw = await client.GetStringAsync(_versionHistoryUrl);
                if (string.IsNullOrWhiteSpace(raw)) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                var wrapper = JsonConvert.DeserializeObject<VersionHistoryWrapper>(raw);
                if (wrapper == null || wrapper.Status?.ToLower() == "fail" || wrapper.Data == null)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                var sorted = wrapper.Data.OrderByDescending(x => x.DOC).ToList();
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(sorted)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        private class VersionHistoryWrapper
        {
            public string? Status { get; set; }
            public List<VersionHistoryItem>? Data { get; set; }
            public string? Response { get; set; }
        }

        private class VersionHistoryItem
        {
            public string? VersionNo { get; set; }
            public string? Description { get; set; }
            public DateTime DOC { get; set; }
        }
    }
}
