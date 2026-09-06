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
        private static readonly string[] PurchaseThrough = { "DIRECT_TRANSFER", "BY_REQUEST_TRANSFER", "PG-TRANSFER" };
        private static readonly string[] ReverseThrough = { "DIRECT_REVERSE", "BY_REVERSE_REQUEST_TRANSFER" };

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
            IOnboardingUserRepository onboardingRepo)
        {
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

            wrapper.list = rows;
            wrapper.Count = rows.Count;
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
            if (request.ServiceID > 0) txns = txns.Where(t => t.ServiceID == request.ServiceID).ToList();

            var funds = (await _fundRepo.GetAllFundTransactions())
                .Where(f => f.Status.EqualsIgnoreCase(StatusType.Success))
                .Where(f => (!from.HasValue || f.TransactionDOC >= from.Value) &&
                            (!to.HasValue || f.TransactionDOC <= to.Value))
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

            wrapper.list = rows;
            wrapper.Count = rows.Count;
            wrapper.HeaderSummary = new Dictionary<string, object>
            {
                ["Total Count"] = rows.Sum(r => r.Count),
                ["Total Amount"] = rows.Sum(r => r.Sum),
                ["Success Amount"] = rows.Sum(r => r.SuccessSum),
            };
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
            int tier = target.UserTypeID ?? UserTypeCode.Retailer;

            var rows = details.Select(d =>
            {
                var (myComm, myType) = tier switch
                {
                    UserTypeCode.SuperDistributor => (d.SuperDistributorComm, d.SuperDistributorCommType),
                    UserTypeCode.Distributor => (d.DistributorComm, d.DistributorCommType),
                    UserTypeCode.ApiUser or UserTypeCode.Client => (d.ClientComm, d.ClientCommType),
                    _ => (d.RetailerComm, d.RetailerCommType),
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
                    StrMyCommission = FormatComm(myComm, myType),
                    RetailerComm = d.RetailerComm,
                    DistributorComm = d.DistributorComm,
                    SuperDistributorComm = d.SuperDistributorComm,
                    ClientComm = d.ClientComm,
                    StrRetailerComm = FormatComm(d.RetailerComm, d.RetailerCommType),
                    StrDistributorComm = FormatComm(d.DistributorComm, d.DistributorCommType),
                    StrSuperDistributorComm = FormatComm(d.SuperDistributorComm, d.SuperDistributorCommType),
                    StrClientComm = FormatComm(d.ClientComm, d.ClientCommType),
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

            var rows = txns.GroupBy(t => t.OperatorID).Select(g =>
            {
                var success = g.Where(IsSuccess).ToList();
                var failed = g.Where(IsFailed).ToList();
                var pending = g.Where(IsPending).ToList();
                operators.TryGetValue(g.Key, out var op);
                return new DaybookViewModel
                {
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

        private static bool IsAdmin(int userTypeId)
            => userTypeId == UserTypeCode.Admin || userTypeId == UserTypeCode.SupportUser;

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
        private static string FormatComm(decimal value, string? type)
        {
            string t = (type ?? string.Empty).Trim();
            bool isPercent = t.Contains("percent", StringComparison.OrdinalIgnoreCase) || t.Contains('%');
            return isPercent ? $"{value:0.####} %" : $"{value:0.##}";
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
