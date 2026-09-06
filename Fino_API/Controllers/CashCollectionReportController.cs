using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.CashCollection;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CashCollectionReportController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ICashCollectionMasterService _cashCollectionService;

        public CashCollectionReportController(IConfiguration configuration, ICashCollectionMasterService cashCollectionService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _cashCollectionService = cashCollectionService;
        }

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, wrapper) = await _cashCollectionService.OutstandingReport(req);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            if (wrapper == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }

        [HttpPost("SelectByUserID")]
        public async Task<APIResponse> SelectByUserID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            SelectCashCollectionRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            int targetUserID = req.UserID > 0 ? req.UserID : keyDataModel.UserID;
            var (failMsg, data) = await _cashCollectionService.SelectUserOutstanding(targetUserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            if (data == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
    }
}
