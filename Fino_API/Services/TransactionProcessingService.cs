using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionProcessing;
using FintechApp.Responses.TransactionMaster;
using FintechApp.Responses.TransactionProcessing;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TransactionProcessingService : ITransactionProcessingService
    {
        private const string StatusSuccess = "SUCCESS";
        private const string StatusFailure = "FAILED";
        private const string StatusPending = "PENDING";
        private const string StatusRefund = "REFUND";

        private readonly ITransactionMasterRepository _transactionRepository;
        private readonly ITransactionManualUpdateRepository _manualUpdateRepository;
        private readonly IUserWalletMasterRepository _walletRepository;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly FintechApp.Interfaces.Providers.ITransactionProviderClient _provider;
        private readonly IProviderCallBackLogRepository _callbackLogRepository;
        private readonly ICallbackDispatchService _callbackDispatch;
        private readonly IComplaintMasterRepository _complaintRepository;
        private readonly Logger _fileLogger = new();

        public TransactionProcessingService(
            ITransactionMasterRepository transactionRepository,
            ITransactionManualUpdateRepository manualUpdateRepository,
            IUserWalletMasterRepository walletRepository,
            IAccountCoreProcessingService accountCore,
            FintechApp.Interfaces.Providers.ITransactionProviderClient provider,
            IProviderCallBackLogRepository callbackLogRepository,
            ICallbackDispatchService callbackDispatch,
            IComplaintMasterRepository complaintRepository)
        {
            _complaintRepository = complaintRepository;
            _transactionRepository = transactionRepository;
            _manualUpdateRepository = manualUpdateRepository;
            _walletRepository = walletRepository;
            _accountCore = accountCore;
            _provider = provider;
            _callbackLogRepository = callbackLogRepository;
            _callbackDispatch = callbackDispatch;
        }

        public async Task<(string fail, TxnListWrapper data)> Select(TxnSelectRequest request, int currentUserID)
        {
            DateTime? fromDate = null, toDate = null;
            if (!string.IsNullOrEmpty(request.FromDate) && DateTime.TryParse(request.FromDate, out var fd)) fromDate = fd.Date;
            if (!string.IsNullOrEmpty(request.ToDate) && DateTime.TryParse(request.ToDate, out var td)) toDate = td.Date.AddDays(1).AddSeconds(-1);

            int uid = request.UserID > 0 ? request.UserID : currentUserID;
            var list = await _transactionRepository.GetAll(uid, request.Status, request.ServiceType, fromDate, toDate, request.PageNumber, request.PageSize);
            var wrapper = new TxnListWrapper
            {
                Count = list.Count,
                data = list.Select(MapToResponse).ToList(),
                ReportPath = string.Empty
            };
            return (StatusType.Ok, wrapper);
        }

        public async Task<(string fail, TransactionResponse? data)> SelectByID(TxnSelectByIDRequest request)
        {
            if (request.ID <= 0) return ("Invalid transaction ID.", null);
            var entity = await _transactionRepository.GetByID(request.ID);
            if (entity == null) return ("Transaction not found.", null);
            return (StatusType.Ok, MapToResponse(entity));
        }

        public async Task<string> Update(TxnUpdateStatusRequest request, int currentUserID)
        {
            if (request.TransactionID <= 0) return "Invalid transaction ID.";
            if (string.IsNullOrEmpty(request.Status)) return "Status is required.";

            var entity = await _transactionRepository.GetByID(request.TransactionID);
            if (entity == null) return "Transaction not found.";

            string oldStatus = entity.Status;
            string newStatus = request.Status.ToUpperInvariant();

            int rows = await _transactionRepository.UpdateStatus(request.TransactionID, newStatus, request.TxnID, request.Remark);
            if (rows <= 0) return Messages.Common.Message.ExceptionOccured;

            var log = new TransactionManualUpdate
            {
                TransactionMasterID = request.TransactionID,
                ManualUpdatedBy = currentUserID,
                ProviderrefNo = request.TxnID,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _manualUpdateRepository.Insert(log);

            return StatusType.Ok;
        }

        public async Task<string> UpdateManualSuccessFail(TxnUpdateStatusRequest request, int currentUserID)
        {
            if (request.TransactionID <= 0) return "Invalid transaction ID.";
            if (string.IsNullOrEmpty(request.Status)) return "Status is required.";

            string newStatus = request.Status.ToUpperInvariant();
            if (newStatus != StatusSuccess && newStatus != StatusFailure && newStatus != StatusRefund)
                return "Status must be SUCCESS, FAILED, or REFUND.";

            var entity = await _transactionRepository.GetByID(request.TransactionID);
            if (entity == null) return "Transaction not found.";

            string oldStatus = entity.Status;
            string remark = string.IsNullOrEmpty(request.Remark) ? "Manual status update by admin" : request.Remark;

            // Settle through the ported AccountProcessing engine (wallet + full commission reversal).
            if (newStatus == StatusRefund || newStatus == StatusFailure)
            {
                string r = await _accountCore.RefundTransaction(request.TransactionID, remark, DOCTypeConstants.Recharge);
                if (!r.EqualsIgnoreCase(StatusType.Ok)) return r;
            }
            else // SUCCESS
            {
                int sr = await _accountCore.SuccessTransaction(request.TransactionID, request.TxnID, remark);
                if (sr <= 0) return Messages.Common.Message.ExceptionOccured;
            }

            await LogManualUpdate(request.TransactionID, currentUserID, request.TxnID, oldStatus, newStatus);

            // A manual settlement is usually the answer to a complaint the customer raised on this
            // very transaction, so close that complaint from the same decision instead of leaving it
            // sitting in the queue for someone to notice and action twice.
            await CloseComplaintsForSettlement(request.TransactionID, newStatus, remark, currentUserID);
            return StatusType.Ok;
        }

        /// <summary>
        /// Settle every still-open complaint on a transaction that has just been settled by hand.
        ///
        /// Marking the transaction FAILED refunds the customer, which is exactly what accepting the
        /// complaint would have done — so the complaint is recorded as ACCEPTED. Marking it SUCCESS
        /// says the service was in fact delivered and no money is owed, so the complaint is recorded
        /// as REJECTED. Either way the money has already moved (or not) through the settlement above;
        /// this only records the outcome, and never calls the refund path again — which is what keeps
        /// a manual refund plus a complaint accept from paying the customer twice.
        ///
        /// Best-effort: a complaint bookkeeping failure must never fail the settlement itself.
        /// </summary>
        private async Task CloseComplaintsForSettlement(int txnID, string newStatus, string remark, int byUserID)
        {
            try
            {
                var open = (await _complaintRepository.GetAll())
                    .Where(c => c.TransactionMasterID == txnID && ComplaintStatus.IsOpen(c.ComplaintStatus))
                    .ToList();
                if (open.Count == 0) return;

                bool refunded = newStatus == StatusFailure || newStatus == StatusRefund;
                string outcome = refunded ? ComplaintStatus.Accept : ComplaintStatus.Reject;
                string note = refunded
                    ? $"[Auto] Transaction {txnID} was manually refunded by user {byUserID} — complaint accepted. {remark}".Trim()
                    : $"[Auto] Transaction {txnID} was manually marked SUCCESS by user {byUserID} — complaint rejected. {remark}".Trim();

                foreach (var complaint in open)
                {
                    complaint.ComplaintStatus = outcome;
                    complaint.AdminRemark = string.IsNullOrWhiteSpace(complaint.AdminRemark)
                        ? note
                        : $"{complaint.AdminRemark}\n{note}";
                    complaint.UpdatedateTime = DateTime.Now;
                    await _complaintRepository.Update(complaint);
                }
            }
            catch (Exception ex)
            {
                _fileLogger.Write(ex);
            }
        }

        // Shared audit log helper for manual status changes.
        private async Task LogManualUpdate(int txnID, int byUserID, string providerRefNo, string oldStatus, string newStatus)
        {
            await _manualUpdateRepository.Insert(new TransactionManualUpdate
            {
                TransactionMasterID = txnID,
                ManualUpdatedBy = byUserID,
                ProviderrefNo = providerRefNo ?? string.Empty,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
                IsDeleted = false
            });
        }

        public async Task<string> RechargeRefundAll(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkRefund(list, currentUserID, "Bulk recharge refund by admin");

        public async Task<string> CMSRefundAll(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkRefund(list, currentUserID, "Bulk CMS refund by admin");

        public async Task<string> DMTTransactionRefund(TxnBatchIDRequest request, int currentUserID)
        {
            var entity = await _transactionRepository.GetByID(request.TransactionID);
            if (entity == null) return "Transaction not found.";
            string oldStatus = entity.Status;

            string r = await _accountCore.RefundTransaction(request.TransactionID, "DMT refund by admin", DOCTypeConstants.Recharge);
            if (!r.EqualsIgnoreCase(StatusType.Ok)) return r;

            await LogManualUpdate(request.TransactionID, currentUserID, string.Empty, oldStatus, StatusFailure);
            await CloseComplaintsForSettlement(request.TransactionID, StatusFailure, "DMT refund by admin", currentUserID);
            return StatusType.Ok;
        }

        // Bulk refund of PENDING transactions through the full settlement engine.
        private async Task<string> BulkRefund(List<TxnBatchIDRequest> list, int currentUserID, string remark)
        {
            if (list == null || list.Count == 0) return "No transactions provided.";

            foreach (var item in list)
            {
                var entity = await _transactionRepository.GetByID(item.TransactionID);
                if (entity == null || entity.Status != StatusPending) continue;

                string oldStatus = entity.Status;
                string r = await _accountCore.RefundTransaction(item.TransactionID, remark, DOCTypeConstants.Recharge);
                if (r.EqualsIgnoreCase(StatusType.Ok))
                {
                    await LogManualUpdate(item.TransactionID, currentUserID, string.Empty, oldStatus, StatusFailure);
                    // Same rule as a one-off manual settlement: refunding answers the dispute, so
                    // the complaint is closed here rather than left in the queue. Settling in bulk
                    // must not behave differently from settling one row at a time.
                    await CloseComplaintsForSettlement(item.TransactionID, StatusFailure, remark, currentUserID);
                }
            }

            return StatusType.Ok;
        }

        // ─── Manual status settlement (ported from old TransactionBal, now AccountProcessing-backed) ──

        public async Task<string> UpdateStatusManually(TxnUpdateStatusRequest request, int currentUserID)
            => await UpdateManualSuccessFail(request, currentUserID);

        public async Task<string> UpdateStatusManuallyPendingProvider(TxnUpdateStatusRequest request, int currentUserID)
        {
            // Same settlement path; intended for transactions stuck in a provider-pending state.
            return await UpdateManualSuccessFail(request, currentUserID);
        }

        public async Task<string> RefundAll(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkRefund(list, currentUserID, "Bulk refund (all pending) by admin");

        public async Task<string> RefundAllProviderPending(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkRefund(list, currentUserID, "Bulk refund (provider pending) by admin");

        public async Task<string> SuccessAll(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkSuccess(list, currentUserID, "Bulk success (all pending) by admin");

        public async Task<string> SuccessAllProviderPending(List<TxnBatchIDRequest> list, int currentUserID)
            => await BulkSuccess(list, currentUserID, "Bulk success (provider pending) by admin");

        // Bulk mark PENDING transactions as SUCCESS (wallet already debited at initiation).
        private async Task<string> BulkSuccess(List<TxnBatchIDRequest> list, int currentUserID, string remark)
        {
            if (list == null || list.Count == 0) return "No transactions provided.";

            foreach (var item in list)
            {
                var entity = await _transactionRepository.GetByID(item.TransactionID);
                if (entity == null || entity.Status != StatusPending) continue;

                string oldStatus = entity.Status;
                int sr = await _accountCore.SuccessTransaction(item.TransactionID, string.Empty, remark);
                if (sr > 0)
                {
                    await LogManualUpdate(item.TransactionID, currentUserID, string.Empty, oldStatus, StatusSuccess);
                    // Marking it SUCCESS says the service was delivered and nothing is owed, so any
                    // complaint on it is recorded as rejected — same decision as the single-row path.
                    await CloseComplaintsForSettlement(item.TransactionID, StatusSuccess, remark, currentUserID);
                }
            }

            return StatusType.Ok;
        }

        public async Task<(string fail, List<ProviderWisePerformanceViewModel> data)> AllProviderWisePerformance(TxnProviderWiseRequest request)
        {
            DateTime? fromDate = null, toDate = null;
            if (!string.IsNullOrEmpty(request.FromDate) && DateTime.TryParse(request.FromDate, out var fd)) fromDate = fd.Date;
            if (!string.IsNullOrEmpty(request.ToDate) && DateTime.TryParse(request.ToDate, out var td)) toDate = td.Date.AddDays(1).AddSeconds(-1);

            var all = await _transactionRepository.GetAll(0, string.Empty, request.ServiceType, fromDate, toDate, 1, int.MaxValue);

            var grouped = all.GroupBy(t => t.ProviderID).Select(g =>
            {
                var total = g.Count();
                var success = g.Count(t => t.Status == StatusSuccess);
                var failed = g.Count(t => t.Status == StatusFailure);
                var pending = g.Count(t => t.Status == StatusPending);
                var refund = g.Count(t => t.Status == StatusRefund);
                var totalAmt = g.Sum(t => t.Amount);
                var successAmt = g.Where(t => t.Status == StatusSuccess).Sum(t => t.Amount);
                return new ProviderWisePerformanceViewModel
                {
                    ProviderID = g.Key,
                    TotalCount = total,
                    SuccessCount = success,
                    FailureCount = failed,
                    PendingCount = pending,
                    RefundCount = refund,
                    TotalAmount = totalAmt,
                    SuccessAmount = successAmt,
                    SuccessRate = total > 0 ? Math.Round((double)success / total * 100, 2) : 0
                };
            }).ToList();

            return (StatusType.Ok, grouped);
        }

        // ─── Read-only analytics (ported from TransactionDAL, no provider dependency) ──

        // Processing-time distribution of successful transactions (seconds → count; 60 = 60+).
        public async Task<List<HourlyPerformanceViewModel>> HourlyPerformance()
        {
            var all = await _transactionRepository.GetAll(0, StatusSuccess, string.Empty, null, null, 1, int.MaxValue);

            var buckets = all
                .Where(t => t.UpdatedateTime > t.DateTime)
                .Select(t =>
                {
                    double secs = Math.Round((t.UpdatedateTime - t.DateTime).TotalSeconds, 0);
                    return secs > 60 ? 60 : secs;
                })
                .GroupBy(s => s)
                .Select(g => new HourlyPerformanceViewModel { ProcTime = g.Key, Count = g.Count() })
                .OrderBy(x => x.ProcTime)
                .ToList();

            return buckets;
        }

        // Average processing time of successful transactions over the last 12 hours, in 2-hour intervals.
        public async Task<List<Last12hrPerformanceViewModel>> Last12hrPerformance()
        {
            DateTime endHour = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            DateTime start = endHour.AddHours(-12);

            var all = await _transactionRepository.GetAll(0, StatusSuccess, string.Empty, start, endHour, 1, int.MaxValue);
            return BuildIntervalPerformance(all, start);
        }

        // Average processing time for a single provider on a given date, in 2-hour intervals.
        public async Task<List<Last12hrPerformanceViewModel>> ProviderWisePerformance(TxnSingleProviderPerfRequest request)
        {
            if (!DateTime.TryParse(request.Date, out DateTime selected))
                return new List<Last12hrPerformanceViewModel>();

            DateTime start = selected.Date;
            DateTime end = start.AddDays(1).AddSeconds(-1);

            var all = await _transactionRepository.GetAll(0, StatusSuccess, string.Empty, start, end, 1, int.MaxValue);
            var providerTxns = all.Where(t => t.ProviderID == request.ProviderID).ToList();
            return BuildIntervalPerformance(providerTxns, start);
        }

        // Groups successful transactions into 2-hour intervals from `start` and averages processing time.
        private static List<Last12hrPerformanceViewModel> BuildIntervalPerformance(List<TransactionMaster> txns, DateTime start)
        {
            return txns
                .Where(t => t.UpdatedateTime > t.DateTime)
                .Select(t => new
                {
                    Bucket = (int)((t.DateTime - start).TotalHours) / 2,
                    ProcTime = Math.Round((t.UpdatedateTime - t.DateTime).TotalSeconds, 2)
                })
                .GroupBy(x => x.Bucket)
                .OrderBy(g => g.Key)
                .Select(g => new Last12hrPerformanceViewModel
                {
                    IntervalStart = start.AddHours(g.Key * 2).ToString("hh:mm tt"),
                    IntervalEnd = start.AddHours((g.Key + 1) * 2).ToString("hh:mm tt"),
                    AvgProcTime = Math.Round(g.Average(x => x.ProcTime), 2),
                    Count = g.Count()
                })
                .ToList();
        }

        // ─── Provider status-check / resend (provider-seam backed; settles locally) ──────

        public Task<string> TransactionResend(TxnSelectByIDRequest request, int currentUserID)
            => SettleFromProvider(request.ID, "RECHARGE", currentUserID, string.Empty, isResend: true);

        public Task<string> TransactionResendForSingleProvider(TxnDMTResendRequest request, int currentUserID, string ipAddress)
            => SettleFromProvider(request.TransactionID, "RECHARGE", currentUserID, ipAddress, isResend: true);

        public Task<string> DMTTransactionResend(TxnDMTResendRequest request, int currentUserID, string ipAddress)
            => SettleFromProvider(request.TransactionID, "DMT", currentUserID, ipAddress, isResend: true);

        public async Task<string> DMTTransactionResendAll(List<TxnDMTResendRequest> list, int currentUserID, string ipAddress)
        {
            if (list == null || list.Count == 0) return "No transactions provided.";
            foreach (var item in list)
                await SettleFromProvider(item.TransactionID, "DMT", currentUserID, ipAddress, isResend: true);
            return StatusType.Ok;
        }

        public async Task<string> DMTCMSTransactionResendAll(List<TxnDMTResendRequest> list, int currentUserID, string ipAddress)
        {
            if (list == null || list.Count == 0) return "No transactions provided.";
            foreach (var item in list)
                await SettleFromProvider(item.TransactionID, "DMT-CMS", currentUserID, ipAddress, isResend: true);
            return StatusType.Ok;
        }

        public Task<string> GetAEPSTransactionStatus(TxnSelectByIDRequest request, int currentUserID, string ipAddress)
            => SettleFromProvider(request.ID, "AEPS", currentUserID, ipAddress, isResend: false);

        public Task<string> GetBBPSTransactionStatus(TxnSelectByIDRequest request, int currentUserID)
            => SettleFromProvider(request.ID, "BBPS", currentUserID, string.Empty, isResend: false);

        public Task<string> GetTransactionStatus(TxnSelectByIDRequest request, int currentUserID)
            => SettleFromProvider(request.ID, "RECHARGE", currentUserID, string.Empty, isResend: false);

        public Task<string> CheckProviderStatus(int transactionID, string serviceType, int currentUserID, string ipAddress)
            => SettleFromProvider(transactionID, serviceType, currentUserID, ipAddress, isResend: false);

        public Task<string> ResendToProvider(int transactionID, string serviceType, int currentUserID, string ipAddress)
            => SettleFromProvider(transactionID, serviceType, currentUserID, ipAddress, isResend: true);

        // Shift: re-dispatch a stuck transaction through an admin-chosen provider rather than the
        // one the switching sequence picked. Same settlement path as a resend.
        public Task<string> ShiftToProvider(int transactionID, int providerID, string serviceType, int currentUserID, string ipAddress)
        {
            if (providerID <= 0) return Task.FromResult("Select a provider to shift to.");
            return SettleFromProvider(transactionID, serviceType, currentUserID, ipAddress, isResend: true, forceProviderID: providerID);
        }

        public Task<string> ShiftToProviderBatch(List<TxnShiftRequest> list, string serviceType, int currentUserID, string ipAddress)
        {
            if (list == null || list.Count == 0) return Task.FromResult("No transactions provided.");
            // A ProviderID of 0 is not "use the default" here - the whole point of a shift is the
            // named provider - so those rows are reported as skipped rather than silently resent
            // down the sequence they were already stuck in.
            if (list.All(x => x.ProviderID <= 0)) return Task.FromResult("Select a provider to shift to.");

            return RunBatch(
                list.Where(x => x.ProviderID > 0).Select(x => (x.TransactionID, x.ProviderID)),
                serviceType, currentUserID, ipAddress, isResend: true, verb: "sent");
        }

        public Task<string> CheckProviderStatusBatch(List<TxnBatchIDRequest> list, string serviceType, int currentUserID, string ipAddress)
        {
            if (list == null || list.Count == 0) return Task.FromResult("No transactions provided.");
            return RunBatch(
                list.Select(x => (x.TransactionID, ForceProviderID: 0)),
                serviceType, currentUserID, ipAddress, isResend: false, verb: "checked");
        }

        public Task<string> ResendToProviderBatch(List<TxnBatchIDRequest> list, string serviceType, int currentUserID, string ipAddress)
        {
            if (list == null || list.Count == 0) return Task.FromResult("No transactions provided.");
            return RunBatch(
                list.Select(x => (x.TransactionID, ForceProviderID: 0)),
                serviceType, currentUserID, ipAddress, isResend: true, verb: "sent");
        }

        /// <summary>
        /// Run one provider call per transaction and report what actually happened.
        ///
        /// Every batch on the pending queue used to discard its per-item results and answer
        /// StatusType.Ok unconditionally, so "Selected recharges shifted to the provider" was
        /// printed whether all of them had shifted or none had. That is the worst answer this
        /// screen can give: the operator moves on believing stuck money has been re-dispatched,
        /// and the rows are still sitting there the next morning.
        ///
        /// A batch where every item succeeded still answers Ok and the screen says so. Anything
        /// else answers with the counts and the distinct reasons, which the queue shows verbatim.
        /// The usual cause - one provider applied to rows spanning several operators, where it is
        /// only routed for some of them - is named exactly by the message that comes back.
        /// </summary>
        private async Task<string> RunBatch(
            IEnumerable<(int TransactionID, int ForceProviderID)> items,
            string serviceType, int currentUserID, string ipAddress, bool isResend, string verb)
        {
            // Materialised once: `items` arrives as a LINQ projection, and counting it after the
            // loop would enumerate it a second time.
            var work = items.ToList();
            if (work.Count == 0) return "No transactions provided.";

            int ok = 0;
            var reasons = new List<string>();

            foreach (var item in work)
            {
                string result;
                try
                {
                    result = await SettleFromProvider(
                        item.TransactionID, serviceType, currentUserID, ipAddress, isResend,
                        forceProviderID: item.ForceProviderID);
                }
                catch (Exception ex)
                {
                    // One bad transaction must not abandon the rest of the batch half-done.
                    _fileLogger.Write(ex);
                    result = Messages.Common.Message.ExceptionOccured;
                }

                if (result.EqualsIgnoreCase(StatusType.Ok)) ok++;
                else if (!reasons.Contains(result)) reasons.Add(result);
            }

            int failed = work.Count - ok;
            if (failed == 0) return StatusType.Ok;

            // Two reasons at most: the point is to name the cause, not to print a log.
            string why = string.Join(" ", reasons.Take(2));
            return ok == 0
                ? $"None could be {verb}. {why}"
                : $"{ok} {verb}, {failed} could not be. {why}";
        }


        // Shared provider status/resend → local settlement. Calls the provider seam, and if it
        // returns a definitive status, finalises (SUCCESS) or refunds (FAILED) the local transaction.
        private async Task<string> SettleFromProvider(int txnID, string serviceType, int userID, string ipAddress, bool isResend, int forceProviderID = 0)
        {
            if (txnID <= 0) return "Invalid transaction ID.";
            var txn = await _transactionRepository.GetByID(txnID);
            if (txn == null) return "Transaction not found.";

            var ctx = new FintechApp.Interfaces.Providers.ProviderStatusContext
            {
                TransactionMasterID = txn.TransactionMasterID,
                ProviderID = forceProviderID > 0 ? forceProviderID : txn.ProviderID,
                ProviderRequestID = txn.ProviderRequestID,
                ServiceID = txn.ServiceID,
                ServiceType = serviceType,
                UserID = userID,
                IPAddress = ipAddress,
                ForceProviderID = forceProviderID
            };

            var result = isResend ? await _provider.Resend(ctx) : await _provider.CheckStatus(ctx);
            if (!result.IsConfigured) return result.Message;

            string status = result.Status.ToUpperInvariant();
            string providerMessage = (result.Message ?? string.Empty).Trim();
            if (status == StatusSuccess && txn.Status.EqualsIgnoreCase(StatusPending))
            {
                await _accountCore.SuccessTransaction(txnID, result.ProviderRefNo,
                    providerMessage.Length > 0 ? providerMessage : $"{serviceType} provider status: success");
                await NotifyApiUser(txn);
            }
            else if (status == StatusFailure && !txn.Status.EqualsIgnoreCase(StatusFailure))
            {
                await _accountCore.RefundTransaction(txnID,
                    providerMessage.Length > 0 ? providerMessage : $"{serviceType} provider status: failed", DOCTypeConstants.Recharge);
                await NotifyApiUser(txn);
            }

            return StatusType.Ok;
        }

        // Provider push callback. Payin (and compatible upstreams) notify us via a GET once a recharge
        // moves from PROCESS to a definitive SUCCESS/FAILURE. We match the callback RequestID back to our
        // transaction and settle it through the same account pipeline as the status-check sweep — so a
        // recharge no longer has to wait for the next 5-minute poll to finalise. Idempotent: a duplicate
        // or late callback for an already-settled transaction is acknowledged and ignored.
        public async Task<string> ApplyRechargeCallback(RechargeCallbackRequest request, string ipAddress)
        {
            string raw = $"RequestID={request.RequestID}&ReferenceNo={request.ReferenceNo}&Status={request.Status}" +
                         $"&Number={request.Number}&Amount={request.Amount}&Balance={request.Balance}" +
                         $"&Reversal={(request.IsReversal ? 1 : 0)}&Description={request.Description}&IP={ipAddress}";

            if (string.IsNullOrWhiteSpace(request.RequestID) || string.IsNullOrWhiteSpace(request.Status))
            {
                await LogCallback(0, 0, $"INVALID {raw}");
                return "Invalid callback: RequestID and Status are required.";
            }

            var txn = await _transactionRepository.GetByProviderRequestIDLoose(request.RequestID);
            if (txn == null)
            {
                await LogCallback(0, 0, $"NOTFOUND {raw}");
                return "Transaction not found for RequestID.";
            }

            await LogCallback(txn.TransactionMasterID, txn.ProviderID, raw);

            string status = request.Status.Trim().ToUpperInvariant();
            string reference = string.IsNullOrWhiteSpace(request.ReferenceNo) ? txn.ProviderRefNo : request.ReferenceNo;
            string providerNote = (request.Description ?? string.Empty).Trim();
            string message = $"Provider callback: {status}. Operator Ref: {reference}" +
                             (providerNote.Length > 0 ? $". {providerNote}" : string.Empty);

            // REVERSAL - the provider is taking back a transaction it had already confirmed and has
            // credited the money back to us upstream. This is the ONLY path allowed to refund a
            // SUCCESS transaction: the retailer's wallet and the commission chain must follow.
            if (request.IsReversal)
            {
                if (txn.Status.EqualsIgnoreCase(StatusFailure) || txn.Status.EqualsIgnoreCase(StatusRefund))
                    return $"Ignored: transaction already {txn.Status}.";
                await _accountCore.RefundTransaction(txn.TransactionMasterID,
                    $"Provider reversal. {message}", DOCTypeConstants.Recharge);
                await NotifyApiUser(txn);
                return StatusType.Ok;
            }

            // SUCCESS — finalise only a still-open (PENDING) transaction.
            if (status == StatusSuccess)
            {
                if (!txn.Status.EqualsIgnoreCase(StatusPending))
                    return $"Ignored: transaction already {txn.Status}.";
                await _accountCore.SuccessTransaction(txn.TransactionMasterID, reference, message);
                await NotifyApiUser(txn);
                return StatusType.Ok;
            }

            // FAILURE — refund unless already terminal.
            if (status.StartsWith("FAIL"))
            {
                if (txn.Status.EqualsIgnoreCase(StatusSuccess))
                    return "Ignored: transaction already SUCCESS.";
                if (txn.Status.EqualsIgnoreCase(StatusFailure) || txn.Status.EqualsIgnoreCase(StatusRefund))
                    return $"Ignored: transaction already {txn.Status}.";
                await _accountCore.RefundTransaction(txn.TransactionMasterID, message, DOCTypeConstants.Recharge);
                await NotifyApiUser(txn);
                return StatusType.Ok;
            }

            // PROCESS / PENDING / unknown — nothing definitive to settle yet.
            return $"Acknowledged: status {status}.";
        }

        // Forward the settled result to the api-user's configured callback URL. Only the
        // transaction id is passed: the dispatcher re-reads the row it was just written to, so
        // the pushed payload is the freshly settled status, reference and balance — never the
        // stale copy this method is holding — and it is built through the same contract builder
        // the synchronous /api/v1 reply uses. No-op for non-API transactions or when no callback
        // URL is configured.
        private async Task NotifyApiUser(FintechApp.Entities.TransactionMaster txn)
        {
            if (string.IsNullOrWhiteSpace(txn.APIUserRequestID)) return;
            await _callbackDispatch.DispatchForTransaction(txn.TransactionMasterID);
        }

        private async Task LogCallback(int txnID, int providerID, string reply)
        {
            try
            {
                await _callbackLogRepository.Insert(new Entities.ProviderCallBackLog
                {
                    TransactionMasterID = txnID,
                    ProviderID = providerID,
                    Type = "RECHARGE",
                    ProviderReply = reply.Length > 4000 ? reply.Substring(0, 4000) : reply,
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now
                });
            }
            catch (Exception ex) { _fileLogger.Write(ex); }
        }

        // ─── Private helpers ─────────────────────────────────────────────────────────

        private async Task CreditWalletBack(FintechApp.Entities.TransactionMaster txn)
        {
            if (txn.DebitAmount <= 0) return;
            var wallets = await _walletRepository.GetByUserID(txn.UserID);
            var wallet = wallets.FirstOrDefault(w => w.WalletID == txn.WalletID);
            if (wallet == null) return;
            wallet.Balance += txn.DebitAmount;
            await _walletRepository.Update(wallet);
            // Commission reversal for RetailerCommission/DistributerCommission/AdminCommission
            // requires AccountProcessing.CreditBalance integration — not yet ported.
        }

        private static TransactionResponse MapToResponse(FintechApp.Entities.TransactionMaster x) => new()
        {
            TransactionMasterID = x.TransactionMasterID,
            UserID = x.UserID,
            ServiceID = x.ServiceID,
            OperatorID = x.OperatorID,
            WalletID = x.WalletID,
            ProviderID = x.ProviderID,
            DistributerUserID = x.DistributerUserID,
            SuperDistributerUserID = x.SuperDistributerUserID,
            ConsumerNumber = x.ConsumerNumber,
            Amount = x.Amount,
            DebitAmount = x.DebitAmount,
            OpeningBalance = x.OpeningBalance,
            ClosingBalance = x.ClosingBalance,
            Status = x.Status,
            Description = x.Description,
            ProviderRequestID = x.ProviderRequestID,
            ReferenceNo = x.ReferenceNo,
            APIUserRequestID = x.APIUserRequestID,
            ProviderDOCNo = x.ProviderDOCNo,
            ProviderRefNo = x.ProviderRefNo,
            RetailerCommission = x.RetailerCommission,
            DistributerCommission = x.DistributerCommission,
            SuperDistributerCommission = x.SuperDistributerCommission,
            AdminCommission = x.AdminCommission,
            ProviderCommission = x.ProviderCommission,
            RetailerTDS = x.RetailerTDS,
            DistributerTDS = x.DistributerTDS,
            SuperDistributerTDS = x.SuperDistributerTDS,
            AdminGST = x.AdminGST,
            RetailerGST = x.RetailerGST,
            DistributerGST = x.DistributerGST,
            SuperDistributerGST = x.SuperDistributerGST,
            CashBack = x.CashBack,
            NetCommission = x.NetCommission,
            Charge = x.Charge,
            AdminProfit = x.AdminProfit,
            DMT_SenderID = x.DMT_SenderID,
            DMT_SenderName = x.DMT_SenderName,
            DMT_SenderNumber = x.DMT_SenderNumber,
            DMT_BeneficiaryID = x.DMT_BeneficiaryID,
            DMT_BeneficiaryName = x.DMT_BeneficiaryName,
            DMT_Bank = x.DMT_Bank,
            DMT_IFSC = x.DMT_IFSC,
            DMT_BeneficaryNameByBank = x.DMT_BeneficaryNameByBank,
            DMT_PaymentType = x.DMT_PaymentType,
            DMT_OriginalAmount = x.DMT_OriginalAmount,
            ErrorCode = x.ErrorCode,
            Location = x.Location,
            Through = x.Through,
            IPAddress = x.IPAddress,
            Circle = x.Circle,
            ComplaintStatus = x.ComplaintStatus,
            CardType = x.CardType,
            ChangeStatus = x.ChangeStatus,
            PendingProviderStatus = x.PendingProviderStatus,
            ResponseType = x.ResponseType,
            BBPS_CustomerName = x.BBPS_CustomerName,
            IsDispute = x.IsDispute,
            IsROffer = x.IsROffer,
            IsOnlineDMT = x.IsOnlineDMT,
            IsCCF = x.IsCCF,
            SettlementAtProviderEnd = x.SettlementAtProviderEnd,
            HitBackCount = x.HitBackCount,
            StateID = x.StateID,
            DateTime = x.DateTime,
            UpdatedateTime = x.UpdatedateTime
        };
    }
}
