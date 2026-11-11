using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.Dashboard;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IComplaintMasterRepository _complaintRepo;
        private readonly IUserDocumentMasterRepository _docRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IUserTypeMasterRepository _userTypeRepo;
        private readonly IWalletMasterRepository _walletMasterRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly Logger _logger;

        // ServiceID constants (from ServiceMaster table, same as old ServiceCode enum)
        private const int ServiceId_PrePaid = 11;
        private const int ServiceId_PostPaid = 14;
        private const int ServiceId_DTH = 13;
        private const int ServiceId_DMT = 10;
        private const int ServiceId_AEPS = 4;

        // FundTransaction Through values
        // FundTransaction.Through values — these MUST match what FundService writes. The two request-
        // approval values were "BY_"-prefixed here, a spelling nothing ever wrote, so approved fund
        // requests and approved reverse requests matched nothing and were missing from these figures.
        // See the note in TodaySummaryService.
        private const string Through_RequestTransfer = "REQUEST_TRANSFER";
        private const string Through_DirectTransfer = "DIRECT_TRANSFER";
        private const string Through_DirectReverse = "DIRECT_REVERSE";
        private const string Through_ReverseRequest = "REVERSE_REQUEST_TRANSFER";
        private const string Through_PGTransfer = "PG-TRANSFER";
        private const string Through_ECollection = "E-COLLECTION";

        public DashboardService(
            ITransactionMasterRepository txnRepo,
            IFundTransactionRepository fundRepo,
            IComplaintMasterRepository complaintRepo,
            IUserDocumentMasterRepository docRepo,
            IUserRepository userRepo,
            IUserWalletMasterRepository walletRepo,
            IProviderMasterRepository providerRepo,
            IUserTypeMasterRepository userTypeRepo,
            IWalletMasterRepository walletMasterRepo,
            IServiceRepository serviceRepo)
        {
            _txnRepo = txnRepo;
            _fundRepo = fundRepo;
            _complaintRepo = complaintRepo;
            _docRepo = docRepo;
            _userRepo = userRepo;
            _walletRepo = walletRepo;
            _providerRepo = providerRepo;
            _userTypeRepo = userTypeRepo;
            _walletMasterRepo = walletMasterRepo;
            _serviceRepo = serviceRepo;
            _logger = new Logger();
        }

        /// <summary>
        /// The platform operator's own tiers, as opposed to the distribution network. These accounts
        /// are excluded from the network-float figures: they are the house, not a counterparty.
        /// </summary>
        private static bool IsOperatorTier(int userTypeId) =>
            userTypeId == UserTypeCode.Admin
            || userTypeId == UserTypeCode.SupportUser
            || userTypeId == UserTypeCode.Accountant;

        /// <summary>
        /// The SURCHARGE the operator kept on one transaction — the fee the retailer paid, as
        /// opposed to commission the operator earned from the provider.
        ///
        /// Charge is where a surcharge is recorded; on older rows it only appears as a NEGATIVE
        /// retailer commission (the same money expressed on the commission column), so both are
        /// read, Charge first. Magnitude, not sign: a surcharge is income to the operator however
        /// the row stores it. Returns 0 for a transaction that simply paid commission down.
        /// </summary>
        private static decimal SurchargeOf(TransactionMaster t) =>
            t.Charge != 0m ? Math.Abs(t.Charge)
            : t.RetailerCommission < 0m ? -t.RetailerCommission
            : 0m;

        public async Task<(string failureMessage, DashboardViewModel? data)> Select(int userId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", null);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // NOTE: these repos share one scoped DbContext, which cannot run concurrent
                // operations (EF: "A second operation was started on this context instance").
                // Await sequentially — do NOT parallelize with Task.WhenAll.
                var txns = await _txnRepo.GetAll(0, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var allComplaints = await _complaintRepo.GetAll();
                var allDocs = await _docRepo.GetAll();

                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                var failed = txns.Where(t => t.Status == StatusType.Failed).ToList();
                var rofferSuccess = success.Where(t => t.IsROffer).ToList();
                // Surcharge the operator kept today. Held once: it is both reported on its own and
                // subtracted from the net to leave the commission margin.
                decimal surchargeMargin = success.Sum(SurchargeOf);

                // A user's own wallet-to-wallet move (AEPS → Recharge) is written to this table
                // with the same user on both sides; it is their money changing pocket, not a fund
                // movement, and it must never be counted here.
                var fundsForDate = allFunds
                    .Where(f => f.TransactionDOC.Date == dateStart && f.SenderID != f.ReceiverID)
                    .ToList();
                // Fund transfers = what the operator sent down directly + the requests it approved
                // (both land here as DIRECT_TRANSFER / REQUEST_TRANSFER with the admin as sender).
                var transfers = fundsForDate.Where(f => f.SenderID == userId &&
                    (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer)).ToList();
                var pgTransfers = fundsForDate.Where(f => f.SenderID == userId && f.Through == Through_PGTransfer).ToList();

                decimal transferAmount = transfers.Sum(f => f.Amount) + pgTransfers.Sum(f => f.Amount);
                int transferCount = transfers.Count + pgTransfers.Count;

                var pendingComplaints = allComplaints.Where(c => c.DateTime.Date == dateStart &&
                    c.ComplaintStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToList();

                int kycCount = allDocs.Count(d => d.Status != null &&
                    d.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));

                decimal rofferTotal = rofferSuccess.Sum(t => t.Amount);
                decimal rofferComm = rofferSuccess.Sum(t => t.ROfferCommission);

                // Balances (sequential — shared scoped DbContext).
                var allWallets = await _walletRepo.GetAll();
                decimal adminWalletBalance = allWallets.Where(w => w.UserID == userId).Sum(w => w.Balance);
                // "User Wallet Float" is what the network holds, so every operator-tier account is
                // out of it — not just the signed-in admin. A second admin or a support account
                // used to land in the network figure and inflate it.
                var operatorUserIds = (await _userRepo.GetAllUsersWithDeleted())
                    .Where(u => IsOperatorTier(u.UserTypeID ?? 0))
                    .Select(u => u.UserMasterID)
                    .ToHashSet();
                operatorUserIds.Add(userId);
                decimal totalUserWalletBalance = allWallets
                    .Where(w => !operatorUserIds.Contains(w.UserID))
                    .Sum(w => w.Balance);

                var providers = await _providerRepo.GetAll();
                // Stock is what can be SOLD from, so a switched-off provider's last known balance
                // is not in the total and not on the list.
                decimal providerStockTotal = providers.Where(p => p.IsActive).Sum(p => p.RechargeBalance + p.DMTBalance);

                // Provider-wise stock list (already filtered to non-deleted by the repo).
                var providerStocks = providers
                    .Where(p => p.IsActive)
                    .Select(p => new ProviderStockItem
                    {
                        ProviderID = p.ProviderID,
                        ProviderName = p.ProviderName,
                        RechargeBalance = p.RechargeBalance,
                        DMTBalance = p.DMTBalance,
                        Total = p.RechargeBalance + p.DMTBalance,
                        IsActive = p.IsActive,
                    })
                    .OrderByDescending(p => p.Total)
                    .ToList();

                // Wallet float per network tier. Sequential awaits — shared scoped DbContext.
                var allUsers = await _userRepo.GetAllUsers();
                var userTypes = await _userTypeRepo.GetAll();
                var typeNameById = userTypes.ToDictionary(t => t.UserTypeID, t => t.UserType);
                var typeByUserId = allUsers.ToDictionary(u => u.UserMasterID, u => u.UserTypeID ?? 0);
                // Network Wallets is the float HELD BY THE NETWORK, so the operator's own money is
                // not one of its rows. It used to include an "Admin" line, which double-counted the
                // figure already shown on its own in the balance strip above and made the panel
                // total read as network float when it was network float plus the operator float.
                // Support and accountant accounts are excluded for the same reason: they are the
                // operator's own staff, not a distribution tier, and hold no trading balance.
                var userTypeBalances = allWallets
                    .Where(w => typeByUserId.ContainsKey(w.UserID))
                    .Where(w => !IsOperatorTier(typeByUserId[w.UserID]))
                    .GroupBy(w => typeByUserId[w.UserID])
                    .Select(g => new UserTypeBalanceItem
                    {
                        UserTypeID = g.Key,
                        UserType = typeNameById.TryGetValue(g.Key, out var n) ? n : $"Type {g.Key}",
                        UserCount = g.Select(w => w.UserID).Distinct().Count(),
                        Balance = g.Sum(w => w.Balance),
                    })
                    .OrderBy(t => t.UserTypeID)
                    .ToList();

                // Admin's own balance per wallet (Recharge / DMT / AEPS …). Select() is active-only, so
                // a disabled wallet is absent from the name map — skip those rows so a wallet the admin
                // turned off disappears from the dashboard too.
                var walletMasters = await _walletMasterRepo.Select();
                var walletNameById = walletMasters.ToDictionary(w => w.WalletID, w => w.Name);
                var adminWallets = allWallets
                    .Where(w => w.UserID == userId && walletNameById.ContainsKey(w.WalletID))
                    .Select(w => new WalletBalanceItem
                    {
                        WalletID = w.WalletID,
                        WalletName = walletNameById[w.WalletID],
                        Balance = w.Balance,
                    })
                    .OrderBy(w => w.WalletID)
                    .ToList();

                // Day volume per service, named from ServiceMaster (ids here are admin-created —
                // do NOT assume the old FinovativeAPI fixed ids).
                var services = await _serviceRepo.GetAllServices();
                var svcNameById = services.ToDictionary(s => s.ServiceID, s => s.Name);
                var serviceSplit = txns
                    .Where(t => t.Status == StatusType.Success || t.Status == StatusType.Failed)
                    .GroupBy(t => t.ServiceID)
                    .Select(g => new ServiceSplitItem
                    {
                        ServiceID = g.Key,
                        ServiceName = svcNameById.TryGetValue(g.Key, out var n) ? n : $"Service {g.Key}",
                        SuccessAmount = g.Where(t => t.Status == StatusType.Success).Sum(t => t.Amount),
                        FailedAmount = g.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount),
                        SuccessCount = g.Count(t => t.Status == StatusType.Success),
                        FailedCount = g.Count(t => t.Status == StatusType.Failed),
                    })
                    .OrderByDescending(s => s.SuccessAmount + s.FailedAmount)
                    .ToList();

                // Monthly Performance: day-wise successful volume, selected month vs the previous
                // month. One range query, grouped in memory.
                DateTime curMonthStart = new DateTime(dateStart.Year, dateStart.Month, 1);
                DateTime lastMonthStart = curMonthStart.AddMonths(-1);
                DateTime curMonthEnd = curMonthStart.AddMonths(1).AddTicks(-1);
                var monthTxns = await _txnRepo.GetAll(0, StatusType.Success, string.Empty, lastMonthStart, curMonthEnd, 1, int.MaxValue);
                int daysInChart = Math.Max(
                    DateTime.DaysInMonth(curMonthStart.Year, curMonthStart.Month),
                    DateTime.DaysInMonth(lastMonthStart.Year, lastMonthStart.Month));
                var curByDay = monthTxns.Where(t => t.DateTime >= curMonthStart)
                    .GroupBy(t => t.DateTime.Day).ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
                var lastByDay = monthTxns.Where(t => t.DateTime < curMonthStart)
                    .GroupBy(t => t.DateTime.Day).ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
                var monthlyPerformance = Enumerable.Range(1, daysInChart)
                    .Select(d => new DayPerfItem
                    {
                        Day = d,
                        CurrentMonth = curByDay.TryGetValue(d, out var c) ? c : 0,
                        LastMonth = lastByDay.TryGetValue(d, out var l) ? l : 0,
                    })
                    .ToList();

                // Pending (PROCESS) fund requests raised to this admin today.
                var pendingFundReqs = fundsForDate.Where(f =>
                    f.RequestToID == userId && f.Status.Equals("PROCESS", StringComparison.OrdinalIgnoreCase)).ToList();

                var vm = new DashboardViewModel
                {
                    TotalSuccessAmount = success.Sum(t => t.Amount),
                    TotalFailedAmount = failed.Sum(t => t.Amount),
                    TransferAmount = transferAmount,
                    TransferCount = transferCount,
                    ComplaintAmount = pendingComplaints.Sum(c => c.Amount),
                    ComplaintCount = pendingComplaints.Count,
                    KycCount = kycCount,
                    TotalTarget = 0,
                    RemainsTarget = 0,

                    PrePaidSuccess = success.Where(t => t.ServiceID == ServiceId_PrePaid).Sum(t => t.Amount),
                    PrePaidFail = failed.Where(t => t.ServiceID == ServiceId_PrePaid).Sum(t => t.Amount),
                    PostPaidSuccess = success.Where(t => t.ServiceID == ServiceId_PostPaid).Sum(t => t.Amount),
                    PostPaidFail = failed.Where(t => t.ServiceID == ServiceId_PostPaid).Sum(t => t.Amount),
                    DthSuccess = success.Where(t => t.ServiceID == ServiceId_DTH).Sum(t => t.Amount),
                    DthFail = failed.Where(t => t.ServiceID == ServiceId_DTH).Sum(t => t.Amount),
                    DMTSuccess = success.Where(t => t.ServiceID == ServiceId_DMT).Sum(t => t.Amount),
                    DMTFail = failed.Where(t => t.ServiceID == ServiceId_DMT).Sum(t => t.Amount),
                    AEPSSuccess = success.Where(t => t.ServiceID == ServiceId_AEPS).Sum(t => t.Amount),
                    AEPSFail = failed.Where(t => t.ServiceID == ServiceId_AEPS).Sum(t => t.Amount),

                    TotalAdminCommission = success.Sum(t => t.AdminCommission),
                    TotalAdminSurcharge = surchargeMargin,
                    // What the network was PAID on the day: every positive commission credited to
                    // a retailer, distributor or super-distributor. (A negative retailer figure is
                    // a surcharge, counted above, not a payout.)
                    TotalUserCommission = success.Sum(t =>
                        Math.Max(0m, t.RetailerCommission) + Math.Max(0m, t.DistributerCommission) + Math.Max(0m, t.SuperDistributerCommission)),
                    TotalUserCommissionCount = success.Count(t =>
                        t.RetailerCommission > 0m || t.DistributerCommission > 0m || t.SuperDistributerCommission > 0m),
                    // Admin profit, split by WHERE THE MONEY CAME FROM — the operator's two income
                    // streams, which is what "surcharge and commission profit, separately" asks for.
                    //
                    // AdminCommission is already the operator's NET on a transaction: the provider
                    // commission it earned, plus any surcharge the retailer paid, less every downline
                    // commission it funded (see AccountCoreProcessingService). Both streams are
                    // therefore inside that one column, so the split is a decomposition of it:
                    //
                    //   surcharge margin  = what the retailer was charged and the operator kept
                    //   commission margin = the net, with that surcharge taken back out
                    //
                    // The two still add up to the total exactly, with nothing double-counted. The
                    // earlier split partitioned TRANSACTIONS instead (surcharge-bearing vs not) and
                    // summed the whole net on each side, so a row's commission margin was reported
                    // as surcharge income whenever that row happened to carry any charge — the label
                    // said "surcharge" while the figure was mostly margin.
                    AdminSurchargeProfit = surchargeMargin,
                    AdminCommissionProfit = success.Sum(t => t.AdminCommission) - surchargeMargin,

                    RofferTotal = rofferTotal,
                    RofferComm = rofferComm,
                    RofferPer = rofferTotal > 0
                        ? (rofferComm / rofferTotal * 100).ToString("F2")
                        : "0",

                    AdminWalletBalance = adminWalletBalance,
                    TotalUserWalletBalance = totalUserWalletBalance,
                    ProviderStockTotal = providerStockTotal,
                    FundRequestPendingAmount = pendingFundReqs.Sum(f => f.Amount),
                    FundRequestPendingCount = pendingFundReqs.Count,

                    ProviderStocks = providerStocks,
                    UserTypeBalances = userTypeBalances,
                    AdminWallets = adminWallets,
                    ServiceSplit = serviceSplit,
                    MonthlyPerformance = monthlyPerformance,
                };

                return (StatusType.Ok, vm);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, DashboardViewModel? data)> SelectForRetailer(int userId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", null);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // Shared scoped DbContext — await sequentially (no concurrent EF operations).
                var txns = await _txnRepo.GetAll(userId, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var allFunds = await _fundRepo.GetAllFundTransactions();

                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                var failed = txns.Where(t => t.Status == StatusType.Failed).ToList();

                var fundsForDate = allFunds.Where(f => f.TransactionDOC.Date == dateStart).ToList();

                var received = fundsForDate.Where(f => f.ReceiverID == userId &&
                    (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer)).ToList();
                var pgReceived = fundsForDate.Where(f => f.ReceiverID == userId &&
                    (f.Through == Through_PGTransfer || f.Through == Through_ECollection)).ToList();

                var vm = new DashboardViewModel
                {
                    TotalSuccessAmount = success.Sum(t => t.Amount),
                    TotalFailedAmount = failed.Sum(t => t.Amount),
                    PurchaseAmount = received.Sum(f => f.Amount),
                    PurchaseCount = received.Count,
                    TotalCommission = success.Sum(t => t.RetailerCommission),
                    Surcharge = success.Sum(t => t.Charge),
                };

                return (StatusType.Ok, vm);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, DashboardViewModel? data)> SelectForDistributor(int userId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                    return ("Invalid date format.", null);

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = dateStart.AddDays(1).AddTicks(-1);

                // Who is asking — a Distributor sees their own network via DistributerUserID,
                // a Super-Distributor via SuperDistributerUserID (both are stamped on each txn
                // at commission time). Sequential awaits — shared scoped DbContext.
                var me = await _userRepo.GetUserById(userId);
                bool isSuper = (me?.UserTypeID ?? 0) == UserTypeCode.SuperDistributor;

                // All of the day's transactions, then scope to this user's network.
                var allTxns = await _txnRepo.GetAll(0, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var txns = allTxns
                    .Where(t => isSuper ? t.SuperDistributerUserID == userId : t.DistributerUserID == userId)
                    .ToList();

                var allFunds = await _fundRepo.GetAllFundTransactions();

                var success = txns.Where(t => t.Status == StatusType.Success).ToList();
                var failed = txns.Where(t => t.Status == StatusType.Failed).ToList();

                var fundsForDate = allFunds.Where(f => f.TransactionDOC.Date == dateStart).ToList();

                // Funds sent out (transfer to downline under this distributor)
                var sent = fundsForDate.Where(f => f.SenderID == userId &&
                    (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer)).ToList();

                // Funds received from upline / admin (purchase/top-up)
                var received = fundsForDate.Where(f => f.ReceiverID == userId &&
                    (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer ||
                     f.Through == Through_PGTransfer)).ToList();

                // Service-wise split for this network's day volume.
                var services = await _serviceRepo.GetAllServices();
                var svcNameById = services.ToDictionary(s => s.ServiceID, s => s.Name);
                var serviceSplit = txns
                    .Where(t => t.Status == StatusType.Success || t.Status == StatusType.Failed)
                    .GroupBy(t => t.ServiceID)
                    .Select(g => new ServiceSplitItem
                    {
                        ServiceID = g.Key,
                        ServiceName = svcNameById.TryGetValue(g.Key, out var n) ? n : $"Service {g.Key}",
                        SuccessAmount = g.Where(t => t.Status == StatusType.Success).Sum(t => t.Amount),
                        FailedAmount = g.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount),
                        SuccessCount = g.Count(t => t.Status == StatusType.Success),
                        FailedCount = g.Count(t => t.Status == StatusType.Failed),
                    })
                    .OrderByDescending(s => s.SuccessAmount + s.FailedAmount)
                    .ToList();

                // Direct downline (retailers under a distributor, distributors under a super-distributor):
                // wallet float + the day's sale each (indexed by the transacting user = t.UserID).
                var children = await _userRepo.GetUsersByParent(userId);
                var allWallets = await _walletRepo.GetAll();
                var walletByUser = allWallets.GroupBy(w => w.UserID)
                    .ToDictionary(g => g.Key, g => g.Sum(w => w.Balance));
                var userTypes = await _userTypeRepo.GetAll();
                var typeNameById = userTypes.ToDictionary(t => t.UserTypeID, t => t.UserType);
                // Per-downline-child sale, keyed by the RIGHT id for that tier: a super-distributor's
                // children are distributors (who don't transact directly), so attribute each distributor's
                // whole sub-network via DistributerUserID; a distributor's children are retailers, who
                // transact directly, so key by the transacting UserID. Both stay within the network-scoped
                // txn set, so the downline totals reconcile with Network Sale.
                var txnByChild = txns
                    .GroupBy(t => isSuper ? t.DistributerUserID : t.UserID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var downline = children
                    .Select(c =>
                    {
                        txnByChild.TryGetValue(c.UserMasterID, out var childTxns);
                        childTxns ??= new List<TransactionMaster>();
                        return new DownlineSummaryItem
                        {
                            UserID = c.UserMasterID,
                            FirmName = c.FirmName,
                            PersonName = c.PersonName,
                            UserTypeID = c.UserTypeID ?? 0,
                            UserType = typeNameById.TryGetValue(c.UserTypeID ?? 0, out var tn) ? tn : string.Empty,
                            MobileNumber = c.MobileNumber,
                            IsActive = c.IsActive,
                            WalletBalance = walletByUser.TryGetValue(c.UserMasterID, out var bal) ? bal : 0,
                            SuccessAmount = childTxns.Where(t => t.Status == StatusType.Success).Sum(t => t.Amount),
                            FailedAmount = childTxns.Where(t => t.Status == StatusType.Failed).Sum(t => t.Amount),
                            SuccessCount = childTxns.Count(t => t.Status == StatusType.Success),
                        };
                    })
                    .OrderByDescending(d => d.SuccessAmount)
                    .ThenBy(d => d.FirmName)
                    .ToList();

                var vm = new DashboardViewModel
                {
                    TotalSuccessAmount = success.Sum(t => t.Amount),
                    TotalFailedAmount = failed.Sum(t => t.Amount),
                    TransferAmount = sent.Sum(f => f.Amount),
                    TransferCount = sent.Count,
                    PurchaseAmount = received.Sum(f => f.Amount),
                    PurchaseCount = received.Count,
                    TotalCommission = isSuper
                        ? success.Sum(t => t.SuperDistributerCommission)
                        : success.Sum(t => t.DistributerCommission),
                    Surcharge = success.Sum(t => t.Charge),

                    // Own wallet float (this distributor's wallets).
                    AdminWalletBalance = walletByUser.TryGetValue(userId, out var myBal) ? myBal : 0,

                    ServiceSplit = serviceSplit,
                    Downline = downline,
                };

                return (StatusType.Ok, vm);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }
    }
}
