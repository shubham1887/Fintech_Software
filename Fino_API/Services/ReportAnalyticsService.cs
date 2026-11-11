using Microsoft.EntityFrameworkCore;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionReport;
using FintechApp.Responses.TransactionReport;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Every aggregate report screen in one place. See <see cref="IReportAnalyticsService"/> for
    /// the contract; the shared plumbing (date parsing, hierarchy scoping, service-kind buckets)
    /// lives at the bottom of this file so each report reads as a short, obvious roll-up.
    /// </summary>
    public class ReportAnalyticsService : IReportAnalyticsService
    {
        private const string StatusSuccess = "SUCCESS";
        // Providers and the legacy DB disagree on the failure/pending spellings, so both are matched.
        private static readonly string[] FailStatuses = { "FAILED", "FAILURE" };
        private static readonly string[] PendingStatuses = { "PENDING", "PROCESS" };

        // Fund-transaction "Through" values, mirrored from LedgerService / UserAccountingService so
        // the money columns reconcile across reports.
        // Through values MUST match what FundService writes. "REQUEST_TRANSFER" and
        // "REVERSE_REQUEST_TRANSFER" were "BY_"-prefixed here, a spelling nothing ever wrote, so every
        // approved fund request / reverse request was excluded. See the note in TodaySummaryService.
        private static readonly string[] PurchaseThrough = { "DIRECT_TRANSFER", "REQUEST_TRANSFER", "PG-TRANSFER" };
        private static readonly string[] ReverseThrough = { "DIRECT_REVERSE", "REVERSE_REQUEST_TRANSFER" };

        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IUserRepository _userRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IUserSlabMappingRepository _userSlabRepo;
        private readonly ISlabMasterRepository _slabMasterRepo;
        private readonly ISlabDetailRepository _slabDetailRepo;
        private readonly IUserServiceChargeRepository _serviceChargeRepo;
        private readonly IOnboardingUserRepository _onboardingRepo;
        // The daybook has to know whether a provider's commission is quoted inclusive of GST
        // before it can report a net profit that means anything.
        private readonly ICommissionTaxResolver _commissionTax;
        // The only complete record of a wallet's balance over time: every posting stamps the
        // balance before and after it. Read through the repository (cached) rather than the
        // context, as UserAccountingService does, so the P&L and the accounting report are
        // reading the same rows.
        private readonly ITransactionDetailsRepository _ledgerRepo;
        private readonly ApplicationDbContext _context;

        public ReportAnalyticsService(
            ITransactionMasterRepository txnRepo,
            IUserRepository userRepo,
            IServiceRepository serviceRepo,
            IOperatorRepository operatorRepo,
            IWalletMasterRepository walletRepo,
            IUserWalletMasterRepository userWalletRepo,
            IFundTransactionRepository fundRepo,
            IUserSlabMappingRepository userSlabRepo,
            ISlabMasterRepository slabMasterRepo,
            ISlabDetailRepository slabDetailRepo,
            IUserServiceChargeRepository serviceChargeRepo,
            IOnboardingUserRepository onboardingRepo,
            ICommissionTaxResolver commissionTax,
            ITransactionDetailsRepository ledgerRepo,
            ApplicationDbContext context)
        {
            _ledgerRepo = ledgerRepo;
            _commissionTax = commissionTax;
            _context = context;
            _txnRepo = txnRepo;
            _userRepo = userRepo;
            _serviceRepo = serviceRepo;
            _operatorRepo = operatorRepo;
            _walletRepo = walletRepo;
            _userWalletRepo = userWalletRepo;
            _fundRepo = fundRepo;
            _userSlabRepo = userSlabRepo;
            _slabMasterRepo = slabMasterRepo;
            _slabDetailRepo = slabDetailRepo;
            _serviceChargeRepo = serviceChargeRepo;
            _onboardingRepo = onboardingRepo;
        }

        // ══════════════════════════════════════════════════════════════
        // Service-wise sale
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<ServiceSaleViewModel> data)> ServicewiseSale(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<ServiceSaleViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = await LoadTransactions(scope, from, to);
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            int id = 0;
            var rows = txns.GroupBy(t => t.ServiceID).Select(g =>
            {
                var success = g.Where(IsSuccess).ToList();
                var failed = g.Where(IsFailed).ToList();
                var pending = g.Where(IsPending).ToList();
                return new ServiceSaleViewModel
                {
                    ID = ++id,
                    ServiceID = g.Key,
                    ServiceName = serviceNames.TryGetValue(g.Key, out var n) ? n : $"Service {g.Key}",
                    SuccessAmt = success.Sum(x => x.Amount),
                    SuccessCount = success.Count,
                    FailAmt = failed.Sum(x => x.Amount),
                    FailCount = failed.Count,
                    PendingAmt = pending.Sum(x => x.Amount),
                    PendingCount = pending.Count,
                    TotalAmt = g.Sum(x => x.Amount),
                    TotalCount = g.Count(),
                    RCommission = success.Sum(x => x.RetailerCommission),
                    DCommission = success.Sum(x => x.DistributerCommission),
                    SCommission = success.Sum(x => x.SuperDistributerCommission),
                    AdminCommission = success.Sum(x => x.AdminCommission),
                    NetProfit = success.Sum(x => x.AdminProfit),
                    SuccessRate = Percent(success.Count, g.Count()),
                };
            }).OrderByDescending(r => r.SuccessAmt).ToList();

            // Re-number after sorting so "#" reads 1,2,3… down the sorted table.
            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.list = Page(rows, request);
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Success Amount"] = rows.Sum(r => r.SuccessAmt),
                ["Success Count"] = rows.Sum(r => r.SuccessCount),
                ["Failed Amount"] = rows.Sum(r => r.FailAmt),
                ["Total Commission"] = rows.Sum(r => r.RCommission + r.DCommission + r.SCommission),
            };
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // User-wise sale
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<UserSaleViewModel> data)> UserwiseSale(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<UserSaleViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = await LoadTransactions(scope, from, to);
            // Service, Operator and Wallet - the screen offers a Wallet dropdown this report used
            // to ignore, so reconciling one wallet was impossible from here.
            txns = Narrow(txns, request);

            var funds = (await _fundRepo.GetAllFundTransactions())
                .Where(f => f.Status.EqualsIgnoreCase(StatusType.Success))
                .Where(f => (!from.HasValue || f.TransactionDOC >= from.Value) &&
                            (!to.HasValue || f.TransactionDOC <= to.Value))
                // The Wallet filter has to narrow BOTH halves. Narrowing only the transactions
                // left every funded user on the report, with their sale columns emptied - so
                // picking a wallet appeared to do nothing except blank out the figures.
                .Where(f => request.WalletID <= 0 || f.WalletID == request.WalletID)
                .ToList();

            // Users that show up anywhere in the period — transacting or funded.
            var userIds = txns.Select(t => t.UserID)
                .Concat(funds.Select(f => f.ReceiverID))
                .Concat(funds.Select(f => f.SenderID))
                .Where(uid => uid > 0 && InScope(scope, uid))
                .Distinct().ToList();
            if (userIds.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);
            var walletNames = (await _walletRepo.SelectAll()).ToDictionary(w => w.WalletID, w => w.Name);
            var walletsByUser = (await _userWalletRepo.GetAll()).GroupBy(w => w.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var txnByUser = txns.GroupBy(t => t.UserID).ToDictionary(g => g.Key, g => g.ToList());

            var rows = new List<UserSaleViewModel>();
            foreach (int uid in userIds)
            {
                users.TryGetValue(uid, out var user);
                var mine = txnByUser.TryGetValue(uid, out var list) ? list : new List<TransactionMaster>();
                var success = mine.Where(IsSuccess).ToList();

                // Chronological order gives a meaningful opening (first txn's opening balance) and
                // closing (last txn's closing balance) for the period; when the user did not
                // transact at all we fall back to the live wallet balance for both.
                var ordered = mine.OrderBy(t => t.DateTime).ToList();
                var userWallets = walletsByUser.TryGetValue(uid, out var w) ? w : new List<UserWalletMaster>();
                decimal liveBalance = userWallets.Sum(x => x.Balance);

                rows.Add(new UserSaleViewModel
                {
                    UserID = uid,
                    FirmName = DisplayName(user),
                    UserMobile = user?.MobileNumber ?? string.Empty,
                    Role = user?.UserType?.UserType ?? string.Empty,
                    WalletName = string.Join(", ", userWallets
                        .Select(x => walletNames.TryGetValue(x.WalletID, out var wn) ? wn : null)
                        .Where(n => !string.IsNullOrWhiteSpace(n))),
                    OpeningBal = ordered.Count > 0 ? ordered[0].OpeningBalance : liveBalance,
                    ClosingBalance = ordered.Count > 0 ? ordered[^1].ClosingBalance : liveBalance,
                    Sale = success.Sum(t => t.Amount),
                    // A negative retailer commission IS a surcharge — split the two so the report
                    // never shows a misleading "negative commission".
                    RetailerCommission = success.Where(t => t.RetailerCommission > 0).Sum(t => t.RetailerCommission),
                    Surcharge = Math.Abs(success.Where(t => t.RetailerCommission < 0).Sum(t => t.RetailerCommission)),
                    DistributerCommission = success.Sum(t => t.DistributerCommission),
                    SuperDistributerCommission = success.Sum(t => t.SuperDistributerCommission),
                    Purchase = funds.Where(f => f.ReceiverID == uid && PurchaseThrough.Contains(f.Through)).Sum(f => f.Amount),
                    TransferAmount = funds.Where(f => f.SenderID == uid && PurchaseThrough.Contains(f.Through)).Sum(f => f.Amount),
                    ReverseAmountDr = funds.Where(f => f.SenderID == uid && ReverseThrough.Contains(f.Through)).Sum(f => f.Amount),
                    ReverseAmountCr = funds.Where(f => f.ReceiverID == uid && ReverseThrough.Contains(f.Through)).Sum(f => f.Amount),
                    SuccessCount = success.Count,
                    FailCount = mine.Count(IsFailed),
                });
            }

            rows = rows.OrderByDescending(r => r.Sale).ThenBy(r => r.FirmName).ToList();
            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Sale"] = rows.Sum(r => r.Sale),
                ["Total Commission"] = rows.Sum(r => r.RetailerCommission),
                ["Total Purchase"] = rows.Sum(r => r.Purchase),
                ["Users"] = rows.Count,
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Operator-wise sale (user × service × operator)
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<OperatorSaleViewModel> data)> OperatorSale(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<OperatorSaleViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, from, to)).Where(IsSuccess).ToList();
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            if (request.OperatorID > 0) txns = txns.Where(t => t.OperatorID == request.OperatorID).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var operatorNames = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);

            var rows = txns
                .GroupBy(t => new { t.UserID, t.ServiceID, t.OperatorID })
                .Select(g =>
                {
                    decimal amount = g.Sum(x => x.Amount);
                    decimal retailer = g.Sum(x => x.RetailerCommission);
                    decimal dist = g.Sum(x => x.DistributerCommission);
                    decimal super = g.Sum(x => x.SuperDistributerCommission);
                    users.TryGetValue(g.Key.UserID, out var user);
                    return new OperatorSaleViewModel
                    {
                        UserID = g.Key.UserID,
                        UserName = DisplayName(user),
                        ServiceName = serviceNames.TryGetValue(g.Key.ServiceID, out var sn) ? sn : $"Service {g.Key.ServiceID}",
                        OperatorName = operatorNames.TryGetValue(g.Key.OperatorID, out var on) ? on : $"Operator {g.Key.OperatorID}",
                        SuccessCount = g.Count(),
                        SuccessAmount = amount,
                        ReatilerComm = retailer,
                        RetailerCommPercentage = Percent(retailer, amount),
                        DistributerComm = dist,
                        DistributerCommPercentage = Percent(dist, amount),
                        SuperDistributerComm = super,
                        SuperDistributerCommPercentage = Percent(super, amount),
                    };
                })
                .OrderByDescending(r => r.SuccessAmount)
                .ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Success Amount"] = rows.Sum(r => r.SuccessAmount),
                ["Success Count"] = rows.Sum(r => r.SuccessCount),
                ["Retailer Commission"] = rows.Sum(r => r.ReatilerComm),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Top performer
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<TopPerformerViewModel> data)> TopPerformer(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<TopPerformerViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, from, to)).Where(IsSuccess).ToList();
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);

            var rows = txns.GroupBy(t => t.UserID).Select(g =>
            {
                users.TryGetValue(g.Key, out var user);
                return new TopPerformerViewModel
                {
                    UserID = g.Key,
                    FirmName = DisplayName(user),
                    EmailID = user?.EmailID ?? string.Empty,
                    UserMobile = user?.MobileNumber ?? string.Empty,
                    Role = user?.UserType?.UserType ?? string.Empty,
                    TotalSale = g.Sum(x => x.Amount),
                    SuccessCount = g.Count(),
                    TotalCommission = g.Sum(x => x.RetailerCommission),
                };
            }).OrderByDescending(r => r.TotalSale).ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].Rank = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Sale"] = rows.Sum(r => r.TotalSale),
                ["Performers"] = rows.Count,
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Range-wise recharge (amount buckets)
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<AmountRangeViewModel> data)> RangewiseRecharge(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<AmountRangeViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = await LoadTransactions(scope, from, to);

            if (request.ServiceID > 0)
            {
                txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            }
            else
            {
                // Default to the recharge family so the report matches its title.
                var rechargeIds = ServiceKinds.IdsOf(await _serviceRepo.GetAllServices(), ServiceKinds.Recharge).ToHashSet();
                if (rechargeIds.Count > 0) txns = txns.Where(t => rechargeIds.Contains(t.ServiceID)).ToList();
            }
            if (txns.Count == 0) return (string.Empty, wrapper);

            int id = 0;
            var rows = RechargeBands
                .Select(band =>
                {
                    var inBand = txns.Where(t => t.Amount >= band.From && t.Amount <= band.To).ToList();
                    var success = inBand.Where(IsSuccess).ToList();
                    var failed = inBand.Where(IsFailed).ToList();
                    var pending = inBand.Where(IsPending).ToList();
                    return new AmountRangeViewModel
                    {
                        ID = ++id,
                        Range = band.Label,
                        Count = inBand.Count,
                        Sum = inBand.Sum(t => t.Amount),
                        SuccessCount = success.Count,
                        SuccessSum = success.Sum(t => t.Amount),
                        FailCount = failed.Count,
                        FailSum = failed.Sum(t => t.Amount),
                        ProcessCount = pending.Count,
                        ProcessSum = pending.Sum(t => t.Amount),
                    };
                })
                .Where(r => r.Count > 0)   // an empty band is noise, not information
                .ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Count"] = rows.Sum(r => r.Count),
                ["Total Amount"] = rows.Sum(r => r.Sum),
                ["Success Amount"] = rows.Sum(r => r.SuccessSum),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // User-wise DMT range
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<UserDMTRangeViewModel> data)> UserwiseDMTRange(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<UserDMTRangeViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, from, to)).Where(IsSuccess).ToList();

            var dmtIds = ServiceKinds.IdsOf(await _serviceRepo.GetAllServices(), ServiceKinds.DMT).ToHashSet();
            if (dmtIds.Count > 0) txns = txns.Where(t => dmtIds.Contains(t.ServiceID)).ToList();
            // The screen offers Service, Operator and Wallet dropdowns and this report read none
            // of them: with more than one DMT service or gateway configured, there was no way to
            // look at one of them.
            txns = Narrow(txns, request);
            if (txns.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);

            decimal Band(IEnumerable<TransactionMaster> src, decimal lo, decimal hi)
                => src.Where(t => t.Amount >= lo && t.Amount <= hi).Sum(t => t.Amount);

            var rows = txns.GroupBy(t => t.UserID).Select(g =>
            {
                users.TryGetValue(g.Key, out var user);
                var mine = g.ToList();
                return new UserDMTRangeViewModel
                {
                    UserID = g.Key,
                    UserName = DisplayName(user),
                    Range1_2 = Band(mine, 1m, 2m),
                    Range3_1000 = Band(mine, 3m, 1000m),
                    Range1001_2000 = Band(mine, 1001m, 2000m),
                    Range2001_3000 = Band(mine, 2001m, 3000m),
                    Range3001_4000 = Band(mine, 3001m, 4000m),
                    Range4001_5000 = Band(mine, 4001m, 5000m),
                    Range5001_10000 = Band(mine, 5001m, 10000m),
                    Range10001_15000 = Band(mine, 10001m, 15000m),
                    Range15001_20000 = Band(mine, 15001m, 20000m),
                    Range20001_25000 = Band(mine, 20001m, 25000m),
                    Range25001_50000 = Band(mine, 25001m, 50000m),
                    Range50001_1L = Band(mine, 50001m, 100000m),
                    TotalTransactions = mine.Count,
                    TotalAmount = mine.Sum(t => t.Amount),
                };
            }).OrderByDescending(r => r.TotalAmount).ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Amount"] = rows.Sum(r => r.TotalAmount),
                ["Transactions"] = rows.Sum(r => r.TotalTransactions),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Daily sale split by service kind
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<DailyServiceSaleViewModel> data)> DailyServiceSale(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<DailyServiceSaleViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, from, to)).Where(IsSuccess).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var buckets = await BuildServiceBuckets();

            int id = 0;
            var rows = txns.GroupBy(t => t.DateTime.Date).Select(g => new DailyServiceSaleViewModel
            {
                ID = ++id,
                DOC = g.Key,
                MobileSale = SumOf(g, buckets, Bucket.Mobile),
                DTHSale = SumOf(g, buckets, Bucket.DTH),
                PostPaid = SumOf(g, buckets, Bucket.PostPaid),
                Electricity = SumOf(g, buckets, Bucket.Electricity),
                MoneyTransfer = SumOf(g, buckets, Bucket.DMT),
                ExpressMoneyTransfer = SumOf(g, buckets, Bucket.ExpressDMT),
                AepsSale = SumOf(g, buckets, Bucket.AEPS),
                PaymentGatewaySale = SumOf(g, buckets, Bucket.PaymentGateway),
                TotalSale = g.Sum(t => t.Amount),
            }).OrderByDescending(r => r.DOC).ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Sale"] = rows.Sum(r => r.TotalSale),
                ["Days"] = rows.Count,
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Yesterday vs today, per user
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<UserServiceSellViewModel> data)> UserwiseServiceSell(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<UserServiceSellViewModel>();

            // This report is defined as "yesterday vs today", so it ignores the date filter and
            // always spans the two days ending now.
            DateTime today = DateTime.Now.Date;
            DateTime yesterday = today.AddDays(-1);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, yesterday, today.AddDays(1).AddSeconds(-1)))
                .Where(IsSuccess).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);
            var buckets = await BuildServiceBuckets();

            var rows = txns.GroupBy(t => t.UserID).Select(g =>
            {
                users.TryGetValue(g.Key, out var user);
                var y = g.Where(t => t.DateTime.Date == yesterday).ToList();
                var t0 = g.Where(t => t.DateTime.Date == today).ToList();
                return new UserServiceSellViewModel
                {
                    UserID = g.Key,
                    FirmName = DisplayName(user),
                    Role = user?.UserType?.UserType ?? string.Empty,
                    YesterdayMobileSale = SumOf(y, buckets, Bucket.Mobile),
                    TodayMobileSale = SumOf(t0, buckets, Bucket.Mobile),
                    YesterdayDTHSale = SumOf(y, buckets, Bucket.DTH),
                    TodayDTHSale = SumOf(t0, buckets, Bucket.DTH),
                    YesterdayPostPaid = SumOf(y, buckets, Bucket.PostPaid),
                    TodayPostPaid = SumOf(t0, buckets, Bucket.PostPaid),
                    YesterdayElectricity = SumOf(y, buckets, Bucket.Electricity),
                    TodayElectricity = SumOf(t0, buckets, Bucket.Electricity),
                    YesterdayMoneyTransfer = SumOf(y, buckets, Bucket.DMT),
                    TodayMoneyTransfer = SumOf(t0, buckets, Bucket.DMT),
                    YesterdayTotal = y.Sum(t => t.Amount),
                    TodayTotal = t0.Sum(t => t.Amount),
                };
            }).OrderByDescending(r => r.TodayTotal).ThenByDescending(r => r.YesterdayTotal).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Today Sale"] = rows.Sum(r => r.TodayTotal),
                ["Yesterday Sale"] = rows.Sum(r => r.YesterdayTotal),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Newly-added users
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<DatewiseUserViewModel> data)> DatewiseUsers(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<DatewiseUserViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var allUsers = await _userRepo.GetAllUsers();
            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);

            var candidates = allUsers
                .Where(u => InScope(scope, u.UserMasterID) && u.UserMasterID != currentUserID)
                .Where(u => (!from.HasValue || u.DateTime >= from.Value) && (!to.HasValue || u.DateTime <= to.Value))
                .OrderByDescending(u => u.DateTime)
                .ToList();
            if (candidates.Count == 0) return (string.Empty, wrapper);

            var byId = allUsers.ToDictionary(u => u.UserMasterID);
            var walletNames = (await _walletRepo.SelectAll()).ToDictionary(w => w.WalletID, w => w.Name);
            var walletsByUser = (await _userWalletRepo.GetAll()).GroupBy(w => w.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rows = candidates.Select(u => new DatewiseUserViewModel
            {
                UserID = u.UserMasterID,
                DOC = u.DateTime,
                FirmName = DisplayName(u),
                UserMobile = u.MobileNumber ?? string.Empty,
                IsKYCApprovedStr = u.IsKYCApproved ? "Approved" : "Pending",
                Balances = BalancesLabel(walletsByUser, walletNames, u.UserMasterID),
                Role = u.UserType?.UserType ?? string.Empty,
                JoiningBy = u.JoiningByUserID.HasValue && byId.TryGetValue(u.JoiningByUserID.Value, out var by)
                    ? DisplayName(by)
                    : (u.ParentID.HasValue && byId.TryGetValue(u.ParentID.Value, out var parent) ? DisplayName(parent) : string.Empty),
                RegistrationType = u.RegistrationType ?? string.Empty,
                Status = u.IsActive ? "Active" : "Inactive",
            }).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["New Users"] = rows.Count,
                ["KYC Approved"] = rows.Count(r => r.IsKYCApprovedStr == "Approved"),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Downline network
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<DownlineReportViewModel> data)> DownlineReport(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<DownlineReportViewModel>();
            var allUsers = await _userRepo.GetAllUsers();

            // Admin sees every non-admin user; everyone else sees strictly their own downline
            // (never themselves, never their upline).
            bool isAdmin = IsAdmin(currentUserTypeID);
            var downline = isAdmin
                ? allUsers.Where(u => u.UserTypeID != UserTypeCode.Admin).Select(u => u.UserMasterID).ToHashSet()
                : UserHierarchy.GetDownlineIds(allUsers, currentUserID);

            if (request.UserID > 0 && downline.Contains(request.UserID))
                downline = new HashSet<int> { request.UserID };

            var members = allUsers.Where(u => downline.Contains(u.UserMasterID)).ToList();
            if (members.Count == 0) return (string.Empty, wrapper);

            var byId = allUsers.ToDictionary(u => u.UserMasterID);
            var wallets = await _walletRepo.SelectAll();
            var walletsByUser = (await _userWalletRepo.GetAll()).GroupBy(w => w.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Wallets are admin-defined, so match the three headline columns by name rather than id.
            int RechargeWalletId = WalletIdMatching(wallets, "recharge", "main", "prepaid");
            int DmtWalletId = WalletIdMatching(wallets, "dmt", "money");
            int AepsWalletId = WalletIdMatching(wallets, "aeps");

            var rows = members.Select(u =>
            {
                var mine = walletsByUser.TryGetValue(u.UserMasterID, out var w) ? w : new List<UserWalletMaster>();
                return new DownlineReportViewModel
                {
                    UserID = u.UserMasterID,
                    FirmName = DisplayName(u),
                    UserMobile = u.MobileNumber ?? string.Empty,
                    Role = u.UserType?.UserType ?? string.Empty,
                    ParentName = u.ParentID.HasValue && byId.TryGetValue(u.ParentID.Value, out var p) ? DisplayName(p) : string.Empty,
                    RechargeBalance = mine.Where(x => x.WalletID == RechargeWalletId).Sum(x => x.Balance),
                    DMTBalance = mine.Where(x => x.WalletID == DmtWalletId).Sum(x => x.Balance),
                    AEPSBalance = mine.Where(x => x.WalletID == AepsWalletId).Sum(x => x.Balance),
                    TotalBalance = mine.Sum(x => x.Balance),
                    Outstanding = mine.Sum(x => x.HoldBalance),
                    Status = u.IsActive ? "Active" : "Inactive",
                };
            }).OrderByDescending(r => r.TotalBalance).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Members"] = rows.Count,
                ["Total Balance"] = rows.Sum(r => r.TotalBalance),
                ["On Hold"] = rows.Sum(r => r.Outstanding),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Merchant onboarding list
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<UserOnboardingViewModel> data)> OnboardingList(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<UserOnboardingViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var onboarded = (await _onboardingRepo.GetAll())
                .Where(o => !o.IsDeleted)
                .Where(o => (!from.HasValue || o.DateTime >= from.Value) && (!to.HasValue || o.DateTime <= to.Value))
                .ToList();

            // Onboarding rows carry no UserID, so a non-admin is scoped by the mobile numbers of
            // the users they can already see — their own branch of the network.
            if (!IsAdmin(currentUserTypeID))
            {
                var scope = await ResolveScope(0, currentUserID, currentUserTypeID);
                var allowedMobiles = (await _userRepo.GetAllUsers())
                    .Where(u => InScope(scope, u.UserMasterID))
                    .Select(u => u.MobileNumber ?? string.Empty)
                    .Where(m => m.Length > 0)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                onboarded = onboarded.Where(o => allowedMobiles.Contains(o.MobileNumber ?? string.Empty)).ToList();
            }

            // The screen's Entity picker. Onboarding rows carry no UserID, so the chosen user is
            // matched by their mobile number - the same key the scope filter above uses. The
            // dropdown was on screen and read by nothing, so picking a merchant changed nothing.
            if (request.UserID > 0)
            {
                var picked = (await _userRepo.GetAllUsers())
                    .FirstOrDefault(u => u.UserMasterID == request.UserID);
                string mobile = picked?.MobileNumber ?? string.Empty;
                onboarded = mobile.Length == 0
                    ? new List<Entities.OnboardingUser>()
                    : onboarded.Where(o => (o.MobileNumber ?? string.Empty).Equals(mobile, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim().ToLowerInvariant();
                onboarded = onboarded.Where(o =>
                    (o.Name ?? string.Empty).ToLowerInvariant().Contains(s) ||
                    (o.MobileNumber ?? string.Empty).Contains(s) ||
                    (o.ReferenceID ?? string.Empty).ToLowerInvariant().Contains(s)).ToList();
            }
            if (onboarded.Count == 0) return (string.Empty, wrapper);

            var rows = onboarded.OrderByDescending(o => o.DateTime).Select((o, i) => new UserOnboardingViewModel
            {
                id = i + 1,
                DOC = o.DateTime,
                FirmName = o.Name ?? string.Empty,
                UserMobile = o.MobileNumber ?? string.Empty,
                MerchantCode = !string.IsNullOrWhiteSpace(o.ReferenceID) ? o.ReferenceID : o.ExternalID,
                Status = !string.IsNullOrWhiteSpace(o.AdminStatus) ? o.AdminStatus : o.Status,
                BankName = o.BankName ?? string.Empty,
                AccountNumber = o.AccountNumber ?? string.Empty,
            }).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object> { ["Onboarded"] = rows.Count };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Service-charge deductions
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<ServiceChargeDeductionViewModel> data)> ServiceChargeDeduction(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<ServiceChargeDeductionViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var charges = (await _serviceChargeRepo.GetAll())
                .Where(c => !c.IsDeleted && InScope(scope, c.UserID))
                .Where(c => (!from.HasValue || c.DateTime >= from.Value) && (!to.HasValue || c.DateTime <= to.Value))
                .OrderByDescending(c => c.DateTime)
                .ToList();
            if (charges.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            var rows = charges.Select((c, i) =>
            {
                users.TryGetValue(c.UserID, out var user);
                return new ServiceChargeDeductionViewModel
                {
                    ID = i + 1,
                    UserID = c.UserID,
                    FirmName = DisplayName(user),
                    UserMobile = user?.MobileNumber ?? string.Empty,
                    ServiceName = serviceNames.TryGetValue(c.ServiceID, out var sn) ? sn : string.Empty,
                    Date = c.DateTime,
                    Charge = c.Charge,
                    Remark = c.Remark ?? string.Empty,
                };
            }).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Deductions"] = rows.Count,
                ["Total Charge"] = rows.Sum(r => r.Charge),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Commission structure — "what does MY slab pay me?"
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<CommissionStructureViewModel> data)> CommissionStructure(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<CommissionStructureViewModel>();

            // Whose structure are we showing? Admin/Support may inspect anyone; a distributor may
            // inspect their own downline; everyone else only ever sees their own slab.
            int targetUserId = currentUserID;
            if (request.UserID > 0 && request.UserID != currentUserID)
            {
                if (IsAdmin(currentUserTypeID))
                {
                    targetUserId = request.UserID;
                }
                else
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepo.GetAllUsers(), currentUserID);
                    if (!downline.Contains(request.UserID)) return (Messages.Common.Message.NoPermission, wrapper);
                    targetUserId = request.UserID;
                }
            }

            var target = await _userRepo.GetUserById(targetUserId);
            if (target == null) return (Messages.Common.Message.DataNotFound, wrapper);

            // The slab assigned to this user. A user can end up with more than one mapping row
            // (re-assignment history), so take the most recent one that actually has rate rows —
            // an empty slab would otherwise render a blank report and look like a bug.
            var mappings = (await _userSlabRepo.GetByUserID(targetUserId))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.UserSlabMappingID)
                .ToList();

            UserSlabMapping? mapping = null;
            List<SlabDetail> details = new();
            foreach (var candidate in mappings)
            {
                var rates = (await _slabDetailRepo.GetBySlabMasterID(candidate.SlabMasterID))
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .ToList();
                if (rates.Count == 0) continue;
                mapping = candidate;
                details = rates;
                break;
            }
            mapping ??= mappings.FirstOrDefault();

            if (mapping == null)
            {
                // Admin has no slab of their own — they set the rates — so point them at the
                // user picker instead of implying something is misconfigured.
                if (targetUserId == currentUserID && IsAdmin(currentUserTypeID))
                    return ("Pick a user above to see the commission slab assigned to them.", wrapper);
                string message = targetUserId == currentUserID
                    ? "No commission slab is assigned to you yet. Ask your admin to assign one."
                    : "No commission slab is assigned to this user yet.";
                return (message, wrapper);
            }

            var slab = await _slabMasterRepo.GetByID(mapping.SlabMasterID);
            if (request.ServiceID > 0) details = details.Where(d => d.ServiceID == request.ServiceID).ToList();
            if (details.Count == 0) return (string.Empty, wrapper);

            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var operatorNames = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);

            // The tier the target user actually earns on — that is the "margin" they care about.
            //
            // It has to be the tier the COMMISSION ENGINE actually pays, not a tier that merely
            // shares the role's name. SlabCommissionResolver pays the transacting user the
            // RETAILER rate for every role except a Client account — so an API user earns the
            // retailer rate — but this report read them the Client column, which is a different
            // number in every slab. An API user was being quoted a margin they would never be
            // paid. Only a Client-type account reads the client column now; that is what it is for.
            int tier = target.UserTypeID ?? UserTypeCode.Retailer;

            // Which OTHER tiers' rates this caller is allowed to read. The whole slab used to go
            // out to everyone and only the web UI dropped the columns — so a retailer who opened a
            // row's "View details", exported the CSV, or simply read the network response saw what
            // their distributor and super distributor earn on the same transaction. That is the
            // operator's entire margin structure, handed to the bottom of the chain.
            bool seeRetailer = CanSeeTier(currentUserTypeID, UserTypeCode.Retailer);
            bool seeDistributor = CanSeeTier(currentUserTypeID, UserTypeCode.Distributor);
            bool seeSuperDistributor = CanSeeTier(currentUserTypeID, UserTypeCode.SuperDistributor);
            bool seeClient = CanSeeTier(currentUserTypeID, UserTypeCode.Client);

            var rows = details.Select(d =>
            {
                // Rate, type AND category travel together — picking the tier's rate without its
                // category would label a surcharge row as a commission on the one line the user
                // reads as "what I earn".
                var (myComm, myType, myCategory) = tier switch
                {
                    UserTypeCode.SuperDistributor => (d.SuperDistributorComm, d.SuperDistributorCommType, d.SuperDistributorCommissionCategory),
                    UserTypeCode.Distributor => (d.DistributorComm, d.DistributorCommType, d.DistributorCommissionCategory),
                    UserTypeCode.Client => (d.ClientComm, d.ClientCommType, d.ClientCommissionCategory),
                    _ => (d.RetailerComm, d.RetailerCommType, d.RetailerCommissionCategory),
                };
                return new CommissionStructureViewModel
                {
                    SlabDetailID = d.SlabDetailID,
                    SlabMasterID = d.SlabMasterID,
                    SlabName = slab?.SlabName ?? string.Empty,
                    UserID = targetUserId,
                    FirmName = DisplayName(target),
                    ServiceID = d.ServiceID,
                    ServiceName = serviceNames.TryGetValue(d.ServiceID, out var sn) ? sn : $"Service {d.ServiceID}",
                    OperatorID = d.OperatorID,
                    OperatorName = d.OperatorID > 0 && operatorNames.TryGetValue(d.OperatorID, out var on) ? on : "All operators",
                    Details = AmountBand(d.FromAmount, d.ToAmount),
                    FromAmount = d.FromAmount,
                    ToAmount = d.ToAmount,
                    MyCommission = myComm,
                    MyCommissionType = myType ?? string.Empty,
                    MyCommissionCategory = myCategory ?? string.Empty,
                    StrMyCommission = FormatComm(myComm, myType, myCategory),
                    RetailerComm = seeRetailer ? d.RetailerComm : null,
                    DistributorComm = seeDistributor ? d.DistributorComm : null,
                    SuperDistributorComm = seeSuperDistributor ? d.SuperDistributorComm : null,
                    ClientComm = seeClient ? d.ClientComm : null,
                    StrRetailerComm = seeRetailer
                        ? FormatComm(d.RetailerComm, d.RetailerCommType, d.RetailerCommissionCategory) : string.Empty,
                    StrDistributorComm = seeDistributor
                        ? FormatComm(d.DistributorComm, d.DistributorCommType, d.DistributorCommissionCategory) : string.Empty,
                    StrSuperDistributorComm = seeSuperDistributor
                        ? FormatComm(d.SuperDistributorComm, d.SuperDistributorCommType, d.SuperDistributorCommissionCategory) : string.Empty,
                    StrClientComm = seeClient
                        ? FormatComm(d.ClientComm, d.ClientCommType, d.ClientCommissionCategory) : string.Empty,
                };
            })
            .OrderBy(r => r.ServiceName).ThenBy(r => r.OperatorName).ThenBy(r => r.FromAmount)
            .ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Slab"] = slab?.SlabName ?? string.Empty,
                ["Services"] = rows.Select(r => r.ServiceID).Distinct().Count(),
                ["Rate Rows"] = rows.Count,
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Daybook — one row per operator, split by outcome
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<DaybookViewModel> data)> Daybook(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<DaybookViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = await LoadTransactions(scope, from, to);
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            if (request.OperatorID > 0) txns = txns.Where(t => t.OperatorID == request.OperatorID).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var operators = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First());
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            // The commission treatment per (service, operator, provider), resolved once each
            // rather than per transaction — a busy day is thousands of rows over a handful of
            // combinations. Without this the provider commission is reported as quoted, and a
            // commission quoted inclusive of GST overstates the profit by the tax inside it.
            // Every combination present, not only the successful ones: an operator with nothing
            // but failed attempts still gets a row, and leaving its mode blank reads as a gap in
            // the report rather than as "nothing settled here yet".
            var treatments = new Dictionary<(int Svc, int Op, int Prov), Responses.CommissionTax.CommissionTaxTreatment>();
            foreach (var key in txns.Select(t => (Svc: t.ServiceID, Op: t.OperatorID, Prov: t.ProviderID))
                                    .Distinct())
            {
                treatments[key] = await _commissionTax.Resolve(key.Svc, key.Op, key.Prov, 0, to ?? DateTime.Today);
            }

            var tdsByTxn = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.TransactionMasterID)
                .Select(g => new { TxnID = g.Key, Total = g.Sum(x => x.TDSAmount) })
                .ToDictionaryAsync(x => x.TxnID, x => x.Total);

            var rows = txns.GroupBy(t => t.OperatorID).Select(g =>
            {
                var success = g.Where(IsSuccess).ToList();
                var failed = g.Where(IsFailed).ToList();
                var pending = g.Where(IsPending).ToList();
                operators.TryGetValue(g.Key, out var op);

                decimal providerQuoted = success.Sum(x => x.ProviderCommission);
                decimal providerNet = 0m, gstOnComm = 0m;
                decimal surchargeNet = 0m, gstOnSurcharge = 0m;
                string mode = string.Empty, basis = string.Empty;

                foreach (var t in success)
                {
                    treatments.TryGetValue((t.ServiceID, t.OperatorID, t.ProviderID), out var tr);

                    // The STAMP wins where there is one. It was written at settlement from the band
                    // that priced this particular transaction, and a per-band decision — one slab
                    // band inclusive, the next exempt — cannot be reproduced from a treatment
                    // resolved per service/operator/provider. It is also the only figure that stays
                    // right after somebody edits the rate card.
                    if (t.ChargeGSTStampedOn != null)
                    {
                        providerNet += t.ProviderBaseCharge;
                        gstOnComm += t.ProviderGSTAmount;

                        if (t.RetailerCommission < 0m)
                        {
                            surchargeNet += Math.Abs(t.RetailerBaseCharge);
                            gstOnSurcharge += Math.Abs(t.RetailerGST);
                        }

                        if (mode.Length == 0)
                        {
                            mode = string.IsNullOrWhiteSpace(t.RetailerGSTType) ? (tr?.TaxMode ?? string.Empty) : t.RetailerGSTType;
                            basis = tr?.TaxBase ?? string.Empty;
                        }
                        continue;
                    }

                    if (tr == null) continue;

                    // Nothing stamped — settled before the breakdown existed. Resolve the treatment
                    // that applies and split as the report always did.
                    //
                    // The provider side. A NEGATIVE commission is a cost we pay the provider, and
                    // the split stays signed: the GST inside it is input credit, not output tax.
                    var split = _commissionTax.Split(t.ProviderCommission, tr, t.Amount);
                    providerNet += split.NetCommission;
                    gstOnComm += split.GST;

                    // The surcharge the retailer paid us. It is quoted to them inclusive of tax
                    // exactly as the provider's charge is, so it has to be split too — mixing a
                    // net provider cost with a gross surcharge overstates profit by the tax inside
                    // the surcharge. GST is on the charge itself, so the base is the charge even
                    // where the service is configured GROSS for its commission.
                    if (t.RetailerCommission < 0m)
                    {
                        var chargeTreatment = SurchargeTreatment(tr);
                        var chargeSplit = _commissionTax.Split(Math.Abs(t.RetailerCommission), chargeTreatment, t.Amount);
                        surchargeNet += chargeSplit.NetCommission;
                        gstOnSurcharge += chargeSplit.GST;
                    }

                    if (mode.Length == 0) { mode = tr.TaxMode; basis = tr.TaxBase; }
                }

                // No settled transaction to read it off, so take the treatment that WOULD apply.
                if (mode.Length == 0)
                {
                    var any = g.First();
                    if (treatments.TryGetValue((any.ServiceID, any.OperatorID, any.ProviderID), out var anyTr))
                    { mode = anyTr.TaxMode; basis = anyTr.TaxBase; }
                }

                decimal retailerPaid = success.Where(x => x.RetailerCommission > 0).Sum(x => x.RetailerCommission);
                decimal surcharge = Math.Abs(success.Where(x => x.RetailerCommission < 0).Sum(x => x.RetailerCommission));
                decimal downline = retailerPaid
                                 + success.Sum(x => x.DistributerCommission)
                                 + success.Sum(x => x.SuperDistributerCommission);

                return new DaybookViewModel
                {
                    ProviderCommission = Math.Round(providerQuoted, 2),
                    ProviderCommissionNet = Math.Round(providerNet, 2),
                    GSTOnCommission = Math.Round(gstOnComm, 2),
                    SurchargeNet = Math.Round(surchargeNet, 2),
                    GSTOnSurcharge = Math.Round(gstOnSurcharge, 2),
                    TDSDeducted = Math.Round(success.Sum(x => tdsByTxn.TryGetValue(x.TransactionMasterID, out var v) ? v : 0m), 2),
                    // What the house keeps, from the figures that are actually income on BOTH
                    // sides — the surcharge net of the GST we owe on it, not the gross charge.
                    NetProfit = Math.Round(providerNet - downline + surchargeNet, 2),
                    CommissionTaxMode = mode,
                    CommissionTaxBase = basis,
                    OperatorID = g.Key,
                    Operator = op?.OperatorName ?? $"Operator {g.Key}",
                    ServiceName = op != null && serviceNames.TryGetValue(op.ServiceID, out var sn) ? sn : string.Empty,
                    TotalCount = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount),
                    SuccessCount = success.Count,
                    SuccessAmount = success.Sum(x => x.Amount),
                    FailedCount = failed.Count,
                    FailedAmount = failed.Sum(x => x.Amount),
                    PendingCount = pending.Count,
                    PendingAmount = pending.Sum(x => x.Amount),
                    // A negative retailer commission is a surcharge, not a payout — keep them apart.
                    RetailerCommission = success.Where(x => x.RetailerCommission > 0).Sum(x => x.RetailerCommission),
                    Surcharge = Math.Abs(success.Where(x => x.RetailerCommission < 0).Sum(x => x.RetailerCommission)),
                    DistributerCommission = success.Sum(x => x.DistributerCommission),
                    SuperDistributerCommission = success.Sum(x => x.SuperDistributerCommission),
                };
            }).OrderByDescending(r => r.TotalAmount).ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Amount"] = rows.Sum(r => r.TotalAmount),
                ["Success Amount"] = rows.Sum(r => r.SuccessAmount),
                ["Failed Amount"] = rows.Sum(r => r.FailedAmount),
                ["Commission"] = rows.Sum(r => r.RetailerCommission),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // TDS — tax deducted at source on commission payouts
        // ══════════════════════════════════════════════════════════════
        public async Task<(string fail, AnalyticsWrapper<TdsViewModel> data)> Tds(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<TdsViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = (await LoadTransactions(scope, from, to)).Where(IsSuccess).ToList();
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();

            // Only transactions that actually deducted tax belong in a TDS report.
            txns = txns.Where(t => t.RetailerTDS != 0 || t.DistributerTDS != 0 || t.SuperDistributerTDS != 0).ToList();
            if (txns.Count == 0) return (string.Empty, wrapper);

            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            var rows = txns.OrderByDescending(t => t.DateTime).Select((t, i) =>
            {
                users.TryGetValue(t.UserID, out var user);
                return new TdsViewModel
                {
                    ID = i + 1,
                    TransactionMasterID = t.TransactionMasterID,
                    DOC = t.DateTime,
                    UserID = t.UserID,
                    FirmName = DisplayName(user),
                    ServiceName = serviceNames.TryGetValue(t.ServiceID, out var sn) ? sn : string.Empty,
                    Amount = t.Amount,
                    RetailerCommission = t.RetailerCommission,
                    RetailerTDS = t.RetailerTDS,
                    DistributerTDS = t.DistributerTDS,
                    SuperDistributerTDS = t.SuperDistributerTDS,
                    TotalTDS = t.RetailerTDS + t.DistributerTDS + t.SuperDistributerTDS,
                };
            }).ToList();

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Transactions"] = rows.Count,
                ["Total TDS"] = rows.Sum(r => r.TotalTDS),
                ["Total Commission"] = rows.Sum(r => r.RetailerCommission),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        // ══════════════════════════════════════════════════════════════
        // Profit & loss - the accounting desk's one-page summary
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Opening, purchase, sale, closing, each commission tier, the tax on both sides, the
        /// margin between them and the net profit - grouped by day, service, operator or user.
        ///
        /// It exists because the figures it puts on one line were spread across four reports that
        /// did not agree: the daybook knew the provider side and the GST treatment, user
        /// accounting knew opening and closing, the sale reports knew the commission tiers, and
        /// nothing put a margin beside the tax on it. Reconciling them by hand is what the
        /// accountant was doing.
        ///
        /// Two rules from the daybook are carried over deliberately, because getting either wrong
        /// leaves every number on the page plausible and wrong:
        ///
        ///  * The per-transaction GST STAMP wins wherever there is one. It was written at
        ///    settlement from the band that priced that transaction, so it survives somebody
        ///    editing the rate card afterwards, and it can express a per-band decision that no
        ///    treatment resolved per service/operator/provider could reproduce.
        ///  * Net profit is computed from the figures that are income on BOTH sides - the provider
        ///    commission net of its GST, and the surcharge net of the GST we owe on it. Mixing a
        ///    net provider cost with a gross surcharge is what made DMT profit read several times
        ///    high before.
        /// </summary>
        public async Task<(string fail, AnalyticsWrapper<ProfitLossViewModel> data)> ProfitLoss(
            TxnReportRequest request, int currentUserID, int currentUserTypeID)
        {
            var wrapper = new AnalyticsWrapper<ProfitLossViewModel>();
            var (fail, from, to) = ParseDates(request.FromDate, request.ToDate);
            if (fail.Length > 0) return (fail, wrapper);

            var scope = await ResolveScope(request.UserID, currentUserID, currentUserTypeID);
            var txns = Narrow(await LoadTransactions(scope, from, to), request);

            // Funds loaded into the network in the window. Purchase is a fund movement rather than
            // a transaction, so it comes from the other table - and only the Through values
            // FundService actually writes count (see the note on PurchaseThrough).
            var funds = (await _fundRepo.GetAllFundTransactions())
                .Where(f => f.Status.EqualsIgnoreCase(StatusSuccess))
                .Where(f => PurchaseThrough.Contains(f.Through ?? string.Empty))
                .Where(f => (!from.HasValue || f.TransactionDOC >= from.Value)
                         && (!to.HasValue || f.TransactionDOC <= to.Value))
                .Where(f => InScope(scope, f.ReceiverID))
                .ToList();

            if (txns.Count == 0 && funds.Count == 0) return (string.Empty, wrapper);

            // The commission treatment per (service, operator, provider), resolved once each
            // rather than per transaction - a busy month is tens of thousands of rows over a
            // handful of combinations.
            var treatments = new Dictionary<(int Svc, int Op, int Prov), Responses.CommissionTax.CommissionTaxTreatment>();
            foreach (var key in txns.Select(t => (Svc: t.ServiceID, Op: t.OperatorID, Prov: t.ProviderID)).Distinct())
                treatments[key] = await _commissionTax.Resolve(key.Svc, key.Op, key.Prov, 0, to ?? DateTime.Today);

            string groupBy = (request.GroupBy ?? string.Empty).Trim();
            bool dateWise = !(groupBy.EqualsIgnoreCase("Service")
                           || groupBy.EqualsIgnoreCase("Operator")
                           || groupBy.EqualsIgnoreCase("User"));
            bool userWise = groupBy.EqualsIgnoreCase("User");

            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var operatorNames = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);
            var users = (await _userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);

            // How each transaction and each fund row is bucketed. Date-wise is the default because
            // a profit and loss is read down the calendar; the others answer "which service" and
            // "which user" from the same screen rather than from three more.
            //
            // A fund load belongs to no service and no operator, so under those two groupings it
            // cannot be attributed to a row - it is carried on its own "Fund load" line instead of
            // being dropped, which would make Purchase here disagree with the fund report.
            Func<TransactionMaster, string> txnKey =
                groupBy.EqualsIgnoreCase("Service") ? (t => serviceNames.TryGetValue(t.ServiceID, out var n) ? n : $"Service {t.ServiceID}")
                : groupBy.EqualsIgnoreCase("Operator") ? (t => operatorNames.TryGetValue(t.OperatorID, out var n) ? n : $"Operator {t.OperatorID}")
                : userWise ? (t => DisplayNameOf(users, t.UserID))
                : (t => t.DateTime.Date.ToString("yyyy-MM-dd"));

            Func<Entities.FundTransaction, string> fundKey =
                dateWise ? (f => f.TransactionDOC.Date.ToString("yyyy-MM-dd"))
                : userWise ? (f => DisplayNameOf(users, f.ReceiverID))
                : (_ => FundLoadRow);

            var purchaseByGroup = funds
                .GroupBy(fundKey)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.Amount));

            var rows = new List<ProfitLossViewModel>();
            foreach (var group in txns.GroupBy(txnKey))
            {
                var all = group.ToList();
                var success = all.Where(IsSuccess).ToList();

                decimal providerNet = 0m, providerGst = 0m;
                decimal downlineGst = 0m, surchargeNet = 0m, gstOnSurcharge = 0m;

                foreach (var t in success)
                {
                    treatments.TryGetValue((t.ServiceID, t.OperatorID, t.ProviderID), out var tr);

                    if (t.ChargeGSTStampedOn != null)
                    {
                        // Settled since the breakdown existed: read what was stamped.
                        providerNet += t.ProviderBaseCharge;
                        // GST is a tax AMOUNT, never a direction, so the magnitude is taken here.
                        // The columns say which way it goes: the downline figure is output tax we
                        // owe, the provider figure is input credit we claim. Left signed, a charge
                        // (stored negative) printed a negative tax, which reads as a refund.
                        providerGst += Math.Abs(t.ProviderGSTAmount);
                        downlineGst += Math.Abs(t.RetailerGST) + Math.Abs(t.DistributerGST) + Math.Abs(t.SuperDistributerGST);

                        if (t.RetailerCommission < 0m)
                        {
                            surchargeNet += Math.Abs(t.RetailerBaseCharge);
                            gstOnSurcharge += Math.Abs(t.RetailerGST);
                        }
                        continue;
                    }

                    if (tr == null)
                    {
                        // No stamp and no treatment configured. The quoted figure is all there is,
                        // so it is counted as net - which is exactly what every report did for
                        // every row before the treatments existed, so an unconfigured install is
                        // no worse off than it was.
                        providerNet += t.ProviderCommission;
                        continue;
                    }

                    var split = _commissionTax.Split(t.ProviderCommission, tr, t.Amount);
                    providerNet += split.NetCommission;
                    providerGst += Math.Abs(split.GST);

                    if (t.RetailerCommission < 0m)
                    {
                        var chargeTreatment = SurchargeTreatment(tr);
                        var chargeSplit = _commissionTax.Split(Math.Abs(t.RetailerCommission), chargeTreatment, t.Amount);
                        surchargeNet += chargeSplit.NetCommission;
                        gstOnSurcharge += Math.Abs(chargeSplit.GST);
                        downlineGst += Math.Abs(chargeSplit.GST);
                    }
                }

                // Signed, as stored: positive = paid to that tier, negative = collected from them.
                decimal retailer = success.Sum(x => x.RetailerCommission);
                decimal distributor = success.Sum(x => x.DistributerCommission);
                decimal superDist = success.Sum(x => x.SuperDistributerCommission);

                // What the network actually COST: only the commission paid out. A surcharge is
                // income and is added back below, net of its tax - it is not a negative cost, and
                // netting it in here is how a loss-making service can read as profitable.
                decimal paidDownline =
                    success.Where(x => x.RetailerCommission > 0).Sum(x => x.RetailerCommission)
                    + success.Where(x => x.DistributerCommission > 0).Sum(x => x.DistributerCommission)
                    + success.Where(x => x.SuperDistributerCommission > 0).Sum(x => x.SuperDistributerCommission);

                rows.Add(new ProfitLossViewModel
                {
                    Group = group.Key,
                    Date = dateWise && DateTime.TryParse(group.Key, out var d) ? d : null,
                    TotalCount = all.Count,
                    SuccessCount = success.Count,
                    Purchase = purchaseByGroup.TryGetValue(group.Key, out var p) ? Math.Round(p, 2) : 0m,
                    Sale = Math.Round(success.Sum(x => x.Amount), 2),
                    RetailerCommission = Math.Round(retailer, 2),
                    DistributorCommission = Math.Round(distributor, 2),
                    SuperDistributorCommission = Math.Round(superDist, 2),
                    TotalCommission = Math.Round(retailer + distributor + superDist, 2),
                    TotalGST = Math.Round(downlineGst, 2),
                    ProviderCommission = Math.Round(providerNet, 2),
                    ProviderGST = Math.Round(providerGst, 2),
                    DiffCommission = Math.Round(providerNet - paidDownline + surchargeNet, 2),
                    DiffGST = Math.Round(providerGst - downlineGst, 2),
                    NetProfit = Math.Round(providerNet - paidDownline + surchargeNet - gstOnSurcharge, 2),
                });
            }

            // A fund load in a bucket with no transactions is still a purchase.
            foreach (var orphan in purchaseByGroup)
            {
                var existing = rows.FirstOrDefault(r => r.Group == orphan.Key);
                if (existing != null) continue;
                rows.Add(new ProfitLossViewModel
                {
                    Group = orphan.Key,
                    Date = dateWise && DateTime.TryParse(orphan.Key, out var d) ? d : null,
                    Purchase = Math.Round(orphan.Value, 2),
                });
            }

            rows = dateWise
                ? rows.OrderBy(r => r.Date ?? DateTime.MinValue).ToList()
                : rows.OrderByDescending(r => r.Sale).ThenBy(r => r.Group).ToList();

            // Balances are a wallet figure. Down the calendar, and per user, they mean something
            // and tie together; per service or per operator they would be the same network float
            // repeated on every line, read as if each service held it.
            if (dateWise || userWise) await FillBalances(rows, scope, from, to, dateWise, users);

            for (int i = 0; i < rows.Count; i++) rows[i].ID = i + 1;

            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Purchase"] = rows.Sum(r => r.Purchase),
                ["Sale"] = rows.Sum(r => r.Sale),
                ["Total Commission"] = rows.Sum(r => r.TotalCommission),
                ["Total GST"] = rows.Sum(r => r.TotalGST),
                ["Provider Commission"] = rows.Sum(r => r.ProviderCommission),
                ["Provider GST"] = rows.Sum(r => r.ProviderGST),
                ["Margin"] = rows.Sum(r => r.DiffCommission),
                ["Net Profit"] = rows.Sum(r => r.NetProfit),
            };
            wrapper.list = Page(rows, request);
            return (string.Empty, wrapper);
        }

        /// <summary>
        /// Label for the purchase line under a grouping a fund load cannot belong to. Named so the
        /// row reads as what it is rather than as a service with no sales.
        /// </summary>
        private const string FundLoadRow = "Fund load (no service)";

        /// <summary>
        /// Fill Opening and Closing from the TransactionDetails ledger, where every posting stamps
        /// the balance before and after it.
        ///
        /// The ledger is used rather than UserBalanceHistory for the reason UserAccountingService
        /// records: nothing in the running system writes that table, so reading it gave Opening 0
        /// for everybody and, on any past date, Closing 0 as well.
        ///
        /// Date-wise, Opening is the balance carried into that day and Closing the balance at the
        /// end of it, so consecutive rows tie together down the column. User-wise, they are the
        /// balances at the two ends of the whole window.
        /// </summary>
        private async Task FillBalances(
            List<ProfitLossViewModel> rows, HashSet<int>? scope, DateTime? from, DateTime? to,
            bool dateWise, Dictionary<int, UserMaster> users)
        {
            if (rows.Count == 0) return;

            var ledger = (await _ledgerRepo.GetAll())
                .Where(e => !e.IsDeleted)
                .Where(e => InScope(scope, e.UserID))
                .OrderBy(e => e.DateTime).ThenBy(e => e.TransactionDetailID)
                .ToList();
            if (ledger.Count == 0) return;

            if (dateWise)
            {
                foreach (var row in rows)
                {
                    if (row.Date == null) continue;
                    DateTime dayStart = row.Date.Value.Date;
                    row.Opening = Math.Round(SumPerUser(ledger, e => e.DateTime < dayStart), 2);
                    row.Closing = Math.Round(SumPerUser(ledger, e => e.DateTime < dayStart.AddDays(1)), 2);
                }
                return;
            }

            // User-wise: match each row back to its user through the same display name the row was
            // keyed on. Two users sharing a firm name are summed, which is also how they were
            // grouped, so the balance and the sale on that line describe the same set of people.
            var idsByName = users.Values
                .GroupBy(DisplayName)
                .ToDictionary(g => g.Key, g => g.Select(u => u.UserMasterID).ToHashSet());

            foreach (var row in rows)
            {
                if (!idsByName.TryGetValue(row.Group, out var ids)) continue;
                var mine = ledger.Where(e => ids.Contains(e.UserID)).ToList();
                if (mine.Count == 0) continue;

                row.Opening = Math.Round(
                    SumPerUser(mine, e => !from.HasValue || e.DateTime < from.Value), 2);
                row.Closing = Math.Round(
                    SumPerUser(mine, e => !to.HasValue || e.DateTime <= to.Value), 2);
            }
        }

        /// <summary>
        /// Sum, over every user who has one, of the LAST matching ledger entry's closing balance.
        ///
        /// A balance is a per-user running total, so the network's balance is the sum of each
        /// user's latest entry - not the latest entry of the combined stream, which would report
        /// one user's wallet as the whole network's.
        /// </summary>
        private static decimal SumPerUser(
            List<Entities.TransactionDetails> ledger,
            Func<Entities.TransactionDetails, bool> upTo)
            => ledger.Where(upTo)
                .GroupBy(e => e.UserID)
                .Sum(g => g.Last().ClosingBalance);

        private static string DisplayNameOf(Dictionary<int, UserMaster> users, int userId)
            => users.TryGetValue(userId, out var u) ? DisplayName(u) : $"User {userId}";

        // ══════════════════════════════════════════════════════════════
        // Shared plumbing
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// The set of user ids a caller may see. `null` means "everyone" (Admin / Support);
        /// otherwise it is the caller plus their full downline, optionally narrowed to a single
        /// requested user when that user is inside the branch.
        /// </summary>
        private async Task<HashSet<int>?> ResolveScope(int requestedUserId, int currentUserID, int currentUserTypeID)
        {
            if (IsAdmin(currentUserTypeID))
                return requestedUserId > 0 ? new HashSet<int> { requestedUserId } : null;

            var allUsers = await _userRepo.GetAllUsers();
            var scope = UserHierarchy.GetDownlineIds(allUsers, currentUserID);
            scope.Add(currentUserID);

            // A requested id outside the caller's branch is ignored, not honoured — narrowing a
            // report must never widen who it can reach.
            if (requestedUserId > 0 && scope.Contains(requestedUserId))
                return new HashSet<int> { requestedUserId };
            return scope;
        }

        /// <summary>
        /// Parses the screen's From/To into an inclusive window. "To" is stretched to the end of
        /// that day so a same-day range still contains the day's transactions. Mirrors
        /// TransactionReportService.ParseDates so every report agrees on what a date range means.
        /// </summary>
        private static (string fail, DateTime? from, DateTime? to) ParseDates(string fromDate, string toDate)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                if (!DateTime.TryParse(fromDate, out DateTime fd)) return ("Invalid FromDate format.", null, null);
                from = fd;
            }
            if (!string.IsNullOrWhiteSpace(toDate))
            {
                if (!DateTime.TryParse(toDate, out DateTime td)) return ("Invalid ToDate format.", null, null);
                to = td.Date.AddDays(1).AddSeconds(-1);
            }
            return (string.Empty, from, to);
        }

        /// <summary>
        /// Whether this caller reads the WHOLE company rather than their own branch of the network.
        ///
        /// The name says Admin for history, but every use of it asks the same question: may this
        /// caller see every user's transactions and every tier's commission? That is true of the
        /// Accountant too — and leaving them out is why their reports came back empty. The scope
        /// resolver fell through to "your own downline", an accountant has none, so the daybook,
        /// the income report and the service-wise sale all returned nothing at all. The role was
        /// granted the endpoints and the menu and still saw a blank screen, which reads as a
        /// broken report rather than a permissions gap.
        ///
        /// This widens READING only. Nothing here lets the accountant change a transaction — those
        /// endpoints are pinned to Admin in ApiAccessPolicy and unaffected by this.
        /// </summary>
        private static bool IsAdmin(int userTypeId)
            => userTypeId == UserTypeCode.Admin
            || userTypeId == UserTypeCode.SupportUser
            || userTypeId == UserTypeCode.Accountant;

        /// <summary>
        /// The same treatment, but with the tax base pinned to the charge itself. A surcharge is a
        /// fee for a service, so the GST is on the fee — never on the transfer amount it was
        /// charged against. Without this, a service configured GROSS (the trading case, where the
        /// tax follows the throughput) would compute the tax on a DMT transfer's whole value.
        /// </summary>
        private static Responses.CommissionTax.CommissionTaxTreatment SurchargeTreatment(
            Responses.CommissionTax.CommissionTaxTreatment source)
            => new()
            {
                CommissionTaxProfileID = source.CommissionTaxProfileID,
                Scope = source.Scope,
                ScopeLabel = source.ScopeLabel,
                TaxMode = source.TaxMode,
                TaxBase = StaticModels.CommissionTaxBase.Commission,
                GSTRatePercent = source.GSTRatePercent,
                RateFromTaxEngine = source.RateFromTaxEngine,
            };

        /// <summary>
        /// May a caller of this user-type read the rate a given tier earns?
        ///
        /// Everyone sees their own tier, and an upline sees the tiers it pays out to — that margin
        /// is theirs to set. Nobody ever sees a tier ABOVE them: a retailer who learns the
        /// distributor's cut (or a distributor who learns the super distributor's) can work out
        /// the operator's whole commission structure from one screen. Admin and support own the
        /// structure, so they see all of it.
        /// </summary>
        private static bool CanSeeTier(int callerTypeId, int tierTypeId)
        {
            if (IsAdmin(callerTypeId) || callerTypeId == tierTypeId) return true;
            return callerTypeId switch
            {
                UserTypeCode.SuperDistributor => tierTypeId is UserTypeCode.Distributor or UserTypeCode.Retailer,
                UserTypeCode.Distributor => tierTypeId == UserTypeCode.Retailer,
                // An API user is paid the RETAILER rate, and that rate is already what
                // "My margin" shows them — so there is no second column left for them to read,
                // and certainly not the client column they are never paid from.
                UserTypeCode.ApiUser => false,
                UserTypeCode.Client => tierTypeId == UserTypeCode.ApiUser,
                _ => false,
            };
        }

        private static bool InScope(HashSet<int>? scope, int userId) => scope == null || scope.Contains(userId);

        /// <summary>
        /// One page of transactions is never enough for a roll-up, so aggregates read the whole
        /// window once and group in memory. A single-user scope is pushed into the query.
        /// </summary>
        private async Task<List<TransactionMaster>> LoadTransactions(HashSet<int>? scope, DateTime? from, DateTime? to)
        {
            int singleUser = scope is { Count: 1 } ? scope.First() : 0;
            var txns = await _txnRepo.GetAll(singleUser, string.Empty, string.Empty, from, to, 1, int.MaxValue);
            if (scope != null && singleUser == 0)
                txns = txns.Where(t => scope.Contains(t.UserID)).ToList();
            return txns;
        }

        /// <summary>
        /// Narrow a loaded transaction set by the structural filters the report screens offer -
        /// Service, Operator and Wallet.
        ///
        /// Every report that shows one of these dropdowns now passes through here. Several showed
        /// a dropdown their endpoint never read, so changing it did nothing at all, which is worse
        /// than not offering it: the reader concludes the data is wrong rather than the control.
        /// </summary>
        private static List<TransactionMaster> Narrow(List<TransactionMaster> txns, TxnReportRequest request)
        {
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();
            if (request.OperatorID > 0) txns = txns.Where(t => t.OperatorID == request.OperatorID).ToList();
            if (request.WalletID > 0) txns = txns.Where(t => t.WalletID == request.WalletID).ToList();
            return txns;
        }

        private static List<T> Page<T>(List<T> rows, TxnReportRequest request)
        {
            if (request.IsExcelDownload) return rows;
            int take = request.EffectiveTake;
            if (take <= 0) return rows;
            return rows.Skip(request.EffectiveSkip).Take(take).ToList();
        }

        private static bool IsSuccess(TransactionMaster t) => t.Status.EqualsIgnoreCase(StatusSuccess);
        private static bool IsFailed(TransactionMaster t) => FailStatuses.Contains(t.Status?.ToUpperInvariant());
        private static bool IsPending(TransactionMaster t) => PendingStatuses.Contains(t.Status?.ToUpperInvariant());

        private static string DisplayName(UserMaster? u)
        {
            if (u == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(u.FirmName)) return u.FirmName;
            if (!string.IsNullOrWhiteSpace(u.PersonName)) return u.PersonName;
            return u.MobileNumber ?? string.Empty;
        }

        private static string Percent(decimal part, decimal whole)
            => whole == 0 ? "0%" : $"{(part / whole * 100):0.##}%";

        private static string Percent(int part, int whole)
            => whole == 0 ? "0%" : $"{((decimal)part / whole * 100):0.##}%";

        /// <summary>"₹ 10 – ₹ 1,000", "Above ₹ 500", or "Any amount" when the band is open.</summary>
        private static string AmountBand(decimal from, decimal to)
        {
            if (from <= 0 && to <= 0) return "Any amount";
            if (to <= 0) return $"Above {from:N0}";
            if (from <= 0) return $"Up to {to:N0}";
            return $"{from:N0} – {to:N0}";
        }

        /// <summary>A rate plus its unit — "1.5 %" for percentage types, "5.00" (flat) otherwise.</summary>
        /// <summary>
        /// A commission rate as a person reads it: "1.5 %" where the slab stores a percentage,
        /// "₹ 5" where it stores a flat amount.
        ///
        /// Both halves were missing, and the first was a correctness problem rather than a
        /// cosmetic one. The percentage test now defers to
        /// <see cref="SlabCommissionResolver.IsPercentType"/> — the same check the commission
        /// ENGINE applies when it pays the money out — instead of the looser "does the word
        /// percent appear in it" test that used to live here. The vocabulary actually written to
        /// the column is "Flexi" / "Fix", which that test never matched, so a row the engine pays
        /// as 12 % was printed as a bare "12", indistinguishable from a flat ₹12 in the next
        /// column. The report and the engine now read the column the same way; two independent
        /// guesses at one vocabulary is how they came to disagree.
        ///
        /// The flat side is marked for the same reason: with a symbol on neither kind, no column
        /// on the page could be read as one or the other.
        ///
        /// The sign says which direction the money moves: <c>+</c> is a COMMISSION paid to the
        /// user, <c>-</c> is a SURCHARGE taken from them. They are opposites sitting in one
        /// column, and the number alone cannot tell them apart — "0.4" being money earned on one
        /// row and money charged on the next is the difference between a slab that pays and one
        /// that bills.
        ///
        /// The sign is also what the engine itself computes:
        /// <see cref="SlabCommissionResolver.Normalize"/> returns a negative figure for a
        /// surcharge and a positive one for a commission, so the column now prints the direction
        /// that is actually applied rather than a separate description of it.
        ///
        /// Surcharge is decided exactly as that method decides it: the category column, OR a
        /// negative value whatever the category says. The magnitude is printed unsigned
        /// underneath, for the same reason the engine takes <c>Math.Abs</c> before applying the
        /// sign — otherwise a surcharge already stored negative would come out as "+".
        ///
        /// Zero is left bare, with no sign. The engine short-circuits it to 0, so it is neither
        /// paid nor charged, and a "+0" would imply an earning that is not there.
        /// </summary>
        private static string FormatComm(decimal value, string? type, string? category)
        {
            if (value == 0) return "0";

            decimal magnitude = Math.Abs(value);
            string rate = SlabCommissionResolver.IsPercentType(type)
                ? $"{magnitude:0.####} %"
                : $"₹ {magnitude:0.##}";

            bool surcharge = value < 0 || SlabCommissionResolver.IsSurchargeCategory(category);
            return surcharge ? $"-{rate}" : $"+{rate}";
        }

        private static string BalancesLabel(
            IReadOnlyDictionary<int, List<UserWalletMaster>> walletsByUser,
            IReadOnlyDictionary<int, string> walletNames,
            int userId)
        {
            if (!walletsByUser.TryGetValue(userId, out var mine) || mine.Count == 0) return "0";
            return string.Join(" | ", mine.Select(w =>
                $"{(walletNames.TryGetValue(w.WalletID, out var n) ? n : $"Wallet {w.WalletID}")}: {w.Balance:N2}"));
        }

        /// <summary>First wallet whose name contains one of the keywords, else 0 (matches nothing).</summary>
        private static int WalletIdMatching(IEnumerable<WalletMaster> wallets, params string[] keywords)
        {
            foreach (string key in keywords)
            {
                var hit = wallets.FirstOrDefault(w => (w.Name ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase)
                                                   || (w.ShortName ?? string.Empty).Contains(key, StringComparison.OrdinalIgnoreCase));
                if (hit != null) return hit.WalletID;
            }
            return 0;
        }

        // ── Service buckets (the columns of the day-wise / user-wise sale grids) ──
        private enum Bucket { Mobile, DTH, PostPaid, Electricity, DMT, ExpressDMT, AEPS, PaymentGateway, Other }

        /// <summary>Maps each ServiceID to the grid column it belongs to, by name keywords.</summary>
        private async Task<Dictionary<int, Bucket>> BuildServiceBuckets()
        {
            var map = new Dictionary<int, Bucket>();
            foreach (var s in await _serviceRepo.GetAllServices())
            {
                string text = $"{s.Name} {s.ServiceCategory}".ToLowerInvariant();
                Bucket b =
                    text.Contains("dth") ? Bucket.DTH :
                    text.Contains("postpaid") || text.Contains("post paid") ? Bucket.PostPaid :
                    text.Contains("electric") ? Bucket.Electricity :
                    text.Contains("express") ? Bucket.ExpressDMT :
                    text.Contains("aeps") ? Bucket.AEPS :
                    text.Contains("payment gateway") || text.Contains("pg") ? Bucket.PaymentGateway :
                    text.Contains("dmt") || text.Contains("money transfer") ? Bucket.DMT :
                    text.Contains("prepaid") || text.Contains("mobile") || text.Contains("recharge") ? Bucket.Mobile :
                    Bucket.Other;
                map[s.ServiceID] = b;
            }
            return map;
        }

        private static decimal SumOf(IEnumerable<TransactionMaster> txns, IReadOnlyDictionary<int, Bucket> buckets, Bucket bucket)
            => txns.Where(t => buckets.TryGetValue(t.ServiceID, out var b) && b == bucket).Sum(t => t.Amount);

        // Amount bands for the Rangewise Recharge report — the classic recharge ladder.
        private static readonly (decimal From, decimal To, string Label)[] RechargeBands =
        {
            (0m, 10m, "0 – 10"),
            (11m, 50m, "11 – 50"),
            (51m, 100m, "51 – 100"),
            (101m, 200m, "101 – 200"),
            (201m, 500m, "201 – 500"),
            (501m, 1000m, "501 – 1,000"),
            (1001m, 2000m, "1,001 – 2,000"),
            (2001m, 5000m, "2,001 – 5,000"),
            (5001m, 10000m, "5,001 – 10,000"),
            (10001m, decimal.MaxValue, "Above 10,000"),
        };
    }
}
