using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionReport;
using FintechApp.Responses.TransactionMaster;
using FintechApp.Responses.TransactionReport;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class TransactionReportService : ITransactionReportService
    {
        // Reports that let the caller pass a target UserID must scope non-admins to themselves,
        // otherwise a retailer could read another user's transactions (IDOR). Admin/Support may
        // view any user; everyone else is forced to their own id regardless of the request body.
        private static int ScopeUser(int requestedUserId, int currentUserID, int currentUserTypeID)
        {
            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            if (isAdmin) return requestedUserId > 0 ? requestedUserId : currentUserID;
            return currentUserID;
        }

        private const string StatusSuccess = "SUCCESS";
        private const string StatusFailed = "FAILED";
        private const string StatusPending = "PENDING";
        private const string StatusRefund = "REFUND";

        private readonly ITransactionMasterRepository _txnRepo;
        private readonly ITransactionManualUpdateRepository _manualRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IUserRepository _userRepo;
        private readonly IComplaintMasterRepository _complaintRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly ITransactionDetailsRepository _txnDetailRepo;
        private readonly ISenderRepository _senderRepo;
        private readonly IProviderPurchaseRepository _providerPurchaseRepo;

        public TransactionReportService(
            ITransactionMasterRepository txnRepo,
            ITransactionManualUpdateRepository manualRepo,
            IServiceRepository serviceRepo,
            IOperatorRepository operatorRepo,
            IUserRepository userRepo,
            IComplaintMasterRepository complaintRepo,
            IWalletMasterRepository walletRepo,
            IProviderMasterRepository providerRepo,
            ITransactionDetailsRepository txnDetailRepo,
            ISenderRepository senderRepo,
            IProviderPurchaseRepository providerPurchaseRepo)
        {
            _txnRepo = txnRepo;
            _manualRepo = manualRepo;
            _serviceRepo = serviceRepo;
            _operatorRepo = operatorRepo;
            _userRepo = userRepo;
            _complaintRepo = complaintRepo;
            _walletRepo = walletRepo;
            _providerRepo = providerRepo;
            _txnDetailRepo = txnDetailRepo;
            _senderRepo = senderRepo;
            _providerPurchaseRepo = providerPurchaseRepo;
        }

        // ── ProviderSale ──────────────────────────────────────────────
        // Per-provider aggregation of transactions in the date range, reconciled against the
        // provider's own stock.
        //
        // Purchase, Opening, Closing and Difference used to be hard-coded to 0 — the whole right-hand
        // side of the report read as zeros, which is what made it look broken. Every one of them is
        // recoverable from data we already store:
        //   Purchase — the ProviderPurchase rows (the "Provider Purchases" screen), i.e. what we paid
        //              this provider to top its stock up during the period.
        //   Opening / Closing — the provider balance each dispatch reply carried back
        //              (TransactionMaster.ProviderOpeningBalance / ProviderClosingBalance). The first
        //              and last transaction in the period that actually reported a balance bound the
        //              period, so these are the provider's own figures, not ours.
        //   Difference — (Opening + Purchase - Sale) - Closing: what the provider's stock SHOULD have
        //              come to versus what it says it holds. Non-zero means the provider's books and
        //              ours disagree for the period, which is the number this report exists to show.
        // A provider whose replies carry no balance still reports honest zeros for those two columns
        // (and therefore no Difference) rather than a number invented from our side of the ledger.
        public async Task<(string fail, ProviderSaleWrapper data)> ProviderSale(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new ProviderSaleWrapper());

            int userId = ScopeUser(request.UserID, currentUserID, currentUserTypeID);
            // Admin/Support and the Accountant see every provider's sale; a scoped user sees only
            // their own transactions. ScopeUser is left alone on purpose — when the caller is
            // network-wide this passes 0 (everyone) and userId is unused, so widening the shared
            // helper would have changed every other report in this service for no reason.
            bool networkWide = ReportScope.IsNetworkWide(currentUserTypeID);
            // Network-wide means the caller MAY see everyone, not that they must: the screen's
            // Entity picker is there to narrow to one, and passing 0 unconditionally made it do
            // nothing. A scoped caller is still pinned to their own rows by ScopeUser.
            int scopeUser = networkWide ? (request.UserID > 0 ? request.UserID : 0) : userId;
            var txns = await _txnRepo.GetAll(scopeUser, string.Empty, string.Empty, from, to, 1, int.MaxValue);
            if (request.ProviderID > 0) txns = txns.Where(t => t.ProviderID == request.ProviderID).ToList();

            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);

            // Stock top-ups paid to each provider in the same period. Only the accounting desk may
            // see what the company pays its providers, so a scoped user gets sale figures with
            // Purchase left at 0.
            var purchaseByProvider = new Dictionary<int, decimal>();
            if (networkWide)
            {
                var purchases = (await _providerPurchaseRepo.GetAll()).AsEnumerable();
                if (from.HasValue) purchases = purchases.Where(p => p.PurchaseDate >= from.Value);
                if (to.HasValue) purchases = purchases.Where(p => p.PurchaseDate <= to.Value);
                purchaseByProvider = purchases
                    .GroupBy(p => p.ProviderID)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            }

            int id = 0;
            // A transaction that never reached a provider (ProviderID 0: refused before dispatch,
            // or still pending on a route that has not answered) is not a provider's sale. Grouped
            // in, it produced a row named "0" with a Purchase of nothing and a Sale of whatever
            // happened to be stuck that day — the "pending" line this report was reported for.
            var rows = txns.Where(t => t.ProviderID > 0 && providerNames.ContainsKey(t.ProviderID))
                .GroupBy(t => t.ProviderID).Select(g =>
            {
                var success = g.Where(x => x.Status == StatusSuccess).ToList();
                var failed = g.Where(x => x.Status == StatusFailed).ToList();

                // Bound the period by the transactions whose reply actually carried a provider
                // balance — a zero there means "not reported", not "the provider held nothing".
                var withBalance = g.Where(x => x.ProviderOpeningBalance > 0 || x.ProviderClosingBalance > 0)
                    .OrderBy(x => x.DateTime).ThenBy(x => x.TransactionMasterID).ToList();
                decimal opening = withBalance.Count > 0 ? withBalance[0].ProviderOpeningBalance : 0m;
                decimal closing = withBalance.Count > 0 ? withBalance[^1].ProviderClosingBalance : 0m;

                decimal sale = success.Sum(x => x.Amount);
                decimal purchase = purchaseByProvider.TryGetValue(g.Key, out var p) ? p : 0m;
                // Only meaningful once the provider has told us a balance at both ends.
                decimal difference = withBalance.Count > 0 ? (opening + purchase - sale) - closing : 0m;

                return new ProviderSaleViewModel
                {
                    ID = ++id,
                    ProvderName = providerNames.TryGetValue(g.Key, out var pn) ? pn : g.Key.ToString(),
                    Purchase = purchase,
                    Sale = sale,
                    Fail = failed.Sum(x => x.Amount),
                    RetailerCommission = success.Sum(x => x.RetailerCommission),
                    DistributerCommission = success.Sum(x => x.DistributerCommission),
                    SuperDistributerCommission = success.Sum(x => x.SuperDistributerCommission),
                    AdminCommission = success.Sum(x => x.AdminCommission),
                    OpeningBalance = opening,
                    ClosingBalance = closing,
                    Difference = difference,
                    // AdminCommission is the operator's NET on every settled transaction on both
                    // rails; AdminProfit is only stamped by the recharge pipeline, so summing it
                    // alone read 0 for every DMT provider.
                    Profit = success.Sum(x => x.AdminProfit != 0m ? x.AdminProfit : x.AdminCommission),
                };
            }).OrderBy(x => x.ProvderName).ToList();

            // Re-number after ordering so the serial matches what is displayed.
            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            var wrapper = new ProviderSaleWrapper { Count = rows.Count, list = rows };
            if (rows.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);
            return (string.Empty, wrapper);
        }

        // ── PurchaseReport ────────────────────────────────────────────
        // Provider purchases = how much we paid each provider, from the manual ProviderPurchase
        // entries (the "Provider Purchases" screen). Aggregated per provider for each period —
        // a month ("2026-08") or a day ("2026-08-25") depending on request.GroupBy. This is a
        // company-level (admin) figure, not a per-user credit/debit.
        public async Task<(string fail, PurchaseReportWrapper data)> PurchaseReport(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new PurchaseReportWrapper());

            // Provider payments are company-wide books: Admin / Support and the Accountant read
            // them, everyone else gets nothing. Returning DataNotFound rather than an error is
            // why this looked like an empty report instead of a refusal.
            if (!ReportScope.IsNetworkWide(currentUserTypeID))
                return (Messages.Common.Message.DataNotFound, new PurchaseReportWrapper());

            var purchases = (await _providerPurchaseRepo.GetAll()).AsEnumerable();
            if (from.HasValue) purchases = purchases.Where(p => p.PurchaseDate >= from.Value);
            if (to.HasValue) purchases = purchases.Where(p => p.PurchaseDate <= to.Value);
            if (request.ProviderID > 0) purchases = purchases.Where(p => p.ProviderID == request.ProviderID);

            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
            var walletNames = (await _walletRepo.SelectAll())
                .GroupBy(w => w.WalletID).ToDictionary(g => g.Key, g => g.First().Name);

            // "Date" → per-day rows; anything else (default) → per-month rows.
            bool byDate = request.GroupBy.Trim().Equals("Date", StringComparison.OrdinalIgnoreCase);
            string PeriodOf(DateTime d) => byDate ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM");

            int id = 0;
            var result = purchases
                .GroupBy(p => new { Period = PeriodOf(p.PurchaseDate), p.ProviderID })
                .Select(g => new PurchaseReportViewModel
                {
                    ID = ++id,
                    Period = g.Key.Period,
                    ProviderName = providerNames.TryGetValue(g.Key.ProviderID, out var pn) ? pn : g.Key.ProviderID.ToString(),
                    // One provider can be paid from more than one wallet in a period; name it only
                    // when the whole group shares a single wallet, otherwise show a combined marker.
                    WalletName = g.Select(x => x.WalletID).Distinct().Count() == 1
                        ? (walletNames.TryGetValue(g.First().WalletID, out var wn) ? wn : string.Empty)
                        : "Multiple",
                    Payments = g.Count(),
                    Amount = g.Sum(x => x.Amount),
                })
                .OrderByDescending(x => x.Period).ThenBy(x => x.ProviderName)
                .ToList();
            // Re-number after the final ordering so the serial matches the displayed order.
            for (int i = 0; i < result.Count; i++) result[i].ID = i + 1;

            var wrapper = new PurchaseReportWrapper { Count = result.Count, list = result };
            if (result.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);
            return (string.Empty, wrapper);
        }

        // ── SelectReport ──────────────────────────────────────────────
        public Task<(string fail, TxnReportWrapper data)> SelectReport(TxnReportRequest request, int currentUserID)
            => PagedReport(request, currentUserID, 0);

        // ── SelectSummary ─────────────────────────────────────────────
        public async Task<(string fail, TxnSummaryViewModel data)> SelectSummary(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnSummaryViewModel());

            var list = await _txnRepo.GetAll(currentUserID, string.Empty, string.Empty, from, to, 1, int.MaxValue);

            var vm = new TxnSummaryViewModel
            {
                TotalCount = list.Count,
                SuccessCount = list.Count(x => x.Status == StatusSuccess),
                FailureCount = list.Count(x => x.Status == StatusFailed),
                PendingCount = list.Count(x => x.Status == StatusPending),
                RefundCount = list.Count(x => x.Status == StatusRefund),
                TotalAmount = list.Sum(x => x.Amount),
                SuccessAmount = list.Where(x => x.Status == StatusSuccess).Sum(x => x.Amount),
                TotalCommission = list.Where(x => x.Status == StatusSuccess)
                    .Sum(x => x.RetailerCommission + x.DistributerCommission + x.SuperDistributerCommission + x.AdminCommission)
            };
            vm.SuccessRate = vm.TotalCount > 0 ? Math.Round((double)vm.SuccessCount / vm.TotalCount * 100, 2) : 0;
            return (string.Empty, vm);
        }

        // ── Service-kind reports (Recharge / AEPS / MicroATM / Cash Deposit) ──────────
        //
        // These used to call _txnRepo.GetAll(..., "AEPS", ...) — a parameter the repository has
        // never looked at. The result was that the AEPS and MicroATM reports listed EVERY
        // transaction of the user, which on a DMT-heavy install reads as "the AEPS report is
        // showing DMT data". They also asked for PageNumber/PageSize, which the web UI does not
        // send (it sends TakeCount/SkipCount), so the page-size picker did nothing either: 20
        // selected, 50 returned.
        //
        // Both are solved by routing them through the same FilterReport the DMT/BBPS/Payout
        // reports already use: it scopes by the SERVICES that belong to the kind, honours the
        // UI's paging, returns a true total count, and enriches the rows (entity name, operator,
        // provider) that BuildWrapper left blank.
        public Task<(string fail, TxnReportWrapper data)> Recharge(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.Recharge);

        public Task<(string fail, TxnReportWrapper data)> Prepaid(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.Recharge);

        public Task<(string fail, TxnReportWrapper data)> AEPS(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.AEPS);

        public Task<(string fail, TxnReportWrapper data)> MicroATM(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.MicroATM);

        public Task<(string fail, TxnReportWrapper data)> CashDeposit(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.CashDeposit);

        // ── LeadGeneration ────────────────────────────────────────────
        // No service kind maps to lead generation, so this stays an unscoped listing — but it
        // pages the way the UI asks, like every other report.
        public Task<(string fail, TxnReportWrapper data)> LeadGeneration(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => PagedReport(request, currentUserID, currentUserTypeID);

        // ── LiveTransaction ───────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> LiveTransaction(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(0, StatusPending, string.Empty, from, to, 1, 100);
            return (string.Empty, BuildWrapper(list));
        }

        // ── ManualSuccessFail ─────────────────────────────────────────
        // Transactions that were settled by hand. The date filter applies to WHEN THE SETTLEMENT
        // happened (the audit row), not when the transaction was created — that is the question
        // this report answers.
        public Task<(string fail, TxnReportWrapper data)> ManualSuccessFail(TxnReportRequest request, int currentUserID)
            => ManuallySettled(request, currentUserID, 0);

        // ── SelectPastSuccessFail ─────────────────────────────────────
        // Same source as ManualSuccessFail, exposed as the "Past Success/Fail" screen. Kept as a
        // separate entry point because that screen adds a Status filter and shows the settlement
        // timestamp / processing time columns.
        public Task<(string fail, TxnReportWrapper data)> SelectPastSuccessFail(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => ManuallySettled(request, currentUserID, currentUserTypeID);

        private async Task<(string fail, TxnReportWrapper data)> ManuallySettled(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var manualUpdates = await _manualRepo.GetAll();
            if (from.HasValue)
                manualUpdates = manualUpdates.Where(x => x.DateTime >= from.Value).ToList();
            if (to.HasValue)
                manualUpdates = manualUpdates.Where(x => x.DateTime <= to.Value).ToList();

            var txnIds = manualUpdates.Select(x => x.TransactionMasterID).Distinct().ToHashSet();
            if (txnIds.Count == 0) return (string.Empty, new TxnReportWrapper());

            // Admin/Support settle other people's transactions, so scoping the lookup to the
            // caller's own id would always come back empty for them. 0 = the whole network.
            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int scopeUserId = isAdmin ? ScopeUser(request.UserID, 0, currentUserTypeID) : currentUserID;

            var allTxns = await _txnRepo.GetAll(scopeUserId, request.Status, string.Empty, null, null, 1, int.MaxValue);
            var filtered = allTxns.Where(x => txnIds.Contains(x.TransactionMasterID))
                .OrderByDescending(x => x.UpdatedateTime)
                .ToList();

            // The screen's selection-criteria box, which it has always shown and this report has
            // always ignored. Applied before the count below, so the total matches the rows.
            filtered = await ApplyCriteria(filtered, request);

            var wrapper = new TxnReportWrapper { Count = filtered.Count };
            var paged = request.EffectiveTake > 0
                ? filtered.Skip(request.EffectiveSkip).Take(request.EffectiveTake).ToList()
                : filtered;
            wrapper.list = paged.Select(MapToResponse).ToList();
            await Enrich(wrapper.list); // resolve user/operator/provider/wallet names for display
            return (string.Empty, wrapper);
        }

        // ── SelectDMTPendingTransactionWithFilter ─────────────────────
        // The DMT pending queue: money-transfer transactions still awaiting a final status.
        public Task<(string fail, TxnReportWrapper data)> SelectDMTPendingTransactionWithFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            // The screen is defined by "still pending", so the status filter is forced rather
            // than taken from the request (which defaults to ALL).
            request.Status = StatusPending;
            return FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.DMT);
        }

        // ── LapuwiseReport ────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> LapuwiseReport(TxnReportRequest request, int currentUserID)
        {
            // Lapu (SIM card based) transaction detail requires a dedicated LapuTransactionDetail
            // repository which is not yet implemented. Return empty wrapper.
            await Task.CompletedTask;
            return ("Lapu transaction detail integration pending.", new TxnReportWrapper());
        }

        // ── SelectByUserWise ──────────────────────────────────────────
        public async Task<(string fail, List<UserWiseSummaryViewModel> data)> SelectByUserWise(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new List<UserWiseSummaryViewModel>());

            var list = await _txnRepo.GetAll(currentUserID, string.Empty, string.Empty, from, to, 1, int.MaxValue);

            var result = list.GroupBy(x => x.UserID)
                .Select(g =>
                {
                    int total = g.Count();
                    int success = g.Count(x => x.Status == StatusSuccess);
                    return new UserWiseSummaryViewModel
                    {
                        UserID = g.Key,
                        TotalCount = total,
                        SuccessCount = success,
                        FailureCount = g.Count(x => x.Status == StatusFailed),
                        PendingCount = g.Count(x => x.Status == StatusPending),
                        TotalAmount = g.Sum(x => x.Amount),
                        SuccessAmount = g.Where(x => x.Status == StatusSuccess).Sum(x => x.Amount),
                        Commission = g.Where(x => x.Status == StatusSuccess).Sum(x => x.RetailerCommission),
                        SuccessRate = total > 0 ? Math.Round((double)success / total * 100, 2) : 0
                    };
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return (string.Empty, result);
        }

        // ── SelectByUserWiseByServiceID ───────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> SelectByUserWiseByServiceID(TxnReportByServiceRequest request, int currentUserID, int currentUserTypeID = 0)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var (count, list) = await _txnRepo.GetFiltered(
                ScopeUser(request.UserID, currentUserID, currentUserTypeID), string.Empty, null,
                request.ServiceID, 0, from, to,
                Math.Max(0, (request.PageNumber - 1) * request.PageSize), request.PageSize);

            var wrapper = new TxnReportWrapper { Count = count, list = list.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);
            return (string.Empty, wrapper);
        }

        // ── SelectByUserWiseByTransactionMasterID ─────────────────────
        public async Task<(string fail, TxnReportWrapper data)> SelectByUserWiseByTransactionMasterID(TxnReportByServiceRequest request, int currentUserID)
        {
            if (request.TransactionMasterID <= 0)
                return ("TransactionMasterID is required.", new TxnReportWrapper());

            var txn = await _txnRepo.GetByID(request.TransactionMasterID);
            if (txn == null)
                return ("Transaction not found.", new TxnReportWrapper());

            // Return child/related transactions for same user + service
            var list = await _txnRepo.GetAll(txn.UserID, string.Empty, string.Empty, null, null, 1, int.MaxValue);
            if (request.ServiceID > 0)
                list = list.Where(x => x.ServiceID == request.ServiceID).ToList();

            return (string.Empty, BuildWrapper(list));
        }

        // ── SpecificUserReport ────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> SpecificUserReport(TxnReportRequest request, int currentUserID, int currentUserTypeID = 0)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            int targetUser = ScopeUser(request.UserID, currentUserID, currentUserTypeID);
            var (count, list) = await _txnRepo.GetFiltered(
                targetUser, request.Status, null, request.ServiceID, request.OperatorID,
                from, to, request.EffectiveSkip, request.ReadTake,
                await BuildTextFilter(request));
            var wrapper = new TxnReportWrapper { Count = count, list = list.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);
            return (string.Empty, wrapper);
        }

        // ── MonthlySummary ────────────────────────────────────────────
        public async Task<(string fail, List<MonthlySummaryViewModel> data)> MonthlySummary(TxnReportRequest request, int currentUserID)
        {
            var list = await _txnRepo.GetAll(currentUserID, string.Empty, string.Empty, null, null, 1, int.MaxValue);

            var result = list.GroupBy(x => new { x.DateTime.Year, x.DateTime.Month })
                .Select(g => new MonthlySummaryViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    TotalCount = g.Count(),
                    SuccessCount = g.Count(x => x.Status == StatusSuccess),
                    FailureCount = g.Count(x => x.Status == StatusFailed),
                    TotalAmount = g.Sum(x => x.Amount),
                    SuccessAmount = g.Where(x => x.Status == StatusSuccess).Sum(x => x.Amount),
                    Commission = g.Where(x => x.Status == StatusSuccess)
                        .Sum(x => x.RetailerCommission + x.DistributerCommission + x.SuperDistributerCommission + x.AdminCommission)
                })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .ToList();

            return (string.Empty, result);
        }

        // ── GSTsummary ────────────────────────────────────────────────
        public async Task<(string fail, List<GSTSummaryViewModel> data)> GSTsummary(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new List<GSTSummaryViewModel>());

            var list = await _txnRepo.GetAll(currentUserID, StatusSuccess, string.Empty, from, to, 1, int.MaxValue);

            var result = list.GroupBy(x => x.OperatorID)
                .Select(g => new GSTSummaryViewModel
                {
                    OperatorID = g.Key,
                    ServiceType = string.Empty, // Needs ServiceMaster join for ServiceCategory
                    Count = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount),
                    AdminGST = g.Sum(x => x.AdminGST),
                    RetailerGST = g.Sum(x => x.RetailerGST),
                    DistributerGST = g.Sum(x => x.DistributerGST),
                    SuperDistributerGST = g.Sum(x => x.SuperDistributerGST),
                    TotalGST = g.Sum(x => x.AdminGST + x.RetailerGST + x.DistributerGST + x.SuperDistributerGST)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return (string.Empty, result);
        }

        // ── GSTOperatorWise ───────────────────────────────────────────
        public async Task<(string fail, List<GSTSummaryViewModel> data)> GSTOperatorWise(TxnReportRequest request, int currentUserID)
        {
            // Same as GSTsummary — grouped by operator
            return await GSTsummary(request, currentUserID);
        }

        // ── Income ────────────────────────────────────────────────────
        // Commission earned per service for the period. Three things were wrong with it, and all
        // three were visible on the screen:
        //
        //  * It answered with field names the Income Report does not read (ServiceType holding a
        //    raw id, RetailerCommission rather than RCommission), so every column rendered empty.
        //  * It scoped to the CALLER only, ignoring the Entity picker - and since the admin has no
        //    transactions of their own, the admin's income report was empty by construction.
        //  * It ignored the Service dropdown beside it.
        //
        // Two more were found later, and both made the figure itself wrong rather than missing:
        //
        //  * It read only the caller's OWN transactions. A distributor earns on its RETAILERS'
        //    transactions and makes almost none of its own, so "Income Report" showed a
        //    distributor roughly nothing — the one tier whose entire income this report exists to
        //    state. The rows are the caller's whole NETWORK now (self + downline), which is what
        //    every other analytics report in this codebase already does (ReportAnalyticsService.
        //    ResolveScope).
        //
        //  * TotalIncome added up all four tiers: retailer + distributor + super distributor +
        //    admin. That is the commission the WHOLE CHAIN earned on those transactions, not the
        //    caller's income — a retailer's income report counted their distributor's cut, their
        //    super distributor's cut and the operator's margin as their own earnings, and printed
        //    the lot as "Total Income". It is now the caller's own tier, net of their own TDS,
        //    which is the number that actually reached their wallet. The tiers a caller may not
        //    see are zeroed in the response as well, because hiding a column in the browser does
        //    not take the figure out of the JSON.
        public async Task<(string fail, AnalyticsWrapper<IncomeSummaryViewModel> data)> Income(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<IncomeSummaryViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, wrapper);

            // Admin, support and the accounting desk read anyone's income, or the whole network
            // with UserID 0. Everyone else reads their own branch.
            bool isAdmin = ReportScope.IsNetworkWide(currentUserTypeID);

            // Which users' transactions count towards this report. null = every user.
            HashSet<int>? scope = null;
            if (isAdmin)
            {
                if (request.UserID > 0)
                {
                    // An admin asking about one entity means that entity's own income, so the
                    // branch beneath it counts too — a distributor's income IS their retailers'
                    // transactions.
                    scope = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), request.UserID);
                    scope.Add(request.UserID);
                }
            }
            else
            {
                scope = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), currentUserID);
                scope.Add(currentUserID);
                // An Entity picker set to someone in their own branch narrows it further; one set
                // to anybody else is ignored rather than honoured.
                if (request.UserID > 0 && scope.Contains(request.UserID))
                {
                    var narrowed = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), request.UserID);
                    narrowed.Add(request.UserID);
                    scope = narrowed;
                }
            }

            // A single-user scope is pushed into the query; a branch is filtered after, because the
            // transaction query takes one user id and not a set.
            int queryUserId = scope is { Count: 1 } ? scope.First() : 0;
            var list = await _txnRepo.GetAll(queryUserId, StatusSuccess, string.Empty, from, to, 1, int.MaxValue);
            if (scope != null && queryUserId == 0)
                list = list.Where(x => scope.Contains(x.UserID)).ToList();
            if (request.ServiceID > 0)
                list = list.Where(x => x.ServiceID == request.ServiceID).ToList();
            if (request.OperatorID > 0)
                list = list.Where(x => x.OperatorID == request.OperatorID).ToList();
            if (list.Count == 0) return (string.Empty, wrapper);

            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            var rows = list.GroupBy(x => x.ServiceID)
                .Select(g =>
                {
                    decimal retailerComm = g.Sum(x => x.RetailerCommission);
                    decimal distributerComm = g.Sum(x => x.DistributerCommission);
                    decimal superDistComm = g.Sum(x => x.SuperDistributerCommission);
                    decimal adminComm = g.Sum(x => x.AdminCommission);
                    decimal retailerTds = g.Sum(x => x.RetailerTDS);
                    decimal distributerTds = g.Sum(x => x.DistributerTDS);
                    decimal superDistTds = g.Sum(x => x.SuperDistributerTDS);

                    // The caller's OWN income: the tier they are paid at, net of the TDS withheld
                    // from it. Admin's income is its own margin on the chain.
                    decimal ownIncome = currentUserTypeID switch
                    {
                        UserTypeCode.SuperDistributor => superDistComm - superDistTds,
                        UserTypeCode.Distributor => distributerComm - distributerTds,
                        UserTypeCode.Retailer or UserTypeCode.ApiUser or UserTypeCode.Client
                            => retailerComm - retailerTds,
                        _ => adminComm, // admin / support / accountant read the operator's margin
                    };

                    return new IncomeSummaryViewModel
                    {
                        ServiceID = g.Key,
                        ServiceName = serviceNames.TryGetValue(g.Key, out var n) ? n : $"Service {g.Key}",
                        SuccessAmt = g.Sum(x => x.Amount),
                        SuccessCount = g.Count(),
                        // Tiers the caller may not read are zeroed rather than sent and hidden.
                        RCommission = CanReadTier(currentUserTypeID, UserTypeCode.Retailer) ? retailerComm : 0m,
                        DCommission = CanReadTier(currentUserTypeID, UserTypeCode.Distributor) ? distributerComm : 0m,
                        SCommission = CanReadTier(currentUserTypeID, UserTypeCode.SuperDistributor) ? superDistComm : 0m,
                        AdminCommission = isAdmin ? adminComm : 0m,
                        RetailerTDS = CanReadTier(currentUserTypeID, UserTypeCode.Retailer) ? retailerTds : 0m,
                        DistributerTDS = CanReadTier(currentUserTypeID, UserTypeCode.Distributor) ? distributerTds : 0m,
                        NetProfit = isAdmin ? g.Sum(x => x.AdminProfit) : 0m,
                        TotalIncome = ownIncome,
                        Count = g.Count(),
                    };
                })
                .OrderByDescending(x => x.TotalIncome)
                .ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Success Amount"] = rows.Sum(r => r.SuccessAmt),
                ["Success Count"] = rows.Sum(r => r.SuccessCount),
                // "My income" rather than "Total Income": it is the caller's own tier, net of
                // their own TDS, and the previous wording invited it to be read as the chain's.
                ["My Income"] = rows.Sum(r => r.TotalIncome),
                ["Net Profit"] = rows.Sum(r => r.NetProfit),
            };
            wrapper.list = request.IsExcelDownload || request.EffectiveTake <= 0
                ? rows
                : rows.Skip(request.EffectiveSkip).Take(request.EffectiveTake).ToList();
            return (string.Empty, wrapper);
        }

        /// <summary>
        /// May a caller of this user-type read what a given tier earns?
        ///
        /// Same rule as ReportAnalyticsService.CanSeeTier, stated here because the income report
        /// lives in this service: everyone sees their own tier, an upline sees the tiers it pays
        /// out to, nobody ever sees a tier above them. A retailer who learns the distributor's cut
        /// can work the operator's whole commission structure out from one screen.
        /// </summary>
        private static bool CanReadTier(int callerTypeId, int tierTypeId)
        {
            if (ReportScope.IsNetworkWide(callerTypeId) || callerTypeId == tierTypeId) return true;
            return callerTypeId switch
            {
                UserTypeCode.SuperDistributor => tierTypeId is UserTypeCode.Distributor or UserTypeCode.Retailer,
                UserTypeCode.Distributor => tierTypeId == UserTypeCode.Retailer,
                _ => false,
            };
        }

        // ── AmountWiseInsightsSelect ──────────────────────────────────
        public async Task<(string fail, List<AmountInsightViewModel> data)> AmountWiseInsightsSelect(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new List<AmountInsightViewModel>());

            var list = await _txnRepo.GetAll(currentUserID, string.Empty, string.Empty, from, to, 1, int.MaxValue);

            var ranges = new[]
            {
                ("0-100",    0m, 100m),
                ("101-500",  101m, 500m),
                ("501-1000", 501m, 1000m),
                ("1001-5000", 1001m, 5000m),
                ("5001+",    5001m, decimal.MaxValue)
            };

            var result = ranges.Select(r => new AmountInsightViewModel
            {
                Range = r.Item1,
                Count = list.Count(x => x.Amount >= r.Item2 && x.Amount <= r.Item3),
                TotalAmount = list.Where(x => x.Amount >= r.Item2 && x.Amount <= r.Item3).Sum(x => x.Amount),
                SuccessCount = list.Count(x => x.Amount >= r.Item2 && x.Amount <= r.Item3 && x.Status == StatusSuccess)
            }).ToList();

            return (string.Empty, result);
        }

        // ── StateWiseSummary ──────────────────────────────────────────
        public async Task<(string fail, List<StateWiseSummaryViewModel> data)> StateWiseSummary(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new List<StateWiseSummaryViewModel>());

            var list = await _txnRepo.GetAll(currentUserID, string.Empty, string.Empty, from, to, 1, int.MaxValue);

            var result = list.GroupBy(x => x.StateID)
                .Select(g =>
                {
                    int total = g.Count();
                    int success = g.Count(x => x.Status == StatusSuccess);
                    return new StateWiseSummaryViewModel
                    {
                        StateID = g.Key,
                        TotalCount = total,
                        SuccessCount = success,
                        TotalAmount = g.Sum(x => x.Amount),
                        SuccessAmount = g.Where(x => x.Status == StatusSuccess).Sum(x => x.Amount),
                        SuccessRate = total > 0 ? Math.Round((double)success / total * 100, 2) : 0
                    };
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return (string.Empty, result);
        }

        // ── Filter reports (web UI report screens) ────────────────────
        // Service-kind scoped report with the UI's Service/Operator/Status/date filters.
        // Admin/Support may query any user (request.UserID, 0 = everyone); others only themselves.
        public Task<(string fail, TxnReportWrapper data)> SelectForRechargeFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.Recharge);

        // Admin "Pending Recharge" queue — every recharge-family transaction still awaiting a final
        // outcome (Status PENDING or PROCESS), newest first, with the same Service/Operator/User
        // filters as the recharge report. This is the list the every-5-min status-check scheduler
        // works through in the background; the page lets an admin also act on them manually
        // (status-check / resend / success / refund).
        public Task<(string fail, TxnReportWrapper data)> SelectPendingTransactionWithFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            request.Status = StatusPending;   // force pending/process only (StatusSynonyms expands to PENDING+PROCESS)
            return FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.Recharge);
        }

        /// <summary>
        /// The pending queue for ANY service kind, named by <c>request.ServiceType</c> - a
        /// ServiceKinds constant (RECHARGE / DMT / BBPS / PAYOUT / AEPS / MATM / CASHDEPOSIT).
        ///
        /// Recharge and DMT had an endpoint each and every other service had none, so bill
        /// payments and electricity stuck in PENDING were invisible: the only screen that listed
        /// them was the ordinary report, which has no status-check or settle action on it. Adding
        /// the next service used to mean another endpoint, another screen and another menu row;
        /// now it means a ServiceMaster row, which the admin creates themselves.
        ///
        /// A blank or unrecognised kind means "every service", which is the all-queues view - and
        /// the safe reading of a kind this build has not heard of, since an empty queue would say
        /// the work is done when nobody has looked.
        /// </summary>
        public Task<(string fail, TxnReportWrapper data)> SelectPendingByKind(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            request.Status = StatusPending;

            string kind = (request.ServiceType ?? string.Empty).Trim();
            bool known = kind.Length > 0 && ServiceKinds.All
                .Any(k => k != ServiceKinds.None && k.EqualsIgnoreCase(kind));

            return known
                ? FilterReport(request, currentUserID, currentUserTypeID, kind.ToUpperInvariant())
                : PagedReport(request, currentUserID, currentUserTypeID);
        }

        /// <summary>
        /// One row per service kind that has something configured behind it, with how many
        /// transactions are waiting and how long the oldest has been waiting.
        ///
        /// This is what makes the queues dynamic rather than a fixed pair: the screen asks which
        /// queues exist instead of being written to know. A kind with no service configured is
        /// left out entirely - an empty "Electricity" queue on an install that does not sell
        /// electricity is noise - but a kind that IS configured is listed even at zero, because
        /// "nothing is stuck" is the answer the operator came for.
        /// </summary>
        public async Task<(string fail, PendingQueueSummary data)> SelectPendingQueues(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var summary = new PendingQueueSummary();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, summary);

            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int userId = isAdmin ? request.UserID : currentUserID;

            var services = (await _serviceRepo.GetAllServices()).Where(s => !s.IsDelete).ToList();

            // One scan of the window for every queue, rather than a query per kind. The status
            // filter is pushed into the repository so a quiet day costs almost nothing.
            var pending = await _txnRepo.GetAll(userId, StatusPending, string.Empty, from, to, 1, int.MaxValue);
            var byService = pending.GroupBy(t => t.ServiceID)
                .ToDictionary(g => g.Key, g => g.ToList());

            DateTime now = DateTime.Now;

            foreach (string kind in ServiceKinds.All)
            {
                if (kind == ServiceKinds.None) continue;

                var kindServices = services.Where(s => ServiceKinds.Matches(s, kind)).ToList();
                if (kindServices.Count == 0) continue;           // not sold here
                if (!kindServices.Any(s => s.IsActive)) continue; // sold once, switched off since

                var rows = kindServices
                    .SelectMany(s => byService.TryGetValue(s.ServiceID, out var list) ? list : new List<TransactionMaster>())
                    .ToList();

                DateTime? oldest = rows.Count == 0 ? null : rows.Min(t => t.DateTime);

                summary.Queues.Add(new PendingQueueItem
                {
                    Kind = kind,
                    Label = QueueLabel(kind, kindServices),
                    PendingCount = rows.Count,
                    PendingAmount = rows.Sum(t => t.Amount),
                    OldestPendingOn = oldest,
                    OldestPendingHours = oldest == null ? 0 : Math.Round((now - oldest.Value).TotalHours, 1),
                    ServiceIDs = kindServices.Select(s => s.ServiceID).ToList(),
                });
            }

            // Busiest queue first: the operator works the one with the most stuck in it.
            summary.Queues = summary.Queues
                .OrderByDescending(q => q.PendingCount)
                .ThenBy(q => q.Label)
                .ToList();
            summary.TotalPending = summary.Queues.Sum(q => q.PendingCount);
            summary.TotalAmount = summary.Queues.Sum(q => q.PendingAmount);
            return (string.Empty, summary);
        }

        /// <summary>
        /// What to call a queue on screen. A kind backed by exactly one service is named after
        /// that service, because that is the word the operator uses for it ("Electricity Bill",
        /// not "BBPS"). More than one, and the kind's own name is the only honest label.
        /// </summary>
        private static string QueueLabel(string kind, List<ServiceMaster> kindServices)
        {
            if (kindServices.Count == 1 && !string.IsNullOrWhiteSpace(kindServices[0].Name))
                return kindServices[0].Name;

            return kind switch
            {
                ServiceKinds.Recharge => "Recharge",
                ServiceKinds.DMT => "Money Transfer",
                ServiceKinds.BBPS => "Bill Payment",
                ServiceKinds.Payout => "Payout",
                ServiceKinds.AEPS => "AEPS",
                ServiceKinds.MicroATM => "Micro ATM",
                ServiceKinds.CashDeposit => "Cash Deposit",
                _ => kind,
            };
        }

        public Task<(string fail, TxnReportWrapper data)> SelectForDMTFilter(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.DMT);

        public Task<(string fail, TxnReportWrapper data)> SelectForBBPS(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.BBPS);

        public Task<(string fail, TxnReportWrapper data)> Payout(TxnReportRequest request, int currentUserID, int currentUserTypeID)
            => FilterReport(request, currentUserID, currentUserTypeID, ServiceKinds.Payout);

        // Quick search: matches a transaction by consumer number, provider reference, our reference,
        // provider request id or the transaction id. Non-admins are scoped to their own transactions.
        public async Task<(string fail, TxnReportWrapper data)> Search(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            string term = (request.Search ?? string.Empty).Trim();
            if (term.Length < 2) return ("Enter at least 2 characters to search.", new TxnReportWrapper());

            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int userId = isAdmin ? request.UserID : currentUserID;

            var list = await _txnRepo.GetAll(userId, string.Empty, string.Empty, from, to, 1, int.MaxValue);
            var matched = list.Where(t =>
                    (t.ConsumerNumber ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (t.ProviderRefNo ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (t.ReferenceNo ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (t.ProviderRequestID ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    t.TransactionMasterID.ToString() == term)
                .OrderByDescending(t => t.DateTime)
                .ToList();

            // Count BEFORE paging, then cut the page the caller asked for.
            //
            // This used to Take() the page size without skipping and report the page's own length
            // as the total, which broke paging twice over: every page returned the same first rows,
            // and the total said "20 of 20" so Next was disabled and nobody could see it happen.
            int take = request.EffectiveTake > 0 ? request.EffectiveTake : 200;
            var page = matched.Skip(request.EffectiveSkip).Take(take).ToList();

            var wrapper = new TxnReportWrapper { Count = matched.Count, list = page.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);
            return (string.Empty, wrapper);
        }

        /// <summary>
        /// An unscoped transaction listing that still pages the way the caller asked and reports a
        /// true total. The same body as <see cref="FilterReport"/> minus the service-kind filter.
        /// </summary>
        private async Task<(string fail, TxnReportWrapper data)> PagedReport(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int userId = isAdmin ? request.UserID : currentUserID;

            var (count, list) = await _txnRepo.GetFiltered(
                userId, request.Status, null, request.ServiceID, request.OperatorID,
                from, to, request.EffectiveSkip, request.ReadTake,
                await BuildTextFilter(request));

            var wrapper = new TxnReportWrapper { Count = count, list = list.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);
            return (string.Empty, wrapper);
        }

        private async Task<(string fail, TxnReportWrapper data)> FilterReport(TxnReportRequest request, int currentUserID, int currentUserTypeID, string kind)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int userId = isAdmin ? request.UserID : currentUserID;

            var services = await _serviceRepo.GetAllServices();
            var serviceIds = ServiceKinds.IdsOf(services, kind);
            if (serviceIds.Count == 0)
                return (string.Empty, new TxnReportWrapper()); // no service of this kind configured yet

            var text = await BuildTextFilter(request);

            var (count, list) = await _txnRepo.GetFiltered(
                userId, request.Status, serviceIds, request.ServiceID, request.OperatorID,
                from, to, request.EffectiveSkip, request.ReadTake, text);

            var wrapper = new TxnReportWrapper { Count = count, list = list.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);

            // The figures strip. Every one of these reports opened on a bare table: the first
            // question anyone asks of a day's recharges is "how much went through, how much
            // failed, how much is still hanging and what did I earn", and the answer was adding up
            // whichever twenty rows happened to be on screen. Aggregated over the whole filter in
            // SQL, so it is the period's figure rather than the page's.
            var totals = await _txnRepo.GetFilteredTotals(
                userId, serviceIds, request.ServiceID, request.OperatorID, from, to, text);
            wrapper.HeaderSummary = SummaryOf(totals);

            return (string.Empty, wrapper);
        }

        /// <summary>
        /// The filter reports' shared figures strip, in the order it is read: what worked, what did
        /// not, what is still waiting, and the margin on it. The keys ARE the labels — the UI
        /// prints them as given — so the wording lives next to the figures it describes instead of
        /// being re-derived from a column name in the browser.
        /// </summary>
        private static Dictionary<string, object> SummaryOf(TxnFilterTotals t) => new()
        {
            ["Success Amount"] = t.SuccessAmount,
            ["Success Count"] = t.SuccessCount,
            ["Failed Amount"] = t.FailedAmount,
            ["Failed Count"] = t.FailedCount,
            ["Pending Amount"] = t.PendingAmount,
            ["Pending Count"] = t.PendingCount,
            ["Commission"] = t.Commission,
            ["Total Count"] = t.TotalCount,
        };

        /// <summary>
        /// Turn the report screen's text filters - selection criteria, free text and complaint
        /// state - into something the transaction query can apply.
        ///
        /// Two of them cannot be answered by the transaction table alone, so they are resolved to
        /// id sets here: "Entity Mobile" is a UserMaster column, and the complaint state lives in
        /// ComplaintMaster. Both resolve to an EMPTY list when nothing matches, which the query
        /// reads as "no rows" - the correct answer for a filter that matched nothing, and the
        /// opposite of what a null (no filter at all) would mean.
        ///
        /// Every one of these filters is on screen on at least one report and was being dropped:
        /// Criteria/CriteriaText were never read by this service, and Complaint was not even a
        /// field on the request. Three visible controls that did nothing is most of what "the
        /// filters do not work" meant.
        /// </summary>
        private async Task<TxnTextFilter> BuildTextFilter(TxnReportRequest request)
        {
            string criteria = (request.Criteria ?? string.Empty).Trim();
            string criteriaText = (request.CriteriaText ?? string.Empty).Trim();

            List<int>? userIds = null;
            if (criteriaText.Length > 0 && criteria.EqualsIgnoreCase("UserMobile"))
            {
                userIds = (await _userRepo.GetAllUsers())
                    .Where(u => (u.MobileNumber ?? string.Empty).Contains(criteriaText))
                    .Select(u => u.UserMasterID)
                    .ToList();
                // Consumed here, so the query does not then try to match a non-existent column.
                criteria = string.Empty;
                criteriaText = string.Empty;
            }

            List<int>? txnIds = null;
            if (IsRealComplaintFilter(request.Complaint))
            {
                string wanted = request.Complaint.Trim();
                txnIds = (await _complaintRepo.GetAll())
                    .Where(c => !c.IsDeleted
                                && (c.ComplaintStatus ?? string.Empty).Equals(wanted, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.TransactionMasterID)
                    .Distinct()
                    .ToList();
            }

            return new TxnTextFilter
            {
                Criteria = criteria,
                CriteriaText = criteriaText,
                Search = request.Search ?? string.Empty,
                UserIDs = userIds,
                TransactionIDs = txnIds,
            };
        }

        /// <summary>
        /// "ALL" is what a dropdown sends for "no filter"; matching it literally against a status
        /// column returns nothing, which reads as an empty report rather than an unfiltered one.
        /// </summary>
        private static bool IsRealComplaintFilter(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            return v.Length > 0 && !v.EqualsIgnoreCase("ALL");
        }

        /// <summary>
        /// Apply the selection criteria, free text and complaint filters to a list already in
        /// memory - for the reports that cannot push them into the query because they start from
        /// another table (the manual-settlement audit) rather than from the transactions.
        ///
        /// Same vocabulary as <see cref="BuildTextFilter"/> so a reader gets the same answer from
        /// the same box on every report; the only difference is where the work happens.
        /// </summary>
        private async Task<List<TransactionMaster>> ApplyCriteria(List<TransactionMaster> rows, TxnReportRequest request)
        {
            var text = await BuildTextFilter(request);
            if (text.IsEmpty || rows.Count == 0) return rows;

            IEnumerable<TransactionMaster> q = rows;

            if (text.UserIDs != null)
            {
                var ids = text.UserIDs.ToHashSet();
                q = q.Where(x => ids.Contains(x.UserID));
            }

            if (text.TransactionIDs != null)
            {
                var txnIds = text.TransactionIDs.ToHashSet();
                q = q.Where(x => txnIds.Contains(x.TransactionMasterID));
            }

            string criteriaText = text.CriteriaText.Trim();
            if (criteriaText.Length > 0)
            {
                switch (text.Criteria.Trim().ToLowerInvariant())
                {
                    case "transactionid":
                        q = q.Where(x => x.TransactionMasterID.ToString() == criteriaText
                                      || (x.ReferenceNo ?? string.Empty).Contains(criteriaText, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "consumernumber":
                        q = q.Where(x => (x.ConsumerNumber ?? string.Empty).Contains(criteriaText, StringComparison.OrdinalIgnoreCase));
                        break;
                    case "providerrefno":
                        q = q.Where(x => (x.ProviderRefNo ?? string.Empty).Contains(criteriaText, StringComparison.OrdinalIgnoreCase)
                                      || (x.ProviderRequestID ?? string.Empty).Contains(criteriaText, StringComparison.OrdinalIgnoreCase));
                        break;
                }
            }

            string search = text.Search.Trim();
            if (search.Length > 0)
            {
                q = q.Where(x =>
                    (x.ConsumerNumber ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (x.ReferenceNo ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (x.ProviderRefNo ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (x.ProviderRequestID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return q.ToList();
        }

        // Fill the display-only fields (operator name/logo, user firm/mobile) for one page of rows.
        private async Task Enrich(List<TransactionResponse> rows)
        {
            if (rows.Count == 0) return;

            var operators = await _operatorRepo.GetAllOperators();
            var opById = operators.ToDictionary(o => o.OperatorID);

            // The transacting user plus their upline, so the report can show names instead of ids.
            var users = new Dictionary<int, UserMaster>();
            var userIds = rows.Select(r => r.UserID)
                .Concat(rows.Select(r => r.DistributerUserID))
                .Concat(rows.Select(r => r.SuperDistributerUserID))
                .Where(id => id > 0)
                .Distinct();
            foreach (var id in userIds)
            {
                var u = await _userRepo.GetUserById(id);
                if (u != null) users[id] = u;
            }

            // SelectAll (not Select) so a disabled wallet still resolves to its name on old rows.
            var walletById = (await _walletRepo.SelectAll()).ToDictionary(w => w.WalletID, w => w.Name);
            var providerById = (await _providerRepo.GetAll()).ToDictionary(p => p.ProviderID, p => p.ProviderName);

            // Complaint remarks for this page's transactions (one lookup, filtered in memory);
            // most pages have no complaints, so this is usually a cheap no-op join.
            var txnIds = rows.Select(r => r.TransactionMasterID).ToHashSet();
            var complaintByTxn = (await _complaintRepo.GetAll())
                .Where(c => !c.IsDeleted && txnIds.Contains(c.TransactionMasterID))
                .GroupBy(c => c.TransactionMasterID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.UpdatedateTime).First());

            // DMT sender name isn't stored on the transaction — resolve it from the Sender the
            // transfer was made from. DMT_SenderID holds the sender's GUID; fall back to the sender's
            // mobile. Only DMT rows carry these, so non-DMT reports skip the lookup entirely.
            var needSenderName = rows.Any(r =>
                string.IsNullOrWhiteSpace(r.DMT_SenderName) &&
                (!string.IsNullOrWhiteSpace(r.DMT_SenderID) || !string.IsNullOrWhiteSpace(r.DMT_SenderNumber)));
            Dictionary<string, Sender> senderByGuid = new();
            Dictionary<string, Sender> senderByMobile = new();
            if (needSenderName)
            {
                var senders = await _senderRepo.GetAll();
                senderByGuid = senders
                    .Where(s => !string.IsNullOrWhiteSpace(s.GUID))
                    .GroupBy(s => s.GUID)
                    .ToDictionary(g => g.Key, g => g.First());
                senderByMobile = senders
                    .Where(s => !string.IsNullOrWhiteSpace(s.Mobile))
                    .GroupBy(s => s.Mobile)
                    .ToDictionary(g => g.Key, g => g.First());
            }

            foreach (var r in rows)
            {
                if (opById.TryGetValue(r.OperatorID, out var op))
                {
                    r.OperatorName = op.OperatorName;
                    r.Logo = op.OperatorImagePath;
                }
                if (users.TryGetValue(r.UserID, out var u))
                {
                    r.FirmName = u.FirmName;
                    r.UserMobile = u.MobileNumber;
                }
                // The chain above the user, as a name AND a number. A tier that is the operator
                // itself — a retailer parked straight under the admin, or an admin account stamped
                // as the "distributor" — reads "Admin" rather than the operator's firm name, which
                // is what a support desk means when it asks "who is this retailer under".
                static bool IsOperator(UserMaster u) =>
                    u.UserTypeID == UserTypeCode.Admin || u.UserTypeID == UserTypeCode.SupportUser;
                if (users.TryGetValue(r.DistributerUserID, out var dist))
                {
                    if (IsOperator(dist)) { r.DistributerName = "Admin"; r.DistributerMobile = string.Empty; }
                    else
                    {
                        r.DistributerName = !string.IsNullOrWhiteSpace(dist.FirmName) ? dist.FirmName : dist.PersonName;
                        r.DistributerMobile = dist.MobileNumber ?? string.Empty;
                    }
                }
                else if (r.DistributerUserID <= 0 && r.UserID > 0 && users.TryGetValue(r.UserID, out var self)
                         && self.ParentID is int pid && users.TryGetValue(pid, out var parent) && IsOperator(parent))
                {
                    r.DistributerName = "Admin";
                }
                if (users.TryGetValue(r.SuperDistributerUserID, out var superDist))
                {
                    if (IsOperator(superDist)) { r.SuperDistributerName = "Admin"; r.SuperDistributerMobile = string.Empty; }
                    else
                    {
                        r.SuperDistributerName = !string.IsNullOrWhiteSpace(superDist.FirmName) ? superDist.FirmName : superDist.PersonName;
                        r.SuperDistributerMobile = superDist.MobileNumber ?? string.Empty;
                    }
                }
                else if (r.SuperDistributerUserID <= 0 && r.DistributerName == "Admin")
                {
                    r.SuperDistributerName = "Admin";
                }
                if (walletById.TryGetValue(r.WalletID, out var walletName))
                    r.WalletName = walletName;
                if (providerById.TryGetValue(r.ProviderID, out var providerName))
                    r.ProviderName = providerName;

                // Fill the DMT sender's name (and mobile, if missing) from the Sender record,
                // matching on the stored GUID first, then the sender's mobile.
                if (needSenderName && string.IsNullOrWhiteSpace(r.DMT_SenderName))
                {
                    Sender? sender = null;
                    if (!string.IsNullOrWhiteSpace(r.DMT_SenderID)) senderByGuid.TryGetValue(r.DMT_SenderID, out sender);
                    if (sender == null && !string.IsNullOrWhiteSpace(r.DMT_SenderNumber)) senderByMobile.TryGetValue(r.DMT_SenderNumber, out sender);
                    if (sender != null)
                    {
                        r.DMT_SenderName = sender.Name;
                        if (string.IsNullOrWhiteSpace(r.DMT_SenderNumber)) r.DMT_SenderNumber = sender.Mobile;
                    }
                }

                if (complaintByTxn.TryGetValue(r.TransactionMasterID, out var complaint))
                {
                    // Prefer the admin's resolution remark, fall back to the customer's.
                    r.ComplaintRemark = !string.IsNullOrWhiteSpace(complaint.AdminRemark)
                        ? complaint.AdminRemark
                        : complaint.CustomerRemark ?? string.Empty;

                    // "Remark" column shows the settlement remark AND the complaint remark together.
                    if (!string.IsNullOrWhiteSpace(r.ComplaintRemark))
                        r.Remark = string.IsNullOrWhiteSpace(r.Remark)
                            ? $"Complaint: {r.ComplaintRemark}"
                            : $"{r.Remark} | Complaint: {r.ComplaintRemark}";
                }
            }
        }

        // ── Private helpers ───────────────────────────────────────────
        private static (string fail, DateTime? from, DateTime? to) ParseDates(string fromDate, string toDate)
        {
            DateTime? from = null;
            DateTime? to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && !DateTime.TryParse(fromDate, out DateTime fd))
                return ("Invalid FromDate format.", null, null);
            else if (!string.IsNullOrWhiteSpace(fromDate))
                from = DateTime.Parse(fromDate);

            if (!string.IsNullOrWhiteSpace(toDate) && !DateTime.TryParse(toDate, out DateTime td))
                return ("Invalid ToDate format.", null, null);
            else if (!string.IsNullOrWhiteSpace(toDate))
                to = DateTime.Parse(toDate).AddDays(1).AddSeconds(-1);

            return (string.Empty, from, to);
        }

        private static TxnReportWrapper BuildWrapper(List<TransactionMaster> list)
        {
            return new TxnReportWrapper
            {
                Count = list.Count,
                list = list.Select(MapToResponse).ToList()
            };
        }

        // Retailer / Distributor / Super-Distributor / Admin commissions in one readable string.
        private static string BuildCommissionInfo(TransactionMaster t)
            => $"R: {t.RetailerCommission:0.##} | D: {t.DistributerCommission:0.##} | "
             + $"SD: {t.SuperDistributerCommission:0.##} | Admin: {t.AdminCommission:0.##}";

        private static TransactionResponse MapToResponse(TransactionMaster t) => new()
        {
            DOC = t.DateTime,
            Response = t.Response,
            // Human settlement remark; complaint remark (if any) is appended during enrichment.
            Remark = t.Description,
            CommissionInfo = BuildCommissionInfo(t),
            TransactionMasterID = t.TransactionMasterID,
            UserID = t.UserID,
            ServiceID = t.ServiceID,
            OperatorID = t.OperatorID,
            WalletID = t.WalletID,
            ProviderID = t.ProviderID,
            DistributerUserID = t.DistributerUserID,
            SuperDistributerUserID = t.SuperDistributerUserID,
            ConsumerNumber = t.ConsumerNumber,
            Amount = t.Amount,
            DebitAmount = t.DebitAmount,
            OpeningBalance = t.OpeningBalance,
            ClosingBalance = t.ClosingBalance,
            Status = t.Status,
            Description = t.Description,
            ProviderRequestID = t.ProviderRequestID,
            ReferenceNo = t.ReferenceNo,
            APIUserRequestID = t.APIUserRequestID,
            ProviderDOCNo = t.ProviderDOCNo,
            ProviderRefNo = t.ProviderRefNo,
            RetailerCommission = t.RetailerCommission,
            DistributerCommission = t.DistributerCommission,
            SuperDistributerCommission = t.SuperDistributerCommission,
            AdminCommission = t.AdminCommission,
            ProviderCommission = t.ProviderCommission,
            RetailerTDS = t.RetailerTDS,
            DistributerTDS = t.DistributerTDS,
            SuperDistributerTDS = t.SuperDistributerTDS,
            AdminGST = t.AdminGST,
            RetailerGST = t.RetailerGST,
            DistributerGST = t.DistributerGST,
            SuperDistributerGST = t.SuperDistributerGST,
            CashBack = t.CashBack,
            NetCommission = t.NetCommission,
            Charge = t.Charge,
            AdminProfit = t.AdminProfit,
            ProviderOpeningBalance = t.ProviderOpeningBalance,
            ProviderClosingBalance = t.ProviderClosingBalance,
            DMT_SenderID = t.DMT_SenderID,
            DMT_SenderName = t.DMT_SenderName,
            DMT_SenderNumber = t.DMT_SenderNumber,
            DMT_BeneficiaryID = t.DMT_BeneficiaryID,
            DMT_BeneficiaryName = t.DMT_BeneficiaryName,
            DMT_Bank = t.DMT_Bank,
            DMT_IFSC = t.DMT_IFSC,
            DMT_BeneficaryNameByBank = t.DMT_BeneficaryNameByBank,
            DMT_PaymentType = t.DMT_PaymentType,
            DMT_OriginalAmount = t.DMT_OriginalAmount,
            ErrorCode = t.ErrorCode,
            Location = t.Location,
            Through = t.Through,
            IPAddress = t.IPAddress,
            Circle = t.Circle,
            ComplaintStatus = t.ComplaintStatus,
            CardType = t.CardType,
            ChangeStatus = t.ChangeStatus,
            PendingProviderStatus = t.PendingProviderStatus,
            ResponseType = t.ResponseType,
            BBPS_CustomerName = t.BBPS_CustomerName,
            IsDispute = t.IsDispute,
            IsROffer = t.IsROffer,
            IsOnlineDMT = t.IsOnlineDMT,
            IsCCF = t.IsCCF,
            SettlementAtProviderEnd = t.SettlementAtProviderEnd,
            HitBackCount = t.HitBackCount,
            StateID = t.StateID,
            DateTime = t.DateTime,
            UpdatedateTime = t.UpdatedateTime,
            UpdatedDOC = t.UpdatedateTime,
            ProcTime = ProcessingTime(t)
        };

        // Seconds from creation to last update, as a display string. A row that was never
        // updated (UpdatedateTime still default/earlier than creation) has no meaningful
        // processing time, so it renders blank rather than a bogus negative.
        private static string ProcessingTime(TransactionMaster t)
        {
            if (t.UpdatedateTime <= t.DateTime) return string.Empty;
            return ((int)(t.UpdatedateTime - t.DateTime).TotalSeconds).ToString();
        }
    }
}
