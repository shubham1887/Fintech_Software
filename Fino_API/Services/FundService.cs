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
        private readonly IWalletMasterRepository _walletRepository;
        private readonly ISettingMasterRepository _settingRepository;
        private readonly IMessageService _messageService;
        private readonly ISystemDowntimeService _downtime;
        private readonly IOutstandingService _outstanding;
        private readonly IWalletHoldHistoryRepository _walletHoldHistoryRepository;
        private readonly ITransactionMasterRepository _transactionMasterRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly Logger _logger = new();

        // Message Setting event names. These rows ship with the product and the admin can edit the
        // template text and switch SMS / WhatsApp / email on per event — but nothing in the fund
        // flows ever raised them, so a transfer, a request or an approval moved real money without
        // telling either side. The names match the seeded Type values exactly (matching is loose,
        // so spacing and case differences in an edited row still resolve).
        private const string EventWalletCredited = "Wallet Credited";
        private const string EventWalletDebited = "Wallet Debited";
        private const string EventRequestRaised = "Fund Request Raised";
        private const string EventRequestApproved = "Fund Request Approved";
        private const string EventRequestRejected = "Fund Request Rejected";

        private const string TransferRequestType = "TRANSFER_REQUEST";
        private const string ReverseRequestType = "REVERSE_REQUEST";
        private const string TransferType = "TRANSFER";
        // A completed reversal is its own kind of movement, not a transfer. It used to be written
        // as TRANSFER, which is why the Reverse report — which selects reversals by Type — never
        // showed a single direct reverse: the rows existed but under the transfer heading.
        private const string ReverseType = "REVERSE";
        private const string ThroughRequestTransfer = "REQUEST_TRANSFER";
        private const string ThroughReverseRequestTransfer = "REVERSE_REQUEST_TRANSFER";
        private const string ThroughDirectTransfer = "DIRECT_TRANSFER";
        private const string ThroughDirectReverse = "DIRECT_REVERSE";
        private const string OTPTypeFundReverse = "FundReverseOTP";
        private const string StatusProcess = "PROCESS";
        private const string StatusSuccess = "SUCCESS";
        private const string StatusFailure = "FAILURE";

        // Key Settings (SettingMaster) that bound a fund request / fund transfer. Each row is
        // honoured only while it is On and holds a positive number, so leaving one at 0 means
        // "no bound on that side" — the same rule ShowLimitInDMT already follows.
        private const string MinFundAmountKey = "FundMinAmount";
        private const string MaxFundAmountKey = "FundMaxAmount";

        // How long a fund-reverse OTP stays usable. With no window an unused OTP stayed valid for
        // ever, so a single text left a standing authorisation to drain a downline wallet.
        private static readonly TimeSpan ReverseOTPValidity = TimeSpan.FromMinutes(10);

        private const string EventFundReverseOTP = "Fund Reverse OTP";
        private const string EventReverseRequestRaised = "Fund Reverse Request Raised";

        public FundService(
            IUserRepository userRepository,
            IFundTransactionRepository fundTransactionRepository,
            IUserWalletMasterRepository userWalletMasterRepository,
            IOTPMasterRepository otpMasterRepository,
            IEcollectionDetailsRepository ecollectionDetailsRepository,
            IModeRepository modeRepository,
            ISubModeRepository subModeRepository,
            IWalletMasterRepository walletRepository,
            ISettingMasterRepository settingRepository,
            IMessageService messageService,
            ISystemDowntimeService downtime,
            IOutstandingService outstanding,
            IWalletHoldHistoryRepository walletHoldHistoryRepository,
            ITransactionMasterRepository transactionMasterRepository,
            IServiceRepository serviceRepository)
        {
            _downtime = downtime;
            _outstanding = outstanding;
            _walletHoldHistoryRepository = walletHoldHistoryRepository;
            _transactionMasterRepository = transactionMasterRepository;
            _serviceRepository = serviceRepository;
            _messageService = messageService;
            _userRepository = userRepository;
            _fundTransactionRepository = fundTransactionRepository;
            _userWalletMasterRepository = userWalletMasterRepository;
            _otpMasterRepository = otpMasterRepository;
            _ecollectionDetailsRepository = ecollectionDetailsRepository;
            _modeRepository = modeRepository;
            _subModeRepository = subModeRepository;
            _walletRepository = walletRepository;
            _settingRepository = settingRepository;
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
            ReverseKind = ResolveReverseKind(ft),
            CashCollectionAmount = ft.CashCollectionAmount,
            CashCollectionRemark = ft.CashCollectionRemark ?? string.Empty,
            RequestToID = ft.RequestToID,
            RequestAcceptedUserID = ft.RequestAcceptedUserID,
            DOC = ft.DateTime
        };

        /// <summary>
        /// Names what a reversal-shaped row actually is, for the Fund Reverse Report — which
        /// carries the raised requests and the completed reversals side by side.
        ///
        /// Status cannot tell them apart (an approved request and a direct reverse are both
        /// SUCCESS), and the raw tokens are not reader-facing. The legacy Through values are
        /// included because reversals predating the REVERSE type were written as TRANSFER rows
        /// marked only by Through.
        /// </summary>
        private static string ResolveReverseKind(FundTransaction ft)
        {
            if (ft.Type == ReverseRequestType) return "Request";
            if (ft.Type == ReverseType
                || ft.Through == ThroughDirectReverse
                || ft.Through == ThroughReverseRequestTransfer) return "Reversal";
            return string.Empty;
        }

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

            // A sub mode is only demanded when the chosen mode actually HAS one. Several seeded
            // modes ("Others", "UPI", "Wallet", "Cash Collection") carry no sub modes at all, and
            // the form hides the picker for them — so insisting on a value here refused a request
            // the user had no way to complete.
            if (subModeIdIn <= 0)
            {
                // Active ones only — that is exactly what the picker offers, so the server and the
                // form agree on whether there was anything to choose.
                bool hasSubModes = (await _subModeRepository.GetByModeID(modeIdIn)).Any(s => s.IsActive);
                return hasSubModes
                    ? ("Please select a sub mode.", null, null)
                    : (StatusType.Ok, modeIdIn, null);
            }

            var subMode = await _subModeRepository.GetByID(subModeIdIn);
            if (subMode == null || !subMode.IsActive || subMode.ModeID != modeIdIn)
                return (Messages.SubModeMessages.NotFound, null, null);

            return (StatusType.Ok, modeIdIn, subModeIdIn);
        }

        /// <summary>
        /// Was this fund movement taken ON CREDIT?
        ///
        /// Credit is a Mode master row, not a flag, so this is matched on the mode's NAME. Modes
        /// are admin-managed data seeded per install (seed_mode_submode.sql) and a reseller may
        /// delete and re-add the row, so its id differs between installs and cannot be hard-coded
        /// — matching the name is the only thing that survives that.
        /// </summary>
        private async Task<bool> IsCreditMode(int? modeID)
        {
            if (modeID is null or <= 0) return false;
            try
            {
                var mode = await _modeRepository.GetByID(modeID.Value);
                return mode != null
                    && (mode.ModeName ?? string.Empty).Trim().EqualsIgnoreCase(OutstandingLedger.CreditModeName);
            }
            catch (Exception ex)
            {
                // A mode lookup that failed must not be read as "not credit" silently — that would
                // fund a wallet and record no debt, which is the one outcome this feature exists to
                // prevent. Logged, and treated as credit so the posting (and its limit check) runs.
                _logger.Write(ex);
                return true;
            }
        }

        /// <summary>
        /// Credit may only be given to a DIRECT child.
        ///
        /// Ordinary funding is allowed anywhere in a downline, and for credit that is wrong. The
        /// debt created has exactly one creditor, and only the direct parent may collect it
        /// (EnsureMayCollect) — so if admin could put credit straight onto a retailer two levels
        /// down, the debt would be owed to admin while the only person positioned to collect cash
        /// from that retailer is their distributor. Nobody could clear it.
        ///
        /// So, with admin -> dist1 -> r1 and admin -> r2:
        ///   admin -> dist1   allowed (direct child)
        ///   dist1 -> r1      allowed (direct child)
        ///   admin -> r2      allowed (direct child — r2 reports to admin)
        ///   admin -> r1      REFUSED (r1 reports to dist1, not to admin)
        ///
        /// Admin is NOT exempt. Everywhere else in this file admin may fund anyone, and that
        /// exemption is precisely what breaks here: being allowed to create a debt is not the same
        /// as being able to collect it.
        /// </summary>
        private async Task<string> EnsureCreditGoesToDirectChild(int senderID, int receiverID)
        {
            var receiver = await _userRepository.GetUserById(receiverID);
            if (receiver == null) return Messages.UserMessages.UserNotFound;

            if (receiver.ParentID != senderID)
            {
                var parent = receiver.ParentID.HasValue
                    ? await _userRepository.GetUserById(receiver.ParentID.Value)
                    : null;

                return parent != null
                    ? $"Credit can only be given to a user directly under you. {receiver.FirmName} reports to {parent.FirmName}, so that credit has to come from {parent.FirmName}."
                    : "Credit can only be given to a user directly under you.";
            }

            return StatusType.Ok;
        }

        /// <summary>
        /// A reversal takes funds back out of a downline's wallet, so any debt for those funds goes
        /// with them: nobody owes for money they no longer have.
        ///
        /// Three deliberate choices here:
        ///
        ///  • It is CAPPED at what is actually owed. A retailer who was funded ₹1,00,000 in cash
        ///    and ₹50,000 on credit, then reversed ₹80,000, owes nothing afterwards — not minus
        ///    ₹30,000. A negative outstanding would be an advance payment, which nothing
        ///    downstream (the wallet tile, the report, the morning reminder) represents.
        ///
        ///  • It never fails the reversal. The money has already moved by the time this runs, and
        ///    refusing here would leave the operator with a reversal they cannot complete and no
        ///    way to retry. A posting that could not be written is logged instead.
        ///
        ///  • It sends no message of its own. The reversal flows already tell the user their
        ///    wallet was debited and why; a second text a moment later announcing that the matching
        ///    debt went away reads as a second movement.
        /// </summary>
        /// <summary>
        /// Takes back a credit posting whose money then failed to move.
        ///
        /// The debt was recorded first on purpose (the credit limit is enforced inside that
        /// posting), so when the wallet move is rejected a moment later the debt is for funds that
        /// never arrived and has to go. It bypasses the guards, because this is the undo of a
        /// change that already passed them: refusing it — because the limit has since been lowered,
        /// say — would leave exactly the inconsistency it exists to prevent. Nothing is messaged,
        /// because nothing was ever announced.
        /// </summary>
        private async Task UndoCreditPosting(OutstandingPosting posting)
        {
            try
            {
                var (fail, _) = await _outstanding.PostDebit(new OutstandingPosting
                {
                    UserID = posting.UserID,
                    ParentID = posting.ParentID,
                    Amount = posting.Amount,
                    Type = OutstandingLedger.TypeCollectionReversed,
                    Remark = "Credit cancelled - the funds could not be transferred",
                    FundTransactionID = posting.FundTransactionID,
                    WalletID = posting.WalletID,
                    BypassLimit = true,
                    SuppressMessage = true,
                });

                if (!fail.EqualsIgnoreCase(StatusType.Ok))
                    _logger.Write($"OUTSTANDING NOT ROLLED BACK: user {posting.UserID} owes {posting.Amount} for funds that were never transferred. Reason: {fail}");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                _logger.Write($"OUTSTANDING ROLLBACK THREW: user {posting.UserID} owes {posting.Amount} for funds that were never transferred.");
            }
        }

        private async Task ReduceOutstandingForReversal(int userID, int counterpartyID, decimal amount,
            string remark, int? fundTransactionID, int walletID)
        {
            try
            {
                if (!await _outstanding.IsEnabled()) return;

                decimal owed = await _outstanding.GetOutstanding(userID);
                if (owed <= 0) return;

                decimal toClear = Math.Min(owed, amount);
                if (toClear <= 0) return;

                var (fail, _) = await _outstanding.PostDebit(new OutstandingPosting
                {
                    UserID = userID,
                    ParentID = counterpartyID,
                    Amount = toClear,
                    Type = OutstandingLedger.TypeFundReverse,
                    Remark = string.IsNullOrWhiteSpace(remark) ? "Fund reversed" : remark,
                    FundTransactionID = fundTransactionID,
                    WalletID = walletID > 0 ? walletID : null,
                    SuppressMessage = true,
                });

                if (!fail.EqualsIgnoreCase(StatusType.Ok))
                    _logger.Write($"Outstanding not reduced on reversal: user {userID}, amount {toClear}, reason: {fail}");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }

        /// <summary>
        /// The min / max amount a fund request or fund transfer may carry, from Key Settings.
        ///
        /// Each bound is honoured only while its row is On and holds a positive number, so a row
        /// left at 0 means "no bound on that side" — the same rule the DMT limit already follows.
        /// A missing row means the same thing, so an install that has never set them behaves
        /// exactly as before.
        /// </summary>
        private async Task<(decimal min, decimal max)> ResolveFundBounds()
        {
            decimal min = await ResolveBound(MinFundAmountKey);
            decimal max = await ResolveBound(MaxFundAmountKey);
            // A max below the min would reject every amount, which reads as "fund transfer is
            // broken" rather than as a misconfigured setting. The narrower one is dropped.
            if (min > 0 && max > 0 && max < min) max = 0;
            return (min, max);
        }

        private async Task<decimal> ResolveBound(string key)
        {
            try
            {
                var row = await _settingRepository.GetByKey(key);
                if (row == null || !row.IsOn) return 0m;
                return decimal.TryParse(row.Value, out decimal value) && value > 0 ? value : 0m;
            }
            catch (Exception ex)
            {
                // A setting that cannot be read must not stop money moving — it falls back to
                // "no bound", which is the behaviour before the setting existed.
                _logger.Write(ex);
                return 0m;
            }
        }

        /// <summary>
        /// Check an amount against the configured bounds. Returns <see cref="StatusType.Ok"/> or
        /// the message to show, which names the bound so the user knows what to enter instead.
        /// </summary>
        /// <summary>
        /// StatusType.Ok unless a scheduled system-down window is open and this caller is subject
        /// to it. Fund movements belong to no service, so there is nothing for the operator's
        /// per-service exception list to exempt - they simply stop while the window is open.
        ///
        /// Admin and support are exempt, matching every other gate in the system: the roles that
        /// run the platform have to keep working during maintenance, or a window becomes something
        /// nobody can end.
        /// </summary>
        private async Task<string> EnsureFundsOpen(UserMaster? caller)
        {
            if (caller != null && IsAdminTier(caller)) return StatusType.Ok;
            return await _downtime.EnsureOpenForMoneyMovement();
        }

        private async Task<string> CheckFundBounds(decimal amount)
        {
            var (min, max) = await ResolveFundBounds();
            if (min > 0 && amount < min) return $"The minimum amount is {min:0.##}.";
            if (max > 0 && amount > max) return $"The maximum amount is {max:0.##}.";
            return StatusType.Ok;
        }

        /// <summary>
        /// Tell <paramref name="user"/> about a fund movement, over whichever channels the admin
        /// has switched on for that Message Setting event.
        ///
        /// Fire-and-forget by design: SendEventAsync queues the work on its own DI scope and never
        /// throws, so a dead SMS gateway can never fail — or roll back — a transfer that has
        /// already moved money. A user with no mobile number on file is simply skipped.
        ///
        /// The token names are the ones the seeded templates use ({Amount}, {Balance}, {Remark},
        /// {CustomerName}); anything a customised template asks for that is not supplied here is
        /// left as-is rather than blanked.
        /// </summary>
        private void Notify(UserMaster? user, string eventType, decimal amount, decimal balance,
            string remark, string fallbackSms, IDictionary<string, string>? extraTokens = null)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.MobileNumber)) return;

            var tokens = new Dictionary<string, string>
            {
                ["Amount"] = amount.ToString("F2"),
                ["Balance"] = balance.ToString("F2"),
                ["Remark"] = remark ?? string.Empty,
                ["CustomerName"] = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName ?? string.Empty : user.PersonName,
                ["Number"] = user.MobileNumber,
            };
            if (extraTokens != null)
                foreach (var kv in extraTokens) tokens[kv.Key] = kv.Value;

            _ = _messageService.SendEventAsync(eventType, user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID, tokens, fallbackSms, emailSubject: eventType);
        }

        // Role checks go through the numeric UserTypeID, never the UserTypeMaster.UserType text.
        //
        // That text is an admin-editable display name — this database ships it as "Admin",
        // "Super Distributor", "Distributor" — while UserTypeConstants spells the same roles
        // "ADMIN", "SUPER_DISTRIBUTOR", "DISTRIBUTOR". The comparison was therefore false for
        // every user in every install, which is why Fund Reverse (and the reverse OTP that
        // guards it) answered "You don't have permission" even to the administrator. The id is
        // what the rest of the system authorises on — the JWT, ApiAccessPolicy and Transfer
        // above all use it — so these now agree with it.
        private static bool IsDistOrSuperDist(UserMaster user) =>
            user.UserTypeID == UserTypeCode.Distributor ||
            user.UserTypeID == UserTypeCode.SuperDistributor;

        private static bool IsValidReverseRole(UserMaster user) =>
            IsDistOrSuperDist(user) || IsAdminTier(user);

        private static bool IsAdminTier(UserMaster user) =>
            user.UserTypeID == UserTypeCode.Admin ||
            user.UserTypeID == UserTypeCode.SupportUser;

        /// <summary>Request-shaped rows: the ones that are waiting for somebody's decision.</summary>
        private static bool IsRequestKind(string? type)
        {
            string t = (type ?? string.Empty).Trim();
            return t.EqualsIgnoreCase(TransferRequestType) || t.EqualsIgnoreCase(ReverseRequestType);
        }

        /// <summary>
        /// Whose queue this request belongs in.
        ///
        /// Normally the RequestToID the raiser chose. When that is missing - 0 on rows written
        /// before the field was populated, or by a path that never set it - fall back to the same
        /// answer RequestInsert would have reached: the raiser's parent, and the first
        /// administrator if they have none. Without the fallback those rows matched no queue at
        /// all, so nobody could approve them and nobody could see why.
        /// </summary>
        private static int ResolveApprover(FundTransaction ft, List<UserMaster> allUsers)
        {
            if (ft.RequestToID > 0) return ft.RequestToID;

            var raiser = allUsers.FirstOrDefault(u => u.UserMasterID == ft.ReceiverID);
            if (raiser?.ParentID is int parent && parent > 0) return parent;

            return allUsers.Where(u => u.UserTypeID == UserTypeCode.Admin)
                .OrderBy(u => u.UserMasterID)
                .Select(u => u.UserMasterID)
                .FirstOrDefault();
        }

        public async Task<string> RequestInsert(FundInsertRequest request, int userID)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (string.IsNullOrWhiteSpace(request.Remark)) return "Remark is required.";

            // Key Settings bounds (Fund - minimum / maximum amount). Checked before anything is
            // written, so a rejected amount leaves no half-made request behind.
            string boundsFail = await CheckFundBounds(request.Amount);
            if (!boundsFail.EqualsIgnoreCase(StatusType.Ok)) return boundsFail;

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (!user.IsActive) return Messages.Common.Message.AccountInactive;

            // Scheduled maintenance - see the note in Transfer.
            string fundDowntime = await EnsureFundsOpen(user);
            if (!fundDowntime.EqualsIgnoreCase(StatusType.Ok)) return fundDowntime;

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

            // Asking for funds on credit is asking to be lent money, so both credit rules are
            // checked here, at the point of asking. The binding checks happen on approval (the
            // limit atomically, inside the posting); these exist so the person is told while the
            // form is still open instead of waiting on an approval that could never complete.
            //
            // Placed after requestToID is resolved, because the first rule is about WHO is being
            // asked — the requester chooses that, so a credit request can otherwise be addressed
            // to somebody who is not their direct parent and would fail on approval.
            if (await IsCreditMode(modeID))
            {
                string directFail = await EnsureCreditGoesToDirectChild(requestToID, userID);
                if (!directFail.EqualsIgnoreCase(StatusType.Ok)) return directFail;

                string creditFail = await _outstanding.CheckCreditAllowed(userID, request.Amount);
                if (!creditFail.EqualsIgnoreCase(StatusType.Ok)) return creditFail;
            }

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

            // Acknowledge to the raiser that it is in, and tell the upline it is waiting on them —
            // the whole point of the queue is that someone acts on it, and until now nobody was
            // told a request existed.
            Notify(user, EventRequestRaised, request.Amount, 0m, request.Remark ?? string.Empty,
                $"Your fund request of Rs.{request.Amount:F2} has been submitted and is awaiting approval.");

            var approver = await _userRepository.GetUserById(requestToID);
            if (approver != null && approver.UserMasterID != user.UserMasterID)
                Notify(approver, EventRequestRaised, request.Amount, 0m, request.Remark ?? string.Empty,
                    $"{user.FirmName} has raised a fund request of Rs.{request.Amount:F2}. Please review it.");

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, List<FundRequestViewModel> data)> SelectByFilter(FundSelectRequest request, int userID, int userTypeID = 0)
        {
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var allUsers = await _userRepository.GetAllUsers();

            // WHO may see a request. Two parties and no others: the user who raised it, who needs
            // to track it, and the user it was raised TO, who has to decide it. Admin and support
            // see the whole queue because they are the platform's final approver - an unanswered
            // request has to be answerable by somebody even when the upline it names has gone.
            //
            // Another upline's queue is nobody else's business, and it used to leak: a row whose
            // RequestToID was never set (0, from older code and from rows created outside the
            // request flow) matched nothing, so ResolveApprover below decides those the same way
            // the request flow would have - the raiser's parent, else the admin - which puts every
            // row in exactly one queue instead of none or everyone's.
            bool isAdminTier = userTypeID == UserTypeCode.Admin || userTypeID == UserTypeCode.SupportUser;

            var query = isAdminTier
                ? allTx.AsEnumerable()
                : allTx.Where(f => f.ReceiverID == userID || ResolveApprover(f, allUsers) == userID);

            // This is the REQUEST queue. A plain transfer that happens to be sitting in PROCESS is
            // not a request and has no decision to make, but it was being listed beside them -
            // with an Accept / Reject pair that answers a question nobody asked. A caller that
            // wants one specific kind still says so through DOCType, below.
            if (!IsRealFilter(request.DOCType))
                query = query.Where(f => IsRequestKind(f.Type));

            // "ALL" is what a status dropdown sends for "no filter" — matching it literally against
            // a Status column returns nothing, which reads as an empty queue rather than as an
            // unfiltered one. IsRealFilter is the same guard the transfer/reverse reports use.
            if (IsRealFilter(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(f => f.TransactionDOC >= fromDate);

            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(f => f.TransactionDOC < toDate.Date.AddDays(1));

            var list = query.OrderByDescending(f => f.TransactionDOC).ToList();

            if (IsRealFilter(request.DOCType))
                list = list.Where(f => f.Type.Equals(request.DOCType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, new List<FundRequestViewModel>());

            // Name the approver on every row. "Only my requests are here" is a claim the screen
            // should be able to show rather than ask to be believed - and when an admin is looking
            // at the whole network's queue, it is the column that says whose decision each row is.
            // The operator's own accounts read "Admin", not the company's firm name: a retailer
            // raising a request to the platform is waiting on the admin, and "finocore" in that
            // column told them nothing.
            var nameByID = allUsers.ToDictionary(
                u => u.UserMasterID,
                u => u.UserTypeID == UserTypeCode.Admin || u.UserTypeID == UserTypeCode.SupportUser
                    ? "Admin"
                    : string.IsNullOrWhiteSpace(u.FirmName) ? (u.PersonName ?? string.Empty) : u.FirmName);

            var mapped = new List<FundRequestViewModel>(list.Count);
            foreach (var ft in list)
            {
                var row = MapToViewModel(ft);
                int approver = ResolveApprover(ft, allUsers);
                row.RequestToID = approver;
                row.RequestToName = nameByID.TryGetValue(approver, out var n) ? n : string.Empty;
                mapped.Add(row);
            }

            return (StatusType.Ok, mapped);
        }

        public async Task<(string failureMessage, FundRequestWrapper data)> SelectHeaderWithRequestByFilter(FundSelectRequest request, int userID, int userTypeID = 0)
        {
            var wrapper = new FundRequestWrapper();
            var (fail, list) = await SelectByFilter(request, userID, userTypeID);
            if (fail != StatusType.Ok) return (fail, wrapper);

            wrapper.Count = list.Count;
            wrapper.HeaderSummary = new FundHeaderSummary
            {
                TotalAmount = list.Sum(x => x.Amount),
                TotalCashCollection = list.Sum(x => x.CashCollectionAmount),
                TotalCount = list.Count
            };

            var matched = list.AsEnumerable();
            if (!string.IsNullOrEmpty(request.Search))
            {
                string s = request.Search.ToLower();
                matched = matched.Where(x => x.SenderName.ToLower().Contains(s) || x.ReceiverName.ToLower().Contains(s)
                    || x.Status.ToLower().Contains(s) || x.UTRNumber.Contains(s) || x.Remark.ToLower().Contains(s));
            }

            // Re-count AFTER the search. The count above is the unsearched total, which told the
            // pager there were more pages than the search had rows - so it offered a Next that
            // led to an empty page, and said "of 400" under a list of three.
            var searched = matched.ToList();
            wrapper.Count = searched.Count;
            wrapper.HeaderSummary.TotalCount = searched.Count;

            var paged = searched.AsEnumerable();
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
            // Who may decide. For a fund REQUEST the sender is the upline being asked for money,
            // so either side of it may act. For a reverse REQUEST the sender is the downline the
            // money would be pulled FROM — letting them decide would make the safeguard pointless
            // in one direction and self-approvable in the other — so only the operator it was
            // addressed to (or an admin) may accept or reject it.
            bool isReverseRequest = ft.Type.Equals(ReverseRequestType, StringComparison.OrdinalIgnoreCase);
            if (isReverseRequest)
            {
                var decider = await _userRepository.GetUserById(userID);
                if (decider == null || (ft.RequestToID != userID && !IsAdminTier(decider)))
                    return Messages.Common.Message.Unauthorized;
            }
            else if (ft.SenderID != userID && ft.RequestToID != userID)
            {
                return Messages.Common.Message.Unauthorized;
            }

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

                var rejected = await _userRepository.GetUserById(ft.ReceiverID);
                Notify(rejected, EventRequestRejected, ft.Amount, 0m, ft.AdminRemark ?? string.Empty,
                    $"Your fund request of Rs.{ft.Amount:F2} was REJECTED. Reason: {ft.AdminRemark}");
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

            string through = isReverseRequest ? ThroughReverseRequestTransfer : ThroughRequestTransfer;

            // Funds taken on credit become a debt the moment the request is approved, and the debt
            // is posted BEFORE the money moves. That order is not cosmetic: the credit limit is
            // enforced inside the posting's own UPDATE, so an over-limit approval has to be
            // refused while nothing has changed yet. If the wallet move then fails, the posting is
            // undone immediately below, so the two never disagree.
            //
            // Only a transfer request can be on credit. A reverse request moves money the other
            // way — out of the downline — and is handled after the move, where it REDUCES the debt.
            bool onCredit = !isReverseRequest && await IsCreditMode(ft.ModeID);
            if (onCredit)
            {
                // The requester picked who to ask (RequestTo), so a credit request can arrive at
                // somebody who is not their direct parent. Refused for the same reason as above:
                // that debt would have no one able to collect it.
                string directFail = await EnsureCreditGoesToDirectChild(ft.SenderID, ft.ReceiverID);
                if (!directFail.EqualsIgnoreCase(StatusType.Ok)) return directFail;
            }
            var creditPosting = onCredit
                ? new OutstandingPosting
                {
                    UserID = ft.ReceiverID,
                    ParentID = ft.SenderID,
                    Amount = ft.Amount,
                    Type = OutstandingLedger.TypeFundRequest,
                    Remark = string.IsNullOrWhiteSpace(ft.Remark) ? "Fund request on credit" : ft.Remark,
                    UTRNumber = ft.UTRNumber ?? string.Empty,
                    FundTransactionID = ft.FundTransactionID,
                    WalletID = ft.WalletID,
                    // Announced after the funds have actually landed - see IOutstandingService.Announce.
                    SuppressMessage = true,
                }
                : null;

            decimal creditClosing = 0m;
            if (creditPosting != null)
            {
                var (creditFail, posted) = await _outstanding.PostCredit(creditPosting);
                if (!creditFail.EqualsIgnoreCase(StatusType.Ok)) return creditFail;
                creditClosing = posted.ClosingBalance;
            }

            // Move the money atomically (race-safe), then record the resulting balances on the request.
            var (moveFail, deb, cred) = await MoveFundsAtomic(ft.SenderID, ft.ReceiverID, ft.WalletID, ft.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok))
            {
                if (creditPosting != null) await UndoCreditPosting(creditPosting);
                return moveFail;
            }

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

            // Approving a request moves money the same way a direct transfer does, so both sides
            // hear about it the same way.
            var creditedUser = await _userRepository.GetUserById(ft.ReceiverID);
            var debitedUser = await _userRepository.GetUserById(ft.SenderID);
            Notify(creditedUser, EventRequestApproved, ft.Amount, cred.ClosingBalance, ft.AdminRemark ?? string.Empty,
                $"Your fund request of Rs.{ft.Amount:F2} is APPROVED. Balance: Rs.{cred.ClosingBalance:F2}");
            Notify(debitedUser, EventWalletDebited, ft.Amount, deb.ClosingBalance, ft.AdminRemark ?? string.Empty,
                $"Rs.{ft.Amount:F2} debited for an approved fund request. Balance: Rs.{deb.ClosingBalance:F2}");

            // A second message, because it says a different thing: the one above is "your money
            // arrived", this one is "it was on credit and here is what you now owe". The person at
            // the counter needs both, and the admin can switch this template off in Message
            // Settings if they would rather it were only on the statement.
            if (creditPosting != null)
            {
                await _outstanding.RecordWalletBalances(creditPosting, cred.OpeningBalance, cred.ClosingBalance);
                await _outstanding.Announce(creditPosting, isCredit: true, ft.Amount, creditClosing);
            }

            // A reverse request pulls funds back OUT of the downline's wallet, so the debt for
            // those funds goes with them.
            if (isReverseRequest)
                await ReduceOutstandingForReversal(ft.SenderID, ft.ReceiverID, ft.Amount,
                    ft.AdminRemark ?? ft.Remark ?? string.Empty, ft.FundTransactionID, ft.WalletID);

            return StatusType.Ok;
        }

        public async Task<string> Transfer(FundTransferRequest request, int userID, string ipAddress)
        {
            if (request.Amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (request.ReceiverID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.ReceiverID == userID) return "You cannot transfer funds to yourself.";

            // Key Settings bounds (Fund - minimum / maximum amount). Checked first: nothing is
            // debited or recorded for an amount the operator has put out of range.
            string boundsFail = await CheckFundBounds(request.Amount);
            if (!boundsFail.EqualsIgnoreCase(StatusType.Ok)) return boundsFail;

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (!user.IsActive) return Messages.Common.Message.AccountInactive;

            // Scheduled maintenance stops money moving. Admin and support are exempt for the same
            // reason they are exempt from every other gate: they are the ones who have to be able
            // to fix and settle things while the platform is closed.
            string fundDowntime = await EnsureFundsOpen(user);
            if (!fundDowntime.EqualsIgnoreCase(StatusType.Ok)) return fundDowntime;

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

            // Sending funds on credit is the same debt as a credit fund request, raised from the
            // other end: the upline pushes money down without having been paid for it. Posted
            // before the money moves, for the reason given in AcceptReject, and undone if the
            // move fails.
            bool transferOnCredit = await IsCreditMode(modeID);
            if (transferOnCredit)
            {
                string directFail = await EnsureCreditGoesToDirectChild(userID, request.ReceiverID);
                if (!directFail.EqualsIgnoreCase(StatusType.Ok)) return directFail;
            }

            var creditPosting = transferOnCredit
                ? new OutstandingPosting
                {
                    UserID = request.ReceiverID,
                    ParentID = userID,
                    Amount = request.Amount,
                    Type = OutstandingLedger.TypeFundTransfer,
                    Remark = string.IsNullOrWhiteSpace(request.Remark) ? "Fund transfer on credit" : request.Remark,
                    UTRNumber = request.UTRNumber ?? string.Empty,
                    WalletID = walletID,
                    SuppressMessage = true,
                }
                : null;

            decimal creditClosing = 0m;
            if (creditPosting != null)
            {
                var (creditFail, posted) = await _outstanding.PostCredit(creditPosting);
                if (!creditFail.EqualsIgnoreCase(StatusType.Ok)) return creditFail;
                creditClosing = posted.ClosingBalance;
            }

            // Move the money atomically: debit the sender (guarded), then credit the receiver. The
            // check-and-apply is a single conditional UPDATE per side, so a concurrent transfer/recharge
            // on the same wallet cannot cause a lost update or oversell.
            var (moveFail, deb, cred) = await MoveFundsAtomic(userID, request.ReceiverID, walletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok))
            {
                if (creditPosting != null) await UndoCreditPosting(creditPosting);
                return moveFail;
            }

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

            // Both sides are told, because both sides' money moved: the receiver is credited and
            // the sender is debited. Raised only after the row is persisted, so a message can
            // never announce a transfer that did not get recorded.
            Notify(receiver, EventWalletCredited, request.Amount, cred.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} credited to your wallet by {user.FirmName}. Balance: Rs.{cred.ClosingBalance:F2}");
            Notify(user, EventWalletDebited, request.Amount, deb.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} transferred to {receiver.FirmName}. Balance: Rs.{deb.ClosingBalance:F2}");

            if (creditPosting != null)
            {
                // The statement row was written before the fund transaction existed and before the
                // wallet moved, so both are stamped on now - otherwise the statement shows a credit
                // line with no route back to the transfer that caused it and no sign of which
                // wallet it landed in.
                await _outstanding.LinkFundTransaction(creditPosting, ft.FundTransactionID);
                await _outstanding.RecordWalletBalances(creditPosting, cred.OpeningBalance, cred.ClosingBalance);
                await _outstanding.Announce(creditPosting, isCredit: true, request.Amount, creditClosing);
            }

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

        /// <summary>
        /// The fund reports' figures strip: what went through, what did not, what is still waiting
        /// to be approved, and how many. A fund transfer carries no commission of its own — moving
        /// float down the chain earns nobody a margin — so the strip states the cash collected
        /// against it instead, which is the figure the person reconciling a day's funding needs
        /// beside the transferred total.
        /// </summary>
        private static Dictionary<string, object> FundSummaryOf(List<FundRequestViewModel> rows)
        {
            bool Is(FundRequestViewModel r, params string[] words) =>
                words.Any(w => (r.Status ?? string.Empty).Equals(w, StringComparison.OrdinalIgnoreCase));

            var success = rows.Where(r => Is(r, "SUCCESS", "APPROVED", "COMPLETED")).ToList();
            var failed = rows.Where(r => Is(r, "FAILED", "FAILURE", "REJECTED", "CANCELLED")).ToList();
            var pending = rows.Where(r => Is(r, "PENDING", "PROCESS")).ToList();

            return new Dictionary<string, object>
            {
                ["Success Amount"] = success.Sum(r => r.Amount),
                ["Success Count"] = success.Count,
                ["Failed Amount"] = failed.Sum(r => r.Amount),
                ["Failed Count"] = failed.Count,
                ["Pending Amount"] = pending.Sum(r => r.Amount),
                ["Pending Count"] = pending.Count,
                ["Cash Collected"] = rows.Sum(r => r.CashCollectionAmount),
                ["Total Count"] = rows.Count,
            };
        }

        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectTransferReport(FundTransferReportRequest request, int userID)
        {
            var wrapper = new FundTransferReportWrapper();
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var query = await ScopeFundQuery(allTx, request.UserID, userID);

            // Fund movements BETWEEN PEOPLE only. A wallet transfer — AEPS earnings moved from one
            // of a user's own wallets into another — is written to this same table with the user on
            // both sides, so it was appearing here as a transfer from a firm to itself, twice (once
            // per leg), inflating the totals and reading as a duplicate of every settlement. Those
            // movements have their own report, which pairs the two legs properly: Settlement Report
            // (Fund/SelectWalletTransferReport), which is the only place they belong.
            query = query.Where(f => f.SenderID != f.ReceiverID);

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
                // Re-count after the search, so the pager's total describes the searched set.
                wrapper.Count = mapped.Count;
                wrapper.HeaderViewModel.TotalCount = mapped.Count;
            }

            // The figures strip, over the whole filtered set — built before paging, from `mapped`,
            // so it describes the same rows the pager counts (including after a search).
            wrapper.HeaderSummary = FundSummaryOf(mapped);

            if (request.TakeCount > 0 && !request.IsExcelDownload)
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

            // Scheduled maintenance - see the note in Transfer. Raising a reversal is a user
            // action like any other; DECIDING one (Reverse / AcceptReject) stays open, so a queue
            // that built up before the window can still be cleared during it.
            string fundDowntime = await EnsureFundsOpen(caller);
            if (!fundDowntime.EqualsIgnoreCase(StatusType.Ok)) return fundDowntime;

            var senderWallets = await _userWalletMasterRepository.GetByUserID(request.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (senderWallet == null) return "The selected wallet does not exist for that user.";
            if (senderWallet.Balance < request.Amount)
                return $"Insufficient balance. Their balance is {senderWallet.Balance:F2}.";

            // The requester is credited on approval, so they must hold the same wallet.
            var receiverWallets = await _userWalletMasterRepository.GetByUserID(userID);
            if (!receiverWallets.Any(w => w.WalletID == request.WalletID))
                return "You do not hold the wallet this reversal would be credited to.";

            // A reversal pulls money OUT of a live downline wallet, so the decision is not the
            // requester's own: it is routed to the platform operator, exactly like a fund request
            // is routed to an upline. It used to be inserted with RequestToID = the requester,
            // which put it in their own pending queue for them to approve themselves — and left
            // the admin with nothing to see on the Reverse Requests screen.
            int approverID = await ResolveReverseApproverID(userID);
            if (approverID <= 0) return "No administrator account is available to approve this request.";

            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: userID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: ReverseRequestType,
                status: StatusProcess, requestToID: approverID, requestAcceptedUserID: 0);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Reverse request failed. Please try again.";

            // Both ends hear about it: the raiser that it is in, and the operator that it is
            // waiting on them. A queue nobody is told about is a queue nobody works.
            Notify(caller, EventReverseRequestRaised, request.Amount, 0m, request.Remark ?? string.Empty,
                $"Your request to reverse Rs.{request.Amount:F2} has been submitted and is awaiting approval.");
            var approver = await _userRepository.GetUserById(approverID);
            var from = await _userRepository.GetUserById(request.SenderID);
            Notify(approver, EventReverseRequestRaised, request.Amount, 0m, request.Remark ?? string.Empty,
                $"{caller?.FirmName} has requested a reversal of Rs.{request.Amount:F2} from {from?.FirmName}. Please review it.");

            return StatusType.Ok;
        }

        /// <summary>
        /// Who decides a reverse request: the first active administrator. Support users are not
        /// used as the default approver — they work the queue when it is theirs to work, but the
        /// request has to be addressed to the account that owns the platform.
        /// </summary>
        private async Task<int> ResolveReverseApproverID(int requesterID)
        {
            var admins = await _userRepository.GetUsersByUserType(UserTypeCode.Admin);
            var admin = admins.FirstOrDefault(u => u.IsActive && !u.IsDeleted && u.UserMasterID != requesterID)
                        ?? admins.FirstOrDefault(u => !u.IsDeleted && u.UserMasterID != requesterID);
            return admin?.UserMasterID ?? 0;
        }

        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectReverseReport(FundTransferReportRequest request, int userID)
        {
            var wrapper = new FundTransferReportWrapper();
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            // Everything reversal-shaped, whatever it was written as:
            //   - REVERSE_REQUEST   : raised by the distributor tier, pending / decided
            //   - REVERSE           : a completed direct reversal (with or without OTP)
            //   - TRANSFER rows whose Through marks them as a reversal — how direct reverses and
            //     approved reverse requests were recorded before REVERSE existed. Without this the
            //     report silently started at the upgrade date and every earlier reversal vanished.
            var query = (await ScopeFundQuery(allTx, request.UserID, userID))
                // A user's own wallet-to-wallet move (AEPS → Recharge) is written as a
                // DIRECT_REVERSE leg out of the source wallet with the SAME user on both sides.
                // It is not a reversal of anything, and listing it here is what made this report
                // "wrong": every AEPS settlement showed up as money pulled back from the user.
                .Where(f => f.SenderID != f.ReceiverID)
                .Where(f => f.Type == ReverseRequestType
                            || f.Type == ReverseType
                            || f.Through == ThroughDirectReverse
                            || f.Through == ThroughReverseRequestTransfer);

            if (IsRealFilter(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            // "Type" narrows the same endpoint to one of its two readings: the pending/decided
            // REQUEST queue (Reverse Requests) or the completed reversals (Fund Reverse Report).
            if (IsRealFilter(request.Type))
            {
                if (request.Type.Equals(ReverseRequestType, StringComparison.OrdinalIgnoreCase))
                    query = query.Where(f => f.Type == ReverseRequestType);
                else
                    query = query.Where(f => f.Type != ReverseRequestType);
            }
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
            if (!string.IsNullOrEmpty(request.Search))
            {
                string s = request.Search.ToLower();
                mapped = mapped.Where(x => x.SenderName.ToLower().Contains(s)
                    || x.ReceiverName.ToLower().Contains(s)
                    || x.Status.ToLower().Contains(s)
                    || x.Remark.ToLower().Contains(s)).ToList();
                wrapper.Count = mapped.Count;
            }
            if (request.TakeCount > 0)
                mapped = mapped.Skip(request.SkipCount).Take(request.TakeCount).ToList();

            wrapper.data = mapped;
            return (StatusType.Ok, wrapper);
        }

        /// <summary>
        /// Lets a reversal use the HELD part of a wallet when the caller asked for it. Returns Ok
        /// when the free balance already covers the amount, or when the shortfall was released;
        /// otherwise a sentence naming the free figure and the choice.
        /// </summary>
        private async Task<string> ReleaseHoldForReversal(FundReverseWithOTPRequest request, UserWalletMaster senderWallet, int actorID)
        {
            decimal free = senderWallet.Balance - senderWallet.HoldBalance;
            if (request.Amount <= free) return StatusType.Ok;

            decimal shortfall = request.Amount - free;
            if (!request.ReleaseHold)
                return $"Only {Math.Max(0m, free):F2} is free to reverse: {senderWallet.HoldBalance:F2} of this wallet is on hold. "
                     + $"Reverse up to {Math.Max(0m, free):F2}, or choose to release {shortfall:F2} of the hold and reverse the full amount.";

            int released = await _userWalletMasterRepository.ApplyHoldDelta(request.SenderID, request.WalletID, -shortfall);
            if (released <= 0) return "The hold could not be released. Nothing was reversed.";
            try
            {
                await _walletHoldHistoryRepository.Insert(new WalletHoldHistory
                {
                    UserID = request.SenderID,
                    WalletID = request.WalletID,
                    Amount = -shortfall,
                    HoldBalanceAfter = senderWallet.HoldBalance - shortfall,
                    WalletBalance = senderWallet.Balance,
                    ActionByUserID = actorID,
                    Action = HoldAction.Release,
                    Remark = $"Released for fund reversal of {request.Amount:F2}. {request.Remark}".Trim(),
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now,
                    IsDeleted = false,
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
            return StatusType.Ok;
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
                return $"{Messages.Common.Message.InsufficientBalance} The wallet holds {senderWallet.Balance:F2} in total.";

            // Part of the wallet may be ON HOLD (a balance cap). The reversal can only take the
            // free part unless the caller said to release the hold; before this it answered a bare
            // "insufficient balance" against a wallet that visibly had the money, and the operator
            // had no way to see the hold or to do anything about it from here.
            string holdFail = await ReleaseHoldForReversal(request, senderWallet, userID);
            if (!holdFail.EqualsIgnoreCase(StatusType.Ok)) return holdFail;

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(receiverID);
            var receiverWallet = receiverWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (receiverWallet == null) return Messages.UserMessages.UserNotFound;

            // Move the money atomically (race-safe) before recording the reversal.
            var (moveFail, deb, cred) = await MoveFundsAtomic(request.SenderID, receiverID, request.WalletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            string description = $"Fund reversed from {senderUser.FirmName}, Amount: {request.Amount:F2}";
            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: receiverID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: ReverseType, status: StatusSuccess,
                requestToID: userID, through: ThroughDirectReverse,
                senderOpenBal: deb.OpeningBalance, senderCloseBal: deb.ClosingBalance,
                receiverOpenBal: cred.OpeningBalance, receiverCloseBal: cred.ClosingBalance,
                adminRemark: request.Remark, description: description, requestAcceptedUserID: userID);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund reversal failed. Please try again.";

            // A reversal takes money OUT of a downline wallet, which is the movement a user is
            // most likely to query — so they are told, with the reason the operator gave.
            Notify(senderUser, EventWalletDebited, request.Amount, deb.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} has been reversed from your wallet. Reason: {request.Remark}. Balance: Rs.{deb.ClosingBalance:F2}");
            var reversedTo = await _userRepository.GetUserById(receiverID);
            Notify(reversedTo, EventWalletCredited, request.Amount, cred.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} reversed from {senderUser.FirmName} to your wallet. Balance: Rs.{cred.ClosingBalance:F2}");

            // The funds have gone back up the chain, so the debt for them goes too - capped at
            // what is actually owed. See ReduceOutstandingForReversal.
            await ReduceOutstandingForReversal(request.SenderID, receiverID, request.Amount,
                request.Remark ?? string.Empty, ft.FundTransactionID, request.WalletID);

            return StatusType.Ok;
        }

        public async Task<(string failureMessage, GenerateFundReverseOTPResponse? data)> GenerateFundReverseOTP(GenerateFundReverseOTPRequest request, int userID)
        {
            if (request.SenderID <= 0 || request.Amount <= 0 || request.WalletID <= 0)
                return ("Invalid request parameters.", null);

            var user = await _userRepository.GetUserById(userID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);
            // Same roles that may complete the OTP-gated reversal (ReverseWithOTP admits an admin
            // reversing from anyone) may start it. Admitting one and not the other left the
            // administrator able to finish a flow they could not begin.
            if (!IsValidReverseRole(user)) return (Messages.Common.Message.NoPermission, null);

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

            // Deliver it. Without this the OTP was generated, stored and never sent, so the
            // OTP-gated reversal could not be completed by anyone — the code existed only in the
            // database. Fire-and-forget, over whichever channels the admin has switched on for
            // the "Fund Reverse OTP" event.
            // {OTP} has to be passed explicitly: every other event this Notify serves is an outcome
            // notice with no secret in it, so the shared token set does not carry one. Without it,
            // the moment a "Fund Reverse OTP" template exists it takes over from the fallback below
            // and sends the user everything about the reversal except the OTP they need to approve
            // it — leaving the reversal impossible to complete and nothing in the message to
            // explain why.
            Notify(sender, EventFundReverseOTP, request.Amount, 0m, request.Remark ?? string.Empty,
                $"OTP {otpValue} to authorise a reversal of Rs.{request.Amount:F2} from your wallet. "
                + $"Valid for {ReverseOTPValidity.TotalMinutes:0} minutes. Do not share it.",
                new Dictionary<string, string> { ["OTP"] = otpValue });

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

            // An OTP with no expiry is a standing authorisation to drain that wallet: one text,
            // never used, stayed valid for ever. Outside the window it is burnt and refused, so a
            // fresh one has to be requested.
            if (DateTime.UtcNow - otpRecord.DateTime > ReverseOTPValidity)
            {
                otpRecord.IsVerified = true;
                otpRecord.UpdatedateTime = DateTime.UtcNow;
                await _otpMasterRepository.Update(otpRecord);
                return "This OTP has expired. Please request a new one.";
            }

            otpRecord.IsVerified = true;
            otpRecord.UpdatedateTime = DateTime.UtcNow;
            await _otpMasterRepository.Update(otpRecord);

            var senderUser = await _userRepository.GetUserById(request.SenderID);
            if (senderUser == null) return Messages.UserMessages.UserNotFound;

            if (currentUser.UserMasterID != senderUser.ParentID && !IsAdminTier(currentUser))
                return "You are not authorized to reverse this user's funds.";

            int receiverID = senderUser.ParentID ?? userID;

            var senderWallets = await _userWalletMasterRepository.GetByUserID(request.SenderID);
            var senderWallet = senderWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (senderWallet == null) return Messages.UserMessages.UserNotFound;
            if (senderWallet.Balance < request.Amount)
                return $"{Messages.Common.Message.InsufficientBalance} The wallet holds {senderWallet.Balance:F2} in total.";

            // Part of the wallet may be ON HOLD (a balance cap). The reversal can only take the
            // free part unless the caller said to release the hold; before this it answered a bare
            // "insufficient balance" against a wallet that visibly had the money, and the operator
            // had no way to see the hold or to do anything about it from here.
            string holdFail = await ReleaseHoldForReversal(request, senderWallet, userID);
            if (!holdFail.EqualsIgnoreCase(StatusType.Ok)) return holdFail;

            var receiverWallets = await _userWalletMasterRepository.GetByUserID(receiverID);
            var receiverWallet = receiverWallets.FirstOrDefault(w => w.WalletID == request.WalletID);
            if (receiverWallet == null) return Messages.UserMessages.UserNotFound;

            // Move the money atomically (race-safe) before recording the reversal.
            var (moveFail, deb, cred) = await MoveFundsAtomic(request.SenderID, receiverID, request.WalletID, request.Amount, senderWallet.Balance);
            if (!moveFail.EqualsIgnoreCase(StatusType.Ok)) return moveFail;

            string description = $"Fund reversed from {senderUser.FirmName}, Amount: {request.Amount:F2}";
            var ft = BuildFundTransaction(
                senderID: request.SenderID, receiverID: receiverID, walletID: request.WalletID,
                amount: request.Amount, remark: request.Remark, type: ReverseType, status: StatusSuccess,
                requestToID: userID, through: ThroughDirectReverse,
                senderOpenBal: deb.OpeningBalance, senderCloseBal: deb.ClosingBalance,
                receiverOpenBal: cred.OpeningBalance, receiverCloseBal: cred.ClosingBalance,
                adminRemark: request.Remark, description: description, requestAcceptedUserID: userID);

            await _fundTransactionRepository.Insert(ft);
            if (ft.FundTransactionID <= 0) return "Fund reversal failed. Please try again.";

            // A reversal takes money OUT of a downline wallet, which is the movement a user is
            // most likely to query — so they are told, with the reason the operator gave.
            Notify(senderUser, EventWalletDebited, request.Amount, deb.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} has been reversed from your wallet. Reason: {request.Remark}. Balance: Rs.{deb.ClosingBalance:F2}");
            var reversedTo = await _userRepository.GetUserById(receiverID);
            Notify(reversedTo, EventWalletCredited, request.Amount, cred.ClosingBalance, request.Remark ?? string.Empty,
                $"Rs.{request.Amount:F2} reversed from {senderUser.FirmName} to your wallet. Balance: Rs.{cred.ClosingBalance:F2}");

            // The funds have gone back up the chain, so the debt for them goes too - capped at
            // what is actually owed. See ReduceOutstandingForReversal.
            await ReduceOutstandingForReversal(request.SenderID, receiverID, request.Amount,
                request.Remark ?? string.Empty, ft.FundTransactionID, request.WalletID);

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

        /// <summary>
        /// "Transfer to e-Balance" — a user moving their OWN money between their OWN wallets
        /// (AEPS → main, say). Nothing leaves their account; only which wallet holds it changes.
        ///
        /// This used to forward straight to <see cref="SelectTransferReport"/>, so the screen listed
        /// every fund movement in the network — downline transfers, requests, reversals — none of
        /// which is a wallet transfer. That is what made it "show wrong data".
        ///
        /// A wallet transfer is recognisable by the sender and receiver being the same user, and it
        /// is written as TWO rows: a DIRECT_REVERSE out of the source wallet and a DIRECT_TRANSFER
        /// into the destination. Listing both would double the amounts, so the pair is folded into
        /// one row that reads the way the user made it — "AEPS → Recharge, ₹500" — with the source
        /// wallet's balances on the sender side and the destination's on the receiver side.
        /// </summary>
        public async Task<(string failureMessage, FundTransferReportWrapper data)> SelectWalletTransferReport(FundTransferReportRequest request, int userID)
        {
            var wrapper = new FundTransferReportWrapper();
            var allTx = await _fundTransactionRepository.GetAllFundTransactions();
            var query = await ScopeFundQuery(allTx, request.UserID, userID);

            // Own-account moves only: same user on both sides.
            query = query.Where(f => f.SenderID == f.ReceiverID && f.SenderID > 0);

            if (IsRealFilter(request.Status))
                query = query.Where(f => f.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(f => f.TransactionDOC >= fromDate);
            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(f => f.TransactionDOC < toDate.Date.AddDays(1));

            var legs = query.ToList();

            var walletNames = (await _walletRepository.SelectAll())
                .GroupBy(w => w.WalletID).ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            // The OTHER half of an AEPS settlement: money paid OUT of the AEPS wallet to a bank
            // account (a payout). The report is read as "where did the AEPS earnings go", and
            // without these rows it only ever answered "into another wallet" — a settlement
            // straight to the bank was invisible here and the report read as wrong.
            var payoutRows = await AepsPayoutRows(request, userID, walletNames);

            if (legs.Count == 0 && payoutRows.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);

            // Pair the two legs of each transfer: same user, same amount, same instant. Both legs
            // are inserted from one timestamp variable, so the second is an exact match.
            var mapped = new List<FundRequestViewModel>();
            foreach (var move in legs.GroupBy(f => new { f.SenderID, f.Amount, f.TransactionDOC }))
            {
                var outLeg = move.FirstOrDefault(f => f.Through == ThroughDirectReverse);
                var inLeg = move.FirstOrDefault(f => f.Through == ThroughDirectTransfer);
                // A lone leg (an interrupted transfer, or an older row) is still worth showing —
                // reporting nothing would hide money that did move.
                var primary = inLeg ?? outLeg ?? move.First();

                var row = MapToViewModel(primary);
                row.WalletName = BuildWalletMoveLabel(outLeg, inLeg, walletNames, row.WalletName);
                // The user is both sides, so name the row after them once rather than twice.
                row.ReceiverName = row.SenderName;
                if (request.WalletID > 0 && !move.Any(f => f.WalletID == request.WalletID)) continue;
                mapped.Add(row);
            }

            if (request.WalletID <= 0 || request.WalletID == (int)WalletCode.AEPS)
                mapped.AddRange(payoutRows);

            if (!string.IsNullOrEmpty(request.Search))
            {
                string s = request.Search.ToLower();
                mapped = mapped.Where(x => x.SenderName.ToLower().Contains(s)
                    || x.WalletName.ToLower().Contains(s)
                    || x.Remark.ToLower().Contains(s)).ToList();
            }

            mapped = mapped.OrderByDescending(x => x.TransactionDOC).ToList();
            wrapper.Count = mapped.Count;
            wrapper.HeaderViewModel = new FundHeaderSummary
            {
                TotalAmount = mapped.Sum(x => x.Amount),
                TotalCashCollection = 0,
                TotalCount = mapped.Count
            };
            wrapper.HeaderSummary = FundSummaryOf(mapped);

            if (request.TakeCount > 0 && !request.IsExcelDownload)
                mapped = mapped.Skip(request.SkipCount).Take(request.TakeCount).ToList();

            wrapper.data = mapped;
            return (StatusType.Ok, wrapper);
        }

        /// <summary>
        /// Payouts made FROM the AEPS wallet, shaped like the wallet-move rows so the Settlement
        /// Report lists both kinds of settlement in one table: "AEPS → Bank (payout)".
        /// </summary>
        private async Task<List<FundRequestViewModel>> AepsPayoutRows(
            FundTransferReportRequest request, int callerUserID, Dictionary<int, string> walletNames)
        {
            var rows = new List<FundRequestViewModel>();
            try
            {
                var services = await _serviceRepository.GetAllServices();
                var payoutIds = ServiceKinds.IdsOf(services, ServiceKinds.Payout);
                if (payoutIds.Count == 0) return rows;

                DateTime? from = DateTime.TryParse(request.FromDate, out var f) ? f.Date : null;
                DateTime? to = DateTime.TryParse(request.ToDate, out var t) ? t.Date.AddDays(1).AddTicks(-1) : null;

                // Same reach as the fund rows: admin sees everyone (or the one user asked for),
                // everyone else their own branch.
                var caller = await _userRepository.GetUserById(callerUserID);
                bool isAdmin = caller != null &&
                    (caller.UserTypeID == UserTypeCode.Admin || caller.UserTypeID == UserTypeCode.SupportUser);
                HashSet<int>? reach = null;
                if (!isAdmin)
                {
                    reach = UserHierarchy.GetDownlineIds(await _userRepository.GetAllUsers(), callerUserID);
                    reach.Add(callerUserID);
                }
                int scopeUser = request.UserID > 0 && (isAdmin || reach!.Contains(request.UserID)) ? request.UserID : 0;

                var txns = await _transactionMasterRepository.GetAll(scopeUser, string.Empty, string.Empty, from, to, 1, int.MaxValue);
                string aepsName = walletNames.TryGetValue((int)WalletCode.AEPS, out var an) ? an : "AEPS";
                var users = (await _userRepository.GetAllUsers()).ToDictionary(u => u.UserMasterID, u => u);

                foreach (var x in txns)
                {
                    if (x.WalletID != (int)WalletCode.AEPS || !payoutIds.Contains(x.ServiceID)) continue;
                    if (reach != null && !reach.Contains(x.UserID)) continue;
                    if (IsRealFilter(request.Status) && !x.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase)) continue;

                    users.TryGetValue(x.UserID, out var u);
                    string bene = !string.IsNullOrWhiteSpace(x.DMT_BeneficiaryName) ? x.DMT_BeneficiaryName : x.ConsumerNumber;
                    rows.Add(new FundRequestViewModel
                    {
                        FundTransactionID = x.TransactionMasterID,
                        SenderID = x.UserID,
                        SenderName = u?.FirmName ?? string.Empty,
                        SenderMobile = u?.MobileNumber ?? string.Empty,
                        ReceiverID = x.UserID,
                        ReceiverName = u?.FirmName ?? string.Empty,
                        WalletID = x.WalletID,
                        WalletName = $"{aepsName} → Bank (payout)",
                        Amount = x.Amount,
                        OriginalAmount = x.Amount,
                        SenderOpeningBalance = x.OpeningBalance,
                        SenderClosingBalance = x.ClosingBalance,
                        Status = x.Status,
                        Type = "PAYOUT",
                        Through = "PAYOUT",
                        Remark = string.IsNullOrWhiteSpace(bene) ? "Bank payout" : $"Payout to {bene}",
                        Description = x.Description ?? string.Empty,
                        TransactionDOC = x.DateTime,
                        TransactionDOCInt = int.Parse(x.DateTime.ToString("yyyyMMdd")),
                        DOC = x.DateTime,
                    });
                }
            }
            catch (Exception ex) { _logger.Write(ex); }
            return rows;
        }

        // "AEPS → Recharge" when both legs are present, else whichever wallet we do know.
        private static string BuildWalletMoveLabel(
            FundTransaction? outLeg, FundTransaction? inLeg, Dictionary<int, string> walletNames, string fallback)
        {
            string Name(FundTransaction? leg) =>
                leg != null && walletNames.TryGetValue(leg.WalletID, out var n) ? n : string.Empty;

            string source = Name(outLeg);
            string destination = Name(inLeg);
            if (source.Length > 0 && destination.Length > 0) return $"{source} → {destination}";
            if (destination.Length > 0) return $"→ {destination}";
            if (source.Length > 0) return $"{source} →";
            return fallback;
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
