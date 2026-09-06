using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.DailyAuthentication;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class DailyAuthenticationController : ControllerBase
    {
        private readonly IDailyAuthenticationMasterService _service;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IAEPSSettingRepository _aepsSettingRepo;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public DailyAuthenticationController(
            IDailyAuthenticationMasterService service,
            IProviderMasterRepository providerRepo,
            IAEPSSettingRepository aepsSettingRepo,
            IConfiguration configuration)
        {
            _service = service;
            _providerRepo = providerRepo;
            _aepsSettingRepo = aepsSettingRepo;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            SelectDailyAuthRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SelectDailyAuthRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                int providerId = await ResolveProviderId(request.ProviderName);
                var data = await _service.GetReport(keyDataModel.UserID, providerId, request.FromDate, request.ToDate);
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectLast2FAByUserID")]
        public async Task<APIResponse> SelectLast2FAByUserID([FromForm] APIRequest apiRequest)
        {
            SelectDailyAuthRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SelectDailyAuthRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                int providerId = await ResolveProviderId(request.ProviderName);
                var aesSetting = await _aepsSettingRepo.GetByName(request.ProviderName);
                bool isMerchantVerify = aesSetting?.IsMerchantVerify ?? false;

                var data = await _service.GetLast2FA(keyDataModel.UserID, providerId, isMerchantVerify);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        private async Task<int> ResolveProviderId(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) return 0;
            var all = await _providerRepo.GetAll();
            var provider = all.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            return provider?.ProviderID ?? 0;
        }
    }
}
