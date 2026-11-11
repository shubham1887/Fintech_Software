using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CashCollection;
using FintechApp.Requests.CashCollectionMaster;
using FintechApp.Responses.CashCollection;
using FintechApp.Responses.CashCollectionMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// The cash-collection screens: who owes what, the statement behind each figure, recording a
    /// payment, and undoing one that was recorded wrongly.
    ///
    /// Balances are never touched here — every movement goes through <see cref="IOutstandingService"/>,
    /// which is the one place that changes a balance, writes the statement row and messages the
    /// person who owes the money, atomically. What this class owns is the part that decides WHO
    /// may do each of those things, and what each screen gets to see.
    /// </summary>
    public class CashCollectionMasterService : ICashCollectionMasterService
    {
        private readonly ICashCollectionMasterRepository _repository;
        private readonly ICashCollectionDetailRepository _detailRepo;
        private readonly IUserRepository _userRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IWalletMasterRepository _walletMasterRepo;
        private readonly IOutstandingService _outstanding;
        private readonly IMessageService _messageService;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly Logger _logger;

        public CashCollectionMasterService(
            ICashCollectionMasterRepository repository,
            ICashCollectionDetailRepository detailRepo,
            IUserRepository userRepo,
            IUserWalletMasterRepository walletRepo,
            IWalletMasterRepository walletMasterRepo,
            IOutstandingService outstanding,
            IMessageService messageService,
            ISettingMasterRepository settingRepo)
        {
            _settingRepo = settingRepo;
            _repository = repository;
            _detailRepo = detailRepo;
            _userRepo = userRepo;
            _walletRepo = walletRepo;
            _walletMasterRepo = walletMasterRepo;
            _outstanding = outstanding;
            _messageService = messageService;
            _logger = new Logger();
        }

        private static CashCollectionMasterResponse MapToResponse(CashCollectionMaster e) => new()
        {
            CashCollectionMasterID = e.CashCollectionMasterID,
            UserID = e.UserID,
            Outstanding = e.Outstanding,
            MaxOutstanding = e.MaxOutstanding,
        };

        public async Task<List<CashCollectionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CashCollectionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<CashCollectionMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateCashCollectionMasterRequest request)
        {
            // A user has exactly one outstanding record. Creating a second would split their debt
            // across two rows, and every read here takes the first one it finds — so the balance
            // would appear to change depending on which row was written last.
            if (await _repository.EnsureRow(request.UserID, request.MaxOutstanding) == null)
                return "Could not open a cash-collection record for this user.";

            return await _outstanding.SetCreditLimit(request.UserID, request.MaxOutstanding);
        }

        public async Task<string> Update(UpdateCashCollectionMasterRequest request)
        {
            var entity = await _repository.GetByID(request.CashCollectionMasterID);
            if (entity == null) return Messages.CashCollectionMasterMessages.NotFound;

            // Only the credit limit is editable. The outstanding on the request is ignored on
            // purpose: a balance that can be typed over is a balance with no statement behind it,
            // and this screen's whole value is that the two always agree. An admin who genuinely
            // needs to change a figure posts an adjustment, which leaves a line explaining it.
            return await _outstanding.SetCreditLimit(entity.UserID, request.MaxOutstanding);
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CashCollectionMasterMessages.NotFound;
        }

        // ── Authorisation ───────────────────────────────────────────────────────────────────

        private static bool IsAdminTier(UserMaster u)
            => u.UserTypeID == UserTypeCode.Admin || u.UserTypeID == UserTypeCode.SupportUser;

        /// <summary>
        /// May <paramref name="actor"/> record or undo a collection against <paramref name="targetUserID"/>?
        ///
        /// The rule is the one the money follows: the person OWED may mark the debt paid, and an
        /// admin may do it for anyone. Nothing else — and in particular not the person who owes
        /// it, who could otherwise clear their own balance and keep the funds.
        ///
        /// This matters more than it looks. The cash-collection controller is reachable by every
        /// transacting role (it is self-service for the "my outstanding" view), so without this
        /// check any retailer could post a collection against any user id they cared to type. The
        /// previous version verified the caller's PIN and then never asked whose debt it was.
        /// </summary>
        private async Task<string> EnsureMayCollect(UserMaster actor, int targetUserID)
        {
            if (actor.UserMasterID == targetUserID)
                return "You cannot record a collection against your own outstanding.";

            if (IsAdminTier(actor)) return StatusType.Ok;

            var target = await _userRepo.GetUserById(targetUserID);
            if (target == null) return Messages.UserMessages.UserNotFound;

            // Direct parent only. Outstanding is owed along ONE edge — a super-distributor is not
            // owed what a retailer took from their distributor, and letting them clear it would
            // wipe a debt they were never paid for.
            if (target.ParentID != actor.UserMasterID)
                return "You can only collect from a user who reports directly to you.";

            return StatusType.Ok;
        }

        // ── Recording a collection ──────────────────────────────────────────────────────────

        public async Task<(string failureMessage, string result)> AddCashCollection(AddCashCollectionRequest request, int senderID)
        {
            if (request.ReceiverID <= 0 || request.Amount <= 0)
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            if (!await _outstanding.IsEnabled())
                return ("Cash collection is not available. Please contact customer care.", string.Empty);

            var sender = await _userRepo.GetUserById(senderID);
            if (sender == null) return (Messages.Common.Message.InvalidData, string.Empty);
            if (!sender.IsActive) return (Messages.Common.Message.AccountInactive, string.Empty);

            string authFail = await EnsureMayCollect(sender, request.ReceiverID);
            if (!authFail.EqualsIgnoreCase(StatusType.Ok)) return (authFail, string.Empty);

            // The PIN is what makes this the collector's own act rather than a session someone
            // walked up to. Checked after the authorisation above so a wrong target is never
            // confirmed by a correct PIN. Asked for only where an MPIN is in force — the install's
            // Key Setting or the user's own opt-in — the same rule the app-open gate applies; an
            // install with MPIN switched off could never record a collection at all.
            if (await IsMpinRequired(sender))
            {
                if (!int.TryParse(request.Pin, out int pin) || sender.Mpin != pin)
                    return ("Invalid PIN.", string.Empty);
            }

            var (fail, result) = await _outstanding.PostDebit(new OutstandingPosting
            {
                UserID = request.ReceiverID,
                ParentID = senderID,
                Amount = request.Amount,
                Type = OutstandingLedger.TypeCollection,
                Remark = request.Remark ?? string.Empty,
                PayMode = request.PayMode ?? string.Empty,
                UTRNumber = request.UTRNumber ?? string.Empty,
            });

            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (fail, string.Empty);

            return (StatusType.Ok, $"Collection recorded. Outstanding is now {result.ClosingBalance:F2}.");
        }

        /// <summary>
        /// Undoes a collection that was recorded in error — wrong user, wrong amount, a cheque
        /// that bounced.
        ///
        /// It puts the debt BACK, so it is posted as a credit, and it bypasses the credit limit:
        /// the debt already existed and was wrongly cleared, so refusing to restore it because the
        /// limit has since been lowered would quietly forgive money that is genuinely owed. It is
        /// posted as a new line rather than by editing the original, so the statement still shows
        /// that both things happened and who did them.
        /// </summary>
        public async Task<(string failureMessage, string result)> ReverseCashCollection(ReverseCashCollectionRequest request, int actorID)
        {
            if (request.ReceiverID <= 0 || request.Amount <= 0)
                return (Messages.Common.Message.InvalidRequest, string.Empty);

            if (!await _outstanding.IsEnabled())
                return ("Cash collection is not available. Please contact customer care.", string.Empty);

            var actor = await _userRepo.GetUserById(actorID);
            if (actor == null) return (Messages.Common.Message.InvalidData, string.Empty);

            string authFail = await EnsureMayCollect(actor, request.ReceiverID);
            if (!authFail.EqualsIgnoreCase(StatusType.Ok)) return (authFail, string.Empty);

            var (fail, result) = await _outstanding.PostCredit(new OutstandingPosting
            {
                UserID = request.ReceiverID,
                ParentID = actorID,
                Amount = request.Amount,
                Type = OutstandingLedger.TypeCollectionReversed,
                Remark = string.IsNullOrWhiteSpace(request.Remark) ? "Collection entry reversed" : request.Remark,
                PayMode = request.PayMode ?? string.Empty,
                BypassLimit = true,
            });

            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (fail, string.Empty);

            return (StatusType.Ok, $"Collection reversed. Outstanding is now {result.ClosingBalance:F2}.");
        }

        // ── Reading ─────────────────────────────────────────────────────────────────────────

        /// <summary>Global Key Setting OR the user's own opt-in — mirrors CredentialService.GetMpinStatus.</summary>
        private async Task<bool> IsMpinRequired(UserMaster user)
        {
            var setting = await _settingRepo.GetByKey("IsMpinEnabled");
            bool globalOn = setting != null && setting.IsOn;
            return globalOn || user.IsMpinEnabled;
        }

        public async Task<(string failureMessage, SelectByUserIDResponse? data)> SelectUserOutstanding(int userID)
        {
            // The user has to exist before a standing row is created for them: EnsureRow inserts a
            // CashCollectionMaster keyed on the UserID, and an id nothing matches (0 from a client
            // that sent none, or a deleted account) hit the foreign key and logged an EF error on
            // every open of the Outstanding tile — 38 of them in three days on live.
            var user = await _userRepo.GetUserById(userID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);

            // EnsureRow rather than "not found": a user who has never taken credit owes nothing,
            // and that is a perfectly good answer. Returning DataNotFound made the Outstanding
            // tile and the collection form look broken for every user with a clean slate.
            var (outstanding, maxOutstanding, isDefaultLimit) = await _outstanding.GetStanding(userID);

            string balanceDetail = string.Empty;
            try
            {
                // Wallet balances live in UserWalletMasters. The previous version read
                // UserBalanceMaster, which is a legacy table with no rows in it, so this line was
                // always empty — and empty read as "this user has no money", on the very screen
                // where someone decides whether to extend more credit.
                var balances = await _walletRepo.GetByUserID(userID);
                var definitions = await _walletMasterRepo.Select();

                balanceDetail = string.Join(", ", definitions
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.UISequence)
                    .Select(w => new
                    {
                        Name = string.IsNullOrWhiteSpace(w.ShortName) ? w.Name : w.ShortName,
                        Balance = balances.FirstOrDefault(b => b.WalletID == w.WalletID)?.Balance ?? 0m,
                    })
                    .Select(w => $"{w.Name} : {w.Balance:F2}"));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            // Who the money is owed to. A retailer reading their own figure needs somebody to pay;
            // the number on its own is not actionable.
            UserMaster? parent = user?.ParentID is int pid and > 0 ? await _userRepo.GetUserById(pid) : null;

            decimal available = maxOutstanding - outstanding;

            return (StatusType.Ok, new SelectByUserIDResponse
            {
                BalanceDetail = balanceDetail,
                Outstanding = outstanding,
                MaxOutStanding = maxOutstanding,
                IsDefaultLimit = isDefaultLimit,
                AvailableCredit = available < 0 ? 0m : available,
                ParentID = parent?.UserMasterID ?? 0,
                ParentName = parent?.FirmName ?? string.Empty,
                ParentMobile = parent?.MobileNumber ?? string.Empty,
                UserID = userID,
                UserName = user?.FirmName ?? string.Empty,
                FeatureEnabled = await _outstanding.IsEnabled(),
            });
        }

        /// <summary>
        /// "Who owes how much", scoped to what the caller is entitled to see.
        ///
        /// Admin and support see the whole network. Everyone else sees their own downline and
        /// nothing else — the figure is a commercial fact about another business, and a
        /// distributor has no business reading what a retailer owes somebody else.
        /// </summary>
        public async Task<(string failureMessage, OutstandingWrapper? data)> OutstandingReport(SelectCashCollectionRequest request, int callerID, int callerTypeID)
        {
            var allUsers = await _userRepo.GetAllUsers();
            var caller = allUsers.FirstOrDefault(u => u.UserMasterID == callerID);
            if (caller == null) return (Messages.Common.Message.Unauthorized, null);

            bool isAdmin = callerTypeID == UserTypeCode.Admin || callerTypeID == UserTypeCode.SupportUser;
            HashSet<int>? visible = isAdmin ? null : UserHierarchy.GetDownlineIds(allUsers, callerID);

            // Did the caller ask for somebody in particular? If so, show them even at zero/zero.
            bool filtering = request.UserID > 0 || !string.IsNullOrWhiteSpace(request.SearchText);

            var masters = await _repository.GetAll();
            var usersById = allUsers.ToDictionary(u => u.UserMasterID);

            var rows = new List<OutstandingViewModel>();
            foreach (var m in masters)
            {
                if (visible != null && !visible.Contains(m.UserID)) continue;
                if (!usersById.TryGetValue(m.UserID, out var user)) continue;

                // Zero balances are not interesting here and would bury the ones that are, but a
                // user with a credit LIMIT and no balance is — that is someone who may take funds
                // on credit tomorrow, and the admin setting the limits needs to see them.
                // ...but they ARE what an admin is looking at when they go to set a limit, and
                // hiding them made that a dead end: the list came back empty and there was nothing
                // to click. So they are hidden only until somebody searches or names a user, at
                // which point finding them is the whole point.
                if (!filtering && m.Outstanding == 0 && m.MaxOutstanding <= 0) continue;

                var parent = user.ParentID.HasValue && usersById.TryGetValue(user.ParentID.Value, out var p) ? p : null;
                decimal available = m.MaxOutstanding - m.Outstanding;

                rows.Add(new OutstandingViewModel
                {
                    UserID = m.UserID,
                    UserName = $"{user.FirmName} ({user.MobileNumber})",
                    FirmName = user.FirmName ?? string.Empty,
                    PersonName = user.PersonName ?? string.Empty,
                    MobileNumber = user.MobileNumber ?? string.Empty,
                    UserType = user.UserType?.UserType ?? string.Empty,
                    ParentID = parent?.UserMasterID ?? 0,
                    ParentName = parent?.FirmName ?? string.Empty,
                    Outstanding = m.Outstanding,
                    MaxOutstanding = m.MaxOutstanding,
                    AvailableCredit = available < 0 ? 0m : available,
                    LastMovementDate = m.UpdatedateTime == default ? null : m.UpdatedateTime,
                });
            }

            // Age the balances before filtering, so the per-row buckets are the same whichever
            // search the reader typed.
            DateTime asOn = DateTime.UtcNow;
            await ApplyAgeing(rows, visible, asOn);

            if (request.UserID > 0)
                rows = rows.Where(r => r.UserID == request.UserID).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                string q = request.SearchText.Trim();
                rows = rows.Where(r =>
                    r.UserName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.PersonName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.MobileNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.ParentName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            rows = rows.OrderByDescending(r => r.Outstanding).ThenBy(r => r.FirmName).ToList();

            // The caller's own position. "Who owes me" and "what do I owe" are different debts to
            // different people and are never netted; the report leads with the second because the
            // reader is in the network too, and for a retailer it is the only one that exists.
            var (myFail, mine) = await SelectUserOutstanding(callerID);
            bool haveMine = myFail.EqualsIgnoreCase(StatusType.Ok) && mine != null;

            return (StatusType.Ok, new OutstandingWrapper
            {
                data = rows,
                IsExcelDownload = request.IsExcelDownload,
                TotalOutstanding = rows.Sum(r => r.Outstanding),
                TotalLimit = rows.Sum(r => r.MaxOutstanding),
                TotalAvailable = rows.Sum(r => r.AvailableCredit),
                AccountsWithBalance = rows.Count(r => r.Outstanding > 0),
                OverLimitCount = rows.Count(r => r.IsOverLimit),
                TotalAge0To30 = rows.Sum(r => r.Age0To30),
                TotalAge31To60 = rows.Sum(r => r.Age31To60),
                TotalAge61To90 = rows.Sum(r => r.Age61To90),
                TotalAge90Plus = rows.Sum(r => r.Age90Plus),
                MyOutstanding = haveMine ? mine!.Outstanding : 0m,
                MyLimit = haveMine ? mine!.MaxOutStanding : 0m,
                MyAvailable = haveMine ? mine!.AvailableCredit : 0m,
                MyPayableTo = haveMine ? mine!.ParentName : string.Empty,
                AsOn = asOn,
            });
        }

        /// <summary>
        /// Splits each row's balance into 0–30 / 31–60 / 61–90 / 90+ day buckets by matching
        /// collections against credits OLDEST FIRST.
        ///
        /// FIFO is not an arbitrary choice: it is what both sides already assume. A retailer who
        /// pays ₹10,000 against a ₹40,000 balance believes they have cleared the oldest ₹10,000
        /// they took, and an upline chasing a debt is chasing the oldest part of it. Matching the
        /// newest first would show a balance that never ages, which is the one thing an ageing
        /// report exists to prevent.
        ///
        /// Outstanding is per EDGE of the network (a retailer can owe two uplines), so credits and
        /// collections are matched within (user, upline) and only then summed onto the user's row.
        ///
        /// Where the statement cannot explain the whole balance — an install that carried balances
        /// over from before this ledger was kept — the unexplained part is put in the NEWEST
        /// bucket and the difference is never hidden: the four buckets always total the balance.
        /// Over-explaining (more open credit than the master says is owed) is resolved the other
        /// way, by retiring the oldest credits first, which is what a collection posted without a
        /// statement row would have done.
        /// </summary>
        private async Task ApplyAgeing(List<OutstandingViewModel> rows, HashSet<int>? visible, DateTime asOn)
        {
            if (rows.Count == 0) return;

            // Admin reads the whole network (visible == null); everyone else only their downline.
            // Either way, ask the database for the rows belonging to the users actually on the
            // report rather than the whole table.
            var wanted = rows.Select(r => r.UserID).ToHashSet();
            var movements = await _detailRepo.GetMovementsForAgeing(
                visible == null ? wanted : wanted.Intersect(visible).ToHashSet(), 200_000);

            // One open-credit queue per edge: (user, upline).
            var openByEdge = new Dictionary<(int User, int Parent), LinkedList<(DateTime On, decimal Amount)>>();
            var lastCollection = new Dictionary<int, DateTime>();

            foreach (var m in movements)
            {
                if (!wanted.Contains(m.UserID)) continue;
                var key = (m.UserID, m.ParentID);
                if (!openByEdge.TryGetValue(key, out var queue))
                    openByEdge[key] = queue = new LinkedList<(DateTime, decimal)>();

                decimal amount = m.OutstandingAmount;
                if (amount <= 0) continue;

                if (m.Credit.EqualsIgnoreCase(OutstandingLedger.CreditMarker))
                {
                    queue.AddLast((m.DateTime, amount));
                    continue;
                }

                // A collection (or a reversal) retires the oldest open credit first.
                if (lastCollection.TryGetValue(m.UserID, out var prev))
                {
                    if (m.DateTime > prev) lastCollection[m.UserID] = m.DateTime;
                }
                else lastCollection[m.UserID] = m.DateTime;

                decimal left = amount;
                while (left > 0 && queue.First != null)
                {
                    var head = queue.First.Value;
                    if (head.Amount > left) { queue.First.Value = (head.On, head.Amount - left); left = 0; }
                    else { left -= head.Amount; queue.RemoveFirst(); }
                }
            }

            foreach (var row in rows)
            {
                if (lastCollection.TryGetValue(row.UserID, out var paid)) row.LastCollectionDate = paid;

                row.UtilisationPercent = row.MaxOutstanding > 0
                    ? Math.Round(row.Outstanding / row.MaxOutstanding * 100m, 2)
                    : 0m;
                row.IsOverLimit = row.MaxOutstanding > 0 && row.Outstanding > row.MaxOutstanding;

                if (row.Outstanding <= 0) continue;

                // Every edge this user owes on, oldest credit first.
                var open = openByEdge
                    .Where(kv => kv.Key.User == row.UserID)
                    .SelectMany(kv => kv.Value)
                    .OrderBy(e => e.On)
                    .ToList();

                decimal explained = open.Sum(e => e.Amount);

                // More open credit on the statement than the master says is owed: retire the
                // oldest until the two agree.
                decimal excess = explained - row.Outstanding;
                int i = 0;
                while (excess > 0 && i < open.Count)
                {
                    if (open[i].Amount > excess) { open[i] = (open[i].On, open[i].Amount - excess); excess = 0; }
                    else { excess -= open[i].Amount; open[i] = (open[i].On, 0m); i++; }
                }

                foreach (var entry in open)
                {
                    if (entry.Amount <= 0) continue;
                    int days = (int)(asOn.Date - entry.On.Date).TotalDays;
                    if (days <= 30) row.Age0To30 += entry.Amount;
                    else if (days <= 60) row.Age31To60 += entry.Amount;
                    else if (days <= 90) row.Age61To90 += entry.Amount;
                    else row.Age90Plus += entry.Amount;

                    if (row.OldestUnpaidDate == null || entry.On < row.OldestUnpaidDate)
                    {
                        row.OldestUnpaidDate = entry.On;
                        row.OldestUnpaidDays = days < 0 ? 0 : days;
                    }
                }

                // Less on the statement than is owed — a balance that predates the ledger. Put the
                // remainder in the newest bucket rather than losing it: the four buckets have to
                // total the balance or the report is arithmetic nobody can check.
                decimal shortfall = row.Outstanding - (row.Age0To30 + row.Age31To60 + row.Age61To90 + row.Age90Plus);
                if (shortfall > 0) row.Age0To30 += shortfall;
            }
        }

        /// <summary>
        /// The statement behind one user's figure: date, who, opening, credit, debit, closing,
        /// remark — the page the View button opens.
        /// </summary>
        public async Task<(string failureMessage, OutstandingStatementWrapper? data)> Statement(
            OutstandingStatementRequest request, int callerID, int callerTypeID)
        {
            if (request.UserID <= 0) return (Messages.Common.Message.InvalidRequest, null);

            bool isAdmin = callerTypeID == UserTypeCode.Admin || callerTypeID == UserTypeCode.SupportUser;
            var allUsers = await _userRepo.GetAllUsers();

            // A statement names amounts owed between two specific businesses. Reading someone
            // else's is reading their books, so it is their own, their downline's, or admin.
            if (!isAdmin && request.UserID != callerID)
            {
                var downline = UserHierarchy.GetDownlineIds(allUsers, callerID);
                if (!downline.Contains(request.UserID))
                    return (Messages.Common.Message.Unauthorized, null);
            }

            DateTime? fromUtc = ParseDate(request.FromDate);
            // An end date means the whole of that day, not midnight at the start of it — otherwise
            // "to today" reliably hides everything that happened today.
            DateTime? toUtc = ParseDate(request.ToDate)?.Date.AddDays(1).AddTicks(-1);

            // A non-admin viewing a downline sees only the rows on the edge between the two of
            // them. What that retailer owes a different upline is not theirs to read.
            int? parentFilter = request.ParentID > 0
                ? request.ParentID
                : (!isAdmin && request.UserID != callerID ? callerID : null);

            var rows = await _detailRepo.GetStatement(request.UserID, parentFilter, fromUtc, toUtc,
                request.SkipCount, request.TakeCount);
            int total = await _detailRepo.CountStatement(request.UserID, parentFilter, fromUtc, toUtc);

            var usersById = allUsers.ToDictionary(u => u.UserMasterID);
            var wallets = await _walletMasterRepo.SelectAll();

            string NameOf(int id) => usersById.TryGetValue(id, out var u) ? (u.FirmName ?? string.Empty) : string.Empty;

            var mapped = rows.Select(r =>
            {
                bool isCredit = r.Credit.EqualsIgnoreCase(OutstandingLedger.CreditMarker);
                return new OutstandingStatementRow
                {
                    CashCollectionDetailID = r.CashCollectionDetailID,
                    Date = r.DateTime,
                    UserID = r.UserID,
                    UserName = NameOf(r.UserID),
                    ParentID = r.ParentID,
                    ParentName = NameOf(r.ParentID),
                    Opening = r.OutstandingOpening,
                    Credit = isCredit ? r.OutstandingAmount : 0m,
                    Debit = isCredit ? 0m : r.OutstandingAmount,
                    Closing = r.OutstandingClosing,
                    Remark = r.ParentRemark ?? string.Empty,
                    PayMode = r.PayMode ?? string.Empty,
                    UTRNumber = r.UTRNumber ?? string.Empty,
                    Type = r.Type ?? string.Empty,
                    TypeLabel = LabelFor(r.Type),
                    FundTransactionID = r.FundTransactionID,
                    WalletID = r.WalletID,
                    WalletName = r.WalletID.HasValue
                        ? wallets.FirstOrDefault(w => w.WalletID == r.WalletID.Value)?.Name ?? string.Empty
                        : string.Empty,
                    WalletOpening = r.WalletOpening,
                    WalletClosing = r.WalletClosing,
                };
            }).ToList();

            var (outstanding, maxOutstanding, _) = await _outstanding.GetStanding(request.UserID);

            return (StatusType.Ok, new OutstandingStatementWrapper
            {
                Rows = mapped,
                TotalCount = total,
                // Totals for the page in hand. The caller that needs the whole window's totals
                // pages through it — TotalCount is there precisely so it knows when it has.
                TotalCredit = mapped.Sum(r => r.Credit),
                TotalDebit = mapped.Sum(r => r.Debit),
                CurrentOutstanding = outstanding,
                MaxOutstanding = maxOutstanding,
                UserID = request.UserID,
                UserName = NameOf(request.UserID),
            });
        }

        private static DateTime? ParseDate(string? value)
            => string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value, out var d) ? null : d;

        private static string LabelFor(string? type) => (type ?? string.Empty) switch
        {
            OutstandingLedger.TypeFundRequest => "Fund request (credit)",
            OutstandingLedger.TypeFundTransfer => "Fund transfer (credit)",
            OutstandingLedger.TypeCollection => "Cash collected",
            OutstandingLedger.TypeFundReverse => "Fund reversed",
            OutstandingLedger.TypeCollectionReversed => "Collection reversed",
            OutstandingLedger.TypeAdjustment => "Adjustment",
            OutstandingLedger.LegacyDecreased => "Cash collected",
            _ => string.IsNullOrWhiteSpace(type) ? "Movement" : type,
        };

        /// <summary>
        /// Sends one user a reminder of what they owe, on demand.
        ///
        /// The same text the morning sweep sends — this is the button for "remind them now",
        /// which is what an operator reaches for after a phone call. It used to write a line to
        /// the log and return success, so the button appeared to work and no message was ever sent.
        /// </summary>
        public async Task<string> SendCollectionMessage(int targetUserID, int actorID)
        {
            var actor = await _userRepo.GetUserById(actorID);
            if (actor == null) return Messages.Common.Message.Unauthorized;

            var target = await _userRepo.GetUserById(targetUserID);
            if (target == null) return Messages.Common.Message.InvalidData;

            // Same rule as collecting: the person owed may chase it, and admin may chase anyone.
            if (!IsAdminTier(actor) && target.ParentID != actor.UserMasterID)
                return Messages.Common.Message.Unauthorized;

            if (string.IsNullOrWhiteSpace(target.MobileNumber))
                return "This user has no mobile number on record.";

            decimal outstanding = await _outstanding.GetOutstanding(targetUserID);
            if (outstanding <= 0) return "This user has no outstanding to remind them about.";

            await SendReminder(target, outstanding, actor.FirmName ?? string.Empty);
            return StatusType.Ok;
        }

        /// <summary>
        /// The reminder text, in one place, so the on-demand button and the morning sweep cannot
        /// drift into saying different things about the same debt.
        /// </summary>
        public async Task SendReminder(UserMaster user, decimal outstanding, string parentName)
        {
            try
            {
                var tokens = new Dictionary<string, string>
                {
                    ["Amount"] = outstanding.ToString("F2"),
                    ["Outstanding"] = outstanding.ToString("F2"),
                    ["Balance"] = outstanding.ToString("F2"),
                    ["ParentName"] = parentName,
                    ["CustomerName"] = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName ?? string.Empty : user.PersonName,
                    ["Number"] = user.MobileNumber,
                };

                string fallback = $"Reminder: Rs.{outstanding:F2} is outstanding on your account" +
                    (string.IsNullOrWhiteSpace(parentName) ? string.Empty : $" with {parentName}") +
                    ". Please clear it at the earliest.";

                await _messageService.SendEventAsync(OutstandingLedger.EventOutstandingReminder,
                    user.UserMasterID, user.MobileNumber, user.WhatsAppNumber, user.EmailID,
                    tokens, fallback, emailSubject: "Outstanding reminder");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }
    }
}
