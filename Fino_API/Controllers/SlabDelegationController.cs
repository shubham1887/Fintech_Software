using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.SlabDelegation;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// "My Commission Slabs" — the delegation screens a super-distributor or distributor works
    /// from, and admin's read-only window onto what the resellers have done with their margin.
    ///
    /// Every action takes the caller's id and role out of the SESSION, never the request: the
    /// request may only name a slab or a user, and whether that slab is the caller's and whether
    /// that user is in their network is decided in the service against the database. None of the
    /// write actions carries a role check in this file, because the role alone does not answer the
    /// question that matters — a distributor may edit *their own* cards, which is ownership, not
    /// a permission. See SlabDelegationService.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SlabDelegationController : ControllerBase
    {
        private readonly ISlabDelegationService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public SlabDelegationController(ISlabDelegationService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>Decrypt the envelope and validate the session. Request body is optional.</summary>
        private (KeyDataModel? keyData, T? request, APIResponse? error) Decode<T>(APIRequest apiRequest, bool requireBody)
            where T : class, new()
        {
            KeyDataModel? keyDataModel;
            T? mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                mainRequest = string.IsNullOrWhiteSpace(apiRequest.Request)
                    ? new T()
                    : JsonConvert.DeserializeObject<T>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null || (requireBody && mainRequest == null))
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return (null, null, ResponseProcess.GetFailResponse(fail));

            return (keyDataModel, mainRequest ?? new T(), null);
        }

        /// <summary>The card assigned to me — my ceiling. Read-only by construction.</summary>
        [HttpPost("MyParentSlab")]
        public async Task<APIResponse> MyParentSlab([FromForm] APIRequest apiRequest)
        {
            var (key, _, error) = Decode<SlabDelegationViewRequest>(apiRequest, requireBody: false);
            if (error != null) return error;

            var (fail, data) = await _service.MyParentSlab(key!.UserID, key.UserTypeID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        /// <summary>The cards I own (every card, for admin).</summary>
        [HttpPost("MySlabs")]
        public async Task<APIResponse> MySlabs([FromForm] APIRequest apiRequest)
        {
            var (key, _, error) = Decode<SlabDelegationViewRequest>(apiRequest, requireBody: false);
            if (error != null) return error;

            var (fail, data) = await _service.MySlabs(key!.UserID, key.UserTypeID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        /// <summary>One card with every band and the ceiling each band is measured against.</summary>
        [HttpPost("SlabView")]
        public async Task<APIResponse> SlabView([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<SlabDelegationViewRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            var (fail, data) = await _service.SlabView(request!.SlabMasterID, key!.UserID, key.UserTypeID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        /// <summary>Cut a new child card out of mine.</summary>
        [HttpPost("CreateSlab")]
        public async Task<APIResponse> CreateSlab([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<CreateChildSlabRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            var (fail, slabMasterID) = await _service.CreateChildSlab(
                request!, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(new { SlabMasterID = slabMasterID }));
        }

        /// <summary>Move the commission figures on one band of one of my cards.</summary>
        [HttpPost("UpdateRow")]
        public async Task<APIResponse> UpdateRow([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<UpdateDelegatedRowRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.UpdateRow(request!, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        /// <summary>Add a band to one of my cards.</summary>
        [HttpPost("CreateRow")]
        public async Task<APIResponse> CreateRow([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<CreateDelegatedRowRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.CreateRow(request!, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse("Band added.");
        }

        /// <summary>Remove a band from one of my cards.</summary>
        [HttpPost("DeleteRow")]
        public async Task<APIResponse> DeleteRow([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<DeleteDelegatedRowRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.DeleteRow(request!.SlabDetailID, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse("Band removed.");
        }

        /// <summary>Remove one of my cards — refused while it is assigned or has children.</summary>
        [HttpPost("DeleteSlab")]
        public async Task<APIResponse> DeleteSlab([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<DeleteDelegatedSlabRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.DeleteSlab(
                request!.SlabMasterID, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse("Slab deleted.");
        }

        /// <summary>Hand one of my cards to one of my downline users.</summary>
        [HttpPost("AssignSlab")]
        public async Task<APIResponse> AssignSlab([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<AssignDelegatedSlabRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.AssignSlab(request!, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse("Slab assigned.");
        }

        /// <summary>Who in my network holds which of my cards.</summary>
        [HttpPost("Assignments")]
        public async Task<APIResponse> Assignments([FromForm] APIRequest apiRequest)
        {
            var (key, _, error) = Decode<SlabDelegationViewRequest>(apiRequest, requireBody: false);
            if (error != null) return error;

            var (fail, data) = await _service.Assignments(key!.UserID, key.UserTypeID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        /// <summary>Take an assignment back — the user follows their upline's card again.</summary>
        [HttpPost("UnassignSlab")]
        public async Task<APIResponse> UnassignSlab([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<Requests.UserSlabMapping.DeleteUserSlabMappingRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            string fail = await _service.UnassignSlab(
                request!.UserSlabMappingID, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse("Assignment removed.");
        }

        /// <summary>Pull every out-of-limit band on one of my cards back inside the parent's limits.</summary>
        [HttpPost("FixToParent")]
        public async Task<APIResponse> FixToParent([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<FixToParentRequest>(apiRequest, requireBody: true);
            if (error != null) return error;

            var (fail, repaired) = await _service.FixToParent(
                request!.SlabMasterID, key!.UserID, key.UserTypeID, key.IPAddress ?? string.Empty);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(repaired == 0
                ? "Nothing needed correcting."
                : $"{repaired} band(s) brought back inside your parent's limits.");
        }

        /// <summary>Rate-change history for my branch (every branch, for admin).</summary>
        [HttpPost("Audit")]
        public async Task<APIResponse> Audit([FromForm] APIRequest apiRequest)
        {
            var (key, request, error) = Decode<SlabAuditRequest>(apiRequest, requireBody: false);
            if (error != null) return error;

            var (fail, data) = await _service.Audit(request!, key!.UserID, key.UserTypeID);
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
    }
}
