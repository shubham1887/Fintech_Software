using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionDetails;
using FintechApp.Responses.TransactionDetails;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TransactionDetailsService : ITransactionDetailsService
    {
        private readonly ITransactionDetailsRepository _repository;
        private readonly IUserRepository _userRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IProviderMasterRepository _providerRepo;
        public TransactionDetailsService(
            ITransactionDetailsRepository repository,
            IUserRepository userRepo,
            IOperatorRepository operatorRepo,
            IWalletMasterRepository walletRepo,
            ITransactionMasterRepository txnRepo,
            IProviderMasterRepository providerRepo)
        {
            _repository = repository;
            _userRepo = userRepo;
            _operatorRepo = operatorRepo;
            _walletRepo = walletRepo;
            _txnRepo = txnRepo;
            _providerRepo = providerRepo;
        }
        private static TransactionDetailsResponse MapToResponse(TransactionDetails e) => new() { TransactionDetailID = e.TransactionDetailID, UserID = e.UserID, ReferenceID = e.ReferenceID, AssignedUserID = e.AssignedUserID, OperatorID = e.OperatorID, ServiceID = e.ServiceID, WalletID = e.WalletID, DOCType = e.DOCType, OpeningBalance = e.OpeningBalance, ClosingBalance = e.ClosingBalance, TransactionAmount = e.TransactionAmount, DiscountAmount = e.DiscountAmount, GST = e.GST, TDS = e.TDS, AmountCreditDebit = e.AmountCreditDebit, CommissionCreditDebit = e.CommissionCreditDebit, TransactionDescription = e.TransactionDescription, ConsumerNumber = e.ConsumerNumber, PayMode = e.PayMode, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<TransactionDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TransactionDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<TransactionDetailsResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateTransactionDetailsRequest request) { var entity = new TransactionDetails { UserID = request.UserID, ReferenceID = request.ReferenceID, AssignedUserID = request.AssignedUserID, OperatorID = request.OperatorID, ServiceID = request.ServiceID, WalletID = request.WalletID, DOCType = request.DOCType, OpeningBalance = request.OpeningBalance, ClosingBalance = request.ClosingBalance, TransactionAmount = request.TransactionAmount, DiscountAmount = request.DiscountAmount, GST = request.GST, TDS = request.TDS, AmountCreditDebit = request.AmountCreditDebit, CommissionCreditDebit = request.CommissionCreditDebit, TransactionDescription = request.TransactionDescription, ConsumerNumber = request.ConsumerNumber, PayMode = request.PayMode, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTransactionDetailsRequest request) { var entity = await _repository.GetByID(request.TransactionDetailID); if (entity == null) return Messages.TransactionDetailsMessages.NotFound; entity.UserID = request.UserID; entity.ReferenceID = request.ReferenceID; entity.AssignedUserID = request.AssignedUserID; entity.OperatorID = request.OperatorID; entity.ServiceID = request.ServiceID; entity.WalletID = request.WalletID; entity.DOCType = request.DOCType; entity.OpeningBalance = request.OpeningBalance; entity.ClosingBalance = request.ClosingBalance; entity.TransactionAmount = request.TransactionAmount; entity.DiscountAmount = request.DiscountAmount; entity.GST = request.GST; entity.TDS = request.TDS; entity.AmountCreditDebit = request.AmountCreditDebit; entity.CommissionCreditDebit = request.CommissionCreditDebit; entity.TransactionDescription = request.TransactionDescription; entity.ConsumerNumber = request.ConsumerNumber; entity.PayMode = request.PayMode; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TransactionDetailsMessages.NotFound; }

        // ── Account Statement (per user) ──────────────────────────────
        // Non-admins are always scoped to themselves (IDOR guard); admin/support may target any
        // user via request.UserID, defaulting to their own id.
        public async Task<AccountStatementWrapper> SelectForFilter(AccountStatementRequest request, int currentUserID, int currentUserTypeID)
        {
            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;
            int userId = isAdmin ? (request.UserID > 0 ? request.UserID : currentUserID) : currentUserID;
            return await BuildStatement(request, userId, currentUserTypeID);
        }

        // ── Admin Accounting (company / admin account) ────────────────
        public async Task<AccountStatementWrapper> SelectAdminAccounting(AccountStatementRequest request, int currentUserID, int currentUserTypeID)
        {
            if (!ReportScope.IsNetworkWide(currentUserTypeID))
                return new AccountStatementWrapper();
            // Company accounting = the admin's own account statement (the admin/company wallet).
            // The Platform Ledger screen sends UserID 0, so the fallback decides what it shows.
            // An accountant has no transactions of their own, so falling back to the CALLER would
            // hand them an empty statement and read as a broken report. Resolve the company
            // account instead — the admin — which is what this screen means by "platform".
            int userId = request.UserID > 0
                ? request.UserID
                : currentUserTypeID == UserTypeCode.Admin
                    ? currentUserID
                    : await ResolveCompanyUserID(currentUserID);
            return await BuildStatement(request, userId, currentUserTypeID);
        }

        /// <summary>The company/admin account id, for callers who are not themselves the admin.</summary>
        private async Task<int> ResolveCompanyUserID(int fallbackUserID)
        {
            // GetAllUsers() already excludes deleted rows (GetAllUsersWithDeleted is the other one).
            var admin = (await _userRepo.GetAllUsers())
                .Where(u => u.UserTypeID == UserTypeCode.Admin)
                .OrderBy(u => u.UserMasterID)
                .FirstOrDefault();
            return admin?.UserMasterID ?? fallbackUserID;
        }

        // Shared: filter TransactionDetails for one user, resolve names, page and map to the report row.
        // callerUserTypeID decides one thing only: whether the PROVIDER behind each row is named.
        // Read the Counterparty mapping below for why that is not a presentation choice.
        private async Task<AccountStatementWrapper> BuildStatement(AccountStatementRequest request, int userId, int callerUserTypeID)
        {
            var rows = (await _repository.GetByUserID(userId)).Where(t => !t.IsDeleted).AsEnumerable();

            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(request.FromDate, out var from))
            {
                fromDate = from.Date;
                rows = rows.Where(t => t.DateTime >= from.Date);
            }
            if (DateTime.TryParse(request.ToDate, out var to))
            {
                toDate = to.Date.AddDays(1).AddTicks(-1);
                rows = rows.Where(t => t.DateTime < to.Date.AddDays(1));
            }

            // One wallet at a time. A statement for someone holding Recharge, DMT and AEPS wallets
            // is three statements interleaved, and the opening / closing figures on it belong to
            // whichever wallet each row happened to move in — so a balance read off a mixed
            // statement is not the balance of anything. Blank / 0 = every wallet, as before.
            if (request.WalletID > 0)
                rows = rows.Where(t => t.WalletID == request.WalletID);

            // Credit / Debit side (entity stores "Credit"/"Debit"; match on the first letter to be
            // tolerant of "CR"/"DR" variants).
            if (!string.IsNullOrWhiteSpace(request.AmountCreditDebit))
            {
                char side = char.ToUpperInvariant(request.AmountCreditDebit.Trim()[0]); // 'C' or 'D'
                rows = rows.Where(t => !string.IsNullOrEmpty(t.AmountCreditDebit)
                    && char.ToUpperInvariant(t.AmountCreditDebit.Trim()[0]) == side);
            }

            if (!string.IsNullOrWhiteSpace(request.CriteriaText))
            {
                string term = request.CriteriaText.Trim();
                if (request.Criteria.Equals("ConsumerNumber", StringComparison.OrdinalIgnoreCase))
                    rows = rows.Where(t => (t.ConsumerNumber ?? "").Contains(term, StringComparison.OrdinalIgnoreCase));
                else if (request.Criteria.Equals("TransactionID", StringComparison.OrdinalIgnoreCase))
                    rows = rows.Where(t => t.ReferenceID.ToString() == term || t.TransactionDetailID.ToString() == term);
            }

            var list = rows.OrderByDescending(t => t.DateTime).ToList();
            int count = list.Count;

            IEnumerable<TransactionDetails> paged = list;
            // An export asks for everything; a screen asks for a page.
            if (request.TakeCount > 0 && !request.IsExcelDownload)
                paged = list.Skip(request.SkipCount).Take(request.TakeCount);
            var pagedList = paged.ToList();

            // Name lookups for this page.
            var user = await _userRepo.GetUserById(userId);
            string personName = user == null ? string.Empty
                : (!string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName : user.PersonName);
            string aliasName = user?.AliasName ?? string.Empty;

            var operatorNames = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);

            // Which wallet each posting moved in — a multi-wallet statement is unreadable without it.
            var walletNames = (await _walletRepo.SelectAll())
                .GroupBy(w => w.WalletID).ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            // The originating transactions for this page, so each row can carry the OPERATOR's
            // reference (what a customer quotes) and the provider that served it — rather than
            // printing our own transaction id in two columns and calling one of them "Txn ID".
            // One query, not one per row. This was a loop of GetByID calls — two hundred round
            // trips to render two hundred lines, and the single reason the statement was the
            // slowest screen in the panel.
            var referenceIds = pagedList.Select(t => t.ReferenceID).Where(id => id > 0).Distinct().ToList();
            var txnByRef = (await _txnRepo.GetByIDs(referenceIds))
                .GroupBy(t => t.TransactionMasterID)
                .ToDictionary(g => g.Key, g => g.First());
            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);

            bool showCounterparty = ReportScope.IsNetworkWide(callerUserTypeID);

            var mapped = pagedList.Select(t =>
            {
                txnByRef.TryGetValue(t.ReferenceID, out var txn);

                // Prefer what the operator called it, then the provider's own reference; an internal
                // posting (a commission credit, say) legitimately has neither.
                string operatorRef = txn == null ? string.Empty
                    : !string.IsNullOrWhiteSpace(txn.ReferenceNo) ? txn.ReferenceNo
                    : !string.IsNullOrWhiteSpace(txn.ProviderRefNo) ? txn.ProviderRefNo
                    : string.Empty;

                // The provider behind the row — the upstream vendor the platform bought from.
                // Admin / support / the accounting desk only. Read down a page it is who the
                // operator buys each service from and in what mix, which is the platform's
                // commercial position and not the retailer's business.
                //
                // Blanked HERE, not merely hidden in the browser. The column is role-gated on the
                // report screen as well, but a hidden column is still in the JSON, and this
                // codebase has already been caught by exactly that: the detail modal and the CSV
                // export both read the raw row and printed what the table had just removed.
                string counterparty = showCounterparty
                    && txn != null && providerNames.TryGetValue(txn.ProviderID, out var pn) ? pn : string.Empty;

                return new AccountStatementViewModel
                {
                    TransactionDetailID = t.TransactionDetailID,
                    DOC = t.DateTime,
                    AliasName = aliasName,
                    PersonName = personName,
                    UserName = personName,
                    MobileNumber = user?.MobileNumber ?? string.Empty,
                    WalletName = walletNames.TryGetValue(t.WalletID, out var wn) ? wn : string.Empty,
                    DOCType = t.DOCType,
                    OperatorName = operatorNames.TryGetValue(t.OperatorID, out var on) ? on : string.Empty,
                    ConsumerNumber = t.ConsumerNumber ?? string.Empty,
                    Counterparty = counterparty,
                    AmountCreditDebit = t.AmountCreditDebit,
                    ReferenceNo = operatorRef,
                    ReferenceID = t.ReferenceID,
                    Amount = t.TransactionAmount,
                    Charge = txn?.Charge ?? 0m,
                    GST = t.GST,
                    DiscountAmount = t.DiscountAmount,
                    TDS = t.TDS,
                    NetCommission = t.DiscountAmount - t.TDS, // commission net of TDS
                    OpeningBalance = t.OpeningBalance,
                    ClosingBalance = t.ClosingBalance,
                    TransactionDescription = t.TransactionDescription,
                };
            }).ToList();

            return new AccountStatementWrapper
            {
                Count = count,
                list = mapped,
                HeaderSummary = await StatementSummary(list, userId, fromDate, toDate, request.WalletID),
            };
        }

        /// <summary>
        /// The statement's figures strip: where the balance started and ended, what settled, what
        /// it earned and what it cost. Computed over the WHOLE filtered period, not the page.
        ///
        /// Opening and closing are not sums. The period's opening is the opening balance of its
        /// earliest posting and its closing is the closing balance of its latest, which is why both
        /// columns are marked totalMode 'first' / 'last' in the table footer too — added up
        /// instead, a week of rows reports about seven times the float as the user's balance.
        ///
        /// "Difference" is closing minus opening: the net movement over the period, which is the
        /// figure anyone reconciling a statement against a bank or a float sheet is looking for.
        /// </summary>
        private async Task<Dictionary<string, object>> StatementSummary(
            List<TransactionDetails> list, int userId, DateTime? fromDate, DateTime? toDate, int walletId)
        {
            // `list` is newest-first, so the period's first posting is at the end.
            decimal opening = list.Count == 0 ? 0m : list[^1].OpeningBalance;
            decimal closing = list.Count == 0 ? 0m : list[0].ClosingBalance;

            // The charge lives on the transaction, not on the ledger posting, so it is summed in
            // SQL over the same user / wallet / period rather than read off these rows.
            decimal charge = await _txnRepo.SumChargeForUser(userId, fromDate, toDate, walletId);

            return new Dictionary<string, object>
            {
                ["Opening Balance"] = opening,
                ["Closing Balance"] = closing,
                ["Success Amount"] = list
                    .Where(t => (t.TransactionDescription ?? string.Empty).Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.TransactionAmount),
                ["Commission"] = list.Sum(t => t.DiscountAmount),
                ["Charge"] = charge,
                ["Difference"] = closing - opening,
            };
        }
    }
}
