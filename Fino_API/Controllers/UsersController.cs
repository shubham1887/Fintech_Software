using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.AppNotification;
using FintechApp.Requests.User;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class UsersController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IReportAnalyticsService _analytics;
        private readonly IAppNotificationService _appNotifications;

        public UsersController(IConfiguration configuration, IUserService userService, IUserRepository userRepository, IReportAnalyticsService analytics, IAppNotificationService appNotifications)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
            _userService = userService;
            _userRepository = userRepository;
            _analytics = analytics;
            _appNotifications = appNotifications;
        }

        private const string AdminOnly = "Only an administrator can perform this action.";

        /// <summary>Admin tier — the platform operator, as opposed to anyone in the distribution
        /// network. Mirrors ApiAccessPolicy.Admin, which already denies these routes to every
        /// other role; this is the second, explicit check on the actions that change a user's
        /// standing (activate / deactivate / delete / restore).</summary>
        private static bool IsAdmin(KeyDataModel keyData) =>
            keyData.UserTypeID == UserTypeCode.Admin || keyData.UserTypeID == UserTypeCode.SupportUser;

        /// <summary>
        /// Decrypt the envelope and validate the session, returning the two payloads or the
        /// failure response to send back. Saves repeating the same twelve lines per endpoint.
        /// </summary>
        private (KeyDataModel? keyData, T? request, APIResponse? error) Decode<T>(APIRequest apiRequest) where T : class
        {
            KeyDataModel? keyDataModel;
            T? mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<T>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null || mainRequest == null)
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return (null, null, ResponseProcess.GetFailResponse(fail));

            return (keyDataModel, mainRequest, null);
        }

        /// <summary>
        /// Decrypt → validate → run one user-network report → serialise. Shared by the report
        /// endpoints below, which all post the same report request shape.
        /// </summary>
        private async Task<APIResponse> Report<T>(
            APIRequest apiRequest,
            Func<Requests.TransactionReport.TxnReportRequest, int, int, Task<(string fail, Responses.TransactionReport.AnalyticsWrapper<T> data)>> run)
        {
            KeyDataModel? keyDataModel;
            Requests.TransactionReport.TxnReportRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<Requests.TransactionReport.TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await run(req ?? new Requests.TransactionReport.TxnReportRequest(), keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("Insert")]
        public async Task<APIResponse> Insert([FromForm] APIRequest apiRequest)
        {
            UserInsertRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserInsertRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.Insert(mainRequest, keyDataModel.UserID, keyDataModel.IPAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.CreatedSuccessfully);
        }

        // Inline duplicate check for the Add User form: as the admin types a mobile / email the
        // client calls this to show an "already registered" hint before submit. Returns
        // { "MobileTaken": bool, "EmailTaken": bool } — only the supplied field(s) are checked.
        [HttpPost("CheckAvailability")]
        public async Task<APIResponse> CheckAvailability([FromForm] APIRequest apiRequest)
        {
            UserCheckAvailabilityRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserCheckAvailabilityRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            bool mobileTaken = false, emailTaken = false;
            string mobile = mainRequest.MobileNumber?.Trim() ?? string.Empty;
            string email = mainRequest.EmailID?.Trim() ?? string.Empty;
            int exclude = mainRequest.ExcludeUserID;
            if (mobile.Length == 10)
            {
                var owner = await _userRepository.GetUserByMobile(mobile);
                mobileTaken = owner != null && owner.UserMasterID != exclude;
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var owner = await _userRepository.GetUserByEmail(email);
                emailTaken = owner != null && owner.UserMasterID != exclude;
            }

            var data = JsonConvert.SerializeObject(new { MobileTaken = mobileTaken, EmailTaken = emailTaken });
            return ResponseProcess.GetSuccessResponse(data);
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            UserUpdateRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserUpdateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.Update(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("Activate")]
        public async Task<APIResponse> Activate([FromForm] APIRequest apiRequest)
        {
            UserActivateRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserActivateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            // Switching an account on or off is the platform operator's call, never the upline's.
            if (!IsAdmin(keyDataModel)) return ResponseProcess.GetFailResponse(AdminOnly);

            string result = await _userService.Activate(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("Deactivate")]
        public async Task<APIResponse> Deactivate([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.Deactivate(keyDataModel.UserID, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("CommissionHoldActivate")]
        public async Task<APIResponse> CommissionHoldActivate([FromForm] APIRequest apiRequest)
        {
            UserCommissionHoldRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserCommissionHoldRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.CommissionHoldActivate(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("IsAppActivate")]
        public async Task<APIResponse> IsAppActivate([FromForm] APIRequest apiRequest)
        {
            UserBooleanToggleRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserBooleanToggleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.IsAppActivate(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("IsWebActivate")]
        public async Task<APIResponse> IsWebActivate([FromForm] APIRequest apiRequest)
        {
            UserBooleanToggleRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserBooleanToggleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.IsWebActivate(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("IsInvoiceActivate")]
        public async Task<APIResponse> IsInvoiceActivate([FromForm] APIRequest apiRequest)
        {
            UserBooleanToggleRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserBooleanToggleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.IsInvoiceActivate(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("BulkUpdate")]
        public async Task<APIResponse> BulkUpdate([FromForm] APIRequest apiRequest)
        {
            UserBulkUpdateRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserBulkUpdateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (bulkFail, bulkSummary) = await _userService.BulkUpdate(mainRequest, keyDataModel.UserID);
            if (!bulkFail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(bulkFail);
            // The summary names how many were changed and which were skipped — more use to the
            // operator than a flat "updated successfully".
            return ResponseProcess.GetSuccessResponse(
                string.IsNullOrWhiteSpace(bulkSummary) ? Messages.Common.Message.UpdatedSuccessfully : bulkSummary);
        }

        [HttpPost("SendPassword")]
        public async Task<APIResponse> SendPassword([FromForm] APIRequest apiRequest)
        {
            UserSendPasswordRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserSendPasswordRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.SendPassword(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Password sent successfully.");
        }

        [HttpPost("AddHoldBalance")]
        public async Task<APIResponse> AddHoldBalance([FromForm] APIRequest apiRequest)
        {
            UserAddHoldBalanceRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserAddHoldBalanceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.AddHoldBalance(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(mainRequest.IsRelease ? "Hold released." : "Balance held.");
        }

        /// <summary>
        /// The hold report: every hold placed or released, on whose wallet, by whom and why.
        /// A frozen balance the user can see but not spend has to be explainable — this is where
        /// that explanation lives.
        /// </summary>
        [HttpPost("HoldHistory")]
        public async Task<APIResponse> HoldHistory([FromForm] APIRequest apiRequest)
        {
            WalletHoldHistoryRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<WalletHoldHistoryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (reportFail, data) = await _userService.HoldHistory(mainRequest, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!reportFail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(reportFail);
            if (data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
        }

        [HttpPost("Deactivate2FAWithOTP")]
        public async Task<APIResponse> Deactivate2FAWithOTP([FromForm] APIRequest apiRequest)
        {
            UserDeactivate2FARequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserDeactivate2FARequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.Deactivate2FAWithOTP(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("Select")]
        public async Task<APIResponse> SelectAll([FromForm] APIRequest apiRequest)
        {
            UserSelectRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserSelectRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.Select(mainRequest, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectByID")]
        public async Task<APIResponse> SelectByID([FromForm] APIRequest apiRequest)
        {
            UserSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectByID(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForDDL")]
        public async Task<APIResponse> SelectForDDL([FromForm] APIRequest apiRequest)
        {
            UserDDLRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserDDLRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectForDDL(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForDDLWithUserType")]
        public async Task<APIResponse> SelectForDDLWithUserType([FromForm] APIRequest apiRequest)
        {
            UserDDLRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserDDLRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectForDDLWithUserType(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("GSTView")]
        public async Task<APIResponse> GSTView([FromForm] APIRequest apiRequest)
        {
            UserGSTRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserGSTRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.GSTView(mainRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("FetchDetailsForPincode")]
        public async Task<APIResponse> FetchDetailsForPincode([FromForm] APIRequest apiRequest)
        {
            UserFetchPincodeRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserFetchPincodeRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.FetchDetailsForPincode(mainRequest.Pincode);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("LoginAsUser")]
        public async Task<APIResponse> LoginAsUser([FromForm] APIRequest apiRequest)
        {
            UserLoginAsRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserLoginAsRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, token) = await _userService.LoginAsUser(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(token);
        }

        // ─── Remaining stubs (not in CRITICAL list) ──────────────────────────────────

        [HttpPost("SelectAll")] public async Task<APIResponse> SelectAll2([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectByType")] public async Task<APIResponse> SelectByType([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUpline")] public async Task<APIResponse> SelectUpline([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectAgent")] public async Task<APIResponse> SelectAgent([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectParent")] public async Task<APIResponse> SelectParent([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDownlinForDropDown")] public async Task<APIResponse> SelectDownlinForDropDown([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                // The caller's full downline (all descendants), active + not deleted, never the caller
                // themselves. Used by fund reverse-request / cash-collection pickers.
                var allUsers = await _userRepository.GetAllUsers();
                var downlineIds = FintechApp.Helpers.UserHierarchy.GetDownlineIds(allUsers, keyDataModel.UserID);
                var result = allUsers
                    .Where(u => downlineIds.Contains(u.UserMasterID) && u.IsActive && !u.IsDeleted)
                    .OrderBy(u => u.FirmName)
                    // UserType and the mobile number ride along so the picker can say WHICH
                    // retailer or distributor each name is. A downline can hold two users with
                    // the same firm name at different tiers, and a list of bare names cannot tell
                    // them apart - which is how funds get reversed from the wrong account.
                    .Select(u => new
                    {
                        UserID = u.UserMasterID,
                        FirmName = u.FirmName,
                        UserType = u.UserType?.UserType ?? string.Empty,
                        MobileNumber = u.MobileNumber ?? string.Empty,
                    })
                    .ToList();
                if (result.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectDownlinForFundTransfer")] public async Task<APIResponse> SelectDownlinForFundTransfer([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDirectDownline")] public async Task<APIResponse> SelectDirectDownline([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDirectDownlineForUserBulk")] public async Task<APIResponse> SelectDirectDownlineForUserBulk([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDownlineWithOutStanding")] public async Task<APIResponse> SelectDownlineWithOutStanding([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDownlinWithBalances")] public async Task<APIResponse> SelectDownlinWithBalances([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserForDropDown")] public async Task<APIResponse> SelectUserForDropDown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserForDropDownForCashCollection")] public async Task<APIResponse> SelectUserForDropDownForCashCollection([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // The caller's own record for the profile page. Takes no target id, so every authenticated
        // role can call it without being able to read anybody else.
        [HttpPost("SelectProfile")]
        public async Task<APIResponse> SelectProfile([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectProfile(keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok) || data == null)
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ── One mobile number, two roles (StaticModels/LinkedProfilePolicy) ──────────────

        /// <summary>
        /// The logins this person holds, and whether they may add a second one.
        ///
        /// Every role may read this — the account switcher has to know whether to appear, and
        /// that includes the secondary account, which needs the switcher to get back. Nothing
        /// here names another person: the group is resolved from the session's own user.
        /// </summary>
        [HttpPost("LinkedProfiles")]
        public async Task<APIResponse> LinkedProfiles([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.GetLinkedProfiles(keyDataModel.UserID);
            if (!string.IsNullOrWhiteSpace(failureMessage)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
        }

        /// <summary>
        /// Set up the caller's second login. No target id — a second role is something you add
        /// to your own account, and the service decides whether your type may have one.
        /// </summary>
        [HttpPost("LinkedProfileAdd")]
        public async Task<APIResponse> LinkedProfileAdd([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, request, error) = Decode<CreateLinkedProfileRequest>(apiRequest);
            if (error != null) return error;

            var (failureMessage, data) = await _userService.CreateLinkedProfile(request!, keyDataModel!.UserID);
            if (!string.IsNullOrWhiteSpace(failureMessage)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)),
                "Second login created. Use the account switcher to move between them.");
        }

        // Self-service profile edit (contact + address). No target id: the session decides who.
        [HttpPost("UpdateProfile")]
        public async Task<APIResponse> UpdateProfile([FromForm] APIRequest apiRequest)
        {
            UserProfileUpdateRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UserProfileUpdateRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.UpdateProfile(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("UpdatePassword")] public async Task<APIResponse> UpdatePassword([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // Recipients a user can raise a fund request to: their parent chain up to admin, plus admin.
        // Value field is named "ParentID" (the recipient's UserMasterID) to match the UI dropdown.
        [HttpPost("SelectParentWithAdmin")] public async Task<APIResponse> SelectParentWithAdmin([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                var result = new List<object>();
                var seen = new HashSet<int>();

                // Walk up the parent chain from the caller (cap the loop for safety).
                var current = await _userRepository.GetUserById(keyDataModel.UserID);
                int guard = 0;
                while (current?.ParentID is int pid && pid > 0 && guard++ < 10)
                {
                    var parent = await _userRepository.GetUserById(pid);
                    if (parent == null) break;
                    if (parent.IsActive && seen.Add(parent.UserMasterID))
                        result.Add(new { ParentID = parent.UserMasterID, FirmName = parent.FirmName });
                    current = parent;
                }

                // Always include the admin(s).
                foreach (var admin in await _userRepository.GetUsersByUserType(UserTypeCode.Admin))
                    if (admin.IsActive && seen.Add(admin.UserMasterID))
                        result.Add(new { ParentID = admin.UserMasterID, FirmName = admin.FirmName });

                if (result.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(result)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectSupportUserForDropDown")] public async Task<APIResponse> SelectSupportUserForDropDown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        /// <summary>
        /// Register this device's FCM token.
        ///
        /// This used to answer "Feature under development." — so no token was ever stored, and
        /// every push the admin panel sent had nowhere to go. It now writes a UserDevices row
        /// (one per handset/browser) via the same service AppNotification/RegisterDevice uses;
        /// that endpoint is the one new clients should call, this stays for older builds.
        /// </summary>
        [HttpPost("UpdateFCMToken")]
        public async Task<APIResponse> UpdateFCMToken([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, request, error) = Decode<RegisterDeviceRequest>(apiRequest);
            if (error != null) return error;
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _appNotifications.RegisterDevice(request, keyDataModel);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("Device registered.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectUserForAdminDashborad")] public async Task<APIResponse> SelectUserForAdminDashborad([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // ─── API-user self-service: API token + IP security (OTP-gated) ──────────────
        // Fetch the caller's current API token (API users only).
        [HttpPost("SelectAPIToken")]
        public async Task<APIResponse> SelectAPIToken([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectAPIToken(keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Send an OTP (to the caller's mobile) before regenerating the API token.
        [HttpPost("SendAPITokenOTP")]
        public async Task<APIResponse> SendAPITokenOTP([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.SendAPITokenOTP(keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("OTP sent successfully.");
        }

        // Regenerate the API token after OTP verification; returns the new token.
        [HttpPost("TokenGeneration")]
        public async Task<APIResponse> TokenGeneration([FromForm] APIRequest apiRequest)
        {
            TokenGenerationRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TokenGenerationRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.TokenGeneration(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("ProfileUpload")] public async Task<APIResponse> ProfileUpload([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // Fetch the caller's IP-security state (toggle + whitelisted IPs).
        [HttpPost("SelectIsSecureIP")]
        public async Task<APIResponse> SelectIsSecureIP([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _userService.SelectIsSecureIP(keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Send an OTP (to the caller's mobile) before changing IP-security settings.
        [HttpPost("SendSecureIPOTP")]
        public async Task<APIResponse> SendSecureIPOTP([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.SendSecureIPOTP(keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("OTP sent successfully.");
        }

        // Save IP-security toggle + whitelisted IPs after OTP verification.
        [HttpPost("UpdateIsSecureIP")]
        public async Task<APIResponse> UpdateIsSecureIP([FromForm] APIRequest apiRequest)
        {
            UpdateSecureIPRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<UpdateSecureIPRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _userService.UpdateIsSecureIP(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("SelectChannelUserForDropDown")] public async Task<APIResponse> SelectChannelUserForDropDown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectParentWithAdminForCallMeRequest")] public async Task<APIResponse> SelectParentWithAdminForCallMeRequest([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectInActiveUsers")] public async Task<APIResponse> SelectInActiveUsers([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserBalanceSummaryForDistributorDashborad")] public async Task<APIResponse> SelectUserBalanceSummaryForDistributorDashborad([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("UpdatePin")] public async Task<APIResponse> UpdatePin([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // The caller's network below them, with per-wallet balances and on-hold amounts.
        [HttpPost("SelectDownlineReport")] public Task<APIResponse> SelectDownlineReport([FromForm] APIRequest apiRequest) => Report(apiRequest, _analytics.DownlineReport);

        [HttpPost("SelectRetailerAndAPIUserForDropDown")] public async Task<APIResponse> SelectRetailerAndAPIUserForDropDown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }


        // Users added in the selected window — the "Newly Added Users" report.
        [HttpPost("SelectDatewise")] public Task<APIResponse> SelectDatewise([FromForm] APIRequest apiRequest) => Report(apiRequest, _analytics.DatewiseUsers);

        [HttpPost("UpdateUserCommission")] public async Task<APIResponse> UpdateUserCommission([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectResponseMessage")] public async Task<APIResponse> SelectResponseMessage([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserCommissionByUserID")] public async Task<APIResponse> SelectUserCommissionByUserID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("TwoFAActivate")] public async Task<APIResponse> TwoFAActivate([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("VerifyTwoFA")] public async Task<APIResponse> VerifyTwoFA([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDeletedUsers")] public async Task<APIResponse> SelectDeletedUsers([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("UserRevert")] public async Task<APIResponse> UserRevert([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectSDAndDistForDropDown")] public async Task<APIResponse> SelectSDAndDistForDropDown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectSDAndDistForDropDownByOldUserID")] public async Task<APIResponse> SelectSDAndDistForDropDownByOldUserID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        /// <summary>
        /// User Transfer: move one user, and the branch beneath them, under a new parent.
        ///
        /// The service applies the tree rules (no cycles, no equal-or-lower parent, nothing
        /// outside the caller network) and records the move on the user change history with the
        /// remark, so the screen can say what happened rather than just "saved".
        /// </summary>
        [HttpPost("TransferUsers")]
        public async Task<APIResponse> TransferUsers([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, mainRequest, error) = Decode<UserTransferRequest>(apiRequest);
            if (error != null) return error;

            var (transferFail, summary) = await _userService.TransferUsers(mainRequest!, keyDataModel!.UserID);
            if (!transferFail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(transferFail);
            return ResponseProcess.GetSuccessResponse(
                string.IsNullOrWhiteSpace(summary) ? "User transferred." : summary);
        }

        /// <summary>
        /// The accounts a given user may legally be mapped under — drives the "New parent" picker
        /// on User Transfer, so the form can only offer moves TransferUsers will accept.
        /// </summary>
        [HttpPost("SelectEligibleParents")]
        public async Task<APIResponse> SelectEligibleParents([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, mainRequest, error) = Decode<UserSelectByIDRequest>(apiRequest);
            if (error != null) return error;

            var (fail, data) = await _userService.SelectEligibleParents(mainRequest!, keyDataModel!.UserID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
        }

        /// <summary>
        /// The user change report: every change made to a user record — what changed, from what to
        /// what, who did it, why, and from which IP / channel.
        /// </summary>
        [HttpPost("ChangeHistory")]
        public async Task<APIResponse> ChangeHistory([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, mainRequest, error) = Decode<UserChangeHistoryRequest>(apiRequest);
            if (error != null) return error;

            var (reportFail, data) = await _userService.ChangeHistory(mainRequest!, keyDataModel!.UserID, keyDataModel.UserTypeID);
            if (!reportFail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(reportFail);
            if (data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
        }

        [HttpPost("SelectByRole")] public async Task<APIResponse> SelectByRole([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectByRoleUserBulk")] public async Task<APIResponse> SelectByRoleUserBulk([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("ReKyc")] public async Task<APIResponse> ReKyc([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // Merchants pushed through onboarding, with their merchant code and current status.
        [HttpPost("SelectUserOnbordingList")] public Task<APIResponse> SelectUserOnbordingList([FromForm] APIRequest apiRequest) => Report(apiRequest, _analytics.OnboardingList);

        [HttpPost("SelectOnlyParentUserForDropdown")] public async Task<APIResponse> SelectOnlyParentUserForDropdown([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelecteKYCUserDetailsByUserID")] public async Task<APIResponse> SelecteKYCUserDetailsByUserID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDownlineForLedger")] public async Task<APIResponse> SelectDownlineForLedger([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserMorePayOnBoarding")] public async Task<APIResponse> SelectUserMorePayOnBoarding([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserByUserID")] public async Task<APIResponse> SelectUserByUserID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUserOnbordingStatusCheckList")] public async Task<APIResponse> SelectUserOnbordingStatusCheckList([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("LoginOTPActivate")] public async Task<APIResponse> LoginOTPActivate([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        /// <summary>
        /// Soft-delete a user (Admin only). The row is retained so history and the Deleted Users
        /// screen still resolve; the service refuses the delete while the account holds a balance
        /// or still has a downline mapped under it.
        /// </summary>
        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, mainRequest, error) = Decode<UserDeleteRequest>(apiRequest);
            if (error != null) return error;
            if (!IsAdmin(keyDataModel!)) return ResponseProcess.GetFailResponse(AdminOnly);

            string result = await _userService.Delete(mainRequest!, keyDataModel!.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("User deleted.");
        }

        /// <summary>Undo a soft delete (Admin only). The user comes back deactivated.</summary>
        [HttpPost("Restore")]
        public async Task<APIResponse> Restore([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, mainRequest, error) = Decode<UserDeleteRequest>(apiRequest);
            if (error != null) return error;
            if (!IsAdmin(keyDataModel!)) return ResponseProcess.GetFailResponse(AdminOnly);

            string result = await _userService.Restore(mainRequest!, keyDataModel!.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("User restored. Activate them to put the account back in service.");
        }

        [HttpPost("Select2FAQRImage")] public async Task<APIResponse> Select2FAQRImage([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("Send2FAOTP")] public async Task<APIResponse> Send2FAOTP([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }
    }
}
