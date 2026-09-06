using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Ledger;
using FintechApp.Responses.Ledger;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LedgerService : ILedgerService
    {
        private readonly ITransactionDetailsRepository _txnDetailsRepo;
        private readonly ITransactionMasterRepository _txnMasterRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IUserBalanceHistoryRepository _balHistoryRepo;
        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IComplaintMasterRepository _complaintRepo;
        private readonly IVirtualBalanceRepository _virtualRepo;
        private readonly IServiceKindResolver _serviceKinds;
        private readonly Logger _logger;

        private const string DOCType_CashCollection = "CASH_COLLECTION";
        private const string DOCType_Recharge = "Recharge";
        private const string DOCType_FundTransfer = "FundTransfer";
        private const string DOCType_FundReceive = "FundReceive";
        private const string DOCType_FundReverse = "FundReverse";

        private const string Through_RequestTransfer = "BY_REQUEST_TRANSFER";
        private const string Through_DirectTransfer = "DIRECT_TRANSFER";
        private const string Through_DirectReverse = "DIRECT_REVERSE";
        private const string Through_ReverseRequest = "BY_REVERSE_REQUEST_TRANSFER";
        private const string Through_PGTransfer = "PG-TRANSFER";
        private const string Through_ECollection = "E-COLLECTION";

        // DB-authoritative role ids (UserTypeCode / UserTypeMasters seed).
        private const int UserTypeID_Admin = UserTypeCode.Admin;
        private const int UserTypeID_SuperDist = UserTypeCode.SuperDistributor;
        private const int UserTypeID_Dist = UserTypeCode.Distributor;
        private const int UserTypeID_Retailer = UserTypeCode.Retailer;
        private const int UserTypeID_APIUser = UserTypeCode.ApiUser;

        public LedgerService(
            ITransactionDetailsRepository txnDetailsRepo,
            ITransactionMasterRepository txnMasterRepo,
            IFundTransactionRepository fundRepo,
            IUserRepository userRepo,
            IWalletMasterRepository walletRepo,
            IServiceRepository serviceRepo,
            IUserBalanceHistoryRepository balHistoryRepo,
            IUserWalletMasterRepository userWalletRepo,
            IComplaintMasterRepository complaintRepo,
            IVirtualBalanceRepository virtualRepo,
            IServiceKindResolver serviceKinds)
        {
            _serviceKinds = serviceKinds;
            _virtualRepo = virtualRepo;
            _txnDetailsRepo = txnDetailsRepo;
            _txnMasterRepo = txnMasterRepo;
            _fundRepo = fundRepo;
            _userRepo = userRepo;
            _walletRepo = walletRepo;
            _serviceRepo = serviceRepo;
            _balHistoryRepo = balHistoryRepo;
            _userWalletRepo = userWalletRepo;
            _complaintRepo = complaintRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, LedgerWrapper? data)> Select(SelectLedgerRequest request, int loginUserID)
        {
            var response = new LedgerWrapper();
            try
            {
                if (string.IsNullOrWhiteSpace(request.FromDate) || string.IsNullOrWhiteSpace(request.ToDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                if (!DateTime.TryParse(request.FromDate, out DateTime fromDate) ||
                    !DateTime.TryParse(request.ToDate, out DateTime toDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                // ExportType → IsExcelDownload / IsPDFDownload
                if (request.ExportType.Equals("EXCEL", StringComparison.OrdinalIgnoreCase))
                    request.IsExcelDownload = true;
                else if (request.ExportType.Equals("PDF", StringComparison.OrdinalIgnoreCase))
                    request.IsPDFDownload = true;

                // If userID not supplied, default to caller's ID
                if (request.UserID == 0)
                    request.UserID = loginUserID;

                DateTime dateStart = fromDate.Date;
                DateTime dateEnd = toDate.Date.AddDays(1);

                // Load everything in parallel
                // Shared scoped DbContext — await sequentially (no concurrent EF queries).
                var allDetails = await _txnDetailsRepo.GetAll();
                var allUsers = await _userRepo.GetAllUsers();
                var allWallets = await _walletRepo.Select();
                var allServices = await _serviceRepo.GetAllServices();
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var allTxnMasters = await _txnMasterRepo.GetAll(0, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);

                // Determine login user type
                var loginUser = allUsers.FirstOrDefault(u => u.UserMasterID == loginUserID);
                int loginUserTypeID = loginUser?.UserTypeID ?? 0;

                // Retailer / APIUser can only view their own ledger
                if (loginUserTypeID == UserTypeID_Retailer || loginUserTypeID == UserTypeID_APIUser)
                    request.UserID = loginUserID;

                // Filter TransactionDetails
                var filtered = allDetails
                    .Where(t => t.DateTime >= dateStart && t.DateTime < dateEnd)
                    .Where(t => t.DOCType != DOCType_CashCollection)
                    .Where(t => request.WalletID == 0 || t.WalletID == request.WalletID);

                // Criteria-based filter
                if (request.Criteria.Equals("ConsumerNumber", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(request.CriteriaText))
                    filtered = filtered.Where(t => t.ConsumerNumber == request.CriteriaText);
                else if (request.Criteria.Equals("ReferenceID", StringComparison.OrdinalIgnoreCase) &&
                         int.TryParse(request.CriteriaText, out int refIdFilter))
                    filtered = filtered.Where(t => t.ReferenceID == refIdFilter);

                // UserID filter (positive = specific user; 0 or admin = all)
                if (request.UserID > 0)
                    filtered = filtered.Where(t => t.UserID == request.UserID);

                // Build lookup dictionaries for O(1) in-memory joins
                var userDict = allUsers.ToDictionary(u => u.UserMasterID, u => u);
                var walletDict = allWallets.ToDictionary(w => w.WalletID, w => w);
                var serviceDict = allServices.ToDictionary(s => s.ServiceID, s => s);
                var txnMasterDict = allTxnMasters.ToDictionary(m => m.TransactionMasterID, m => m);
                var fundDict = allFunds.ToDictionary(f => f.FundTransactionID, f => f);

                var report = new List<LedgerResponse>();
                foreach (var td in filtered.OrderByDescending(t => t.TransactionDetailID))
                {
                    userDict.TryGetValue(td.UserID, out var user);
                    userDict.TryGetValue(td.AssignedUserID, out var assignedUser);
                    walletDict.TryGetValue(td.WalletID, out var wallet);
                    serviceDict.TryGetValue(td.ServiceID, out var service);

                    string firmName = user != null
                        ? $"{user.FirmName} {user.MobileNumber} {user.PersonName}".Trim()
                        : string.Empty;

                    string referenceID = td.ReferenceID.ToString();
                    string transactionID = string.Empty;
                    string remark = string.Empty;
                    string rechargeDate = string.Empty;

                    // Enrich from TransactionMaster (Recharge type)
                    if (td.DOCType == DOCType_Recharge && txnMasterDict.TryGetValue(td.ReferenceID, out var txnMaster))
                    {
                        referenceID = txnMaster.ReferenceNo;
                        transactionID = txnMaster.ReferenceNo;
                        rechargeDate = txnMaster.DateTime.ToString("dd/MM/yyyy hh:mm tt");
                    }
                    // Enrich from FundTransaction (Fund types)
                    else if ((td.DOCType == DOCType_FundTransfer ||
                              td.DOCType == DOCType_FundReceive ||
                              td.DOCType == DOCType_FundReverse) &&
                             fundDict.TryGetValue(td.ReferenceID, out var fund))
                    {
                        referenceID = fund.FundTransactionID.ToString();
                        remark = fund.Remark ?? string.Empty;
                    }

                    report.Add(new LedgerResponse
                    {
                        TransactionDetailID = td.TransactionDetailID,
                        UserID = td.UserID,
                        FirmName = firmName,
                        AssignedUserName = assignedUser?.FirmName ?? string.Empty,
                        OpeningBalance = td.OpeningBalance,
                        ConsumerNumber = td.ConsumerNumber,
                        Amount = td.TransactionAmount,
                        WalletID = td.WalletID,
                        TDS = td.TDS,
                        WalletName = wallet?.Name ?? string.Empty,
                        ReferenceID = referenceID,
                        TransactionID = transactionID,
                        RechargeDate = rechargeDate,
                        DOCType = td.DOCType,
                        ClosingBalance = td.ClosingBalance,
                        CommissionAmount = td.DiscountAmount,
                        Description = td.TransactionDescription,
                        DOC = td.DateTime.ToString("dd/MM/yyyy hh:mm tt"),
                        AmountCreditDebit = td.AmountCreditDebit,
                        customAmount = new CustomAmount
                        {
                            Amount = td.TransactionAmount,
                            Status = td.AmountCreditDebit
                        },
                        ServiceName = service?.Name ?? string.Empty,
                        Remark = remark
                    });
                }

                // TransactionID criteria — applied after join (needs ReferenceNo from TransactionMaster)
                if (request.Criteria.Equals("TransactionID", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(request.CriteriaText))
                    report = report.Where(r => r.TransactionID == request.CriteriaText).ToList();

                response.Count = report.Count;

                bool isDownload = request.IsExcelDownload || request.IsPDFDownload;
                if (!isDownload && request.TakeCount > 0)
                    report = report.Skip(request.SkipCount).Take(request.TakeCount).ToList();

                // SearchText filter across key fields
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    string s = request.SearchText.ToLower();
                    report = report.Where(r =>
                        r.FirmName.ToLower().Contains(s) ||
                        r.ConsumerNumber.ToLower().Contains(s) ||
                        r.ReferenceID.ToLower().Contains(s) ||
                        r.TransactionID.ToLower().Contains(s) ||
                        r.DOCType.ToLower().Contains(s) ||
                        r.ServiceName.ToLower().Contains(s) ||
                        r.Description.ToLower().Contains(s)).ToList();
                }

                response.list = report;
                response.IsExcelDownload = request.IsExcelDownload;
                response.IsPDFDownload = request.IsPDFDownload;

                // Header computed only on first page
                if (request.SkipCount <= 0)
                {
                    var header = await ComputeHeader(request, loginUserTypeID, dateStart, toDate.Date, allFunds, allTxnMasters);
                    response.Headerlist.Add(header);
                }

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, AdminLedgerWrapper? data)> AdminLedger(AdminLedgerRequest request, int userID)
        {
            var response = new AdminLedgerWrapper();
            try
            {
                if (string.IsNullOrWhiteSpace(request.FromDate) || string.IsNullOrWhiteSpace(request.ToDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                if (!DateTime.TryParse(request.FromDate, out DateTime fromDate) ||
                    !DateTime.TryParse(request.ToDate, out DateTime toDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                DateTime dateStart = fromDate.Date;
                DateTime dateEnd = toDate.Date.AddDays(1);

                // Shared scoped DbContext — await sequentially (no concurrent EF queries).
                var allFunds = await _fundRepo.GetAllFundTransactions();
                var allWallets = await _walletRepo.Select();
                var walletDict = allWallets.ToDictionary(w => w.WalletID, w => w);

                var fundsInPeriod = allFunds
                    .Where(f => f.TransactionDOC >= dateStart && f.TransactionDOC < dateEnd)
                    .ToList();

                // Debit entries: user sent funds (sender)
                var debitEntries = fundsInPeriod
                    .Where(f => f.SenderID == userID && f.Status == StatusType.Success)
                    .Select(f => new
                    {
                        SortKey = f.FundTransactionID,
                        Item = new AdminLedgerViewModel
                        {
                            DOC = f.TransactionDOC.ToString("dd/MM/yyyy hh:mm tt"),
                            Description = f.Description ?? string.Empty,
                            Remark = f.AdminRemark ?? string.Empty,
                            DebitAmount = f.Amount,
                            CreditAmount = 0m,
                            OpeningBalance = f.SenderOpeningBalance,
                            ClosingBalance = f.SenderClosingBalance,
                            WalletID = f.WalletID,
                            BankName = f.BankName ?? string.Empty,
                            Through = f.Through ?? string.Empty
                        }
                    });

                // Credit entries: user received funds (receiver)
                var creditEntries = fundsInPeriod
                    .Where(f => f.ReceiverID == userID && f.Status == StatusType.Success)
                    .Select(f => new
                    {
                        SortKey = f.FundTransactionID,
                        Item = new AdminLedgerViewModel
                        {
                            DOC = f.TransactionDOC.ToString("dd/MM/yyyy hh:mm tt"),
                            Description = f.Description ?? string.Empty,
                            Remark = f.AdminRemark ?? string.Empty,
                            DebitAmount = 0m,
                            CreditAmount = f.Amount,
                            OpeningBalance = f.ReceiverOpeningBalance,
                            ClosingBalance = f.ReceiverClosingBalance,
                            WalletID = f.WalletID,
                            BankName = f.BankName ?? string.Empty,
                            Through = f.Through ?? string.Empty
                        }
                    });

                // Merge and sort by FundTransactionID descending
                var combined = debitEntries.Concat(creditEntries)
                    .OrderByDescending(x => x.SortKey)
                    .Select(x => x.Item)
                    .ToList();

                // Set WalletName from dictionary
                foreach (var item in combined)
                {
                    if (walletDict.TryGetValue(item.WalletID, out var wallet))
                        item.WalletName = wallet.Name;
                }

                // Optional WalletID filter
                if (request.WalletID != 0)
                    combined = combined.Where(x => x.WalletID == request.WalletID).ToList();

                if (!combined.Any())
                    return (Messages.Common.Message.DataNotFound, null);

                response.data = combined;
                response.IsExcelDownload = request.IsExcelDownload;
                response.Count = combined.Count;

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        /// <summary>
        /// Company (admin) wallet-wise accounting. For each platform wallet (or the selected one),
        /// computes over [FromDate, ToDate]:
        ///   OP / CL   = opening / closing admin wallet balance (balance-history + live wallet balance)
        ///   Sale      = successful transaction value routed through the wallet (TransactionMaster)
        ///   Purchase  = virtual balance loaded into the wallet (VirtualBalance)
        ///   Credit / Debit = admin wallet movement summed from the admin's own TransactionDetails
        ///   Commission = admin commission earned on the successful transactions
        /// The admin's own TransactionDetails IS the company statement (same source Admin Ledger shows).
        /// </summary>
        public async Task<(string failureMessage, AdminReconciliationWrapper? data)> AdminReconciliation(
            AdminLedgerRequest request, int userID, int userTypeID)
        {
            var response = new AdminReconciliationWrapper();
            try
            {
                if (userTypeID != UserTypeID_Admin && userTypeID != UserTypeCode.SupportUser)
                    return (Messages.Common.Message.InvalidRequest, null);

                if (string.IsNullOrWhiteSpace(request.FromDate) || string.IsNullOrWhiteSpace(request.ToDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                if (!DateTime.TryParse(request.FromDate, out DateTime fromDate) ||
                    !DateTime.TryParse(request.ToDate, out DateTime toDate))
                    return (Messages.Common.Message.InvalidRequest, null);

                DateTime dateStart = fromDate.Date;
                DateTime dateEnd = toDate.Date.AddDays(1);
                DateTime today = DateTime.Now.Date;

                // Company account = the admin's own wallet(s). Shared scoped DbContext → await sequentially.
                var allDetails = (await _txnDetailsRepo.GetByUserID(userID)).Where(t => !t.IsDeleted).ToList();
                var allWallets = await _walletRepo.Select();
                var balHist = await _balHistoryRepo.GetByUserID(userID);
                var walletBals = await _userWalletRepo.GetByUserID(userID);
                var txnMasters = await _txnMasterRepo.GetAll(0, string.Empty, string.Empty, dateStart, dateEnd, 1, int.MaxValue);
                var virtuals = await _virtualRepo.GetAll();

                var walletNames = allWallets.ToDictionary(w => w.WalletID, w => w.Name);

                // Business volume per wallet (platform-wide): Sale = successful transaction value,
                // Commission = admin commission earned on those, Purchase = virtual balance loaded.
                var successTxns = txnMasters.Where(t => t.Status == StatusType.Success).ToList();
                var saleByWallet = successTxns.GroupBy(t => t.WalletID)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
                var commByWallet = successTxns.GroupBy(t => t.WalletID)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.AdminCommission));
                var purchaseByWallet = virtuals
                    .Where(v => v.DateTime >= dateStart && v.DateTime < dateEnd)
                    .GroupBy(v => v.WalletID)
                    .ToDictionary(g => g.Key, g => g.Sum(v => v.Amount));

                // Wallets to report: the selected one, or every platform wallet (empty rows dropped below).
                IEnumerable<int> walletIds = request.WalletID != 0
                    ? new[] { request.WalletID }
                    : walletNames.Keys.OrderBy(id => id);

                var rows = new List<AdminReconciliationViewModel>();
                foreach (int wid in walletIds)
                {
                    var inPeriod = allDetails
                        .Where(t => t.WalletID == wid && t.DateTime >= dateStart && t.DateTime < dateEnd)
                        .OrderBy(t => t.TransactionDetailID)
                        .ToList();

                    decimal credit = inPeriod.Where(t => IsSide(t.AmountCreditDebit, 'C')).Sum(t => t.TransactionAmount);
                    decimal debit = inPeriod.Where(t => IsSide(t.AmountCreditDebit, 'D')).Sum(t => t.TransactionAmount);

                    // Opening = last balance-history point strictly before the range; else first in-range row's open.
                    var histOpen = balHist
                        .Where(b => b.WalletID == wid && b.DateTime.Date < dateStart)
                        .OrderByDescending(b => b.DateTime)
                        .FirstOrDefault();
                    decimal opening = histOpen?.Balance ?? (inPeriod.Count > 0 ? inPeriod[0].OpeningBalance : 0m);

                    // Closing = live wallet balance when the range ends today, else last balance-history
                    // point inside the range; fall back to the last in-range row's closing balance.
                    decimal closing;
                    if (toDate.Date == today && walletBals.Any(w => w.WalletID == wid))
                    {
                        closing = walletBals.Where(w => w.WalletID == wid).Sum(w => w.Balance);
                    }
                    else
                    {
                        var histClose = balHist
                            .Where(b => b.WalletID == wid && b.DateTime.Date < dateEnd)
                            .OrderByDescending(b => b.DateTime)
                            .FirstOrDefault();
                        closing = histClose?.Balance ?? (inPeriod.Count > 0 ? inPeriod[^1].ClosingBalance : opening);
                    }

                    rows.Add(new AdminReconciliationViewModel
                    {
                        WalletID = wid,
                        WalletName = walletNames.TryGetValue(wid, out var nm) && !string.IsNullOrWhiteSpace(nm)
                            ? nm : $"Wallet {wid}",
                        OpeningBalance = opening,
                        ClosingBalance = closing,
                        Sale = saleByWallet.TryGetValue(wid, out var sv) ? sv : 0m,
                        Purchase = purchaseByWallet.TryGetValue(wid, out var pv) ? pv : 0m,
                        Credit = credit,
                        Debit = debit,
                        Commission = commByWallet.TryGetValue(wid, out var cv) ? cv : 0m,
                    });
                }

                // When listing all wallets, hide dormant ones (no balance and no activity in the range).
                if (request.WalletID == 0)
                    rows = rows.Where(r => r.OpeningBalance != 0 || r.ClosingBalance != 0 || r.Sale != 0
                        || r.Purchase != 0 || r.Credit != 0 || r.Debit != 0 || r.Commission != 0).ToList();

                response.data = rows;
                response.Totals = new AdminReconciliationViewModel
                {
                    WalletID = 0,
                    WalletName = "Total",
                    OpeningBalance = rows.Sum(r => r.OpeningBalance),
                    ClosingBalance = rows.Sum(r => r.ClosingBalance),
                    Sale = rows.Sum(r => r.Sale),
                    Purchase = rows.Sum(r => r.Purchase),
                    Credit = rows.Sum(r => r.Credit),
                    Debit = rows.Sum(r => r.Debit),
                    Commission = rows.Sum(r => r.Commission),
                };
                response.Count = rows.Count;
                response.IsExcelDownload = request.IsExcelDownload;

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        // "Credit"/"Debit" (also tolerant of "CR"/"DR") → match on first letter.
        private static bool IsSide(string? value, char side)
            => !string.IsNullOrWhiteSpace(value) && char.ToUpperInvariant(value.Trim()[0]) == side;

        private async Task<LedgerHeader> ComputeHeader(
            SelectLedgerRequest request,
            int loginUserTypeID,
            DateTime dateStart,
            DateTime toDate,
            List<FundTransaction> allFunds,
            List<TransactionMaster> allTxnMasters)
        {
            var header = new LedgerHeader();
            int targetUserID = request.UserID;

            // Shared scoped DbContext — await sequentially (no concurrent EF queries).
            var balHist = targetUserID > 0
                ? await _balHistoryRepo.GetByUserID(targetUserID)
                : await _balHistoryRepo.GetAll();
            var walletBals = targetUserID > 0
                ? await _userWalletRepo.GetByUserID(targetUserID)
                : await _userWalletRepo.GetAll();
            var allComplaints = await _complaintRepo.GetAll();

            // Filter balance history by walletID if specified
            var filteredBalHist = request.WalletID != 0
                ? balHist.Where(b => b.WalletID == request.WalletID).ToList()
                : balHist;

            // Opening balance = last record before start of selected period
            var openingRecord = filteredBalHist
                .Where(b => b.DateTime.Date < dateStart)
                .OrderByDescending(b => b.DateTime)
                .FirstOrDefault();
            header.OpeningBal = openingRecord?.Balance ?? 0;

            // Closing balance: today = current wallet balance; past = history
            DateTime today = DateTime.Now.Date;
            if (toDate == today)
            {
                header.ClosingBalance = request.WalletID != 0
                    ? walletBals.Where(w => w.WalletID == request.WalletID).Sum(w => w.Balance)
                    : walletBals.Sum(w => w.Balance);
            }
            else
            {
                DateTime dayAfterTo = toDate.AddDays(1);
                var closingRecord = filteredBalHist
                    .Where(b => b.DateTime.Date < dayAfterTo)
                    .OrderByDescending(b => b.DateTime)
                    .FirstOrDefault();
                header.ClosingBalance = closingRecord?.Balance ?? 0;
            }

            // Fund transfer aggregations
            var fundsInPeriod = allFunds
                .Where(f => f.TransactionDOC.Date >= dateStart && f.TransactionDOC.Date <= toDate)
                .Where(f => request.WalletID == 0 || f.WalletID == request.WalletID)
                .ToList();

            if (loginUserTypeID == UserTypeID_Admin)
            {
                header.TransferAmount = fundsInPeriod
                    .Where(f => f.SenderID == targetUserID &&
                                (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                header.PGAmount = fundsInPeriod
                    .Where(f => f.SenderID == targetUserID && f.Through == Through_PGTransfer)
                    .Sum(f => f.Amount);
                header.ECollectionAmount = fundsInPeriod
                    .Where(f => f.SenderID == targetUserID && f.Through == Through_ECollection)
                    .Sum(f => f.Amount);
                // Admin: ReceiverAmount is virtualBal (not computed here; left at 0)
            }
            else
            {
                header.TransferAmount = fundsInPeriod
                    .Where(f => f.SenderID == targetUserID &&
                                (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                header.ReceiverAmount = fundsInPeriod
                    .Where(f => f.ReceiverID == targetUserID &&
                                (f.Through == Through_RequestTransfer || f.Through == Through_DirectTransfer))
                    .Sum(f => f.Amount);
                header.PGAmount = fundsInPeriod
                    .Where(f => f.ReceiverID == targetUserID && f.Through == Through_PGTransfer)
                    .Sum(f => f.Amount);
                header.ECollectionAmount = fundsInPeriod
                    .Where(f => f.ReceiverID == targetUserID && f.Through == Through_ECollection)
                    .Sum(f => f.Amount);
            }

            header.ReverseAmountDr = fundsInPeriod
                .Where(f => f.SenderID == targetUserID &&
                            (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                .Sum(f => f.Amount);
            header.ReverseAmountCr = fundsInPeriod
                .Where(f => f.ReceiverID == targetUserID &&
                            (f.Through == Through_DirectReverse || f.Through == Through_ReverseRequest))
                .Sum(f => f.Amount);

            // Transaction aggregations from TransactionMaster
            var userTxns = (targetUserID > 0 && loginUserTypeID != UserTypeID_Admin)
                ? allTxnMasters.Where(t => t.UserID == targetUserID).ToList()
                : allTxnMasters;

            var successTxns = userTxns.Where(t => t.Status == StatusType.Success).ToList();

            header.Requested = successTxns.Sum(t => t.Amount);
            header.Debited = successTxns.Sum(t => t.DebitAmount);
            // AEPS/MATM ids resolved by service name/category — never hardcoded (ids vary per deployment).
            var aepsIDs = await _serviceKinds.GetAEPSServiceIDs();
            var matmIDs = await _serviceKinds.GetMicroATMServiceIDs();
            header.AEPSAmount = successTxns.Where(t => aepsIDs.Contains(t.ServiceID)).Sum(t => t.Amount);
            header.MATMAmount = successTxns.Where(t => matmIDs.Contains(t.ServiceID)).Sum(t => t.Amount);

            // Commission / TDS / Surcharge per user type
            if (loginUserTypeID == UserTypeID_Admin)
            {
                header.CommissionAmount = successTxns.Sum(t => t.AdminCommission);
                header.TDS = 0;
                header.Surcharge = successTxns.Sum(t => t.Charge);
            }
            else if (loginUserTypeID == UserTypeID_SuperDist)
            {
                header.CommissionAmount = successTxns.Sum(t => t.SuperDistributerCommission);
                header.TDS = successTxns.Sum(t => t.SuperDistributerTDS);
                header.Surcharge = successTxns.Sum(t => t.Charge);
            }
            else if (loginUserTypeID == UserTypeID_Dist)
            {
                header.CommissionAmount = successTxns.Sum(t => t.DistributerCommission);
                header.TDS = successTxns.Sum(t => t.DistributerTDS);
                header.Surcharge = successTxns.Sum(t => t.Charge);
            }
            else
            {
                header.CommissionAmount = successTxns.Sum(t => t.RetailerCommission);
                header.TDS = successTxns.Sum(t => t.RetailerTDS);
                header.Surcharge = successTxns.Sum(t => t.Charge);
            }

            // Complaint amount in period for the user
            header.ComplaintAmount = allComplaints
                .Where(c => (targetUserID <= 0 || c.UserID == targetUserID) &&
                             c.DateTime.Date >= dateStart && c.DateTime.Date <= toDate)
                .Sum(c => c.Amount);

            // Expected closing balance and difference
            decimal totalCredit;
            decimal totalDebit;
            if (loginUserTypeID == UserTypeID_Admin)
            {
                totalCredit = header.OpeningBal + header.ReverseAmountCr + header.ReceiverAmount + header.PastFailAmount;
                totalDebit = header.Requested + header.TransferAmount + header.PastSuccessAmount +
                             header.PGAmount + header.ECollectionAmount + header.AEPSAmount + header.MATMAmount;
            }
            else
            {
                totalCredit = header.OpeningBal + header.ReverseAmountCr + header.CommissionAmount +
                              header.ReceiverAmount + header.PastFailAmount + header.PGAmount +
                              header.ECollectionAmount + header.AEPSAmount + header.MATMAmount;
                totalDebit = header.Requested + header.Surcharge + header.TransferAmount +
                             header.PastSuccessAmount + header.TDS;
            }
            header.ExpectedCloseBal = totalCredit - totalDebit;
            header.Difference = header.ClosingBalance - header.ExpectedCloseBal;

            return header;
        }
    }
}
