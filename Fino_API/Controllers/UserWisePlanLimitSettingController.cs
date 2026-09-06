using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.UserWisePlanLimit;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class UserWisePlanLimitSettingController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IUserWisePlanLimitService _service;

        public UserWisePlanLimitSettingController(IConfiguration configuration, IUserWisePlanLimitService service)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _service = service;
        }

        [HttpPost("Insert")]
        public async Task<APIResponse> Insert([FromForm] APIRequest apiRequest)
        {
            CreateUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<CreateUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _service.Create(req);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.CreatedSuccessfully);
        }

        // Returns plan limits for a specific user (by UserID in request) or all records
        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            GetUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<GetUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            if (req.UserID > 0)
            {
                var list = await _service.GetByUserID(req.UserID);
                if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
            }
            else
            {
                var all = await _service.GetAll();
                if (all == null || all.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(all));
            }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            UpdateUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UpdateUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _service.Update(req);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            DeleteUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<DeleteUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _service.Delete(req.UserWisePlanLimitID);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse(Messages.Common.Message.DeletedSuccessfully);
        }

        // Activate or deactivate a single plan limit record — fetches current record, updates IsActive
        [HttpPost("ActiveDeactive")]
        public async Task<APIResponse> ActiveDeactive([FromForm] APIRequest apiRequest)
        {
            UpdateUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UpdateUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var current = await _service.GetByID(req.UserWisePlanLimitID);
            if (current == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            var updateReq = new UpdateUserWisePlanLimitRequest
            {
                UserWisePlanLimitID = current.UserWisePlanLimitID,
                UserID = current.UserID,
                UserTypeID = current.UserTypeID,
                CircleHitCount = current.CircleHitCount,
                PlanHitCount = current.PlanHitCount,
                BillFetchHitCount = current.BillFetchHitCount,
                IsActive = req.IsActive,
                IsDelete = current.IsDelete
            };
            string result = await _service.Update(updateReq);
            if (!result.EqualsIgnoreCase(StatusType.Success)) return ResponseProcess.GetFailResponse(result);
            string msg = req.IsActive ? "Plan limit activated successfully." : "Plan limit deactivated successfully.";
            return ResponseProcess.GetSuccessResponse(msg);
        }

        // Bulk activate/deactivate all plan limits for a user — iterates GetByUserID records and updates IsActive
        [HttpPost("AllActiveDeactive")]
        public async Task<APIResponse> AllActiveDeactive([FromForm] APIRequest apiRequest)
        {
            UpdateUserWisePlanLimitRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UpdateUserWisePlanLimitRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            int targetUserID = req.UserID > 0 ? req.UserID : keyDataModel.UserID;
            var list = await _service.GetByUserID(targetUserID);
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            foreach (var item in list)
            {
                var updateReq = new UpdateUserWisePlanLimitRequest
                {
                    UserWisePlanLimitID = item.UserWisePlanLimitID,
                    UserID = item.UserID,
                    UserTypeID = item.UserTypeID,
                    CircleHitCount = item.CircleHitCount,
                    PlanHitCount = item.PlanHitCount,
                    BillFetchHitCount = item.BillFetchHitCount,
                    IsActive = req.IsActive,
                    IsDelete = item.IsDelete
                };
                await _service.Update(updateReq);
            }

            string msg = req.IsActive ? "All plan limits activated successfully." : "All plan limits deactivated successfully.";
            return ResponseProcess.GetSuccessResponse(msg);
        }
    }
}
