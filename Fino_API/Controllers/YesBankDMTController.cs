using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Beneficiary;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class YesBankDMTController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IBankTransferService _bankTransferService;

        public YesBankDMTController(IConfiguration configuration, IBeneficiaryService beneficiaryService, IBankTransferService bankTransferService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _beneficiaryService = beneficiaryService;
            _bankTransferService = bankTransferService;
        }

        // Registers sender with Yes Bank DMT API and stores in local DB — external Yes Bank integration pending
        [HttpPost("SenderRegistration")]
        public async Task<APIResponse> SenderRegistration([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "SenderRegistration pending: Yes Bank DMT sender registration via SenderBAL.SenderRegistration (YesBankCore API) not yet integrated."));
        }

        // Selects sender by GUID/mobile via SenderBAL (Yes Bank DMT specific lookup) — external Yes Bank integration pending
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "Select pending: Yes Bank DMT sender lookup by GUID/ClientID via SenderBAL not yet integrated."));
        }

        // Adds beneficiary via Yes Bank DMT API (generates OTP for verification) — external Yes Bank integration pending
        [HttpPost("AddBeneficiary")]
        public async Task<APIResponse> AddBeneficiary([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "AddBeneficiary pending: Yes Bank DMT beneficiary registration via BeneficiaryBAL.AddBeneficiary (YesBankCore API) not yet integrated."));
        }

        // Validates beneficiary OTP with Yes Bank DMT API to complete beneficiary registration — external Yes Bank integration pending
        [HttpPost("BeneficiaryOTPValidate")]
        public async Task<APIResponse> BeneficiaryOTPValidate([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "BeneficiaryOTPValidate pending: Yes Bank DMT OTP validation via BeneficiaryBAL.BeneficiaryOTPValidate (YesBankCore API) not yet integrated."));
        }

        // Returns registered beneficiaries for a given user from local DB
        [HttpPost("ViewBeneficiary")]
        public async Task<APIResponse> ViewBeneficiary([FromForm] APIRequest apiRequest)
        {
            GetBeneficiaryRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<GetBeneficiaryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            int targetUserID = req.UserID > 0 ? req.UserID : keyDataModel.UserID;
            var list = await _beneficiaryService.GetByUserID(targetUserID);
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
        }

        // Removes beneficiary via Yes Bank DMT API — external Yes Bank integration pending
        [HttpPost("DeleteBeneficiary")]
        public async Task<APIResponse> DeleteBeneficiary([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "DeleteBeneficiary pending: Yes Bank DMT beneficiary deactivation via BeneficiaryBAL.DeleteBeneficiary (YesBankCore API) not yet integrated."));
        }

        // Sends OTP to initiate refund for a failed DMT transaction via Yes Bank API — external Yes Bank integration pending
        [HttpPost("RefundSendOTP")]
        public async Task<APIResponse> RefundSendOTP([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "RefundSendOTP pending: Yes Bank DMT refund OTP via DMTRefundBAL.RefundSendOTP (YesBankCore API) not yet integrated."));
        }

        // Executes real-time money transfer via Yes Bank DMT API — external Yes Bank API + DMTProcessing integration pending
        // Yes Bank DMT money transfer — runs the full DMT pipeline (validate → balance → debit →
        // dispatch via bank-transfer provider seam → settle). Plug a YesBank IBankTransferProviderClient to go live.
        [HttpPost("MoneyTransfer")]
        public async Task<APIResponse> MoneyTransfer([FromForm] APIRequest apiRequest)
        {
            FintechApp.Requests.Transaction.BankTransferRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<FintechApp.Requests.Transaction.BankTransferRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            var (failureMessage, data) = await _bankTransferService.Transfer(req, FintechApp.Requests.Transaction.TransferKind.DMT, keyDataModel.UserID, ipAddress);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
    }
}
