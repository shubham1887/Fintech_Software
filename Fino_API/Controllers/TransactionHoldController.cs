using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Jobs;
using FintechApp.Requests;
using FintechApp.Requests.Hold;
using FintechApp.Responses;
using FintechApp.Responses.Hold;
using FintechApp.StaticModels;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The global money-transfer hold.
    ///
    /// With the hold ON, a DMT transfer is still validated, debited and commissioned — it is simply
    /// not handed to the upstream gateway, and sits as PENDING. With the hold OFF, every parked
    /// transfer is released through the normal DMT sequence (provider switching → dispatch →
    /// settlement), so turning the switch back off changes nothing about how a transfer is
    /// processed, only when. Used to stop money going out during a bank outage, a reconciliation
    /// window, or a suspected provider problem, without taking the service off the panel.
    ///
    /// Admin / Support only — this parks live customer money.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization(UserTypeCode.Admin, UserTypeCode.SupportUser)]
    public class TransactionHoldController : ControllerBase
    {
        private readonly ISettingMasterRepository _settingRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IBankTransferService _transfers;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TransactionHoldController(
            ISettingMasterRepository settingRepo,
            ITransactionMasterRepository txnRepo,
            IBankTransferService transfers,
            IConfiguration configuration)
        {
            _settingRepo = settingRepo;
            _txnRepo = txnRepo;
            _transfers = transfers;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>Current state of the DMT hold and what it is holding.</summary>
        [HttpPost("GetDMT")]
        public async Task<APIResponse> GetDMT([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(await ReadState())));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Turn the DMT hold on or off. Switching it OFF immediately enqueues the release job, so
        /// parked transfers start moving without waiting for the hourly safety-net run.
        /// </summary>
        [HttpPost("SetDMT")]
        public async Task<APIResponse> SetDMT([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            SetTransactionHoldRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<SetTransactionHoldRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                string remark = string.IsNullOrWhiteSpace(request.Remark)
                    ? $"{(request.IsOn ? "Held" : "Resumed")} by user {keyDataModel.UserID}"
                    : request.Remark.Trim();

                var setting = await _settingRepo.GetByKey(TransactionHold.DMTHoldSettingKey);
                if (setting == null)
                {
                    setting = new Entities.SettingMaster
                    {
                        Key = TransactionHold.DMTHoldSettingKey,
                        Value = request.IsOn ? "1" : "0",
                        IsOn = request.IsOn,
                        Description = remark,
                        DateTime = DateTime.Now,
                        UpdatedateTime = DateTime.Now,
                        IsDeleted = false
                    };
                    await _settingRepo.Insert(setting);
                }
                else
                {
                    setting.Value = request.IsOn ? "1" : "0";
                    setting.IsOn = request.IsOn;
                    setting.Description = remark;
                    setting.UpdatedateTime = DateTime.Now;
                    await _settingRepo.Update(setting);
                }

                // Releasing can take a while (one provider round-trip per parked transfer), so it
                // runs as a background job rather than on the admin's request thread.
                if (!request.IsOn) TryEnqueueRelease();

                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(await ReadState())));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Release the parked transfers now, without changing the switch. Useful when the hold has
        /// already been turned off and the operator does not want to wait for the queued job.
        /// </summary>
        [HttpPost("ReleaseDMT")]
        public async Task<APIResponse> ReleaseDMT([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var state = await ReadState();
            if (state.IsOn) return ResponseProcess.GetFailResponse("Turn the hold off before releasing the parked transfers.");
            if (state.HeldCount == 0) return ResponseProcess.GetFailResponse("There are no transfers on hold.");

            if (!TryEnqueueRelease())
                return ResponseProcess.GetFailResponse("The background job server is not available — try again once it is running.");

            return ResponseProcess.GetSuccessResponse($"Releasing {state.HeldCount} transfer(s) in the background.");
        }

        /// <summary>
        /// Release specific parked transfers now — the per-row / selected-rows action on the Pending
        /// DMT queue. Runs inline (rather than as a job) because the operator is asking about a
        /// handful of transfers and wants the answer on screen; the whole-queue release stays a
        /// background job. Works while the hold is still ON, so a single urgent transfer can be let
        /// through without lifting the hold for everyone.
        /// </summary>
        [HttpPost("ReleaseTransactions")]
        public async Task<APIResponse> ReleaseTransactions([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            List<ReleaseHeldTransferRequest>? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<List<ReleaseHeldTransferRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null || request.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            int released = 0;
            var problems = new List<string>();
            foreach (var item in request.Where(i => i.TransactionID > 0))
            {
                try
                {
                    var (itemFail, _) = await _transfers.ReleaseHeldTransfer(item.TransactionID, ip);
                    if (itemFail.EqualsIgnoreCase(StatusType.Ok)) released++;
                    else problems.Add($"#{item.TransactionID}: {itemFail}");
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                    problems.Add($"#{item.TransactionID}: {Messages.Common.Message.ExceptionOccured}");
                }
            }

            if (released == 0)
                return ResponseProcess.GetFailResponse(problems.Count > 0 ? string.Join(" ", problems) : "Nothing was released.");

            string message = $"Released {released} transfer(s).";
            if (problems.Count > 0) message += $" Not released — {string.Join(" ", problems)}";
            return ResponseProcess.GetSuccessResponse(message);
        }

        private async Task<TransactionHoldResponse> ReadState()
        {
            var setting = await _settingRepo.GetByKey(TransactionHold.DMTHoldSettingKey);
            var held = await _txnRepo.GetHeldDMT(int.MaxValue);
            return new TransactionHoldResponse
            {
                IsOn = setting?.IsOn ?? false,
                Remark = setting?.Description ?? string.Empty,
                UpdatedOn = setting?.UpdatedateTime,
                HeldCount = held.Count,
                HeldAmount = held.Sum(t => t.Amount),
            };
        }

        // Hangfire may not be configured (no connection string, storage unreachable). The switch
        // itself must still work in that case — the hourly recurring release, or a later restart,
        // picks the parked transfers up.
        private bool TryEnqueueRelease()
        {
            try
            {
                BackgroundJob.Enqueue<TransactionStatusCheckJob>(j => j.ReleaseHeldDmt());
                return true;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return false;
            }
        }
    }
}
