using FintechApp.Entities;
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
        // Per-provider aggregation of transactions in the date range. Purchase and provider
        // opening/closing balances are not tracked yet → returned as 0 (Admin-only figures).
        public async Task<(string fail, ProviderSaleWrapper data)> ProviderSale(TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new ProviderSaleWrapper());

            int userId = ScopeUser(request.UserID, currentUserID, currentUserTypeID);
            // Admin sees all providers' sale; a scoped user sees only their own transactions.
            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            var txns = await _txnRepo.GetAll(isAdmin ? 0 : userId, string.Empty, string.Empty, from, to, 1, int.MaxValue);
            if (request.ProviderID > 0) txns = txns.Where(t => t.ProviderID == request.ProviderID).ToList();

            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);

            int id = 0;
            var rows = txns.GroupBy(t => t.ProviderID).Select(g =>
            {
                var success = g.Where(x => x.Status == StatusSuccess).ToList();
                var failed = g.Where(x => x.Status == StatusFailed).ToList();
                return new ProviderSaleViewModel
                {
                    ID = ++id,
                    ProvderName = providerNames.TryGetValue(g.Key, out var pn) ? pn : g.Key.ToString(),
                    Purchase = 0m,        // provider stock purchase not tracked here
                    Sale = success.Sum(x => x.Amount),
                    Fail = failed.Sum(x => x.Amount),
                    RetailerCommission = success.Sum(x => x.RetailerCommission),
                    DistributerCommission = success.Sum(x => x.DistributerCommission),
                    SuperDistributerCommission = success.Sum(x => x.SuperDistributerCommission),
                    AdminCommission = success.Sum(x => x.AdminCommission),
                    OpeningBalance = 0m,  // provider balance not tracked here
                    ClosingBalance = 0m,
                    Difference = 0m,
                    Profit = success.Sum(x => x.AdminProfit),
                };
            }).OrderBy(x => x.ProvderName).ToList();

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

            // Provider payments are company-wide books; only Admin / Support may read them.
            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            if (!isAdmin) return (Messages.Common.Message.DataNotFound, new PurchaseReportWrapper());

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
        public async Task<(string fail, TxnReportWrapper data)> SelectReport(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, string.Empty, from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

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

        // ── Recharge ──────────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> Recharge(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            // serviceType "RECHARGE" — repository will filter when implementation is added
            var list = await _txnRepo.GetAll(currentUserID, request.Status, "RECHARGE", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

        // ── Prepaid ───────────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> Prepaid(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, "PREPAID", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

        // ── AEPS ──────────────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> AEPS(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, "AEPS", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

        // ── MicroATM ──────────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> MicroATM(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, "MATM", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

        // ── CashDeposit ───────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> CashDeposit(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, "CASHDEPOSIT", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

        // ── LeadGeneration ────────────────────────────────────────────
        public async Task<(string fail, TxnReportWrapper data)> LeadGeneration(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new TxnReportWrapper());

            var list = await _txnRepo.GetAll(currentUserID, request.Status, "LEAD", from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
        }

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

            var list = await _txnRepo.GetAll(ScopeUser(request.UserID, currentUserID, currentUserTypeID), string.Empty, string.Empty, from, to, request.PageNumber, request.PageSize);

            if (request.ServiceID > 0)
                list = list.Where(x => x.ServiceID == request.ServiceID).ToList();

            return (string.Empty, BuildWrapper(list));
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
            var list = await _txnRepo.GetAll(targetUser, request.Status, string.Empty, from, to, request.PageNumber, request.PageSize);
            return (string.Empty, BuildWrapper(list));
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
        public async Task<(string fail, List<IncomeSummaryViewModel> data)> Income(TxnReportRequest request, int currentUserID)
        {
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail != string.Empty) return (fail, new List<IncomeSummaryViewModel>());

            var list = await _txnRepo.GetAll(currentUserID, StatusSuccess, string.Empty, from, to, 1, int.MaxValue);

            var result = list.GroupBy(x => x.ServiceID)
                .Select(g =>
                {
                    decimal retailerComm = g.Sum(x => x.RetailerCommission);
                    decimal distributerComm = g.Sum(x => x.DistributerCommission);
                    decimal superDistComm = g.Sum(x => x.SuperDistributerCommission);
                    decimal adminComm = g.Sum(x => x.AdminCommission);
                    decimal retailerTds = g.Sum(x => x.RetailerTDS);
                    decimal distributerTds = g.Sum(x => x.DistributerTDS);
                    return new IncomeSummaryViewModel
                    {
                        ServiceType = g.Key.ToString(), // Needs ServiceMaster join for actual name
                        RetailerCommission = retailerComm,
                        DistributerCommission = distributerComm,
                        SuperDistributerCommission = superDistComm,
                        AdminCommission = adminComm,
                        RetailerTDS = retailerTds,
                        DistributerTDS = distributerTds,
                        TotalIncome = retailerComm + distributerComm + superDistComm + adminComm - retailerTds - distributerTds,
                        Count = g.Count()
                    };
                })
                .OrderByDescending(x => x.TotalIncome)
                .ToList();

            return (string.Empty, result);
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
                .Take(request.EffectiveTake > 0 ? request.EffectiveTake : 200)
                .ToList();

            var wrapper = new TxnReportWrapper { Count = matched.Count, list = matched.Select(MapToResponse).ToList() };
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

            var (count, list) = await _txnRepo.GetFiltered(
                userId, request.Status, serviceIds, request.ServiceID, request.OperatorID,
                from, to, request.EffectiveSkip, request.EffectiveTake);

            var wrapper = new TxnReportWrapper { Count = count, list = list.Select(MapToResponse).ToList() };
            await Enrich(wrapper.list);
            return (string.Empty, wrapper);
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
                if (users.TryGetValue(r.DistributerUserID, out var dist))
                    r.DistributerName = !string.IsNullOrWhiteSpace(dist.FirmName) ? dist.FirmName : dist.PersonName;
                if (users.TryGetValue(r.SuperDistributerUserID, out var superDist))
                    r.SuperDistributerName = !string.IsNullOrWhiteSpace(superDist.FirmName) ? superDist.FirmName : superDist.PersonName;
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
