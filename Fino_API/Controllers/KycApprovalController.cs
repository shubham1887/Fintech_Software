using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Kyc;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The admin KYC desk — the review queue, the approve / reject decision, and the policy for
    /// what a user may still do while their KYC is pending.
    ///
    /// This controller is deliberately absent from ApiAccessPolicy's role map, so deny-by-default
    /// already keeps every role below Admin/Support out of it. The write actions assert the admin
    /// tier a second time: approving somebody's KYC is the platform operator's call, never the
    /// upline's.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class KycApprovalController : ControllerBase
    {
        private const string AdminOnly = "Only an administrator can perform this action.";

        private readonly IKycApprovalService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public KycApprovalController(IKycApprovalService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        private static bool IsAdmin(KeyDataModel keyData) =>
            keyData.UserTypeID == UserTypeCode.Admin || keyData.UserTypeID == UserTypeCode.SupportUser;

        /// <summary>Only the true administrator may change a KYC decision or the KYC policy.</summary>
        private static bool CanDecide(KeyDataModel keyData) => keyData.UserTypeID == UserTypeCode.Admin;

        private (KeyDataModel? keyData, T? request, APIResponse? error) Decode<T>(APIRequest apiRequest) where T : class, new()
        {
            KeyDataModel? keyDataModel;
            T? mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                // Filter-only endpoints are legitimately called with an empty body.
                string body = _encryptDecrypt.Decrypt(apiRequest.Request ?? string.Empty);
                mainRequest = string.IsNullOrWhiteSpace(body) ? new T() : JsonConvert.DeserializeObject<T>(body);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null) return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, null, ResponseProcess.GetFailResponse(fail));
            if (!IsAdmin(keyDataModel)) return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.NoPermission));

            return (keyDataModel, mainRequest ?? new T(), null);
        }

        private APIResponse Ok(object data) =>
            ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));

        // ── Queue ────────────────────────────────────────────────────────────────────

        /// <summary>The KYC queue: every user with their derived KYC state, plus the headline counts.</summary>
        [HttpPost("SelectUsers")]
        public async Task<APIResponse> SelectUsers([FromForm] APIRequest apiRequest)
        {
            var (_, request, error) = Decode<KycUserListRequest>(apiRequest);
            if (error != null) return error;
            try { return Ok(await _service.SelectUsers(request!)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>One user's full KYC file — profile, every configured document, E-KYC history.</summary>
        [HttpPost("SelectUser")]
        public async Task<APIResponse> SelectUser([FromForm] APIRequest apiRequest)
        {
            var (_, request, error) = Decode<KycUserDetailRequest>(apiRequest);
            if (error != null) return error;
            try
            {
                var (fail, data) = await _service.SelectUser(request!.UserID);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return Ok(data!);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Decisions ────────────────────────────────────────────────────────────────

        [HttpPost("ReviewDocument")]
        public async Task<APIResponse> ReviewDocument([FromForm] APIRequest apiRequest)
        {
            var (keyData, request, error) = Decode<KycReviewDocumentRequest>(apiRequest);
            if (error != null) return error;
            if (!CanDecide(keyData!)) return ResponseProcess.GetFailResponse(AdminOnly);
            try
            {
                string fail = await _service.ReviewDocument(request!, keyData!.UserID);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(request!.IsApproved ? "Document approved." : "Document rejected.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("ReviewUser")]
        public async Task<APIResponse> ReviewUser([FromForm] APIRequest apiRequest)
        {
            var (keyData, request, error) = Decode<KycReviewUserRequest>(apiRequest);
            if (error != null) return error;
            if (!CanDecide(keyData!)) return ResponseProcess.GetFailResponse(AdminOnly);
            try
            {
                string fail = await _service.ReviewUser(request!, keyData!.UserID);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(request!.IsApproved ? "KYC approved." : "KYC rejected.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Policy ───────────────────────────────────────────────────────────────────

        /// <summary>The KYC gate: is it enforced, and which services work without an approved KYC.</summary>
        [HttpPost("SelectPolicy")]
        public async Task<APIResponse> SelectPolicy([FromForm] APIRequest apiRequest)
        {
            var (_, _, error) = Decode<KycUserDetailRequest>(apiRequest);
            if (error != null) return error;
            try { return Ok(await _service.SelectPolicy()); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SavePolicy")]
        public async Task<APIResponse> SavePolicy([FromForm] APIRequest apiRequest)
        {
            var (keyData, request, error) = Decode<KycPolicySaveRequest>(apiRequest);
            if (error != null) return error;
            if (!CanDecide(keyData!)) return ResponseProcess.GetFailResponse(AdminOnly);
            try
            {
                string fail = await _service.SavePolicy(request!, keyData!.UserID);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse("KYC policy saved.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
