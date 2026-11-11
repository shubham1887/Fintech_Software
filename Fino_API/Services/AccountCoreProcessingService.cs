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
        private readonly ITransactionDetailsRepository _txnDetailsRepo;
        private readonly IUserRepository _userRepo;
        private readonly IServiceKindResolver _serviceKinds;
        private readonly IMessageService _messageService;
        private readonly ISettlementAccountingService _settlementAccounting;
        private readonly Logger _logger;

        public AccountCoreProcessingService(
            IAccountProcessingService account,
            ITransactionMasterRepository txnRepo,
            ITransactionDetailsRepository txnDetailsRepo,
            IUserRepository userRepo,
            IServiceKindResolver serviceKinds,
            IMessageService messageService,
            ISettlementAccountingService settlementAccounting)
        {
            _account = account;
            _txnRepo = txnRepo;
            _txnDetailsRepo = txnDetailsRepo;
            _userRepo = userRepo;
            _serviceKinds = serviceKinds;
            _messageService = messageService;
            _settlementAccounting = settlementAccounting;
            _logger = new Logger();
        }

        public async Task<int> SuccessTransaction(int transactionMasterID, string referenceNo, string remark)
        {
            var txn = await _txnRepo.GetByID(transactionMasterID);
            if (txn == null) return 0;

            // The wallet debit AND every commission (downline + admin) were posted at initiation, the
            // moment the transaction went to PROCESS. Success just finalises the status.
            int rows = await _txnRepo.UpdateStatus(transactionMasterID, StatusType.Success, referenceNo, remark);
            if (rows > 0)
            {
                await NotifyOutcome(txn, success: true, reference: referenceNo, reason: string.Empty);

                // Stamp the tax and enqueue the ledger events. Every settlement path in the product
                // converges here — inline dispatch, the status-check sweep, reconciliation, a manual
                // settle — so this one call is what puts a figure into the books for all of them.
                // It is idempotent and it cannot throw: see ISettlementAccountingService.
                await _settlementAccounting.OnTransactionSettled(transactionMasterID);
            }
            return rows;
        }

        /// <summary>
        /// Text the customer the outcome of their transaction, using the Message Setting template
        /// that matches the service kind (recharge / money transfer / bill payment).
        ///
        /// These templates ship with the product and the admin can edit them, but nothing ever
        /// raised them — a recharge could succeed or be refunded and the retailer was told only by
        /// the screen in front of them, never by SMS. Both outcomes go through here because it is
        /// the single place every settlement path (inline dispatch, the status-check job,
        /// reconciliation, a manual settle) already converges on.
        ///
        /// Fire-and-forget and fully guarded: a messaging failure must never affect a settlement
        /// that has already moved money.
        /// </summary>
        private async Task NotifyOutcome(TransactionMaster txn, bool success, string reference, string reason)
        {
            try
            {
                var user = await _userRepo.GetUserById(txn.UserID);
                if (user == null || string.IsNullOrWhiteSpace(user.MobileNumber)) return;

                bool isDmt = await _serviceKinds.IsDMTService(txn.ServiceID);
                bool isRecharge = await _serviceKinds.IsRechargeService(txn.ServiceID);
                // A failed bill payment gets its own event. It used to fall through to "Recharge
                // Failed", so a customer whose electricity bill did not go through was told their
                // recharge had failed — for a consumer number they had never recharged.
                string eventType = isDmt
                    ? (success ? "Money Transfer Success" : "Money Transfer Failed")
                    : isRecharge
                        ? (success ? "Recharge Success" : "Recharge Failed")
                        : (success ? "Bill Payment Success" : "Bill Payment Failed");

                string operatorName = txn.OperatorMaster?.OperatorName ?? txn.ServiceMaster?.Name ?? string.Empty;
                var tokens = new Dictionary<string, string>
                {
                    ["Amount"] = txn.Amount.ToString("F2"),
                    ["Operator"] = operatorName,
                    ["Number"] = txn.ConsumerNumber ?? string.Empty,
                    ["TxnID"] = string.IsNullOrWhiteSpace(reference) ? txn.TransactionMasterID.ToString() : reference,
                    ["Balance"] = txn.ClosingBalance.ToString("F2"),
                    ["Remark"] = reason ?? string.Empty,
                    ["CustomerName"] = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName ?? string.Empty : user.PersonName,
                };

                string fallback = success
                    ? $"{operatorName} {txn.ConsumerNumber} of Rs.{txn.Amount:F2} is SUCCESSFUL. Txn ID: {tokens["TxnID"]}"
                    : $"{operatorName} {txn.ConsumerNumber} of Rs.{txn.Amount:F2} FAILED and the amount has been refunded. Txn ID: {tokens["TxnID"]}";

                _ = _messageService.SendEventAsync(eventType, user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, user.EmailID, tokens, fallback, emailSubject: eventType);
            }
            catch (Exception ex) { _logger.Write(ex); }
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

            // The admin ancestor still funds everything the operator prices itself, and it is
            // resolved lazily because finding it walks the whole parent chain — work there is no
            // reason to do on a service that funds nothing.
            int? adminCache = null;
            async Task<int> Admin()
            {
                if (adminCache is int cached) return cached;
                var found = await FindAdminAncestor(txn.UserID);
                adminCache = found?.UserMasterID ?? 0;
                return adminCache.Value;
            }

            decimal r = txn.RetailerCommission;   // signed: >0 commission earned, <0 surcharge paid
            decimal d = txn.DistributerCommission;
            decimal s = txn.SuperDistributerCommission;
            int txnID = txn.TransactionMasterID;
            string number = txn.ConsumerNumber;

            // Who pays each tier. A stamped funder is a reseller whose own card priced that tier:
            // their wallet pays, and it pays whatever the service is, because a reseller handing
            // out money it was never debited for is money created from nothing. An unstamped tier
            // (0) is the operator's own and keeps the legacy gate, so only the services that
            // settle through the balanced chain post a funding debit at all.
            int rFunder = txn.RetailerCommissionFunderID > 0 ? txn.RetailerCommissionFunderID : await Admin();
            int dFunder = txn.DistributerCommissionFunderID > 0 ? txn.DistributerCommissionFunderID : await Admin();
            int sFunder = txn.SuperDistributerCommissionFunderID > 0 ? txn.SuperDistributerCommissionFunderID : await Admin();
            bool rFunds = txn.RetailerCommissionFunderID > 0 || adminFunded;
            bool dFunds = txn.DistributerCommissionFunderID > 0 || adminFunded;
            bool sFunds = txn.SuperDistributerCommissionFunderID > 0 || adminFunded;

            // TOP DOWN, and the order is load-bearing once a reseller funds a tier below it. A
            // distributor is credited its own commission here and debited for its retailer here;
            // post the retailer leg first and the debit can be refused for want of balance on a
            // transaction that was always going to leave the distributor better off.
            //
            // Super-distributor: credit them, debit whoever funds them.
            if (s > 0.00001m && txn.SuperDistributerUserID > 0)
            {
                await PostFundedCommission(txn, txn.SuperDistributerUserID, s, sFunder, sFunds,
                    ChainLegDescriptions.SuperDistributorCommission(txnID),
                    ChainLegDescriptions.SuperDistributorFunded(txnID),
                    ChainLegDescriptions.SuperDistributorCommissionReversal(txnID));
            }
            // Super-distributor-tier surcharge (S<0): the retailer already paid it inside the
            // principal debit, and it accrues to whoever priced that tier.
            else if (s < -0.00001m && sFunds && sFunder > 0 && sFunder != txn.UserID)
            {
                await PostSingleLeg(sFunder, txn, Math.Abs(s), ChainLegDescriptions.SuperDistributorSurchargeReceived(txnID), credit: true);
            }

            // Distributor commission: credit distributor, debit whoever funds them.
            if (d > 0.00001m && txn.DistributerUserID > 0)
            {
                await PostFundedCommission(txn, txn.DistributerUserID, d, dFunder, dFunds,
                    ChainLegDescriptions.DistributorCommission(txnID),
                    ChainLegDescriptions.DistributorFunded(txnID),
                    ChainLegDescriptions.DistributorCommissionReversal(txnID));
            }
            else if (d < -0.00001m && dFunds && dFunder > 0 && dFunder != txn.UserID)
            {
                await PostSingleLeg(dFunder, txn, Math.Abs(d), ChainLegDescriptions.DistributorSurchargeReceived(txnID), credit: true);
            }

            // Retailer commission (R>0): credit the retailer, debit whoever priced them — the
            // distributor on a delegated card, the operator otherwise.
            if (r > 0.00001m)
            {
                await PostFundedCommission(txn, txn.UserID, r, rFunder, rFunds,
                    ChainLegDescriptions.RetailerCommission(txnID, number),
                    ChainLegDescriptions.RetailerFunded(txnID),
                    ChainLegDescriptions.RetailerCommissionReversal(txnID));
            }
            else if (r < -0.00001m && rFunds && rFunder > 0 && rFunder != txn.UserID)
            {
                await PostSingleLeg(rFunder, txn, Math.Abs(r), ChainLegDescriptions.SurchargeReceived(txnID), credit: true);
            }
        }

        /// <summary>
        /// One tier's commission at initiation, posted as a PAIR: credit the downline, and debit the
        /// admin that funds it. Both legs or neither.
        ///
        /// The funding debit used to be fire-and-forget — `await PostDebit(...)` with the result
        /// dropped on the floor. AccountProcessingService.Process returns -1 on a caught exception or
        /// a rejected delta, so a single transient SQL failure left the downline holding a commission
        /// that nobody paid for, and the refund path, being symmetric, later credited the admin back
        /// for a debit it never took. That is exactly what happened to txn #268 on this install:
        /// Rs 7.65 created out of nothing, with no error recorded anywhere, because the Logs folder is
        /// not writable. Nothing asserted double-entry, so the books simply stopped balancing.
        ///
        /// If the funding leg does not land, the credit is taken straight back. An unpaid commission
        /// is a far smaller wrong than money created from nothing, and the downline sees the wallet it
        /// would have seen had the dispatch failed outright. Should the compensating debit fail too,
        /// LedgerIntegrityService.ChainCommissionsFunded reports the pair as unfunded — a broken leg
        /// now ends up on a report instead of nowhere.
        /// </summary>
        private async Task PostFundedCommission(TransactionMaster txn, int downlineID, decimal amount,
            int funderID, bool postFunding, string creditDesc, string fundedDesc, string undoDesc)
        {
            if (await CreditDownline(downlineID, txn, amount, creditDesc) != LegOutcome.Posted) return;

            // Not a funded leg (the legacy single net posting on a service the operator does not
            // settle through the chain), or the funder IS the downline: there is no funding leg to
            // pair with, and nothing to undo.
            if (!postFunding || funderID <= 0 || funderID == downlineID) return;

            if (await PostDebit(funderID, txn, amount, fundedDesc) > 0) return;

            _logger.Write(new InvalidOperationException(
                $"Funding leg failed to post: {fundedDesc} (Rs {amount:F2}, funder {funderID}). Taking the downline credit back."));
            if (await PostDebit(downlineID, txn, amount, undoDesc) <= 0)
                _logger.Write(new InvalidOperationException(
                    $"Could not take back an unfunded commission: {creditDesc} (Rs {amount:F2}, user {downlineID})."));
        }

        /// <summary>
        /// A leg with no second posting to pair with — a surcharge the retailer already paid inside
        /// the principal debit, or the admin's income from the provider. There is nothing to
        /// compensate with if it fails, so the result is checked and recorded; the refund path then
        /// refuses to reverse a leg that was never posted, which is the half of this that creates
        /// money when it goes wrong.
        /// </summary>
        private async Task PostSingleLeg(int userID, TransactionMaster txn, decimal amount, string desc, bool credit)
        {
            int result = credit
                ? await PostCredit(userID, txn, amount, desc)
                : await PostDebit(userID, txn, amount, desc);
            if (result <= 0)
                _logger.Write(new InvalidOperationException($"Leg failed to post: {desc} (Rs {amount:F2}, user {userID})."));
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
                    await PostSingleLeg(admin.UserMasterID, txn, txn.ProviderCommission,
                        ChainLegDescriptions.ProviderCommissionIncome(txn.TransactionMasterID), credit: true);
            }
            else if (txn.AdminCommission > 0.00001m)
            {
                await PostSingleLeg(admin.UserMasterID, txn, txn.AdminCommission,
                    ChainLegDescriptions.LegacyAdminCommission(txn.TransactionMasterID, txn.ConsumerNumber), credit: true);
            }
        }

        // ── Refund / reversal ─────────────────────────────────────────────
        public async Task<string> RefundTransaction(int transactionMasterID, string message, string docType)
        {
            // The item key is namespaced ("txn:") because this lock and the wallet lock taken by
            // AccountProcessingService share one key space. Both used to key on a bare number, so
            // a refund on transaction N would self-deadlock the moment N also matched the user id
            // and wallet id it was refunding — the outer txn lock and the inner wallet lock became
            // the same lock. It times out after 30s and throws, which means the wallet is never
            // credited back: the customer's money stays debited for a transfer that failed.
            await using (await ConcurrencyControl.AcquireLockAsync(transactionMasterID, $"txn:{transactionMasterID}"))
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

                await NotifyOutcome(txn, success: false, reference: txn.ReferenceNo, reason: message);

                // A failed transaction earns nothing — clear the commission outcome recorded while pending.
                await _txnRepo.UpdateCommissionOutcome(transactionMasterID, 0m, 0m);

                // ...and it owes no GST on what it did not earn. Clears the charge/GST breakdown if
                // this transaction had already been settled once and is now being reversed. Same
                // contract as the stamp itself: it cannot throw, and the refund stands either way.
                await _settlementAccounting.OnTransactionRefunded(transactionMasterID);

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

                // Which legs this transaction actually posted. Read once, and used by every reversal
                // below to refuse to reverse a leg that is not in the ledger. Cheap next to the
                // postings it guards, and the only way to tell a leg that was posted from one that a
                // transient failure dropped on the way in.
                var posted = await PostedLegs(transactionMasterID);

                if (adminFunded)
                {
                    // Mirror-reverse the admin-funded chain commissions posted at step 7.
                    await ReverseInitiationCommissions(txn, posted);

                    // Reverse the provider-commission income credited at step 10 — only if credited.
                    // Step 10 credits provider income for the RECHARGE family alone
                    // (CreditAdminOutcome). A DMT transfer is chain-funded too, and now records what
                    // the provider charged, so without this guard a refund would debit the admin for
                    // income it was never given.
                    if (providerCommissionBefore > 0.00001m && await _serviceKinds.IsRechargeService(txn.ServiceID))
                    {
                        var admin = await FindAdminAncestor(txn.UserID);
                        if (admin != null)
                            await ReverseSingleLeg(admin.UserMasterID, txn, providerCommissionBefore, posted,
                                ChainLegDescriptions.ProviderCommissionIncome(txn.TransactionMasterID),
                                $"Recharge provider commission reversal, txn #{txn.TransactionMasterID}", credit: false);
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
                            await ReverseSingleLeg(admin.UserMasterID, txn, adminCommissionBefore, posted,
                                ChainLegDescriptions.LegacyAdminCommission(txn.TransactionMasterID, txn.ConsumerNumber),
                                $"Admin commission reversal, txn #{txn.TransactionMasterID}", credit: false);
                    }
                }

                return StatusType.Ok;
            }
        }

        // Exact mirror of PostInitiationCommissions for the recharge family: debit each downline back
        // and credit the admin back (reverse the funding); debit the admin the surcharge back.
        //
        // "Mirror of what was POSTED", not "mirror of what should have been posted" — see
        // ReverseFundedCommission. A reversal whose original is missing from the ledger hands someone
        // money for a movement that never happened.
        private async Task ReverseInitiationCommissions(TransactionMaster txn, HashSet<string>? posted)
        {
            var admin = await FindAdminAncestor(txn.UserID);
            int adminID = admin?.UserMasterID ?? 0;
            decimal r = txn.RetailerCommission, d = txn.DistributerCommission, s = txn.SuperDistributerCommission;
            int txnID = txn.TransactionMasterID;

            // The funder each leg was POSTED against, read off the transaction rather than off the
            // rate card. A distributor who re-priced its retailers between the sale and the refund
            // must still be the one credited back, or the pair that balanced on the way in does
            // not balance on the way out.
            int rFunder = txn.RetailerCommissionFunderID > 0 ? txn.RetailerCommissionFunderID : adminID;
            int dFunder = txn.DistributerCommissionFunderID > 0 ? txn.DistributerCommissionFunderID : adminID;
            int sFunder = txn.SuperDistributerCommissionFunderID > 0 ? txn.SuperDistributerCommissionFunderID : adminID;

            if (r > 0.00001m)
            {
                await ReverseFundedCommission(txn, txn.UserID, r, rFunder, posted,
                    ChainLegDescriptions.RetailerCommission(txnID, txn.ConsumerNumber),
                    ChainLegDescriptions.RetailerFunded(txnID),
                    ChainLegDescriptions.RetailerCommissionReversal(txnID),
                    ChainLegDescriptions.RetailerFundedRefund(txnID));
            }
            else if (r < -0.00001m && rFunder > 0 && rFunder != txn.UserID)
            {
                await ReverseSingleLeg(rFunder, txn, Math.Abs(r), posted,
                    ChainLegDescriptions.SurchargeReceived(txnID),
                    $"Recharge surcharge reversal, txn #{txnID}", credit: false);
            }

            if (d > 0.00001m && txn.DistributerUserID > 0)
            {
                await ReverseFundedCommission(txn, txn.DistributerUserID, d, dFunder, posted,
                    ChainLegDescriptions.DistributorCommission(txnID),
                    ChainLegDescriptions.DistributorFunded(txnID),
                    ChainLegDescriptions.DistributorCommissionReversal(txnID),
                    ChainLegDescriptions.DistributorFundedRefund(txnID));
            }
            else if (d < -0.00001m && dFunder > 0 && dFunder != txn.UserID)
            {
                await ReverseSingleLeg(dFunder, txn, Math.Abs(d), posted,
                    ChainLegDescriptions.DistributorSurchargeReceived(txnID),
                    $"Recharge distributor surcharge reversal, txn #{txnID}", credit: false);
            }

            if (s > 0.00001m && txn.SuperDistributerUserID > 0)
            {
                await ReverseFundedCommission(txn, txn.SuperDistributerUserID, s, sFunder, posted,
                    ChainLegDescriptions.SuperDistributorCommission(txnID),
                    ChainLegDescriptions.SuperDistributorFunded(txnID),
                    ChainLegDescriptions.SuperDistributorCommissionReversal(txnID),
                    ChainLegDescriptions.SuperDistributorFundedRefund(txnID));
            }
            else if (s < -0.00001m && sFunder > 0 && sFunder != txn.UserID)
            {
                await ReverseSingleLeg(sFunder, txn, Math.Abs(s), posted,
                    ChainLegDescriptions.SuperDistributorSurchargeReceived(txnID),
                    $"Recharge super-distributor surcharge reversal, txn #{txnID}", credit: false);
            }
        }

        /// <summary>
        /// Reverse one tier's commission: debit the downline back, and credit the admin back for the
        /// funding debit it took. Each side is justified by its OWN original in the ledger, which is
        /// the fix for the way txn #268 lost Rs 7.65. There, the funding debit never posted, yet this
        /// method — symmetric with an initiation that was assumed to have succeeded — credited the
        /// admin back for it anyway. The credit back was real money; the debit it reversed was not.
        ///
        /// The pair still moves together: the admin is only credited back once the downline side is
        /// settled, either because it was debited back or because its credit never landed in the
        /// first place. Reversing one leg and not the other is how a balanced pair becomes an
        /// unbalanced one.
        /// </summary>
        private async Task ReverseFundedCommission(TransactionMaster txn, int downlineID, decimal amount,
            int funderID, HashSet<string>? posted, string creditDesc, string fundedDesc,
            string undoCreditDesc, string undoFundedDesc)
        {
            bool downlineSettled;
            if (WasPosted(posted, creditDesc))
            {
                downlineSettled = await DebitDownline(downlineID, txn, amount, undoCreditDesc) == LegOutcome.Posted;
            }
            else
            {
                // The credit never landed — it failed outright, or PostFundedCommission took it back
                // when its funding leg failed. Nothing to debit back, and no reason to stop here:
                // the funding debit may still have been posted and still needs returning.
                downlineSettled = true;
            }

            if (!downlineSettled) return;
            if (funderID <= 0 || funderID == downlineID) return;

            if (!WasPosted(posted, fundedDesc))
            {
                _logger.Write(new InvalidOperationException(
                    $"Refusing a reversal with no original: '{undoFundedDesc}' (Rs {amount:F2}, funder {funderID}) — '{fundedDesc}' was never posted."));
                return;
            }

            await PostSingleLeg(funderID, txn, amount, undoFundedDesc, credit: true);
        }

        /// <summary>
        /// Reverse a leg that had no pair of its own — a surcharge, or the admin's provider income.
        /// Posted only if the original is in the ledger, for the same reason as above: this is the
        /// direction in which a missing original turns into money that nobody ever paid.
        /// </summary>
        private async Task ReverseSingleLeg(int userID, TransactionMaster txn, decimal amount,
            HashSet<string>? posted, string originalDesc, string reversalDesc, bool credit)
        {
            if (!WasPosted(posted, originalDesc))
            {
                _logger.Write(new InvalidOperationException(
                    $"Refusing a reversal with no original: '{reversalDesc}' (Rs {amount:F2}, user {userID}) — '{originalDesc}' was never posted."));
                return;
            }

            await PostSingleLeg(userID, txn, amount, reversalDesc, credit);
        }

        /// <summary>
        /// Every ledger description posted against this transaction, or NULL when the ledger could
        /// not be read.
        ///
        /// Null means "unknown", and <see cref="WasPosted"/> treats an unknown leg as posted. The
        /// guard is here to stop the reversal of a leg that is positively absent; it must never let a
        /// read failure become a failed transaction whose commissions were left standing, which is
        /// the worse of the two errors and the harder one to spot.
        /// </summary>
        private async Task<HashSet<string>?> PostedLegs(int txnID)
        {
            try
            {
                var rows = await _txnDetailsRepo.GetByReference(txnID);
                return rows.Select(x => x.TransactionDescription ?? string.Empty).ToHashSet(StringComparer.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return null;
            }
        }

        private static bool WasPosted(HashSet<string>? posted, string description)
            => posted == null || posted.Contains(description);

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

        /// <summary>
        /// What happened to one posting. The old helpers returned a bool that conflated "this tier
        /// does not apply" with "the money moved", and the funding leg hung off that bool while its
        /// own result was discarded — so a leg that failed looked exactly like a leg that succeeded.
        /// Three states, because the callers need to tell those two apart.
        /// </summary>
        private enum LegOutcome
        {
            /// <summary>No posting was attempted: no such user, or the target is the admin itself.</summary>
            Skipped,
            /// <summary>The ledger row was written and the wallet moved.</summary>
            Posted,
            /// <summary>A posting was attempted and did not land.</summary>
            Failed,
        }

        // Credit a downline its commission; Skipped (and nothing posted) when the target is missing
        // or is the admin itself (admin does not earn a downline commission on its own txn).
        private async Task<LegOutcome> CreditDownline(int downlineID, TransactionMaster txn, decimal amount, string desc)
        {
            if (downlineID <= 0) return LegOutcome.Skipped;
            var target = await _userRepo.GetUserById(downlineID);
            if (target == null || target.UserTypeID == UserTypeID_Admin) return LegOutcome.Skipped;
            if (await PostCredit(downlineID, txn, amount, desc) > 0) return LegOutcome.Posted;

            _logger.Write(new InvalidOperationException($"Leg failed to post: {desc} (Rs {amount:F2}, user {downlineID})."));
            return LegOutcome.Failed;
        }

        // Reverse of CreditDownline.
        private async Task<LegOutcome> DebitDownline(int downlineID, TransactionMaster txn, decimal amount, string desc)
        {
            if (downlineID <= 0) return LegOutcome.Skipped;
            var target = await _userRepo.GetUserById(downlineID);
            if (target == null || target.UserTypeID == UserTypeID_Admin) return LegOutcome.Skipped;
            if (await PostDebit(downlineID, txn, amount, desc) > 0) return LegOutcome.Posted;

            _logger.Write(new InvalidOperationException($"Leg failed to post: {desc} (Rs {amount:F2}, user {downlineID})."));
            return LegOutcome.Failed;
        }

        // Services whose slab commissions/surcharges settle through the balanced chain double-entry
        // (credit downline + debit the funding admin; surcharge accrues to admin). Recharge earns the
        // funding from the provider commission; DMT funds it from the transfer charge the retailer
        // pays on top of the principal. Both are symmetric and reverse cleanly on refund. Other
        // services keep the legacy single net-admin-commission posting.
        // Delegated to the resolver so this answer has ONE home: LedgerIntegrityService asks the same
        // question when it checks that every downline commission was funded, and a service classed
        // differently by the two would show up as a permanent, false break on that report.
        private async Task<bool> IsChainFunded(int serviceID)
            => await _serviceKinds.IsChainFundedService(serviceID);

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
