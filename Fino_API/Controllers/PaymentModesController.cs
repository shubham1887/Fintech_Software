using FintechApp.Authorization;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
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
    public class PaymentModesController : ControllerBase
    {
        private readonly IPaymentModeMasterService _paymentModeService;
        private readonly ICashCollectionMasterRepository _cashCollectionRepo;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public PaymentModesController(
            IPaymentModeMasterService paymentModeService,
            ICashCollectionMasterRepository cashCollectionRepo,
            IConfiguration configuration)
        {
            _paymentModeService = paymentModeService;
            _cashCollectionRepo = cashCollectionRepo;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var data = await _paymentModeService.GetAll();
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectByReceiverID")]
        public async Task<APIResponse> SelectByReceiverID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var allModes = await _paymentModeService.GetAll();

                // Get or auto-create the cash collection master for this user
                var ccList = await _cashCollectionRepo.GetByUserID(keyDataModel.UserID);
                CashCollectionMaster? ccMaster = ccList.FirstOrDefault();
                if (ccMaster == null)
                {
                    var newCc = new CashCollectionMaster
                    {
                        UserID = keyDataModel.UserID,
                        Outstanding = 0,
                        MaxOutstanding = 0
                    };
                    await _cashCollectionRepo.Insert(newCc);
                    ccList = await _cashCollectionRepo.GetByUserID(keyDataModel.UserID);
                    ccMaster = ccList.FirstOrDefault();
                }

                if (ccMaster == null)
                    return ResponseProcess.GetFailResponse("User not found");

                // If MaxOutstanding <= 0, exclude Credit payment mode
                const string creditType = "Credit";
                var filtered = ccMaster.MaxOutstanding <= 0
                    ? allModes.Where(m => !m.Type.Equals(creditType, StringComparison.OrdinalIgnoreCase)).ToList()
                    : allModes;

                if (filtered == null || !filtered.Any()) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(filtered)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
