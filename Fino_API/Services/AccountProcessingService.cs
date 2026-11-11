using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Processing;
using FintechApp.Requests.AccountProcessing;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old Processing/AccountProcessing.cs — the atomic wallet ledger engine.
    // DebitBalance/CreditBalance update the user's wallet balance under an item-level lock and
    // write a TransactionDetails ledger row (opening/closing balance, TDS/GST, commission flag).
    public class AccountProcessingService : IAccountProcessingService
    {
        private const int UserTypeID_Admin = 1;

        private readonly IUserWalletMasterRepository _userWalletRepo;
        private readonly IUserBalanceCappingRepository _cappingRepo;
        private readonly ITransactionDetailsRepository _txnDetailsRepo;
        private readonly IUserRepository _userRepo;
        private readonly Logger _logger;

        public AccountProcessingService(
            IUserWalletMasterRepository userWalletRepo,
            IUserBalanceCappingRepository cappingRepo,
            ITransactionDetailsRepository txnDetailsRepo,
            IUserRepository userRepo)
        {
            _userWalletRepo = userWalletRepo;
            _cappingRepo = cappingRepo;
            _txnDetailsRepo = txnDetailsRepo;
            _userRepo = userRepo;
            _logger = new Logger();
        }

        public async Task<string> BalanceValidationBeforeTransaction(int userID, int walletID, decimal amount, decimal commission)
        {
            await using (await ConcurrencyControl.AcquireLockAsync(userID, $"wallet:{walletID}"))
            {
                var wallet = (await _userWalletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == walletID);
                if (wallet == null) return "Wallet not found.";

                if (wallet.Balance < amount)
                    return "Insufficient Balance";

                if (wallet.Balance < -0.001M)
                    return "Insufficient Balance";

                if ((wallet.Balance + commission) < amount)
                    return $"Insufficient Balance, due to commission ({commission:0.00})";

                decimal cappingAmount = (await _cappingRepo.GetByUserID(userID))
                    .Where(c => c.WalletID == walletID && !c.IsDelete)
                    .Select(c => c.Amount)
                    .FirstOrDefault();

                if ((wallet.Balance + commission - cappingAmount) < amount)
                    return $"Insufficient Balance, due to commission ({commission:0.00}) and capping Amount ({cappingAmount:0.00})";

                return StatusType.Ok;
            }
        }

        public async Task<int> DebitBalance(AccountPostingRequest request)
        {
            await using (await ConcurrencyControl.AcquireLockAsync(request.UserID, $"wallet:{request.WalletID}"))
            {
                return await Process(request, request.AmountWithCommission * -1, AmountCreditDebitType.Debit);
            }
        }

        public async Task<int> CreditBalance(AccountPostingRequest request)
        {
            await using (await ConcurrencyControl.AcquireLockAsync(request.UserID, $"wallet:{request.WalletID}"))
            {
                return await Process(request, request.AmountWithCommission, AmountCreditDebitType.Credit);
            }
        }

        // Core posting: apply the signed amount to the wallet with a single ATOMIC conditional UPDATE
        // (the DB row-lock enforces the overdraft/hold guard — correct under concurrency and across
        // instances, no read-modify-write race), then write a TransactionDetails ledger row using the
        // exact opening/closing the DB saw.
        private async Task<int> Process(AccountPostingRequest request, decimal amountWithFormula, string amountCreditDebitFlag)
        {
            bool isDebit = amountCreditDebitFlag.EqualsIgnoreCase(AmountCreditDebitType.Debit);
            bool isAdmin = await IsAdminUser(request.UserID);

            // The Admin float account is seeded (seed_admin.sql) WITHOUT wallet rows — unlike users
            // created through UserService, which get a row per active wallet. Without this, every
            // admin-side recharge posting (funding debits + provider-commission income credit) would
            // silently no-op, leaving downline commission credits unmatched and breaking double-entry.
            // Provision the admin's wallet row on demand so its postings always land. A missing
            // NON-admin wallet stays an error.
            var wallet = await _userWalletRepo.GetByUserAndWallet(request.UserID, request.WalletID);
            if (wallet == null)
            {
                if (!isAdmin) return -1;
                if (await _userWalletRepo.Insert(new UserWalletMaster { UserID = request.UserID, WalletID = request.WalletID, Balance = 0m, BalanceHash = null }) <= 0)
                    return -1;
            }

            // Only the admin float account may go negative. For everyone else the on-hold portion is
            // frozen: a debit may not eat into it (spendable = Balance - HoldBalance). HoldBalance is 0
            // unless an admin placed a hold, so this leaves normal transactions untouched. A credit
            // (positive delta) always applies. The check-and-apply is one atomic statement, so a
            // concurrent debit can never oversell.
            WalletDeltaResult delta;
            try
            {
                delta = await _userWalletRepo.TryApplyDelta(
                    request.UserID, request.WalletID, amountWithFormula,
                    allowOverdraft: isAdmin,
                    respectHold: isDebit && !isAdmin);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return -1;
            }

            if (!delta.Applied) return -1;   // insufficient balance / hold guard

            request.PostedOpeningBalance = delta.OpeningBalance;
            request.PostedClosingBalance = delta.ClosingBalance;

            var details = new TransactionDetails
            {
                UserID = request.UserID,
                TransactionAmount = request.Amount,
                ConsumerNumber = request.ConsumerNumber,
                AmountCreditDebit = amountCreditDebitFlag,
                CommissionCreditDebit = request.CommissionCreditDebit,
                DOCType = request.DOCType,
                AssignedUserID = request.AssignedUserID,
                OpeningBalance = delta.OpeningBalance,
                ClosingBalance = delta.ClosingBalance,
                ServiceID = request.ServiceID,
                OperatorID = request.OperatorID,
                WalletID = request.WalletID,
                TransactionDescription = request.TransactionDescription,
                ReferenceID = request.ReferenceID,
                TDS = request.TDS,
                GST = request.GST,
                DiscountAmount = request.DiscountAmount,
                PayMode = request.PayMode,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
                IsDeleted = false
            };

            try
            {
                return await _txnDetailsRepo.Insert(details);
            }
            catch (Exception ex)
            {
                // The balance already moved atomically; if the ledger row can't be written, reverse the
                // balance so we never leave money changed without a matching ledger entry.
                _logger.Write(ex);
                try { await _userWalletRepo.TryApplyDelta(request.UserID, request.WalletID, -amountWithFormula, allowOverdraft: true, respectHold: false); }
                catch (Exception rex) { _logger.Write(rex); }
                return -1;
            }
        }

        private async Task<bool> IsAdminUser(int userID)
        {
            var user = await _userRepo.GetUserById(userID);
            return user != null && user.UserTypeID == UserTypeID_Admin;
        }
    }
}
