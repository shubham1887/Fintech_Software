using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Outstanding = what a user owes their upline for funds taken on credit.
    ///
    /// The model in one paragraph: a fund request or transfer whose Mode is "Credit" funds the
    /// receiver's wallet exactly as any other would, and adds the same amount to their
    /// outstanding. Which wallet it went into does not matter — recharge, AEPS and DMT credit all
    /// roll into the one debt, because the debt is to a person, not to a wallet. A reversal pulls
    /// the funds back out and takes the debt with them. Cash collected at the counter reduces it.
    /// Nothing else touches it: commissions, charges and ordinary (non-credit) funding do not.
    ///
    /// This class is the ONLY place the balance moves, for the same reason the settlement hook is
    /// the only place a transaction reaches Success: three things have to agree afterwards — the
    /// balance, the statement row that explains it, and the message to the person who owes the
    /// money. Spread across five call sites that stops being true within a release or two.
    ///
    /// The ordering rule every caller must respect: post the outstanding FIRST, move the money
    /// second. The credit limit is enforced inside the posting UPDATE, so a refusal has to be able
    /// to happen before anything else has changed. <see cref="FundService"/> compensates the
    /// posting if the wallet move then fails.
    /// </summary>
    public class OutstandingService : IOutstandingService
    {
        private readonly ICashCollectionMasterRepository _masterRepo;
        private readonly ICashCollectionDetailRepository _detailRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IMessageService _messageService;
        private readonly Logger _logger = new();

        public OutstandingService(
            ICashCollectionMasterRepository masterRepo,
            ICashCollectionDetailRepository detailRepo,
            IUserRepository userRepo,
            ISettingMasterRepository settingRepo,
            IMessageService messageService)
        {
            _masterRepo = masterRepo;
            _detailRepo = detailRepo;
            _userRepo = userRepo;
            _settingRepo = settingRepo;
            _messageService = messageService;
        }

        /// <summary>
        /// The feature switch, read tolerantly.
        ///
        /// Key Settings rows carry both an <c>IsOn</c> flag and a free-text <c>Value</c>, and the
        /// first version of this feature wrote the word "false" into the Value while the rest of
        /// the product uses IsOn. Honouring both means an install configured either way behaves
        /// the way its operator meant, instead of the switch appearing not to work.
        ///
        /// Absent row = ON. The tables ship empty on an upgrade, and a feature that silently
        /// disabled itself on every existing install would be the worse default here: outstanding
        /// balances already recorded must stay visible and collectable.
        /// </summary>
        public async Task<bool> IsEnabled()
        {
            try
            {
                var setting = await _settingRepo.GetByKey(OutstandingLedger.SettingKey);
                if (setting == null) return true;
                if (!setting.IsOn) return false;
                string v = (setting.Value ?? string.Empty).Trim();
                return !(v.EqualsIgnoreCase("false") || v == "0" || v.EqualsIgnoreCase("off") || v.EqualsIgnoreCase("no"));
            }
            catch (Exception ex)
            {
                // A settings read that failed must not take the feature down with it.
                _logger.Write(ex);
                return true;
            }
        }

        /// <summary>
        /// What a zero credit limit means, in one place so both the courtesy check and the atomic
        /// refusal say the same thing. It names the screen, because the person reading it is an
        /// operator who needs to know the next step rather than the arithmetic.
        /// </summary>
        /// <summary>Key Setting holding the network-wide default credit limit.</summary>
        private const string DefaultLimitKey = "DefaultCreditLimit";

        private const string NoLimitMessage =
            "This account has no credit limit, so funds cannot be given on credit. An admin can set one under Funds \u2192 Outstanding.";

        public async Task<decimal> GetOutstanding(int userID)
        {
            var row = await _masterRepo.GetLiveByUserID(userID);
            return row?.Outstanding ?? 0m;
        }

        public async Task<(decimal outstanding, decimal maxOutstanding, bool isDefaultLimit)> GetStanding(int userID)
        {
            var row = await _masterRepo.EnsureRow(userID);
            if (row == null) return (0m, 0m, false);

            if (row.HasExplicitLimit) return (row.Outstanding, row.MaxOutstanding, false);

            decimal fallback = await GetDefaultCreditLimit();
            return (row.Outstanding, fallback, true);
        }

        /// <summary>
        /// The network default limit. Honoured only while the Key Setting is ON and holds a
        /// positive number — the same rule the other amount-bearing Key Settings follow, so an
        /// operator who types a number and forgets the switch has not accidentally extended credit
        /// to everybody.
        /// </summary>
        public async Task<decimal> GetDefaultCreditLimit()
        {
            try
            {
                var setting = await _settingRepo.GetByKey(DefaultLimitKey);
                if (setting == null || !setting.IsOn) return 0m;
                return decimal.TryParse((setting.Value ?? string.Empty).Trim(), out decimal v) && v > 0 ? v : 0m;
            }
            catch (Exception ex)
            {
                // A settings read that failed must not invent a credit limit.
                _logger.Write(ex);
                return 0m;
            }
        }

        /// <summary>
        /// The limit to enforce for this account right now: its own if one was set, else the
        /// network default. One method so the courtesy check and the atomic posting can never
        /// disagree about what the ceiling is.
        /// </summary>
        private async Task<decimal> EffectiveLimit(CashCollectionMaster row)
            => row.HasExplicitLimit ? row.MaxOutstanding : await GetDefaultCreditLimit();

        public async Task<string> CheckCreditAllowed(int userID, decimal amount)
        {
            if (amount <= 0) return Messages.Common.Message.InvalidAmount;
            if (!await IsEnabled())
                return "Credit is not available. Please contact customer care.";

            var row = await _masterRepo.EnsureRow(userID);
            if (row == null) return "Could not read the credit standing for this user.";

            decimal limit = await EffectiveLimit(row);
            if (limit <= 0) return NoLimitMessage;

            decimal available = limit - row.Outstanding;
            if (amount > available)
                return $"Credit limit exceeded. Limit: {limit:F2}, already outstanding: {row.Outstanding:F2}, available: {(available < 0 ? 0m : available):F2}.";

            return StatusType.Ok;
        }

        public Task<(string failureMessage, OutstandingDeltaResult result)> PostCredit(OutstandingPosting posting)
            => Post(posting, isCredit: true);

        public Task<(string failureMessage, OutstandingDeltaResult result)> PostDebit(OutstandingPosting posting)
            => Post(posting, isCredit: false);

        private async Task<(string failureMessage, OutstandingDeltaResult result)> Post(
            OutstandingPosting posting, bool isCredit)
        {
            if (posting == null || posting.UserID <= 0 || posting.Amount <= 0)
                return (Messages.Common.Message.InvalidRequest, default);

            // Round to paise before anything else. The column is decimal(18,5) and an amount
            // arriving with more precision than that would be stored rounded while the statement
            // row recorded what was asked for — a one-paise disagreement that compounds over
            // thousands of rows and makes the statement stop tying out to the balance.
            decimal amount = Math.Round(posting.Amount, 2, MidpointRounding.AwayFromZero);
            if (amount <= 0) return (Messages.Common.Message.InvalidAmount, default);

            // Make sure a row exists. A user with no debt yet has no record, and the atomic UPDATE
            // below cannot create one — it would report "missing" on every first-ever credit.
            var standing = await _masterRepo.EnsureRow(posting.UserID);
            if (standing == null)
                return ("Could not open a cash-collection record for this user.", default);

            bool enforceLimit = isCredit && !posting.BypassLimit;
            // Resolved here, in the service, because only it knows whether this account's own
            // limit applies or the network default does. The affordability check itself stays
            // inside the UPDATE, so two concurrent credits still cannot both slip past the same
            // ceiling; a limit changed in the same instant is a race nobody can observe, and
            // limits change rarely.
            decimal limit = enforceLimit ? await EffectiveLimit(standing) : 0m;

            decimal delta = isCredit ? amount : -amount;
            var result = await _masterRepo.TryApplyDelta(posting.UserID, delta, enforceLimit, limit);

            if (!result.Applied)
            {
                if (result.RowMissing) return (Messages.CashCollectionMasterMessages.NotFound, result);
                if (result.WouldGoNegative)
                    return ($"The amount is more than the outstanding. Outstanding: {result.OpeningBalance:F2}.", result);
                if (result.LimitBreached)
                {
                    // A limit of zero is "this account may not take credit at all", which is the
                    // default every account starts on - not a limit that has been used up. Saying
                    // "Credit limit exceeded. Limit: 0.00, already outstanding: 0.00" to someone
                    // who owes nothing reads as a broken calculation, and sends them looking for a
                    // bug instead of to the screen where the limit is set.
                    if (result.MaxOutstanding <= 0)
                        return (NoLimitMessage, result);

                    return ($"Credit limit exceeded. Limit: {result.MaxOutstanding:F2}, already outstanding: {result.OpeningBalance:F2}.", result);
                }
                return ("The outstanding could not be updated. Please try again.", result);
            }

            // The balance has moved. The statement row is not optional from here on: a balance
            // with no row behind it is exactly the state nobody can reconcile later, and the
            // product's own reports would disagree with the wallet tile. So if the insert fails,
            // put the balance back and report the failure rather than leaving the two apart.
            var ledgerRow = new CashCollectionDetail
            {
                UserID = posting.UserID,
                ParentID = posting.ParentID,
                OutstandingOpening = result.OpeningBalance,
                OutstandingAmount = amount,
                OutstandingClosing = result.ClosingBalance,
                ParentRemark = Trim(BuildRemark(posting, isCredit, amount, result.ClosingBalance), 100),
                PayMode = Trim(posting.PayMode, 100),
                Credit = isCredit ? OutstandingLedger.CreditMarker : string.Empty,
                Debit = isCredit ? string.Empty : OutstandingLedger.DebitMarker,
                UTRNumber = Trim(posting.UTRNumber, 100),
                Type = Trim(string.IsNullOrWhiteSpace(posting.Type) ? OutstandingLedger.TypeAdjustment : posting.Type, 50),
                FundTransactionID = posting.FundTransactionID,
                WalletID = posting.WalletID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };

            try
            {
                int written = await _detailRepo.Insert(ledgerRow);
                if (written <= 0) throw new InvalidOperationException("The cash-collection statement row was not written.");
                // EF fills the identity back onto the entity, so the caller can link it to a fund
                // transaction whose id only exists later.
                posting.LedgerRowID = ledgerRow.CashCollectionDetailID;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                await CompensateSilently(posting.UserID, -delta);
                return ("The outstanding could not be recorded. Please try again.", default);
            }

            if (!posting.SuppressMessage)
                await TellTheUser(posting, isCredit, amount, result.ClosingBalance);

            return (StatusType.Ok, result);
        }

        /// <summary>
        /// Puts a balance back after the statement row could not be written.
        ///
        /// Bypasses both guards on purpose: this is an undo of a change that already passed them,
        /// and refusing it (because, say, the credit limit has since been lowered) would leave the
        /// very inconsistency it exists to prevent. If even this fails there is nothing more the
        /// request can do, so it is logged loudly for the operator — the balance is then higher
        /// than the statement explains, which the daily integrity read will surface.
        /// </summary>
        private async Task CompensateSilently(int userID, decimal delta)
        {
            try
            {
                var undo = await _masterRepo.TryApplyDelta(userID, delta, enforceLimit: false, effectiveLimit: 0m);
                if (!undo.Applied)
                    _logger.Write($"OUTSTANDING COMPENSATION FAILED: user {userID} delta {delta}. The balance no longer matches its statement.");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                _logger.Write($"OUTSTANDING COMPENSATION THREW: user {userID} delta {delta}. The balance no longer matches its statement.");
            }
        }

        private static string BuildRemark(OutstandingPosting posting, bool isCredit, decimal amount, decimal closing)
        {
            string movement = isCredit
                ? $"Outstanding increased by {amount:F2}, new outstanding {closing:F2}"
                : $"Outstanding reduced by {amount:F2}, new outstanding {closing:F2}";

            string note = (posting.Remark ?? string.Empty).Trim();
            return note.Length > 1 ? $"{note} — {movement}" : movement;
        }

        private static string Trim(string? value, int max)
        {
            value ??= string.Empty;
            return value.Length <= max ? value : value[..max];
        }

        /// <summary>
        /// Tells the person who owes the money that their balance moved.
        ///
        /// Both directions are announced, which is the whole point: a retailer who is told only
        /// when credit is extended has no record that the cash they handed over was booked, and
        /// "I paid you yesterday" against "I have no note of it" is the argument this is here to
        /// prevent. The template is admin-editable (Message Settings); the text passed here is the
        /// fallback used when no template exists, so a message always goes out.
        ///
        /// Never allowed to throw: a messaging gateway being down is not a reason to fail a
        /// posting that has already been written.
        /// </summary>
        public async Task<string> ClearCreditLimit(int userID)
        {
            if (userID <= 0) return Messages.Common.Message.InvalidRequest;
            if (await _masterRepo.EnsureRow(userID) == null)
                return "Could not open a cash-collection record for this user.";

            int updated = await _masterRepo.SetMaxOutstanding(userID, 0m, hasExplicitLimit: false);
            return updated > 0 ? StatusType.Ok : "The credit limit could not be cleared.";
        }

        public Task Announce(OutstandingPosting posting, bool isCredit, decimal amount, decimal closing)
            => TellTheUser(posting, isCredit, amount, closing);

        public async Task RecordWalletBalances(OutstandingPosting posting, decimal walletOpening, decimal walletClosing)
        {
            if (posting == null || posting.LedgerRowID <= 0) return;
            try
            {
                await _detailRepo.SetWalletBalances(posting.LedgerRowID, walletOpening, walletClosing);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }

        public async Task LinkFundTransaction(OutstandingPosting posting, int fundTransactionID)
        {
            if (posting == null || posting.LedgerRowID <= 0 || fundTransactionID <= 0) return;
            try
            {
                posting.FundTransactionID = fundTransactionID;
                await _detailRepo.SetFundTransactionID(posting.LedgerRowID, fundTransactionID);
            }
            catch (Exception ex)
            {
                // The statement line is already correct in every way that matters to the balance;
                // only the link back to the transfer is missing. Not worth failing a completed
                // transfer over.
                _logger.Write(ex);
            }
        }

        private async Task TellTheUser(OutstandingPosting posting, bool isCredit, decimal amount, decimal closing)
        {
            try
            {
                var user = await _userRepo.GetUserById(posting.UserID);
                if (user == null || string.IsNullOrWhiteSpace(user.MobileNumber)) return;

                var counterparty = posting.ParentID > 0 ? await _userRepo.GetUserById(posting.ParentID) : null;
                string counterpartyName = counterparty?.FirmName ?? string.Empty;

                string eventType = isCredit
                    ? OutstandingLedger.EventOutstandingCredited
                    : OutstandingLedger.EventOutstandingDebited;

                string fallback = isCredit
                    ? $"Rs.{amount:F2} credit given{(counterpartyName.Length > 0 ? $" by {counterpartyName}" : string.Empty)}. Total outstanding: Rs.{closing:F2}."
                    : $"Rs.{amount:F2} received against your outstanding{(counterpartyName.Length > 0 ? $" by {counterpartyName}" : string.Empty)}. Balance outstanding: Rs.{closing:F2}.";

                var tokens = new Dictionary<string, string>
                {
                    ["Amount"] = amount.ToString("F2"),
                    ["Outstanding"] = closing.ToString("F2"),
                    // Statements and texts are read side by side, so the balance token means the
                    // same thing in both: what is still owed after this row.
                    ["Balance"] = closing.ToString("F2"),
                    ["Remark"] = posting.Remark ?? string.Empty,
                    ["PayMode"] = posting.PayMode ?? string.Empty,
                    ["ParentName"] = counterpartyName,
                    ["CustomerName"] = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName ?? string.Empty : user.PersonName,
                    ["Number"] = user.MobileNumber,
                };

                await _messageService.SendEventAsync(eventType, user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, user.EmailID, tokens, fallback, emailSubject: eventType);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }

        public async Task<string> SetCreditLimit(int userID, decimal maxOutstanding)
        {
            if (userID <= 0) return Messages.Common.Message.InvalidRequest;
            if (maxOutstanding < 0) return "A credit limit cannot be negative.";

            if (await _masterRepo.EnsureRow(userID) == null)
                return "Could not open a cash-collection record for this user.";

            // Lowering a limit below what is already owed is allowed and is a normal thing to do —
            // it is how an upline stops further credit to someone who is behind on payments. It
            // must not be read as an error, and it must not change the existing debt, which is
            // why this writes the limit column alone.
            //
            // Marked EXPLICIT: setting an account to 0 here means "this account gets no credit",
            // and it has to keep meaning that even while a network default is switched on.
            int updated = await _masterRepo.SetMaxOutstanding(
                userID, Math.Round(maxOutstanding, 2, MidpointRounding.AwayFromZero), hasExplicitLimit: true);

            return updated > 0 ? StatusType.Ok : "The credit limit could not be saved.";
        }
    }
}
