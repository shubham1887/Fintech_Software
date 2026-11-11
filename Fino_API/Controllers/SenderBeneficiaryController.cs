using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Beneficiary;
using FintechApp.Requests.Sender;
using FintechApp.Requests.SenderBeneficiary;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SenderBeneficiaryController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ISenderService _senderService;
        private readonly IBeneficiaryService _beneficiaryService;

        public SenderBeneficiaryController(IConfiguration configuration, ISenderService senderService, IBeneficiaryService beneficiaryService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _senderService = senderService;
            _beneficiaryService = beneficiaryService;
        }

        /// <summary>
        /// The Sender / Beneficiary report: registered DMT senders, who they belong to, and how many
        /// beneficiaries each holds.
        ///
        /// This used to return only the senders registered under the CALLER. An admin never
        /// registers DMT senders themselves, so the report was permanently empty for exactly the
        /// role that opens it — which is why it "showed nothing". It also ignored the report's date
        /// range, entity filter and paging, and never carried the beneficiary count the report is
        /// named after. It is now scoped the way every other report is: admin and support see the
        /// whole platform (or one chosen entity), an upline sees its own network, everyone else sees
        /// their own senders.
        /// </summary>
        [HttpPost("SelectSender")]
        public async Task<APIResponse> SelectSender([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            SelectSenderReportRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                // The report screen always sends a filter body; older callers may not.
                request = JsonConvert.DeserializeObject<SelectSenderReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request))
                          ?? new SelectSenderReportRequest();
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (reportFail, data) = await _senderService.SelectReport(request, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!reportFail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(reportFail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Returns all beneficiaries for a given user (by UserID in request, or current user as fallback)
        [HttpPost("SelectBeneficiary")]
        public async Task<APIResponse> SelectBeneficiary([FromForm] APIRequest apiRequest)
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

        // Activate or deactivate a sender — fetches current record, toggles IsActive, updates
        [HttpPost("SenderActiveDeactive")]
        public async Task<APIResponse> SenderActiveDeactive([FromForm] APIRequest apiRequest)
        {
            SenderActivateRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<SenderActivateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var current = await _senderService.GetByID(req.SenderID);
            if (current == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            var updateReq = new UpdateSenderRequest
            {
                SenderID = current.SenderID,
                Name = current.Name,
                Mobile = current.Mobile,
                GUID = current.GUID,
                Address = current.Address,
                UserID = current.UserID,
                IsActive = req.IsActive,
                IsOTPVerified = current.IsOTPVerified,
                IsKYC = current.IsKYC,
                KYCDocumentURL = current.KYCDocumentURL,
                IPAddress = current.IPAddress,
                Pincode = current.Pincode
            };
            string result = await _senderService.Update(updateReq);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            string msg = req.IsActive ? "Sender activated successfully." : "Sender deactivated successfully.";
            return ResponseProcess.GetSuccessResponse(msg);
        }

        // Activate or deactivate a beneficiary — fetches current record, toggles IsActive, updates
        [HttpPost("BeneficiaryActDeact")]
        public async Task<APIResponse> BeneficiaryActDeact([FromForm] APIRequest apiRequest)
        {
            BeneficiaryActivateRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<BeneficiaryActivateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var current = await _beneficiaryService.GetByID(req.BeneficiaryID);
            if (current == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            var updateReq = new UpdateBeneficiaryRequest
            {
                BeneficiaryID = current.BeneficiaryID,
                Name = current.Name,
                BankName = current.BankName,
                AccountNo = current.AccountNo,
                IFSC = current.IFSC,
                GUID = current.GUID,
                SenderMobile = current.SenderMobile,
                IPAddress = current.IPAddress,
                SenderID = current.SenderID,
                UserID = current.UserID,
                IsActive = req.IsActive,
                IsOTPVerified = current.IsOTPVerified,
                IsNameVerified = current.IsNameVerified
            };
            string result = await _beneficiaryService.Update(updateReq);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            string msg = req.IsActive ? "Beneficiary activated successfully." : "Beneficiary deactivated successfully.";
            return ResponseProcess.GetSuccessResponse(msg);
        }
    }
}
