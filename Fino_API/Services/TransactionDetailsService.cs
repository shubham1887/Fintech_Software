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
        public TransactionDetailsService(ITransactionDetailsRepository repository, IUserRepository userRepo, IOperatorRepository operatorRepo)
        {
            _repository = repository;
            _userRepo = userRepo;
            _operatorRepo = operatorRepo;
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
            return await BuildStatement(request, userId);
        }

        // ── Admin Accounting (company / admin account) ────────────────
        public async Task<AccountStatementWrapper> SelectAdminAccounting(AccountStatementRequest request, int currentUserID, int currentUserTypeID)
        {
            if (currentUserTypeID != UserTypeCode.Admin && currentUserTypeID != UserTypeCode.SupportUser)
                return new AccountStatementWrapper();
            // Company accounting = the admin's own account statement (the admin/company wallet).
            int userId = request.UserID > 0 ? request.UserID : currentUserID;
            return await BuildStatement(request, userId);
        }

        // Shared: filter TransactionDetails for one user, resolve names, page and map to the report row.
        private async Task<AccountStatementWrapper> BuildStatement(AccountStatementRequest request, int userId)
        {
            var rows = (await _repository.GetByUserID(userId)).Where(t => !t.IsDeleted).AsEnumerable();

            if (DateTime.TryParse(request.FromDate, out var from)) rows = rows.Where(t => t.DateTime >= from.Date);
            if (DateTime.TryParse(request.ToDate, out var to)) rows = rows.Where(t => t.DateTime < to.Date.AddDays(1));

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
            if (request.TakeCount > 0) paged = list.Skip(request.SkipCount).Take(request.TakeCount);
            var pagedList = paged.ToList();

            // Name lookups for this page.
            var user = await _userRepo.GetUserById(userId);
            string personName = user == null ? string.Empty
                : (!string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName : user.PersonName);
            string aliasName = user?.AliasName ?? string.Empty;

            var operatorNames = (await _operatorRepo.GetAllOperators())
                .GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);

            var mapped = pagedList.Select(t => new AccountStatementViewModel
            {
                TransactionDetailID = t.TransactionDetailID,
                DOC = t.DateTime,
                AliasName = aliasName,
                PersonName = personName,
                DOCType = t.DOCType,
                OperatorName = operatorNames.TryGetValue(t.OperatorID, out var on) ? on : string.Empty,
                AmountCreditDebit = t.AmountCreditDebit,
                ReferenceNo = t.ReferenceID.ToString(),
                ReferenceID = t.ReferenceID,
                Amount = t.TransactionAmount,
                Charge = 0m, // not tracked on TransactionDetails yet
                GST = t.GST,
                DiscountAmount = t.DiscountAmount,
                TDS = t.TDS,
                NetCommission = t.DiscountAmount - t.TDS, // commission net of TDS
                OpeningBalance = t.OpeningBalance,
                ClosingBalance = t.ClosingBalance,
                TransactionDescription = t.TransactionDescription,
            }).ToList();

            return new AccountStatementWrapper { Count = count, list = mapped };
        }
    }
}
