using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Processing;
using FintechApp.Requests.AccountProcessing;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old Processing/AccountCoreProcessing.cs. Builds on IAccountProcessingService to
    // settle transactions and reverse commissions on refund.
    //
    // RECHARGE-FAMILY DOUBLE-ENTRY ACCOUNTING (per product owner):
    //   Chain: Retailer -> Admin -> Provider. The Admin FUNDS every downline commission and EARNS
    //   the provider commission. For one recharge (A = amount, R = retailer commission SIGNED, D =
    //   distributor commission, S = super-distributor commission, P = provider commission):
    //     - Retailer: DEBIT A (principal, external sale). If R>0: CREDIT R (funded by Admin).
    //       If R<0 (surcharge): DEBIT |R| folded into the principal (retailer pays A+|R|).
    //     - Each downline commission (R>0, D, S): CREDIT downline + DEBIT Admin (Admin funds it).
    //     - Surcharge (R<0): CREDIT Admin |R| (surcharge accrues to Admin).
    //     - Provider commission: CREDIT Admin P (income from the provider).
    //   Admin NET for the txn = P - (R+D+S) = its profit/loss (stored as AdminCommission). Every
    //   commission credit is matched by an equal Admin debit, so the internal ledger balances; the
    //   only unmatched legs are the two external ones (principal A and provider commission P).
    //   A failure reverses every leg symmetrically, returning all parties to net zero.
    //
    // Non-recharge services keep the legacy single net-admin-commission posting untouched.
    public class AccountCoreProcessingService : IAccountCoreProcessingService
    {
        // Role ids are the DB-authoritative values (UserTypeCode / UserTypeMasters seed):
        // Admin=1, SuperDistributor=2, Distributor=3, Retailer=4, ApiUser=5.
        private const int UserTypeID_Admin = UserTypeCode.Admin;
        private const int UserTypeID_SuperDistributor = UserTypeCode.SuperDistributor;
        private const int UserTypeID_Distributor = UserTypeCode.Distributor;

        private readonly IAccountProcessingService _account;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IUserRepository _userRepo;
        private readonly IServiceKindResolver _serviceKinds;
        private readonly Logger _logger;

        public AccountCoreProcessingService(
            IAccountProcessingService account,
            ITransactionMasterRepository txnRepo,
            IUserRepository userRepo,
            IServiceKindResolver serviceKinds)
        {
            _account = account;
            _txnRepo = txnRepo;
            _userRepo = userRepo;
            _serviceKinds = serviceKinds;
            _logger = new Logger();
        }

        public async Task<int> SuccessTransaction(int transactionMasterID, string referenceNo, string remark)
        {
            var txn = await _txnRepo.GetByID(transactionMasterID);
            if (txn == null) return 0;

            // The wallet debit AND every commission (downline + admin) were posted at initiation, the
            // moment the transaction went to PROCESS. Success just finalises the status.
            return await _txnRepo.UpdateStatus(transactionMasterID, StatusType.Success, referenceNo, remark);
        }

        // ── Initiation postings (step 7): chain commissions ───────────────
        // Posts the downline commission credits and, for the recharge family, the matching admin
        // funding debits (and the surcharge credit to admin). Always posted the moment the txn goes
        // to PROCESS, therefore always reversed on failure.
        public async Task PostInitiationCommissions(int transactionMasterID)
        {
            var txn = await _txnRepo.GetByID(transactionMasterID);
            if (txn == null) return;

            bool adminFunded = await IsChainFunded(txn.ServiceID);
            var admin = adminFunded ? await FindAdminAncestor(txn.UserID) : null;
            int adminID = admin?.UserMasterID ?? 0;

            decimal r = txn.RetailerCommission;   // signed: >0 commission earned, <0 surcharge paid
            decimal d = txn.DistributerCommission;
            decimal s = txn.SuperDistributerCommission;
            int txnID = txn.TransactionMasterID;
            string number = txn.ConsumerNumber;

            // Retailer commission (R>0): credit retailer; recharge -> debit the admin that funds it.
            if (r > 0.00001m)
            {
                if (await CreditDownline(txn.UserID, txn, r, $"Recharge commission, txn #{txnID} to: {number}")
                    && adminFunded && adminID > 0 && adminID != txn.UserID)
                    await PostDebit(adminID, txn, r, $"Recharge retailer commission funded, txn #{txnID}");
            }
            // Retailer surcharge (R<0): the retailer already paid |R| inside the principal debit; the
            // surcharge accrues to the admin.
            else if (r < -0.00001m && adminFunded && adminID > 0 && adminID != txn.UserID)
            {
                await PostCredit(adminID, txn, Math.Abs(r), $"Recharge surcharge received, txn #{txnID}");
            }

            // Distributor commission: credit distributor; recharge -> debit admin (funds it).
            if (d > 0.00001m && txn.DistributerUserID > 0)
            {
                if (await CreditDownline(txn.DistributerUserID, txn, d, $"Recharge distributor commission, txn #{txnID}")
                    && adminFunded && adminID > 0 && adminID != txn.DistributerUserID)
                    await PostDebit(adminID, txn, d, $"Recharge distributor commission funded, txn #{txnID}");
            }
            // Distributor-tier surcharge (D<0): the retailer already paid |D| inside the principal
            // debit; it accrues to the admin (no distributor payout).
            else if (d < -0.00001m && adminFunded && adminID > 0 && adminID != txn.UserID)
            {
                await PostCredit(adminID, txn, Math.Abs(d), $"Recharge distributor surcharge received, txn #{txnID}");
            }

            // Super-distributor commission: credit super-distributor; recharge -> debit admin.
            if (s > 0.00001m && txn.SuperDistributerUserID > 0)
            {
                if (await CreditDownline(txn.SuperDistributerUserID, txn, s, $"Recharge super-distributor commission, txn #{txnID}")
                    && adminFunded && adminID > 0 && adminID != txn.SuperDistributerUserID)
                    await PostDebit(adminID, txn, s, $"Recharge super-distributor commission funded, txn #{txnID}");
            }
            // Super-distributor-tier surcharge (S<0): retailer paid |S| in the principal debit; accrues to admin.
            else if (s < -0.00001m && adminFunded && adminID > 0 && adminID != txn.UserID)
            {
                await PostCredit(adminID, txn, Math.Abs(s), $"Recharge super-distributor surcharge received, txn #{txnID}");
            }
        }

        // ── Initiation postings (step 10): admin income ───────────────────
        // Credits the admin's income at initiation (non-failed). Recharge: the provider commission P
        // (which, netted against the step-7 admin debits, leaves the admin its profit P-(R+D+S)).
        // Non-recharge services: the single net admin commission (legacy).
        public async Task CreditAdminOutcome(int transactionMasterID)
        {
            var txn = await _txnRepo.GetByID(transactionMasterID);
            if (txn == null) return;

            var admin = await FindAdminAncestor(txn.UserID);
            if (admin == null) return;

            if (await _serviceKinds.IsRechargeService(txn.ServiceID))
            {
                if (txn.ProviderCommission > 0.00001m)
                    await PostCredit(admin.UserMasterID, txn, txn.ProviderCommission,
                        $"Recharge provider commission income, txn #{txn.TransactionMasterID}");
            }
            else if (txn.AdminCommission > 0.00001m)
            {
                await PostCredit(admin.UserMasterID, txn, txn.AdminCommission,
                    $"Admin commission, from txn #{txn.TransactionMasterID} to: {txn.ConsumerNumber}");
            }
        }

        // ── Refund / reversal ─────────────────────────────────────────────
        public async Task<string> RefundTransaction(int transactionMasterID, string message, string docType)
        {
            await using (await ConcurrencyControl.AcquireLockAsync(transactionMasterID, transactionMasterID.ToString()))
            {
                var txn = await _txnRepo.GetByID(transactionMasterID);
                if (txn == null) return "Transaction not found.";

                // Already failed/refunded → nothing to do.
                if (txn.Status.EqualsIgnoreCase(StatusType.Failed) || txn.Status.EqualsIgnoreCase("REFUND"))
                    return "Transaction already failed/refunded.";

                bool adminFunded = await IsChainFunded(txn.ServiceID);
                // Captured BEFORE UpdateCommissionOutcome zeroes them. ProviderCommission>0 is the
                // reliable "admin income was credited" signal (credited only on a non-failed dispatch);
                // a first-time inline failure never ran step 10 so both are 0 and nothing is reversed.
                decimal providerCommissionBefore = txn.ProviderCommission;
                decimal adminCommissionBefore = txn.AdminCommission;

                int rows = await _txnRepo.UpdateStatus(transactionMasterID, StatusType.Failed, string.Empty, message);
                if (rows <= 0) return Messages.Common.Message.ExceptionOccured;

                // A failed transaction earns nothing — clear the commission outcome recorded while pending.
                await _txnRepo.UpdateCommissionOutcome(transactionMasterID, 0m, 0m);

                string refundMessage = $"{message}, PID : {txn.TransactionMasterID}";

                // AEPS / MicroATM are cash-in services — no wallet refund.
                if (await _serviceKinds.IsCashInService(txn.ServiceID))
                    return StatusType.Ok;

                // Credit the debited amount back to the user wallet (principal, incl. any surcharge |R|).
                decimal discountAmountWithTDS = txn.RetailerCommission - txn.RetailerTDS;
                await _account.CreditBalance(new AccountPostingRequest
                {
                    UserID = txn.UserID,
                    WalletID = txn.WalletID,
                    AmountWithCommission = txn.DebitAmount,
                    Amount = txn.Amount,
                    ConsumerNumber = txn.ConsumerNumber,
                    CommissionCreditDebit = CommissionCreditDebitType.None,
                    DOCType = docType,
                    AssignedUserID = txn.UserID,
                    ServiceID = txn.ServiceID,
                    OperatorID = txn.OperatorID,
                    TransactionDescription = refundMessage,
                    ReferenceID = txn.TransactionMasterID,
                    TDS = txn.RetailerTDS * -1,
                    GST = 0,
                    DiscountAmount = discountAmountWithTDS
                });

                if (adminFunded)
                {
                    // Mirror-reverse the admin-funded chain commissions posted at step 7.
                    await ReverseInitiationCommissions(txn);

                    // Reverse the provider-commission income credited at step 10 — only if credited.
                    if (providerCommissionBefore > 0.00001m)
                    {
                        var admin = await FindAdminAncestor(txn.UserID);
                        if (admin != null)
                            await PostDebit(admin.UserMasterID, txn, providerCommissionBefore,
                                $"Recharge provider commission reversal, txn #{txn.TransactionMasterID}");
                    }
                }
                else
                {
                    // Legacy reversal for non-recharge services (bank transfer / DMT etc.): debit the
                    // downlines and reverse the single net admin commission credit. Unchanged behaviour.
                    await RetailerDebitCommission(txn.UserID, txn.WalletID, txn.ServiceID, txn.OperatorID, txn.TransactionMasterID,
                        txn.RetailerCommission, txn.ConsumerNumber, docType, refundMessage, txn.RetailerTDS);
                    await UplineDebitCommission(txn.UserID, txn.WalletID, txn.ServiceID, txn.OperatorID, txn.TransactionMasterID,
                        txn.DistributerCommission, txn.ConsumerNumber, docType, refundMessage, txn.DistributerTDS, isForSuperDist: false);
                    await UplineDebitCommission(txn.UserID, txn.WalletID, txn.ServiceID, txn.OperatorID, txn.TransactionMasterID,
                        txn.SuperDistributerCommission, txn.ConsumerNumber, docType, refundMessage, txn.SuperDistributerTDS, isForSuperDist: true);
                    if (adminCommissionBefore > 0.00001m)
                    {
                        var admin = await FindAdminAncestor(txn.UserID);
                        if (admin != null)
                            await PostDebit(admin.UserMasterID, txn, adminCommissionBefore,
                                $"Admin commission reversal, txn #{txn.TransactionMasterID}");
                    }
                }

                return StatusType.Ok;
            }
        }

        // Exact mirror of PostInitiationCommissions for the recharge family: debit each downline back
        // and credit the admin back (reverse the funding); debit the admin the surcharge back.
        private async Task ReverseInitiationCommissions(TransactionMaster txn)
        {
            var admin = await FindAdminAncestor(txn.UserID);
            int adminID = admin?.UserMasterID ?? 0;
            decimal r = txn.RetailerCommission, d = txn.DistributerCommission, s = txn.SuperDistributerCommission;
            int txnID = txn.TransactionMasterID;

            if (r > 0.00001m)
            {
                if (await DebitDownline(txn.UserID, txn, r, $"Recharge commission reversal, txn #{txnID}")
                    && adminID > 0 && adminID != txn.UserID)
                    await PostCredit(adminID, txn, r, $"Recharge retailer commission refund, txn #{txnID}");
            }
            else if (r < -0.00001m && adminID > 0 && adminID != txn.UserID)
            {
                await PostDebit(adminID, txn, Math.Abs(r), $"Recharge surcharge reversal, txn #{txnID}");
            }

            if (d > 0.00001m && txn.DistributerUserID > 0)
            {
                if (await DebitDownline(txn.DistributerUserID, txn, d, $"Recharge distributor commission reversal, txn #{txnID}")
                    && adminID > 0 && adminID != txn.DistributerUserID)
                    await PostCredit(adminID, txn, d, $"Recharge distributor commission refund, txn #{txnID}");
            }
            else if (d < -0.00001m && adminID > 0 && adminID != txn.UserID)
            {
                await PostDebit(adminID, txn, Math.Abs(d), $"Recharge distributor surcharge reversal, txn #{txnID}");
            }

            if (s > 0.00001m && txn.SuperDistributerUserID > 0)
            {
                if (await DebitDownline(txn.SuperDistributerUserID, txn, s, $"Recharge super-distributor commission reversal, txn #{txnID}")
                    && adminID > 0 && adminID != txn.SuperDistributerUserID)
                    await PostCredit(adminID, txn, s, $"Recharge super-distributor commission refund, txn #{txnID}");
            }
            else if (s < -0.00001m && adminID > 0 && adminID != txn.UserID)
            {
                await PostDebit(adminID, txn, Math.Abs(s), $"Recharge super-distributor surcharge reversal, txn #{txnID}");
            }
        }

        // ── Small posting helpers (commission-style: no principal, ledger row written) ──
        private Task<int> PostCredit(int userID, TransactionMaster txn, decimal amount, string desc)
            => _account.CreditBalance(BuildPosting(userID, txn, amount, desc, CommissionCreditDebitType.Credit));

        private Task<int> PostDebit(int userID, TransactionMaster txn, decimal amount, string desc)
            => _account.DebitBalance(BuildPosting(userID, txn, amount, desc, CommissionCreditDebitType.Debit));

        private static AccountPostingRequest BuildPosting(int userID, TransactionMaster txn, decimal amount, string desc, string creditDebit)
            => new()
            {
                UserID = userID,
                WalletID = txn.WalletID,
                AmountWithCommission = amount,
                Amount = 0,
                ConsumerNumber = txn.ConsumerNumber,
                CommissionCreditDebit = creditDebit,
                DOCType = DOCTypeConstants.Recharge,
                AssignedUserID = txn.UserID,
                ServiceID = txn.ServiceID,
                OperatorID = txn.OperatorID,
                TransactionDescription = desc,
                ReferenceID = txn.TransactionMasterID,
                TDS = 0,
                GST = 0,
                DiscountAmount = amount
            };

        // Credit a downline its commission; returns false (and posts nothing) when the target is
        // missing or is the admin itself (admin does not earn a downline commission on its own txn).
        private async Task<bool> CreditDownline(int downlineID, TransactionMaster txn, decimal amount, string desc)
        {
            if (downlineID <= 0) return false;
            var target = await _userRepo.GetUserById(downlineID);
            if (target == null || target.UserTypeID == UserTypeID_Admin) return false;
            await PostCredit(downlineID, txn, amount, desc);
            return true;
        }

        // Reverse of CreditDownline.
        private async Task<bool> DebitDownline(int downlineID, TransactionMaster txn, decimal amount, string desc)
        {
            if (downlineID <= 0) return false;
            var target = await _userRepo.GetUserById(downlineID);
            if (target == null || target.UserTypeID == UserTypeID_Admin) return false;
            await PostDebit(downlineID, txn, amount, desc);
            return true;
        }

        // Services whose slab commissions/surcharges settle through the balanced chain double-entry
        // (credit downline + debit the funding admin; surcharge accrues to admin). Recharge earns the
        // funding from the provider commission; DMT funds it from the transfer charge the retailer
        // pays on top of the principal. Both are symmetric and reverse cleanly on refund. Other
        // services keep the legacy single net-admin-commission posting.
        private async Task<bool> IsChainFunded(int serviceID)
            => await _serviceKinds.IsRechargeService(serviceID) || await _serviceKinds.IsDMTService(serviceID);

        // Walk the parent chain from the transacting user up to the Admin-typed account.
        private async Task<UserMaster?> FindAdminAncestor(int userID)
        {
            var user = await _userRepo.GetUserById(userID);
            for (int guard = 0; user != null && guard < 10; guard++)
            {
                if (user.UserTypeID == UserTypeID_Admin) return user;
                int parentID = user.ParentID ?? 0;
                if (parentID <= 0) break;
                user = await _userRepo.GetUserById(parentID);
            }
            return null;
        }

        // ── Legacy commission reversal (non-recharge services only) ───────
        // Debits a reversed commission from the retailer, in the transaction's own wallet.
        private async Task RetailerDebitCommission(int retailerID, int walletID, int serviceID, int operatorID, int txnID,
            decimal commissionAmount, string consumerNumber, string docType, string description, decimal tds)
        {
            if (commissionAmount <= 0) return;
            commissionAmount -= tds;

            await _account.DebitBalance(new AccountPostingRequest
            {
                UserID = retailerID,
                WalletID = walletID,
                AmountWithCommission = commissionAmount,
                Amount = 0,
                ConsumerNumber = consumerNumber,
                CommissionCreditDebit = CommissionCreditDebitType.Debit,
                DOCType = docType,
                AssignedUserID = retailerID,
                ServiceID = serviceID,
                OperatorID = operatorID,
                TransactionDescription = description,
                ReferenceID = txnID,
                TDS = 0,
                GST = 0,
                DiscountAmount = commissionAmount
            });
        }

        // Debits a reversed commission from the upline (distributor or super-distributor).
        private async Task UplineDebitCommission(int retailerID, int walletID, int serviceID, int operatorID, int txnID,
            decimal commissionAmount, string consumerNumber, string docType, string description, decimal tds, bool isForSuperDist)
        {
            if (commissionAmount == 0) return;
            commissionAmount -= tds;

            var retailer = await _userRepo.GetUserById(retailerID);
            int uplineID = retailer?.ParentID ?? 0;
            if (uplineID <= 0) return;

            if (isForSuperDist)
            {
                var distributor = await _userRepo.GetUserById(uplineID);
                int superDistID = distributor?.ParentID ?? 0;
                if (superDistID <= 0) return;

                var superDist = await _userRepo.GetUserById(superDistID);
                if (superDist != null && superDist.UserTypeID == UserTypeID_Admin)
                    superDistID = uplineID;

                uplineID = superDistID;
            }

            var upline = await _userRepo.GetUserById(uplineID);
            if (upline == null) return;
            if (upline.UserTypeID == UserTypeID_Admin) return;

            await _account.DebitBalance(new AccountPostingRequest
            {
                UserID = uplineID,
                WalletID = walletID,
                AmountWithCommission = commissionAmount,
                Amount = 0,
                ConsumerNumber = consumerNumber,
                CommissionCreditDebit = CommissionCreditDebitType.Debit,
                DOCType = docType,
                AssignedUserID = retailerID,
                ServiceID = serviceID,
                OperatorID = operatorID,
                TransactionDescription = description,
                ReferenceID = txnID,
                TDS = 0,
                GST = 0,
                DiscountAmount = commissionAmount
            });
        }
    }
}
