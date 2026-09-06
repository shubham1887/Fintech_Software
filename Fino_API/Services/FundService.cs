using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Fund;
using FintechApp.Responses.Fund;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class FundService : IFundService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFundTransactionRepository _fundTransactionRepository;
        private readonly IUserWalletMasterRepository _userWalletMasterRepository;
        private readonly IOTPMasterRepository _otpMasterRepository;
        private readonly IEcollectionDetailsRepository _ecollectionDetailsRepository;
        private readonly IModeRepository _modeRepository;
        private readonly ISubModeRepository _subModeRepository;

        private const string TransferRequestType = "TRANSFER_REQUEST";
        private const string ReverseRequestType = "REVERSE_REQUEST";
        private const string TransferType = "TRANSFER";
        private const string ThroughRequestTransfer = "REQUEST_TRANSFER";
        private const string ThroughReverseRequestTransfer = "REVERSE_REQUEST_TRANSFER";
        private const string ThroughDirectTransfer = "DIRECT_TRANSFER";
        private const string ThroughDirectReverse = "DIRECT_REVERSE";
        private const string OTPTypeFundReverse = "FundReverseOTP";
        private const string StatusProcess = "PROCESS";
        private const string StatusSuccess = "SUCCESS";
        private const string StatusFailure = "FAILURE";

        public FundService(
            IUserRepository userRepository,
            IFundTransactionRepository fundTransactionRepository,
            IUserWalletMasterRepository userWalletMasterRepository,
            IOTPMasterRepository otpMasterRepository,
            IEcollectionDetailsRepository ecollectionDetailsRepository,
            IModeRepository modeRepository,
            ISubModeRepository subModeRepository)
        {
            _userRepository = userRepository;
            _fundTransactionRepository = fundTransactionRepository;
            _userWalletMasterRepository = userWalletMasterRepository;
            _otpMasterRepository = otpMasterRepository;
            _ecollectionDetailsRepository = ecollectionDetailsRepository;
            _modeRepository = modeRepository;
            _subModeRepository = subModeRepository;
        }

        private static FundRequestViewModel MapToViewModel(FundTransaction ft) => new()
        {
            FundTransactionID = ft.FundTransactionID,
            SenderID = ft.SenderID,
            SenderName = ft.Sender?.FirmName ?? string.Empty,
            SenderMobile = ft.Sender?.MobileNumber ?? string.Empty,
            ReceiverID = ft.ReceiverID,
            ReceiverName = ft.Receiver?.FirmName ?? string.Empty,
            ReceiverMobile = ft.Receiver?.MobileNumber ?? string.Empty,
            WalletID = ft.WalletID,
            WalletName = ft.WalletMaster?.Name ?? string.Empty,
            Amount = ft.Amount,
            OriginalAmount = ft.OriginalAmount,
            SenderOpeningBalance = ft.SenderOpeningBalance,
            SenderClosingBalance = ft.SenderClosingBalance,
            ReceiverOpeningBalance = ft.ReceiverOpeningBalance,
            ReceiverClosingBalance = ft.ReceiverClosingBalance,
            Remark = ft.Remark ?? string.Empty,
            UTRNumber = ft.UTRNumber ?? string.Empty,
            Status = ft.Status ?? string.Empty,
            Type = ft.Type ?? string.Empty,
            AdminRemark = ft.AdminRemark ?? string.Empty,
            PayMode = ft.PayMode ?? string.Empty,
            ModeID = ft.ModeID,
            ModeName = ft.Mode?.ModeName ?? string.Empty,
            SubModeID = ft.SubModeID,
            SubModeName = ft.SubMode?.SubModeName ?? string.Empty,
            Through = ft.Through ?? string.Empty,
            Description = ft.Description ?? string.Empty,
            BankName = ft.BankName ?? string.Empty,
            ReceiptPath = ft.ReceiptPath ?? string.Empty,
            TransactionDOC = ft.TransactionDOC,
            TransactionDOCInt = ft.TransactionDOCInt,
            DOCType = ft.Type == TransferRequestType ? "TransferRequest" : ft.Type == ReverseRequestType ? "ReverseRequest" : ft.Type ?? string.Empty,
            CashCollectionAmount = ft.CashCollectionAmount,
            CashCollectionRemark = ft.CashCollectionRemark ?? string.Empty,
            RequestToID = ft.RequestToID,
            RequestAcceptedUserID = ft.RequestAcceptedUserID,
            DOC = ft.DateTime
        };

        private static FundTransaction BuildFundTransaction(
            int senderID, int receiverID, int walletID, decimal amount, string remark,
            string type, string status, int requestToID, string through = "",
            decimal senderOpenBal = 0, decimal senderCloseBal = 0,
            decimal receiverOpenBal = 0, decimal receiverCloseBal = 0,
            string utrNumber = "", string payMode = "", string bankName = "",
            string adminRemark = "", string cashCollectionRemark = "", decimal cashCollectionAmount = 0,
            string description = "", int requestAcceptedUserID = 0,
            int? modeID = null, int? subModeID = null)
        {
            DateTime now = DateTime.Now;
            return new FundTransaction
            {
                SenderID = senderID,
                ReceiverID = receiverID,
                WalletID = walletID,
                Amount = amount,
                OriginalAmount = amount,
                CashCollectionAmount = cashCollectionAmount,
                Remark = string.IsNullOrEmpty(remark) ? "Fund transaction" : remark,
                UTRNumber = utrNumber,
                PayMode = payMode,
                ModeID = modeID,
                SubModeID = subModeID,
                SenderOpeningBalance = senderOpenBal,
                SenderClosingBalance = senderCloseBal,
                ReceiverOpeningBalance = receiverOpenBal,
                ReceiverClosingBalance = receiverCloseBal,
                Type = type,
                Status = status,
                RequestToID = requestToID,
                BankName = bankName,
                ReceiptPath = string.Empty,
                TransactionDOC = now,
                TransactionDOCInt = int.Parse(now.ToString("yyyyMMdd")),
                AdminRemark = string.IsNullOrEmpty(adminRemark) ? "System generated" : adminRemark,
                CashCollectionRemark = cashCollectionRemark,
                Description = description,
                Through = through,
                IsDelete = false,
                RequestAcceptedUserID = requestAcceptedUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        // Validates an optional Mode/SubMode pair: when a Mode is chosen, an active SubMode that
        // belongs to it is required. Returns the resolved nullable ids to persist on the transaction.
        private async Task<(string fail, int? modeID, int? subModeID)> ResolveModeSubMode(int modeIdIn, int subModeIdIn)
        {
            if (modeIdIn <= 0) return (StatusType.Ok, null, null);

            var mode = await _modeRepository.GetByID(modeIdIn);
            if (mode == null || !mode.IsActive) return (Messages.SubModeMessages.InvalidMode, null, null);
            if (subModeIdIn <= 0) return ("Please select a sub mode.", null, null);

            var subMode = await _subModeRepository.GetByID(subModeIdIn);
            if (subMode == null || !subMode.IsActive || subMode.ModeID != modeIdIn)
                return (Messages.SubModeMessages.NotFound, null, null);

            return (StatusType.Ok, modeIdIn, subModeIdIn);
        }

        private static bool IsDistOrSuperDist(UserMaster user) =>
            user.UserType?.UserType == UserTypeConstants.Distributor ||
            user.UserType?.UserType == UserTypeConstants.SuperDistributor;

        private static bool IsValidReverseRole(UserMaster user) =>
            user.UserType?.UserType == UserTypeConstants.Distributor ||
            user.UserType?.UserType == UserTypeConstants.SuperDistributor ||
            user.UserType?.UserType == UserTypeConstants.Admin;

        public async Task<string> RequestInsert(FundInsertRequest request, int userID)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (string.IsNullOrWhiteSpace(request.Remark)) return "Remark is required.";

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (!user.IsActive) return Messages.Common.Message.AccountInactive;

            // Wallet is no longer a form field — the source/destination now lives in Mode/SubMode.
            // Default to the user's main (Recharge) wallet, else their first wallet.
            var receiverWallets = await _userWalletMasterRepository.GetByUserID(userID);
            if (receiverWallets.Count == 0) return "Please select a valid wallet.";
            int walletID = request.WalletID > 0 && receiverWallets.Any(w => w.WalletID == request.WalletID)
                ? request.WalletID
                : (receiverWallets.FirstOrDefault(w => w.WalletID == (int)WalletCode.Recharge)?.WalletID
                   ?? receiverWallets.First().WalletID);

            var allTx = await _fundTransactionRepository.GetAllFundTransactions();

            bool duplicateRemark = allTx.Any(f => f.ReceiverID == userID && f.Remark == request.Remark && f.Status == StatusProcess);
            if (duplicateRemark) return "Duplicate remark. Please use a unique remark.";

            if (!string.IsNullOrWhiteSpace(request.UTRNumber))
            {
                bool duplicateUTR = allTx.Any(f => f.ReceiverID == userID && f.UTRNumber == request.UTRNumber);
                if (duplicateUTR) return "Duplicate UTR number.";
            }

            // New master-driven Mode / SubMode. When a Mode is chosen, a SubMode belonging
            // to that Mode is required; when a SubMode is chosen, it must match the chosen Mode.
            var (modeFail, modeID, subModeID) = await ResolveModeSubMode(request.ModeID, request.SubModeID);
            if (!modeFail.EqualsIgnoreCase(StatusType.Ok)) return modeFail;

            DateTime txnDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(request.TransactionDate) && request.TransactionDate.Length > 5)
                DateTime.TryParse(request.TransactionDate, out txnDate);

            int senderID = user.ParentID ?? userID;
            int requestToID = request.RequestTo > 0 ? request.RequestTo : senderID;

            var ft = BuildFundTransaction(
                senderID: senderID, receiverID: userID, walletID: walletID,
                amount: request.Amount, remark: request.Remark, type: TransferRequestType,
                status: StatusProcess, requestToID: requestToID, utrNumber: request.UTRNumber ?? string.Empty,
                requestAcceptedUserID: userID,
                modeID: modeID, subModeID: subModeID);

            ft.TransactionDOC = txnDate;
            ft.TransactionDOCInt = int.Parse(txnDate.ToString("yyyyMMdd"));
            ft.ReceiptPath = request.Document ?? string.Empty;

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund transfer request failed. Please try again.";

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, List<FundRequestViewModel> data)> SelectByFilter(FundSelectRequest request, int userID)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            // Show requests raised TO me (to approve) AND requests I raised (to track status) — so
            // retailers who only raise requests still see their own list, newest first.
            var query = allTx.Where(f => f.RequestToID == userID || f.ReceiverID == userID);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(f => f.TransactionDOC >= fromDate);

            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(f => f.TransactionDOC < toDate.Date.AddDays(1));

            var list = query.OrderByDescending(f => f.TransactionDOC).ToList();

            if (!string.IsNullOrEmpty(request.DOCType))
                list = list.Where(f => f.Type.Equals(request.DOCType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, new List<FundRequestViewModel>());
            return (StatusType.Ok, list.Select(MapToViewModel).ToList());
        }

        public async Task<(string failureMessage, FundRequestWrapper data)> SelectHeaderWithRequestByFilter(FundSelectRequest request, int userID)
        {
            var wrapper = new FundRequestWrapper();
            var (fail, list) = await SelectByFilter(request, userID);
            if (fail != StatusType.Ok) return (fail, wrapper);

            wrapper.Count = list.Count;
            wrapper.HeaderSummary = new FundHeaderSummary
            {
                TotalAmount = list.Sum(x => x.Amount),
                TotalCashCollection = list.Sum(x => x.CashCollectionAmount),
                TotalCount = list.Count
            };

            var paged = list.AsEnumerable();
            if (!string.IsNullOrEmpty(request.Search))
            {
                string s = request.Search.ToLower();
                paged = paged.Where(x => x.SenderName.ToLower().Contains(s) || x.ReceiverName.ToLower().Contains(s)
                    || x.Status.ToLower().Contains(s) || x.UTRNumber.Contains(s) || x.Remark.ToLower().Contains(s));
            }
            if (request.TakeCount > 0)
                paged = paged.Skip(request.SkipCount).Take(request.TakeCount);

            wrapper.list = paged.ToList();
            return (StatusType.Ok, wrapper);
        }

        public async Task<(string failureMessage, FundRequestViewModel? data)> SelectByID(int userID, int fundTransactionID)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var ft = allTx.FirstOrDefault(f => f.FundTransactionID == fundTransactionID &&
                (f.RequestToID == userID || f.SenderID == userID || f.ReceiverID == userID));

            if (ft == null) return (Messages.Common.Message.DataNotFound, null);
            return (StatusType.Ok, MapToViewModel(ft));
        }

        public async Task<string> AcceptReject(FundUpdateRequest request, int userID)
        {
            if (request.FundTransactionID <= 0) return "Invalid fund transaction ID.";
            if (string.IsNullOrWhiteSpace(request.Status)) return "Status is required.";

            var ft = await _fundTransactionRepository.GetFundTransactionById(request.FundTransactionID);
            if (ft == null) return Messages.FundTransactionMessages.FundTransactionNotFound;
            if (!ft.Status.Equals(StatusProcess, StringComparison.OrdinalIgnoreCase))
                return "This request has already been processed.";
            if (ft.SenderID != userID && ft.RequestToID != userID)
                return Messages.Common.Message.Unauthorized;

            ft.AdminRemark = string.IsNullOrWhiteSpace(request.SenderRemark)
                ? $"Processed by user {userID}" : request.SenderRemark;
            ft.UpdatedateTime = DateTime.UtcNow;

            // The approver may correct a wrongly-selected Mode / Sub-Mode before deciding.
            if (request.ModeID.HasValue && request.ModeID.Value > 0) ft.ModeID = request.ModeID.Value;
            if (request.SubModeID.HasValue && request.SubModeID.Value > 0) ft.SubModeID = request.SubModeID.Value;

            if (request.Status.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                ft.Status = StatusFailure;
                await _fundTransactionRepository.Update(ft);
                return StatusType.Ok;
            }

            if (!request.Status.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                return "Invalid status value. Use 'Accept' or 'Reject'.";

            if (request.Amount > 0) ft.Amount = request.Amount;

            var senderWallets = await _userWalletMasterRepository.GetByUserID(ft.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == ft.WalletID);
            if (senderWallet == null) return "Sender wallet not found.";
            if (senderWallet.Balance < ft.Amount)
                return $"{Messages.Common.Message.InsufficientBalance} Sender balance: {senderWallet.Balance:F2}";

            string through = ft.Type.Equals(ReverseRequestType, StringComparison.OrdinalIgnoreCase)
                ? ThroughReverseRequestTransfer : ThroughRequestTransfer;

            // Move the money atomically (race-safe), then record the resulting balances on the request.
            var (moveFail, deb, cred) = await MoveFundsAtomic(ft.SenderID, ft.ReceiverID, ft.WalletID, ft.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            ft.SenderOpeningBalance = deb.OpeningBalance;
            ft.SenderClosingBalance = deb.ClosingBalance;
            ft.ReceiverOpeningBalance = cred.OpeningBalance;
            ft.ReceiverClosingBalance = cred.ClosingBalance;
            ft.Status = StatusSuccess;
            ft.Through = through;
            ft.CashCollectionAmount = request.CashCollectionAmount;
            ft.CashCollectionRemark = request.CashCollectionRemark ?? string.Empty;
            ft.RequestAcceptedUserID = userID;

            await _fundTransactionRepository.Update(ft);

            return StatusType.Ok;
        }

        public async Task<string> Transfer(FundTransferRequest request, int userID, string ipAddress)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (request.ReceiverID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.ReceiverID == userID) return "You cannot transfer funds to yourself.";

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (!user.IsActive) return Messages.Common.Message.AccountInactive;

            var receiver = await _userRepository.GetUserById(request.ReceiverID);
            if (receiver == null || !receiver.IsActive) return "Receiver not found or inactive.";

            // Who can fund whom: admin may fund anyone (except themselves, already blocked above);
            // everyone else may only fund a user within their own downline.
            bool senderIsAdmin = user.UserTypeID == UserTypeCode.Admin || user.UserTypeID == UserTypeCode.SupportUser;
            if (!senderIsAdmin)
            {
                var allUsers = await _userRepository.GetAllUsers();
                if (!UserHierarchy.GetDownlineIds(allUsers, userID).Contains(request.ReceiverID))
                    return "You can only transfer funds to a user in your downline.";
            }

            if (!string.IsNullOrWhiteSpace(request.Remark))
            {
                var allTx = await _fundTransactionRepository.GetAllFundTransactions();
                if (allTx.Any(f => f.ReceiverID == request.ReceiverID && f.Remark == request.Remark && f.Status == StatusProcess))
                    return "Duplicate remark. Please use a unique remark.";
            }

            // Wallet is no longer a form field — default to the sender's main (Recharge) wallet.
            var senderWallets = await _userWalletMasterRepository.GetByUserID(userID);
            if (senderWallets.Count == 0) return "Sender wallet not found.";
            int walletID = request.WalletID > 0 && senderWallets.Any(w => w.WalletID == request.WalletID)
                ? request.WalletID
                : (senderWallets.FirstOrDefault(w => w.WalletID == (int)WalletCode.Recharge)?.WalletID
                   ?? senderWallets.First().WalletID);

            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == walletID);
            if (senderWallet == null) return "Sender wallet not found.";
            if (senderWallet.Balance < request.Amount)
                return $"{Messages.Common.Message.InsufficientBalance} Available: {senderWallet.Balance:F2}";

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(request.ReceiverID);
            var receiverWallet = receiverWallets.FirstOrDefault(w => w.WalletID == walletID);
            if (receiverWallet == null) return "Receiver wallet not found.";

            var (modeFail, modeID, subModeID) = await ResolveModeSubMode(request.ModeID, request.SubModeID);
            if (!modeFail.EqualsIgnoreCase(StatusType.Ok)) return modeFail;

            // Move the money atomically: debit the sender (guarded), then credit the receiver. The
            // check-and-apply is a single conditional UPDATE per side, so a concurrent transfer/recharge
            // on the same wallet cannot cause a lost update or oversell.
            var (moveFail, deb, cred) = await MoveFundsAtomic(userID, request.ReceiverID, walletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            var ft = BuildFundTransaction(
                senderID: userID, receiverID: request.ReceiverID, walletID: walletID,
                amount: request.Amount, remark: request.Remark, type: TransferType, status: StatusSuccess,
                requestToID: userID, through: ThroughDirectTransfer,
                senderOpenBal: deb.OpeningBalance, senderCloseBal: deb.ClosingBalance,
                receiverOpenBal: cred.OpeningBalance, receiverCloseBal: cred.ClosingBalance,
                utrNumber: request.UTRNumber ?? string.Empty,
                payMode: request.PayMode ?? string.Empty, bankName: request.BankName ?? string.Empty,
                cashCollectionAmount: request.CashCollectionAmount,
                cashCollectionRemark: request.CashCollectionRemark ?? string.Empty,
                adminRemark: request.Remark,
                description: $"Fund transfer to {receiver.FirmName}, Amount: {request.Amount:F2}",
                requestAcceptedUserID: userID,
                modeID: modeID, subModeID: subModeID);

            ft.ReceiptPath = request.Document ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.TransactionDate) && request.TransactionDate.Length > 5
                && DateTime.TryParse(request.TransactionDate, out DateTime tDate))
            {
                ft.TransactionDOC = tDate;
                ft.TransactionDOCInt = int.Parse(tDate.ToString("yyyyMMdd"));
            }

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund transfer failed. Please try again.";

            return StatusType.Ok;
        }

        // Atomically debit the sender then credit the receiver for an internal fund movement. Both
        // sides use TryApplyDelta (a single conditional UPDATE each) so the operation is race-safe
        // under concurrency. On a sender-side rejection nothing moves; on a receiver-side failure the
        // sender debit is rolled back so money is never lost. `senderBalanceForMessage` is only used
        // to format the insufficient-balance message.
        private async Task<(string fail, WalletDeltaResult debit, WalletDeltaResult credit)> MoveFundsAtomic(
            int senderID, int receiverID, int walletID, decimal amount, decimal senderBalanceForMessage)
        {
            var debit = await _userWalletMasterRepository.TryApplyDelta(senderID, walletID, -amount,
                allowOverdraft: false, respectHold: true);
            if (!debit.Applied)
                return ($"{Messages.Common.Message.InsufficientBalance} Available: {senderBalanceForMessage:F2}", default, default);

            var credit = await _userWalletMasterRepository.TryApplyDelta(receiverID, walletID, amount,
                allowOverdraft: false, respectHold: false);
            if (!credit.Applied)
            {
                // Undo the sender debit so no money is lost.
                await _userWalletMasterRepository.TryApplyDelta(senderID, walletID, amount, allowOverdraft: true, respectHold: false);
                return ("Receiver wallet not found.", default, default);
            }

            return (StatusType.Ok, debit, credit);
        }

        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectTransferReport(FundTransferReportRequest request, int userID)
        {
            var wrapper = new FundTransferReportWrapper();
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var query = await ScopeFundQuery(allTx, request.UserID, userID);

            // The report screen's status dropdown sends "ALL" for "no filter" — treating that as
            // a literal status is what used to make this report come back empty.
            if (IsRealFilter(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            if (IsRealFilter(request.Type))
                query = query.Where(f => f.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(f => f.TransactionDOC >= fromDate);
            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(f => f.TransactionDOC < toDate.Date.AddDays(1));
            if (request.WalletID > 0)
                query = query.Where(f => f.WalletID == request.WalletID);

            var list = query.OrderByDescending(f => f.TransactionDOC).ToList();
            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);

            wrapper.Count = list.Count;
            wrapper.HeaderViewModel = new FundHeaderSummary
            {
                TotalAmount = list.Sum(x => x.Amount),
                TotalCashCollection = list.Sum(x => x.CashCollectionAmount),
                TotalCount = list.Count
            };

            var mapped = list.Select(MapToViewModel).ToList();
            if (!string.IsNullOrEmpty(request.Search))
            {
                string s = request.Search.ToLower();
                mapped = mapped.Where(x => x.SenderName.ToLower().Contains(s) || x.ReceiverName.ToLower().Contains(s)
                    || x.UTRNumber.Contains(s) || x.Remark.ToLower().Contains(s)).ToList();
            }
            if (request.TakeCount > 0)
                mapped = mapped.Skip(request.SkipCount).Take(request.TakeCount).ToList();

            wrapper.data = mapped;
            return (StatusType.Ok, wrapper);
        }

        public async Task<string> ReverseRequest(FundReverseInsertRequest request, int userID)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (request.WalletID <= 0 || request.SenderID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.SenderID == userID) return "You cannot reverse funds from yourself.";

            // A reversal pulls funds back FROM a downline user, so the target must be in the
            // caller's downline (admins may reverse from anyone).
            var caller = await _userRepository.GetUserById(userID);
            bool callerIsAdmin = caller != null && (caller.UserTypeID == UserTypeCode.Admin || caller.UserTypeID == UserTypeCode.SupportUser);
            if (!callerIsAdmin)
            {
                var allUsersForScope = await _userRepository.GetAllUsers();
                if (!UserHierarchy.GetDownlineIds(allUsersForScope, userID).Contains(request.SenderID))
                    return "You can only reverse funds from a user in your downline.";
            }

            var senderWallets = await _userWalletMasterRepository.GetByUserID(request.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (senderWallet == null) return "Sender wallet not found.";
            if (senderWallet.Balance < request.Amount)
                return $"Insufficient balance. Sender balance: {senderWallet.Balance:F2}";

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(userID);
            if (!receiverWallets.Any(w => w.WalletID == request.WalletID))
                return "Please select a valid sender.";

            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: userID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: ReverseRequestType,
                status: StatusProcess, requestToID: userID, requestAcceptedUserID: userID);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Reverse request failed. Please try again.";
            return StatusType.Ok;
        }

        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectReverseReport(FundTransferReportRequest request, int userID)
        {
            var wrapper = new FundTransferReportWrapper();
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var query = (await ScopeFundQuery(allTx, request.UserID, userID))
                .Where(f => f.Type == ReverseRequestType || f.Type == "REVERSE");

            if (IsRealFilter(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(f => f.TransactionDOC >= fromDate);
            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(f => f.TransactionDOC < toDate.Date.AddDays(1));
            if (request.WalletID > 0)
                query = query.Where(f => f.WalletID == request.WalletID);

            var list = query.OrderByDescending(f => f.TransactionDOC).ToList();
            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);

            wrapper.Count = list.Count;
            wrapper.HeaderViewModel = new FundHeaderSummary
            {
                TotalAmount = list.Sum(x => x.Amount),
                TotalCount = list.Count
            };

            var mapped = list.Select(MapToViewModel).ToList();
            if (request.TakeCount > 0)
                mapped = mapped.Skip(request.SkipCount).Take(request.TakeCount).ToList();

            wrapper.data = mapped;
            return (StatusType.Ok, wrapper);
        }

        public async Task<string> Reverse(FundReverseWithOTPRequest request, int userID, string ipAddress)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (request.SenderID <= 0 || request.WalletID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.SenderID == userID) return "You cannot reverse your own funds.";

            var currentUser = await _userRepository.GetUserById(userID);
            if (currentUser == null) return Messages.UserMessages.UserNotFound;
            if (!IsValidReverseRole(currentUser)) return Messages.Common.Message.NoPermission;

            var senderUser = await _userRepository.GetUserById(request.SenderID);
            if (senderUser == null) return Messages.UserMessages.UserNotFound;

            int receiverID = senderUser.ParentID ?? userID;

            var senderWallets = await _userWalletMasterRepository.GetByUserID(request.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (senderWallet == null) return Messages.UserMessages.UserNotFound;
            if (senderWallet.Balance < request.Amount)
                return $"{Messages.Common.Message.InsufficientBalance} Balance: {senderWallet.Balance:F2}";

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(receiverID);
            var receiverWallet = receiverWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (receiverWallet == null) return Messages.UserMessages.UserNotFound;

            // Move the money atomically (race-safe) before recording the reversal.
            var (moveFail, deb, cred) = await MoveFundsAtomic(request.SenderID, receiverID, request.WalletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            string description = $"Fund reversed from {senderUser.FirmName}, Amount: {request.Amount:F2}";
            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: receiverID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: TransferType, status: StatusSuccess,
                requestToID: userID, through: ThroughDirectReverse,
                senderOpenBal: deb.OpeningBalance, senderCloseBal: deb.ClosingBalance,
                receiverOpenBal: cred.OpeningBalance, receiverCloseBal: cred.ClosingBalance,
                adminRemark: request.Remark, description: description, requestAcceptedUserID: userID);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund reversal failed. Please try again.";

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, GenerateFundReverseOTPResponse? data)> GenerateFundReverseOTP(GenerateFundReverseOTPRequest request, int userID)
        {
            if (request.SenderID <= 0 || request.Amount <= 0 || request.WalletID <= 0)
                return ("Invalid request parameters.", null);

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);
            if (!IsDistOrSuperDist(user)) return (Messages.Common.Message.NoPermission, null);

            var sender = await _userRepository.GetUserById(request.SenderID);
            if (sender == null) return (Messages.UserMessages.UserNotFound, null);

            var existing = await _otpMasterRepository.GetActiveByUserIdAndType(request.SenderID, OTPTypeFundReverse);
            if (existing != null)
            {
                existing.IsVerified = true;
                existing.UpdatedateTime = DateTime.UtcNow;
                await _otpMasterRepository.Update(existing);
            }

            string otpValue = new Random().Next(100000, 999999).ToString();
            var newOtp = new OTPMaster
            {
                UserID = request.SenderID,
                OTP = otpValue,
                Type = OTPTypeFundReverse,
                IsVerified = false,
                MobileNumber = sender.MobileNumber,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _otpMasterRepository.Insert(newOtp);

            // SMS/Message integration required here.
            // When ported: call MessageSelection().SendFundReverseOTP(sender) to deliver OTP via SMS.

            return (StatusType.Ok, new GenerateFundReverseOTPResponse
            {
                IsOTPSent = true,
                Amount = request.Amount,
                WalletID = request.WalletID,
                SenderID = request.SenderID,
                Pin = request.Pin,
                Remark = request.Remark
            });
        }

        public async Task<string> ReverseWithOTP(FundReverseWithOTPRequest request, int userID, string ipAddress)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (request.SenderID <= 0 || request.WalletID <= 0) return Messages.Common.Message.InvalidRequest;
            if (string.IsNullOrWhiteSpace(request.OTP)) return "OTP is required.";
            if (request.SenderID == userID) return "You cannot reverse your own funds.";

            var currentUser = await _userRepository.GetUserById(userID);
            if (currentUser == null) return Messages.UserMessages.UserNotFound;
            if (!IsValidReverseRole(currentUser)) return Messages.Common.Message.NoPermission;

            var otpRecord = await _otpMasterRepository.GetActiveByUserIdAndType(request.SenderID, OTPTypeFundReverse);
            if (otpRecord == null || otpRecord.OTP != request.OTP)
                return Messages.Common.Message.InvalidOTP;

            otpRecord.IsVerified = true;
            otpRecord.UpdatedateTime = DateTime.UtcNow;
            await _otpMasterRepository.Update(otpRecord);

            var senderUser = await _userRepository.GetUserById(request.SenderID);
            if (senderUser == null) return Messages.UserMessages.UserNotFound;

            if (currentUser.UserMasterID != senderUser.ParentID && currentUser.UserType?.UserType != UserTypeConstants.Admin)
                return "You are not authorized to reverse this user's funds.";

            int receiverID = senderUser.ParentID ?? userID;

            var senderWallets = await _userWalletMasterRepository.GetByUserID(request.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (senderWallet == null) return Messages.UserMessages.UserNotFound;
            if (senderWallet.Balance < request.Amount)
                return $"{Messages.Common.Message.InsufficientBalance} Balance: {senderWallet.Balance:F2}";

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(receiverID);
            var receiverWallet = receiverWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (receiverWallet == null) return Messages.UserMessages.UserNotFound;

            // Move the money atomically (race-safe) before recording the reversal.
            var (moveFail, deb, cred) = await MoveFundsAtomic(request.SenderID, receiverID, request.WalletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            string description = $"Fund reversed from {senderUser.FirmName}, Amount: {request.Amount:F2}";
            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: receiverID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: TransferType, status: StatusSuccess,
                requestToID: userID, through: ThroughDirectReverse,
                senderOpenBal: deb.OpeningBalance, senderCloseBal: deb.ClosingBalance,
                receiverOpenBal: cred.OpeningBalance, receiverCloseBal: cred.ClosingBalance,
                adminRemark: request.Remark, description: description, requestAcceptedUserID: userID);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund reversal failed. Please try again.";

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10ByReceiverID(int userID, FundSelectLast10ByReceiverRequest request)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var list = allTx
                .Where(f => f.ReceiverID == request.ReceiverID && f.WalletID == request.WalletID && f.RequestToID == userID)
                .OrderByDescending(f => f.TransactionDOC)
                .Take(10)
                .Select(MapToViewModel)
                .ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, list);
            return (StatusType.Ok, list);
        }

        public async Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10Request(int userID)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var list = allTx
                .Where(f => f.RequestToID == userID && f.Status == StatusProcess)
                .OrderByDescending(f => f.TransactionDOC)
                .Take(10)
                .Select(MapToViewModel)
                .ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, list);
            return (StatusType.Ok, list);
        }

        public async Task<(string failureMessage, List<FundRequestViewModel> data)> SelectLast10ForReverseTransaction(FundSelectLast10ForReverseRequest request, int userID)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var list = allTx
                .Where(f => f.SenderID == request.SenderID && f.WalletID == request.WalletID)
                .OrderByDescending(f => f.TransactionDOC)
                .Take(10)
                .Select(MapToViewModel)
                .ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, list);
            return (StatusType.Ok, list);
        }

        public async Task<(string failureMessage, EcollectionWrapper data)> SelectEcollection(FundSelectEcollectionRequest request)
        {
            var wrapper = new EcollectionWrapper();
            var all = await _ecollectionDetailsRepository.GetAll();
            var query = all.AsQueryable();

            if (request.UserID > 0)
                query = query.Where(x => x.UserID == request.UserID);
            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(x => x.DateTime >= fromDate);
            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(x => x.DateTime < toDate.Date.AddDays(1));

            var list = query.OrderByDescending(x => x.DateTime).ToList();
            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);

            wrapper.Count = list.Count;
            wrapper.list = list;
            return (StatusType.Ok, wrapper);
        }

        public async Task<string> Delete(int userID, int fundTransactionID)
        {
            var ft = await _fundTransactionRepository.GetFundTransactionById(fundTransactionID);
            if (ft == null) return Messages.Common.Message.DataNotFound;
            if (ft.ReceiverID != userID && ft.SenderID != userID)
                return Messages.Common.Message.Unauthorized;
            if (!ft.Status.Equals(StatusProcess, StringComparison.OrdinalIgnoreCase))
                return "Only pending transactions can be deleted.";

            ft.IsDelete = true;
            ft.IsDeleted = true;
            ft.UpdatedateTime = DateTime.UtcNow;
            await _fundTransactionRepository.Delete(ft);
            return StatusType.Ok;
        }

        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectWalletTransferReport(FundTransferReportRequest request, int userID)
        {
            return await SelectTransferReport(request, userID);
        }

        /// <summary>
        /// "ALL" (and the empty string) mean "do not filter" on the report screens' dropdowns.
        /// </summary>
        private static bool IsRealFilter(string? value)
            => !string.IsNullOrWhiteSpace(value) && !value.Trim().Equals("ALL", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Which fund movements a caller may see. Admin / Support operate the whole network and
        /// see every row; everyone else sees the rows they are a party to, plus those of their
        /// downline (a distributor funds their retailers, so those transfers are their business).
        /// A requested target user is honoured only when it is inside that reach.
        /// </summary>
        private async Task<IEnumerable<FundTransaction>> ScopeFundQuery(
            IEnumerable<FundTransaction> all, int requestedUserId, int callerUserId)
        {
            var caller = await _userRepository.GetUserById(callerUserId);
            bool isAdmin = caller != null &&
                (caller.UserTypeID == UserTypeCode.Admin || caller.UserTypeID == UserTypeCode.SupportUser);

            HashSet<int> reach;
            if (isAdmin)
            {
                if (requestedUserId <= 0) return all;   // whole network
                reach = new HashSet<int> { requestedUserId };
            }
            else
            {
                var allUsers = await _userRepository.GetAllUsers();
                reach = UserHierarchy.GetDownlineIds(allUsers, callerUserId);
                reach.Add(callerUserId);
                // A target outside the caller's branch is ignored, never honoured.
                if (requestedUserId > 0 && reach.Contains(requestedUserId))
                    reach = new HashSet<int> { requestedUserId };
            }

            return all.Where(f => reach.Contains(f.SenderID) || reach.Contains(f.ReceiverID) || reach.Contains(f.RequestToID));
        }
    }
}
