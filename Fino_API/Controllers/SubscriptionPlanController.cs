using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.SubscriptionPlan;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Subscription plans end to end: admin management (a plan header + the services it
    /// bundles) and the user-facing subscribe flow (charge the plan amount from the Recharge
    /// wallet, then activate every service in the plan for the user).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SubscriptionPlanController : ControllerBase
    {
        private const int AdminUserTypeID = 1;

        private const string SubscriptionEnabledKey = "IsSubscriptionEnabled";

        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ISubscriptionPlanService _plans;
        private readonly IUserServiceMappingService _userServiceMappingService;
        private readonly IAccountProcessingService _account;
        private readonly ISettingMasterService _settings;

        public SubscriptionPlanController(
            IConfiguration configuration,
            ISubscriptionPlanService plans,
            IUserServiceMappingService userServiceMappingService,
            IAccountProcessingService account,
            ISettingMasterService settings)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _plans = plans;
            _userServiceMappingService = userServiceMappingService;
            _account = account;
            _settings = settings;
        }

        // Master Key Setting gate: IsSubscriptionEnabled (curated feature toggle).
        private async Task<bool> IsSubscriptionEnabled()
        {
            var all = await _settings.GetAll();
            var row = all.FirstOrDefault(s => s.Key.EqualsIgnoreCase(SubscriptionEnabledKey));
            if (row == null) return false;
            if (row.IsOn) return true;
            var v = (row.Value ?? string.Empty).Trim().ToLowerInvariant();
            return v is "true" or "1" or "yes" or "on" or "y";
        }

        // ── Admin: create / update a plan together with its bundled services ──────────
        [HttpPost("SavePlan")]
        public async Task<APIResponse> SavePlan([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<SavePlanRequest>(apiRequest);
            if (error != null) return error;
            if (keyDataModel!.UserTypeID != AdminUserTypeID) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
            if (string.IsNullOrWhiteSpace(req!.PlanName)) return ResponseProcess.GetFailResponse("Plan name is required.");

            try
            {
                var result = await _plans.SavePlan(req);
                if (!result.Status.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result.Status);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(result)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Admin: all plans, each with its bundled service ids ───────────────────────
        [HttpPost("SelectPlans")]
        public async Task<APIResponse> SelectPlans([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = DecodeKey(apiRequest);
            if (error != null) return error;
            try
            {
                var list = await _plans.GetPlansWithServices();
                if (list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(list)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Admin / edit: a single plan with its services ─────────────────────────────
        [HttpPost("SelectPlanByID")]
        public async Task<APIResponse> SelectPlanByID([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<GetPlanRequest>(apiRequest);
            if (error != null) return error;
            try
            {
                var data = await _plans.GetPlanWithServices(req!.PlanID);
                if (data == null) return ResponseProcess.GetFailResponse(Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Admin: delete a plan (and its service links) ──────────────────────────────
        [HttpPost("DeletePlan")]
        public async Task<APIResponse> DeletePlan([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<GetPlanRequest>(apiRequest);
            if (error != null) return error;
            if (keyDataModel!.UserTypeID != AdminUserTypeID) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
            try
            {
                string result = await _plans.DeletePlan(req!.PlanID);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(Messages.ServiceSubscriptionCreatePlanMasterMessages.DeletedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── User: available plans, each flagged with whether the caller is subscribed ─
        [HttpPost("AvailablePlans")]
        public async Task<APIResponse> AvailablePlans([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = DecodeKey(apiRequest);
            if (error != null) return error;
            try
            {
                var plans = await _plans.GetPlansWithServices();
                var mappings = await _userServiceMappingService.GetByUserID(keyDataModel!.UserID);
                var activeServiceIds = mappings?.Where(m => m.IsActive).Select(m => m.ServiceID).ToHashSet() ?? new HashSet<int>();

                foreach (var plan in plans)
                {
                    plan.IsSubscribed = plan.ServiceIDs.Count > 0 && plan.ServiceIDs.All(activeServiceIds.Contains);
                }
                if (plans.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(plans)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Subscribe a user to a plan: charge the plan amount from the Recharge wallet
        //    (amount resolved server-side, never trusted from the client), then activate
        //    every service in the plan for the user. ───────────────────────────────────
        [HttpPost("Subscribe")]
        public async Task<APIResponse> Subscribe([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<SubscribeRequest>(apiRequest);
            if (error != null) return error;
            if (req!.ServiceSubscriptionCreatePlanMasterID <= 0) return ResponseProcess.GetFailResponse(Messages.Subscription.Message.InvalidPlan);

            // Admin may subscribe on behalf of a user; everyone else subscribes themselves.
            int targetUserID = (keyDataModel!.UserTypeID == AdminUserTypeID && req.UserID > 0) ? req.UserID : keyDataModel.UserID;

            var planServices = await _plans.GetServiceIdsForPlan(req.ServiceSubscriptionCreatePlanMasterID);
            if (planServices.Count == 0) return ResponseProcess.GetFailResponse(Messages.Subscription.Message.NoServices);

            // Amount is resolved from the plan itself — the client cannot dictate the charge.
            decimal amount = await _plans.GetPlanAmount(req.ServiceSubscriptionCreatePlanMasterID);
            if (amount > 0)
            {
                string balanceCheck = await _account.BalanceValidationBeforeTransaction(targetUserID, (int)WalletCode.Recharge, amount, 0);
                if (!balanceCheck.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(balanceCheck);

                int debit = await _account.DebitBalance(new FintechApp.Requests.AccountProcessing.AccountPostingRequest
                {
                    UserID = targetUserID,
                    WalletID = (int)WalletCode.Recharge,
                    AmountWithCommission = amount,
                    Amount = amount,
                    ConsumerNumber = string.Empty,
                    CommissionCreditDebit = CommissionCreditDebitType.None,
                    DOCType = "ServiceSubscriptionCharge",
                    AssignedUserID = targetUserID,
                    ServiceID = 0,
                    OperatorID = 0,
                    TransactionDescription = $"Service subscription charge for plan {req.ServiceSubscriptionCreatePlanMasterID}",
                    ReferenceID = req.ServiceSubscriptionCreatePlanMasterID,
                    TDS = 0,
                    GST = 0,
                    DiscountAmount = 0
                });
                if (debit <= 0) return ResponseProcess.GetFailResponse(Messages.Wallet.Message.InsufficientBalance);

                // Credit the subscription revenue to the company/admin wallet (writes a ledger row),
                // so subscriptions reconcile in the admin accounting / income reports.
                int adminUserID = await _plans.GetAdminUserId();
                if (adminUserID > 0 && adminUserID != targetUserID)
                {
                    await _account.CreditBalance(new FintechApp.Requests.AccountProcessing.AccountPostingRequest
                    {
                        UserID = adminUserID,
                        WalletID = (int)WalletCode.Recharge,
                        AmountWithCommission = amount,
                        Amount = amount,
                        ConsumerNumber = string.Empty,
                        CommissionCreditDebit = CommissionCreditDebitType.None,
                        DOCType = "ServiceSubscriptionIncome",
                        AssignedUserID = targetUserID,
                        ServiceID = 0,
                        OperatorID = 0,
                        TransactionDescription = $"Subscription income — plan {req.ServiceSubscriptionCreatePlanMasterID} from user {targetUserID}",
                        ReferenceID = req.ServiceSubscriptionCreatePlanMasterID,
                        TDS = 0,
                        GST = 0,
                        DiscountAmount = 0
                    });
                }
            }

            // Activate every service in the plan for the user.
            var existing = await _userServiceMappingService.GetByUserID(targetUserID);
            foreach (int serviceId in planServices)
            {
                var match = existing?.FirstOrDefault(m => m.ServiceID == serviceId);
                if (match == null)
                {
                    await _userServiceMappingService.Create(new FintechApp.Requests.UserServiceMapping.CreateUserServiceMappingRequest
                    {
                        UserID = targetUserID,
                        ServiceID = serviceId,
                        IsActive = true
                    });
                }
                else if (!match.IsActive)
                {
                    await _userServiceMappingService.Update(new FintechApp.Requests.UserServiceMapping.UpdateUserServiceMappingRequest
                    {
                        UserServiceMappingID = match.UserServiceMappingID,
                        UserID = targetUserID,
                        ServiceID = serviceId,
                        IsActive = true
                    });
                }
            }

            // Record the subscription / renewal so the validity window and expiry are tracked.
            await _plans.RecordSubscription(targetUserID, req.ServiceSubscriptionCreatePlanMasterID);

            return ResponseProcess.GetSuccessResponse(Messages.Subscription.Message.Success);
        }

        // ── Caller's subscription standing (header counter, banner, gating). Lazily locks
        //    the paid (plan-bundled) services when a required user's subscription has lapsed. ──
        [HttpPost("MyStatus")]
        public async Task<APIResponse> MyStatus([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = DecodeKey(apiRequest);
            if (error != null) return error;
            try
            {
                bool enabled = await IsSubscriptionEnabled();
                var status = await _plans.GetMyStatus(keyDataModel!.UserID, enabled, keyDataModel.UserTypeID);

                // Lapsed & required → lock the paid services (deactivate their mappings). Other
                // services stay active, so the rest of the platform keeps working.
                if (status.Required && !status.Active)
                {
                    var paid = (await _plans.GetAllPlanServiceIds()).ToHashSet();
                    var mappings = await _userServiceMappingService.GetByUserID(keyDataModel.UserID);
                    foreach (var m in (mappings ?? new()).Where(m => m.IsActive && paid.Contains(m.ServiceID)))
                    {
                        await _userServiceMappingService.Update(new FintechApp.Requests.UserServiceMapping.UpdateUserServiceMappingRequest
                        {
                            UserServiceMappingID = m.UserServiceMappingID,
                            UserID = keyDataModel.UserID,
                            ServiceID = m.ServiceID,
                            IsActive = false
                        });
                    }
                }
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(status)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Admin: who is subscribed / who is out ─────────────────────────────────────
        [HttpPost("UsersStatus")]
        public async Task<APIResponse> UsersStatus([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = DecodeKey(apiRequest);
            if (error != null) return error;
            if (keyDataModel!.UserTypeID != AdminUserTypeID) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
            try
            {
                var list = await _plans.GetUsersStatus();
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(list)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Admin: mark a user permanently free (cascades to their downline) ──────────
        [HttpPost("SetExempt")]
        public async Task<APIResponse> SetExempt([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<SetExemptRequest>(apiRequest);
            if (error != null) return error;
            if (keyDataModel!.UserTypeID != AdminUserTypeID) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
            if (req!.UserID <= 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            try
            {
                string result = await _plans.SetExempt(req.UserID, req.IsExempt);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(req.IsExempt ? "User marked as free." : "User moved back to paid.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ── Whether the caller is subscribed to a plan (all its services active) ──────
        [HttpPost("IsSubscribed")]
        public async Task<APIResponse> IsSubscribed([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<IsSubscribedRequest>(apiRequest);
            if (error != null) return error;

            var planServices = await _plans.GetServiceIdsForPlan(req!.ServiceSubscriptionCreatePlanMasterID);
            if (planServices.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            var mappings = await _userServiceMappingService.GetByUserID(keyDataModel!.UserID);
            bool isSubscribed = planServices.All(sid => mappings != null && mappings.Any(m => m.ServiceID == sid && m.IsActive));
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new { IsSubscribed = isSubscribed })));
        }

        // ── Is a specific service active for the caller (via UserServiceMapping) ──────
        [HttpPost("IsServiceAndEKYCActive")]
        public async Task<APIResponse> IsServiceAndEKYCActive([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, req, error) = Decode<SelectByServiceIDRequest>(apiRequest);
            if (error != null) return error;

            var mappings = await _userServiceMappingService.GetByUserID(keyDataModel!.UserID);
            bool isActive = mappings != null && mappings.Any(m => m.ServiceID == req!.ServiceID && m.IsActive);
            if (!isActive) return ResponseProcess.GetFailResponse("Service is not activated for this user.");
            return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new { IsActive = isActive })));
        }

        // ── Decode helpers ────────────────────────────────────────────────────────────
        private (KeyDataModel?, APIResponse?) DecodeKey(APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured)); }
            if (keyDataModel == null) return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, ResponseProcess.GetFailResponse(fail));
            return (keyDataModel, null);
        }

        private (KeyDataModel?, T?, APIResponse?) Decode<T>(APIRequest apiRequest) where T : class
        {
            KeyDataModel? keyDataModel;
            T? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<T>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured)); }
            if (keyDataModel == null || req == null) return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, null, ResponseProcess.GetFailResponse(fail));
            return (keyDataModel, req, null);
        }
    }
}
