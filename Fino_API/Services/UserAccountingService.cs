using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserAccounting;
using FintechApp.Responses.UserAccounting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old UserAccountingBAL/UserAccountingDAL. Per-user accounting reconciliation
    // built from reliable sources: TransactionMaster (sale/commission/surcharge), FundTransaction
    // (purchase/reverse), UserBalanceHistory (opening), UserWalletMaster (closing), and
    // TransactionManualUpdate (manual adjustments). Old project's adjustment columns relied on
    // TransactionMaster.ChangeStatusDOC which is not yet populated by the status-update flow, so
    // adjustments here are derived from the reliable TransactionManualUpdate audit table instead.
    public class UserAccountingService : IUserAccountingService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly ITransactionManualUpdateRepository _manualRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IUserBalanceHistoryRepository _balHistoryRepo;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly Logger _logger;

        // Through-type constants mirror LedgerService for cross-report consistency
        private static readonly string[] PurchaseThrough = { "DIRECT_TRANSFER", "BY_REQUEST_TRANSFER", "PG-TRANSFER" };
        private static readonly string[] ReverseThrough = { "DIRECT_REVERSE", "BY_REVERSE_REQUEST_TRANSFER" };

        // Accounting is kept for the roles that actually transact and hold a float. Matched on the
        // numeric UserTypeID, not the display name: UserTypeMaster stores "Retailer" while
        // UserTypeConstants spells it "RETAILER", and the case-sensitive name compare this used to
        // do silently matched nothing — which is why the report always came back empty.
        private static readonly int[] AccountingUserTypes =
        {
            UserTypeCode.Retailer, UserTypeCode.Client, UserTypeCode.ApiUser
        };

        public UserAccountingService(
            ITransactionMasterRepository txnRepo,
            ITransactionManualUpdateRepository manualRepo,
            IFundTransactionRepository fundRepo,
            IUserBalanceHistoryRepository balHistoryRepo,
            IUserWalletMasterRepository userWalletRepo,
            IUserRepository userRepo,
            IWalletMasterRepository walletRepo)
        {
            _txnRepo = txnRepo;
            _manualRepo = manualRepo;
            _fundRepo = fundRepo;
            _balHistoryRepo = balHistoryRepo;
            _userWalletRepo = userWalletRepo;
            _userRepo = userRepo;
            _walletRepo = walletRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, UserwiseAccountingWrapper? data)> UserwiseAccounting(UserwiseAccountingRequest request, int currentUserID)
        {
            try
            {
                // The day to reconcile comes from Date, or the report screen's ToDate, or today.
                DateTime dayStart = request.EffectiveDate;
                DateTime dayEnd = dayStart.AddDays(1);
                bool isToday = dayStart == DateTime.Now.Date;

                var allUsers = await _userRepo.GetAllUsers();
                var allWallets = await _walletRepo.Select();

                // Accounting reconciles ONE wallet at a time. When the screen sends no wallet
                // (its dropdown defaults to "All"), fall back to the first wallet in display
                // order rather than refusing the request — the report then shows something
                // meaningful and the user can switch wallets from the filter.
                var wallet = request.WalletID > 0
                    ? allWallets.FirstOrDefault(w => w.WalletID == request.WalletID)
                    : allWallets.OrderBy(w => w.UISequence).ThenBy(w => w.WalletID).FirstOrDefault();
                if (wallet == null) return (Messages.Common.Message.DataNotFound, null);

                int walletId = wallet.WalletID;
                string walletName = wallet.Name ?? string.Empty;

                // Eligible accounting users (Retailer/Client/APIUser), optionally one user
                var users = allUsers
                    .Where(u => u.UserTypeID.HasValue && AccountingUserTypes.Contains(u.UserTypeID.Value))
                    .Where(u => request.UserID == 0 || u.UserMasterID == request.UserID)
                    .OrderBy(u => u.UserMasterID)
                    .ToList();

                if (!request.IsExcelDownload)
                    users = users.Skip(request.SkipCount).Take(request.TakeCount > 0 ? request.TakeCount : 50).ToList();

                if (users.Count == 0) return (Messages.Common.Message.DataNotFound, null);

                // Day's transactions for this wallet
                var dayTxns = (await _txnRepo.GetAll(0, string.Empty, string.Empty, dayStart, dayEnd, 1, int.MaxValue))
                    .Where(t => t.WalletID == walletId && t.DateTime >= dayStart && t.DateTime < dayEnd)
                    .ToList();

                var fundsAll = await _fundRepo.GetAllFundTransactions();
                var dayFunds = fundsAll
                    .Where(f => f.WalletID == walletId &&
                                f.TransactionDOC >= dayStart && f.TransactionDOC < dayEnd &&
                                f.Status == StatusType.Success)
                    .ToList();

                var manualAll = await _manualRepo.GetAll();
                var balHistAll = await _balHistoryRepo.GetAll();
                var walletBalAll = await _userWalletRepo.GetAll();

                // Manual adjustments: map TransactionMasterID → manual update (latest)
                var manualByTxn = manualAll
                    .GroupBy(m => m.TransactionMasterID)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.DateTime).First());

                var list = new List<UserwiseAccountingViewModel>();

                foreach (var user in users)
                {
                    int uid = user.UserMasterID;

                    var userSuccessTxns = dayTxns.Where(t => t.UserID == uid && t.Status == StatusType.Success).ToList();
                    decimal sale = userSuccessTxns.Sum(t => t.Amount);
                    decimal retailerCommission = userSuccessTxns.Where(t => t.RetailerCommission > 0).Sum(t => t.RetailerCommission);
                    decimal surcharge = Math.Abs(userSuccessTxns.Where(t => t.RetailerCommission < 0).Sum(t => t.RetailerCommission));

                    decimal purchase = dayFunds.Where(f => f.ReceiverID == uid && PurchaseThrough.Contains(f.Through)).Sum(f => f.Amount);
                    decimal reverseAmount = dayFunds.Where(f => f.SenderID == uid && ReverseThrough.Contains(f.Through)).Sum(f => f.Amount);

                    // Manual update count for this user's day transactions
                    var userTxnIDsToday = dayTxns.Where(t => t.UserID == uid).Select(t => t.TransactionMasterID).ToHashSet();
                    int manualUpdateCount = manualAll.Count(m => m.DateTime >= dayStart && m.DateTime < dayEnd && userTxnIDsToday.Contains(m.TransactionMasterID));

                    // Adjustments from the reliable TransactionManualUpdate audit table (failed + manually adjusted)
                    var (oldAdj, newAdj) = await ComputeAdjustmentTotals(uid, walletId, dayStart, dayEnd, manualByTxn);

                    decimal openingBal = balHistAll
                        .Where(b => b.UserID == uid && b.WalletID == walletId && b.DateTime < dayStart)
                        .OrderByDescending(b => b.DateTime)
                        .Select(b => b.Balance)
                        .FirstOrDefault();

                    decimal closingBal;
                    if (isToday)
                        closingBal = walletBalAll.Where(w => w.UserID == uid && w.WalletID == walletId).Sum(w => w.Balance);
                    else
                        closingBal = balHistAll
                            .Where(b => b.UserID == uid && b.WalletID == walletId && b.DateTime < dayEnd)
                            .OrderByDescending(b => b.DateTime)
                            .Select(b => b.Balance)
                            .FirstOrDefault();

                    decimal commissionAmount = retailerCommission - surcharge;
                    decimal totalCr = openingBal + purchase + oldAdj;
                    decimal totalDr = (sale - retailerCommission - reverseAmount) + surcharge;
                    decimal expClosing = totalCr - totalDr;
                    decimal difference = expClosing - closingBal;

                    list.Add(new UserwiseAccountingViewModel
                    {
                        UserID = uid,
                        FirmName = $"{user.FirmName} {user.AliasName}".Trim(),
                        UserMobile = user.MobileNumber,
                        Role = user.UserType?.UserType ?? string.Empty,
                        WalletName = walletName,
                        OpeningBal = openingBal,
                        Purchase = purchase,
                        Sale = sale,
                        CommsionAmount = commissionAmount,
                        Surcharge = surcharge,
                        TotalOldAmountAdjustment = oldAdj,
                        TotalNewAmountAdjustment = newAdj,
                        TotalCrAmt = totalCr,
                        TotalDrAmt = totalDr,
                        ClosingBalance = closingBal,
                        ExpClosingBal = expClosing,
                        Difference = difference,
                        ManualUpdateCount = manualUpdateCount
                    });
                }

                // Total row
                if (list.Count > 0)
                {
                    list.Add(new UserwiseAccountingViewModel
                    {
                        FirmName = "Total",
                        OpeningBal = list.Sum(x => x.OpeningBal),
                        Purchase = list.Sum(x => x.Purchase),
                        Sale = list.Sum(x => x.Sale),
                        CommsionAmount = list.Sum(x => x.CommsionAmount),
                        Surcharge = list.Sum(x => x.Surcharge),
                        TotalOldAmountAdjustment = list.Sum(x => x.TotalOldAmountAdjustment),
                        TotalNewAmountAdjustment = list.Sum(x => x.TotalNewAmountAdjustment),
                        TotalCrAmt = list.Sum(x => x.TotalCrAmt),
                        TotalDrAmt = list.Sum(x => x.TotalDrAmt),
                        ClosingBalance = list.Sum(x => x.ClosingBalance),
                        ExpClosingBal = list.Sum(x => x.ExpClosingBal),
                        Difference = list.Sum(x => x.Difference),
                        ManualUpdateCount = list.Sum(x => x.ManualUpdateCount)
                    });
                }

                return (StatusType.Ok, new UserwiseAccountingWrapper { Count = list.Count, list = list });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        // Computes (oldAdjustmentDebit, newAdjustmentDebit) for a user+wallet relative to the selected day,
        // using the reliable TransactionManualUpdate audit table joined to the user's transactions.
        private async Task<(decimal oldAdj, decimal newAdj)> ComputeAdjustmentTotals(
            int userID, int walletID, DateTime dayStart, DateTime dayEnd,
            Dictionary<int, TransactionManualUpdate> manualByTxn)
        {
            var userTxns = (await _txnRepo.GetAll(userID, string.Empty, string.Empty, null, null, 1, int.MaxValue))
                .Where(t => t.WalletID == walletID && t.Status == StatusType.Failed)
                .ToList();

            decimal oldAdj = 0, newAdj = 0;
            foreach (var t in userTxns)
            {
                if (!manualByTxn.TryGetValue(t.TransactionMasterID, out var mu)) continue;

                bool adjustedOnDay = mu.DateTime >= dayStart && mu.DateTime < dayEnd;
                bool createdBeforeDay = t.DateTime < dayStart;
                bool createdOnDay = t.DateTime >= dayStart && t.DateTime < dayEnd;

                // Old: created before the day, adjusted on the day → refund reflected today
                if (createdBeforeDay && adjustedOnDay)
                    oldAdj += t.DebitAmount;

                // New: created on the day, adjusted on a different (later) day
                if (createdOnDay && !adjustedOnDay)
                    newAdj += t.DebitAmount;
            }
            return (oldAdj, newAdj);
        }

        public async Task<(string failureMessage, AmountAdjustmentWrapper? data)> SelectForFilterByOldAmountAdjustment(UserAccountingCompRequest request, int currentUserID)
            => await AdjustmentReport(request, isOld: true);

        public async Task<(string failureMessage, AmountAdjustmentWrapper? data)> SelectForFilterByNewAmountAdjustment(UserAccountingCompRequest request, int currentUserID)
            => await AdjustmentReport(request, isOld: false);

        private async Task<(string failureMessage, AmountAdjustmentWrapper? data)> AdjustmentReport(UserAccountingCompRequest request, bool isOld)
        {
            try
            {
                if (!DateTime.TryParse(request.Date, out DateTime selDate) || request.WalletID <= 0)
                    return (Messages.Common.Message.InvalidRequest, null);

                DateTime dayStart = selDate.Date;
                DateTime dayEnd = dayStart.AddDays(1);

                var allUsers = await _userRepo.GetAllUsers();
                var userDict = allUsers.ToDictionary(u => u.UserMasterID, u => u);

                var manualAll = await _manualRepo.GetAll();
                var manualByTxn = manualAll
                    .GroupBy(m => m.TransactionMasterID)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.DateTime).First());

                var failedTxns = (await _txnRepo.GetAll(request.UserID, string.Empty, string.Empty, null, null, 1, int.MaxValue))
                    .Where(t => t.WalletID == request.WalletID && t.Status == StatusType.Failed)
                    .ToList();

                var list = new List<AmountAdjustmentViewModel>();
                foreach (var t in failedTxns)
                {
                    if (!manualByTxn.TryGetValue(t.TransactionMasterID, out var mu)) continue;

                    bool adjustedOnDay = mu.DateTime >= dayStart && mu.DateTime < dayEnd;
                    bool createdBeforeDay = t.DateTime < dayStart;
                    bool createdOnDay = t.DateTime >= dayStart && t.DateTime < dayEnd;

                    bool include = isOld ? (createdBeforeDay && adjustedOnDay) : (createdOnDay && !adjustedOnDay);
                    if (!include) continue;

                    userDict.TryGetValue(t.UserID, out var u);
                    list.Add(new AmountAdjustmentViewModel
                    {
                        TransactionMasterID = t.TransactionMasterID,
                        UserID = t.UserID,
                        FirmName = u != null ? $"{u.FirmName} {u.AliasName}".Trim() : string.Empty,
                        DOC = t.DateTime.ToString("dd/MM/yyyy hh:mm tt"),
                        UpdatedDOC = mu.DateTime.ToString("dd/MM/yyyy hh:mm tt"),
                        OperatorID = t.OperatorID,
                        ProviderID = t.ProviderID,
                        ConsumerNumber = t.ConsumerNumber,
                        Amount = t.Amount,
                        Commission = t.RetailerCommission,
                        DebitAmount = t.DebitAmount,
                        Status = t.Status,
                        Description = t.Description
                    });
                }

                if (list.Count == 0) return (Messages.Common.Message.DataNotFound, null);
                return (StatusType.Ok, new AmountAdjustmentWrapper { Count = list.Count, list = list });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, ManualSuccessFailWrapper? data)> ManualSuccessFail(UserAccountingCompRequest request, int currentUserID)
        {
            try
            {
                if (!DateTime.TryParse(request.Date, out DateTime selDate) || request.WalletID <= 0)
                    return (Messages.Common.Message.InvalidRequest, null);

                DateTime dayStart = selDate.Date;
                DateTime dayEnd = dayStart.AddDays(1);

                var manualUpdates = (await _manualRepo.GetAll())
                    .Where(m => m.DateTime >= dayStart && m.DateTime < dayEnd)
                    .ToList();
                if (manualUpdates.Count == 0) return (Messages.Common.Message.DataNotFound, null);

                var txnIDs = manualUpdates.Select(m => m.TransactionMasterID).Distinct().ToHashSet();
                var txns = (await _txnRepo.GetAll(request.UserID, string.Empty, string.Empty, null, null, 1, int.MaxValue))
                    .Where(t => t.WalletID == request.WalletID && txnIDs.Contains(t.TransactionMasterID))
                    .ToDictionary(t => t.TransactionMasterID, t => t);

                var allUsers = await _userRepo.GetAllUsers();
                var userDict = allUsers.ToDictionary(u => u.UserMasterID, u => u);

                var list = new List<ManualSuccessFailViewModel>();
                foreach (var mu in manualUpdates.OrderByDescending(m => m.DateTime))
                {
                    if (!txns.TryGetValue(mu.TransactionMasterID, out var t)) continue;
                    if (request.UserID != 0 && t.UserID != request.UserID) continue;
                    userDict.TryGetValue(t.UserID, out var u);

                    list.Add(new ManualSuccessFailViewModel
                    {
                        TransactionMasterID = mu.TransactionMasterID,
                        UserID = t.UserID,
                        FirmName = u != null ? $"{u.FirmName} {u.MobileNumber} {u.AliasName}".Trim() : string.Empty,
                        ConsumerNumber = t.ConsumerNumber,
                        OperatorID = t.OperatorID,
                        Amount = t.Amount,
                        OldStatus = mu.OldStatus,
                        NewStatus = mu.NewStatus,
                        ReferenceNo = mu.ProviderrefNo,
                        DOC = t.DateTime.ToString("dd/MM/yyyy hh:mm tt"),
                        UpdatedDOC = mu.DateTime.ToString("dd/MM/yyyy hh:mm tt")
                    });
                }

                if (list.Count == 0) return (Messages.Common.Message.DataNotFound, null);
                return (StatusType.Ok, new ManualSuccessFailWrapper { Count = list.Count, list = list });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }
    }
}
