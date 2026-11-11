using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AccountProcessing;
using FintechApp.Requests.Transaction;
using FintechApp.Responses.Transaction;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Ported from old DMTProcessing/PayoutProcessing/VendorPayProcessing money-out flows. The
    // provider-agnostic part (validation, wallet debit, transaction record, settlement) is fully
    // implemented; the upstream bank-transfer dispatch is delegated to IBankTransferProviderClient.
    public class BankTransferService : IBankTransferService
    {
        private const string StatusPending = "PENDING";
        private const int ServiceID_DMT = 10;

        private readonly IAccountProcessingService _account;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly ITransactionMasterRepository _txnRepo;
        // What the upstream charges us per transfer, in amount bands (Provider Setting -> Commission).
        private readonly IProviderConfigurationAdminCommissionService _commissionSlabs;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IBankTransferProviderClient _provider;
        private readonly IProviderRequestResponseLogRepository _logRepo;
        private readonly ISlabCommissionResolver _commissionResolver;
        private readonly IUserServiceAccessService _serviceAccess;
        private readonly ISettingMasterRepository _settingRepo;
        // The remitter behind a transfer. The transaction row carries DMT_SenderID (the sender's
        // GUID) and the mobile, but never carried the NAME — so the DMT report's "Sender Name"
        // column was blank on every row it had ever written.
        private readonly ISenderRepository _senderRepo;
        // Which wallet this service debits, per the admin's Service Wallet Mapping.
        private readonly IServiceWalletResolver _serviceWallet;
        private readonly Logger _logger;

        public BankTransferService(
            IAccountProcessingService account,
            IAccountCoreProcessingService accountCore,
            ITransactionMasterRepository txnRepo,
            IProviderConfigurationAdminCommissionService commissionSlabs,
            IUserWalletMasterRepository walletRepo,
            IOperatorRepository operatorRepo,
            IBankTransferProviderClient provider,
            IProviderRequestResponseLogRepository logRepo,
            ISlabCommissionResolver commissionResolver,
            IUserServiceAccessService serviceAccess,
            ISettingMasterRepository settingRepo,
            ISenderRepository senderRepo,
            IServiceWalletResolver serviceWallet)
        {
            _serviceWallet = serviceWallet;
            _senderRepo = senderRepo;
            _settingRepo = settingRepo;
            _serviceAccess = serviceAccess;
            _account = account;
            _accountCore = accountCore;
            _txnRepo = txnRepo;
            _commissionSlabs = commissionSlabs;
            _walletRepo = walletRepo;
            _operatorRepo = operatorRepo;
            _provider = provider;
            _logRepo = logRepo;
            _commissionResolver = commissionResolver;
            _logger = new Logger();
        }

        /// <summary>
        /// The remitter's name, looked up from the Sender record the transfer was raised against —
        /// by the stored GUID first, then by mobile. Stored ON the transaction so the DMT report
        /// (and the receipt, and the pending queue) can show who sent the money without a join to
        /// a record that may later be edited or removed. A payout has no remitter, so it resolves
        /// to blank and the column simply stays empty for it.
        /// </summary>
        private async Task<string> ResolveSenderName(string senderGuid, string mobile)
        {
            if (string.IsNullOrWhiteSpace(senderGuid) && string.IsNullOrWhiteSpace(mobile))
                return string.Empty;
            try
            {
                var senders = await _senderRepo.GetAll();
                var sender = !string.IsNullOrWhiteSpace(senderGuid)
                    ? senders.FirstOrDefault(s => s.GUID == senderGuid)
                    : null;
                sender ??= !string.IsNullOrWhiteSpace(mobile)
                    ? senders.FirstOrDefault(s => s.Mobile == mobile)
                    : null;
                return sender?.Name ?? string.Empty;
            }
            catch (Exception ex)
            {
                // A missing name must never stop a transfer — log it and carry on.
                _logger.Write(ex);
                return string.Empty;
            }
        }

        public async Task<(string failureMessage, BankTransferResponse? data)> Transfer(BankTransferRequest request, TransferKind kind, int userID, string ipAddress)
        {
            try
            {
                // 1. Validate
                if (request.Amount <= 0) return (Messages.Common.Message.InvalidAmount, null);
                if (string.IsNullOrWhiteSpace(request.AccountNo)) return (Messages.Transaction.Message.AccountNumberRequired, null);
                if (string.IsNullOrWhiteSpace(request.IFSC)) return (Messages.Transaction.Message.IFSCRequired, null);

                int serviceID = request.ServiceID > 0 ? request.ServiceID : ServiceID_DMT;
                // The wallet the admin mapped this service to ("Service Wallet Mapping"), falling
                // back to the wallet this pipeline always used — DMT for a transfer, AEPS for a
                // payout — so an install that maps nothing behaves exactly as before.
                //
                // A payout is asked for BY KIND. `serviceID` above falls back to the DMT service
                // when the caller names none (the panel's payout form does not), and reading the
                // DMT service's mapping for a payout would route payouts into the transfer wallet.
                // The Payout service row carries its own mapping, and that is the one to read.
                int fallbackWalletID = kind == TransferKind.DMT ? (int)WalletCode.DMT : (int)WalletCode.AEPS;
                int walletID = kind == TransferKind.DMT || request.ServiceID > 0
                    ? await _serviceWallet.Resolve(serviceID, fallbackWalletID)
                    : await _serviceWallet.ResolveForKind(ServiceKinds.Payout, fallbackWalletID);

                // Refuse a service the user's record has set to "Lock" or "Off" (see RechargeService).
                string access = await _serviceAccess.EnsureUsable(userID, serviceID);
                if (!access.EqualsIgnoreCase(StatusType.Ok)) return (access, null);

                // Resolve the operator. TransactionMasters.OperatorID is FK-constrained to OperatorMasters,
                // so a 0/invalid id aborts the whole transfer. When the caller doesn't pin an operator, pick
                // the money-out operator for this service by payment type (IMPS/NEFT/RTGS), else the first
                // active one for the service.
                int operatorID = request.OperatorID;
                var serviceOperators = (await _operatorRepo.GetAllOperators()).Where(o => o.ServiceID == serviceID).ToList();
                if (operatorID <= 0 || serviceOperators.All(o => o.OperatorID != operatorID))
                {
                    var match = serviceOperators.FirstOrDefault(o => o.OperatorName.EqualsIgnoreCase(request.PaymentType))
                                ?? serviceOperators.FirstOrDefault(o => o.IsActive)
                                ?? serviceOperators.FirstOrDefault();
                    if (match == null) return ("No transfer operator is configured for this service.", null);
                    operatorID = match.OperatorID;
                }

                // 2. Resolve the slab commission/surcharge for this user + operator + amount. Each tier
                //    value is SIGNED: positive = commission credited to that tier, negative = a transfer
                //    charge/surcharge the retailer pays on top of the principal (accrues to the admin).
                //    This mirrors the recharge pipeline; the difference is that DMT has no upstream
                //    provider commission — the charge the retailer pays funds the whole chain.
                var comm = await _commissionResolver.Resolve(userID, serviceID, operatorID, request.Amount);
                // See RechargeService: an out-of-limit delegated band is refused, not paid at the
                // upline's (higher) rate and not silently paid as zero.
                if (comm.IsBlocked) return (comm.BlockedReason, null);
                decimal commission = comm.RetailerComm; // signed retailer tier

                // Total surcharge collected from the retailer = the magnitude of every negative tier;
                // positive retailer commission is credited back separately (not netted into the debit).
                decimal surchargeTotal =
                    (commission < 0 ? -commission : 0m) +
                    (comm.DistributorComm < 0 ? -comm.DistributorComm : 0m) +
                    (comm.SuperDistributorComm < 0 ? -comm.SuperDistributorComm : 0m);
                decimal retailerCredit = commission > 0 ? commission : 0m;

                // DMT has no upstream provider commission, so the admin's net for the txn is simply
                // the mirror of what the chain earns: every surcharge collected minus every
                // commission the admin funds (the recharge formula P - (R+D+SD) with P = 0).
                // Recorded only so reports show the real admin figure - the money itself moves in
                // PostInitiationCommissions, which is what the refund path reverses. Left at 0 for
                // AEPS, whose service is not admin-funded and whose refund path would otherwise
                // reverse an admin credit that was never posted.
                decimal adminNet = kind == TransferKind.DMT ? -comm.AdminFundedCommission : 0m;

                // 3. Balance check: the retailer must afford principal + all surcharges; a positive
                //    retailer commission offsets it (same affordability semantics as recharge).
                string balanceCheck = await _account.BalanceValidationBeforeTransaction(userID, walletID, request.Amount, retailerCredit - surchargeTotal);
                if (!balanceCheck.EqualsIgnoreCase(StatusType.Ok)) return (balanceCheck, null);

                var wallet = (await _walletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == walletID);
                if (wallet == null) return (Messages.Wallet.Message.NotFound, null);
                decimal openingBalance = wallet.Balance;

                // The retailer pays the principal plus every tier's surcharge; positive commissions are
                // credited back separately (PostInitiationCommissions), not folded into the debit.
                decimal debitAmount = request.Amount + surchargeTotal;

                // 4. Create the PENDING transaction record
                string providerRequestID = $"{kind.ToString().ToUpperInvariant()[..3]}{userID}{Guid.NewGuid():N}".Substring(0, 30);
                DateTime now = DateTime.Now;
                string senderName = await ResolveSenderName(request.SenderID, request.Mobile);
                var txn = new TransactionMaster
                {
                    UserID = userID,
                    ServiceID = serviceID,
                    OperatorID = operatorID,
                    WalletID = walletID,
                    ProviderID = 0,
                    ConsumerNumber = request.AccountNo,
                    Amount = request.Amount,
                    DebitAmount = debitAmount,
                    OpeningBalance = openingBalance,
                    ClosingBalance = openingBalance - debitAmount,
                    Status = StatusPending,
                    Description = $"{kind} initiated",
                    ProviderRequestID = providerRequestID,
                    Through = string.IsNullOrWhiteSpace(request.Through) ? "WEB" : request.Through,
                    IPAddress = ipAddress,
                    RetailerCommission = commission,
                    DistributerCommission = comm.DistributorComm,
                    SuperDistributerCommission = comm.SuperDistributorComm,
                    DistributerUserID = comm.DistributorUserID,
                    SuperDistributerUserID = comm.SuperDistributorUserID,
                    RetailerCommissionFunderID = comm.RetailerFunderID,
                    DistributerCommissionFunderID = comm.DistributorFunderID,
                    SuperDistributerCommissionFunderID = comm.SuperDistributorFunderID,
                    NetCommission = commission,
                    AdminCommission = adminNet,
                    DMT_BeneficiaryName = request.BeneficiaryName,
                    DMT_Bank = request.BankName,
                    DMT_IFSC = request.IFSC,
                    DMT_SenderID = request.SenderID,
                    DMT_BeneficiaryID = request.BeneficiaryID,
                    DMT_PaymentType = request.PaymentType,
                    DMT_SenderNumber = request.Mobile,
                    DMT_SenderName = senderName,
                    DMT_OriginalAmount = request.Amount,
                    // The B2B caller's own reference, and the ONLY thing that makes a transfer
                    // idempotent. The recharge pipeline has always stored it; this one did not,
                    // so every /api/v1/dmt transfer was invisible to the duplicate check — a
                    // retried requestid created a second transfer and debited the wallet twice,
                    // and /dmt/status by requestid could never find the transfer it had just made.
                    APIUserRequestID = request.APIUserRequestID ?? string.Empty,
                    Location = string.IsNullOrWhiteSpace(request.Latitude) ? string.Empty : $"{request.Latitude},{request.Longitude}",
                    DateTime = now,
                    UpdatedateTime = now,
                    IsDeleted = false
                };
                await _txnRepo.Insert(txn);
                int txnID = txn.TransactionMasterID;
                if (txnID <= 0) return (Messages.Common.Message.ExceptionOccured, null);

                // 5. Debit the wallet (ledger row written by AccountProcessing)
                int debitResult = await _account.DebitBalance(new AccountPostingRequest
                {
                    UserID = userID,
                    WalletID = walletID,
                    AmountWithCommission = txn.DebitAmount,
                    Amount = request.Amount,
                    ConsumerNumber = request.AccountNo,
                    CommissionCreditDebit = CommissionCreditDebitType.None,
                    DOCType = DOCTypeConstants.Recharge,
                    AssignedUserID = userID,
                    ServiceID = serviceID,
                    OperatorID = operatorID,
                    TransactionDescription = $"{kind} debit",
                    ReferenceID = txnID,
                    TDS = 0,
                    GST = 0,
                    DiscountAmount = commission
                });

                if (debitResult <= 0)
                {
                    await _txnRepo.UpdateStatus(txnID, StatusType.Failed, string.Empty, Messages.Wallet.Message.InsufficientBalance);
                    return (Messages.Wallet.Message.InsufficientBalance, null);
                }

                // 6. Post the slab commissions up-front (reversed automatically on refund): credit each
                //     downline its positive commission (funded by the admin) and credit the admin any
                //     surcharge the retailer paid. Balanced double-entry — see AccountCoreProcessingService.
                await _accountCore.PostInitiationCommissions(txnID);

                // 6b. Global "Hold DMT" switch. While it is on, the transfer stops here: validated,
                //     debited and commissioned exactly as usual, but never handed to the upstream
                //     gateway. It stays PENDING with ProviderID = 0 and the hold marker in its
                //     description, which is what the held-DMT query keys off. Turning the hold off
                //     releases every parked transfer through the normal DMT sequence (resend →
                //     provider switching → settlement), so from the provider's point of view the
                //     transfer simply arrives later; nothing is skipped and nothing is charged twice.
                //     Money-out kinds other than DMT (AEPS payout, vendor pay) are not affected.
                if (kind == TransferKind.DMT && await IsDmtOnHold())
                {
                    await _txnRepo.UpdateStatus(txnID, StatusPending, string.Empty, TransactionHold.HeldDescription);
                    var afterHoldWallet = (await _walletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == walletID);
                    return (StatusType.Ok, new BankTransferResponse
                    {
                        TransactionMasterID = txnID,
                        Kind = kind.ToString(),
                        AccountNo = request.AccountNo,
                        IFSC = request.IFSC,
                        Amount = request.Amount,
                        OpeningBalance = openingBalance,
                        ClosingBalance = afterHoldWallet?.Balance ?? openingBalance,
                        Status = StatusPending,
                        Message = Messages.Fund.Message.TransferOnHold,
                        DateTime = now.ToString("dd/MM/yyyy hh:mm tt")
                    });
                }

                // 7 + 8. Dispatch to the bank-transfer provider and settle on its answer.
                return (StatusType.Ok, await DispatchAndSettle(new DispatchContext
                {
                    Kind = kind,
                    TransactionID = txnID,
                    UserID = userID,
                    ServiceID = serviceID,
                    OperatorID = operatorID,
                    WalletID = walletID,
                    Amount = request.Amount,
                    AccountNo = request.AccountNo,
                    IFSC = request.IFSC,
                    BankName = request.BankName,
                    BeneficiaryName = request.BeneficiaryName,
                    PaymentType = request.PaymentType,
                    Remark = request.Remark,
                    SenderMobile = request.Mobile,
                    SenderID = request.SenderID,
                    BeneficiaryID = request.BeneficiaryID,
                    ProviderRequestID = providerRequestID,
                    OpeningBalance = openingBalance,
                    IPAddress = ipAddress,
                }));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        /// <summary>
        /// Send a transfer parked by the global hold, exactly as it would have been sent at the time.
        ///
        /// This is the release path, and it deliberately goes through the same bank-transfer client
        /// as a live transfer rather than through the recharge gateway's generic "resend": a DMT
        /// gateway is configured in DMT Settings, not as a recharge route, so a generic re-dispatch
        /// would have nothing to send it through. The transfer keeps its original ProviderRequestID,
        /// so an upstream that already saw it rejects the duplicate instead of paying twice.
        /// </summary>
        public async Task<(string failureMessage, BankTransferResponse? data)> ReleaseHeldTransfer(int transactionMasterID, string ipAddress)
        {
            try
            {
                var txn = await _txnRepo.GetByID(transactionMasterID);
                if (txn == null) return (Messages.Transaction.Message.NotFound, null);
                if (!TransactionHold.IsHeld(txn)) return ("This transfer is not on hold.", null);

                var kind = txn.WalletID == (int)WalletCode.DMT ? TransferKind.DMT : TransferKind.Payout;

                return (StatusType.Ok, await DispatchAndSettle(new DispatchContext
                {
                    Kind = kind,
                    TransactionID = txn.TransactionMasterID,
                    UserID = txn.UserID,
                    ServiceID = txn.ServiceID,
                    OperatorID = txn.OperatorID,
                    WalletID = txn.WalletID,
                    Amount = txn.Amount,
                    AccountNo = txn.ConsumerNumber,
                    IFSC = txn.DMT_IFSC,
                    BankName = txn.DMT_Bank,
                    BeneficiaryName = txn.DMT_BeneficiaryName,
                    PaymentType = txn.DMT_PaymentType,
                    SenderMobile = txn.DMT_SenderNumber,
                    SenderID = txn.DMT_SenderID,
                    BeneficiaryID = txn.DMT_BeneficiaryID,
                    ProviderRequestID = txn.ProviderRequestID,
                    OpeningBalance = txn.OpeningBalance,
                    IPAddress = ipAddress,
                }));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        /// <summary>
        /// Everything a dispatch needs, whether it happens inline during <see cref="Transfer"/> or
        /// later when a held transfer is released. Money has already moved by this point — the wallet
        /// is debited and the commissions posted — so this stage only sends and settles.
        /// </summary>
        private sealed class DispatchContext
        {
            public TransferKind Kind { get; init; }
            public int TransactionID { get; init; }
            public int UserID { get; init; }
            public int ServiceID { get; init; }
            public int OperatorID { get; init; }
            public int WalletID { get; init; }
            public decimal Amount { get; init; }
            public string AccountNo { get; init; } = string.Empty;
            public string IFSC { get; init; } = string.Empty;
            public string BankName { get; init; } = string.Empty;
            public string BeneficiaryName { get; init; } = string.Empty;
            public string PaymentType { get; init; } = string.Empty;
            public string Remark { get; init; } = string.Empty;
            public string SenderMobile { get; init; } = string.Empty;
            public string SenderID { get; init; } = string.Empty;
            public string BeneficiaryID { get; init; } = string.Empty;
            public string ProviderRequestID { get; init; } = string.Empty;
            public decimal OpeningBalance { get; init; }
            public string IPAddress { get; init; } = string.Empty;
        }

        // Send the transfer upstream, log the exchange, and settle the local transaction on the
        // reply: SUCCESS finalises, FAILED refunds the debit and reverses every commission,
        // PENDING is left for the status-check sweep. Shared by the live and release paths so a
        // released transfer settles by exactly the same rules as one sent the moment it was made.
        private async Task<BankTransferResponse> DispatchAndSettle(DispatchContext c)
        {
            DateTime now = DateTime.Now;

            var dispatch = await _provider.SendTransfer(new BankTransferContext
            {
                Kind = c.Kind,
                TransactionMasterID = c.TransactionID,
                OperatorID = c.OperatorID,
                ServiceID = c.ServiceID,
                UserID = c.UserID,
                Amount = c.Amount,
                AccountNo = c.AccountNo,
                IFSC = c.IFSC,
                BankName = c.BankName,
                BeneficiaryName = c.BeneficiaryName,
                PaymentType = c.PaymentType,
                Remark = c.Remark,
                SenderMobile = c.SenderMobile,
                SenderID = c.SenderID,
                BeneficiaryID = c.BeneficiaryID,
                ProviderRequestID = c.ProviderRequestID,
                IPAddress = c.IPAddress
            });

            // Record the provider request/response so the DMT report's "Log" action has data
            // (mirrors what ProviderGatewayService does for recharge/BBPS). Never fatal.
            try
            {
                await _logRepo.Insert(new ProviderRequestResponseLog
                {
                    ProviderID = dispatch.ProviderID,
                    UserID = c.UserID > 0 ? c.UserID : null,
                    TransactionID = c.TransactionID,
                    ServiceID = c.ServiceID,
                    IPAddress = c.IPAddress,
                    Through = c.Kind.ToString(),
                    Request = Truncate(string.IsNullOrWhiteSpace(dispatch.RawRequest)
                        ? $"{c.Kind} {c.Amount:0.00} → {c.AccountNo} ({c.IFSC}) {c.PaymentType} req={c.ProviderRequestID}"
                        : dispatch.RawRequest, 8000),
                    Response = Truncate(string.IsNullOrWhiteSpace(dispatch.RawResponse)
                        ? $"{dispatch.Status}: {dispatch.Message} ref={dispatch.ProviderRefNo}"
                        : dispatch.RawResponse, 8000),
                    DateTime = now,
                    UpdatedateTime = now
                });
            }
            catch (Exception logEx) { _logger.Write(logEx); }

            // Stamp the provider that actually took the transfer onto the transaction. Without this
            // the row stayed at ProviderID 0 for its whole life, and everything that keys off it
            // silently did nothing: the DMT status-check sweep skips ProviderID 0 (that is how a
            // HELD transfer is kept out of it), the API user's /dmt/status probe skips it too, and
            // the reports showed a blank provider. A dispatch that never reached a gateway
            // (unconfigured / sandbox) reports ProviderID 0 and is left alone, so a held or
            // undispatched transfer still reads as undispatched.
            if (dispatch.ProviderID > 0)
            {
                try
                {
                    await _txnRepo.UpdateProviderReply(c.TransactionID, dispatch.ProviderID,
                        dispatch.ProviderRefNo, Truncate(dispatch.RawResponse, 4000), null, null);
                }
                catch (Exception stampEx) { _logger.Write(stampEx); }
            }

            // What this transfer actually cost us upstream.
            //
            // DMT and payout recorded nothing here: the admin's net was written as the mirror of
            // what the chain earned, on the assumption that a transfer carries no provider
            // commission. It does - JaiJui charges per amount band (500-1000 = 5.90) and returns
            // the figure on every reply. The provider's own figure wins when it sends one;
            // otherwise the band configured on Provider Setting -> Commission is used.
            //
            // No money moves here. A DMT transfer's admin income is never posted from
            // AdminCommission (only the recharge path credits it), so both figures are what the
            // reports read - which is exactly where this was missing.
            try
            {
                decimal providerCommission = dispatch.ProviderCommission
                    ?? await _commissionSlabs.Resolve(dispatch.ProviderID, c.ServiceID, c.OperatorID, c.Amount);

                if (providerCommission != 0m)
                {
                    // Admin NET = P - (R + D + S), the same formula the recharge family uses. What is
                    // already on the row is that expression with P assumed to be zero, so adding P
                    // completes it - and re-reading the row keeps the released-from-hold path right.
                    var row = await _txnRepo.GetByID(c.TransactionID);
                    decimal adminNet = (row?.AdminCommission ?? 0m) + providerCommission;
                    await _txnRepo.UpdateCommissionOutcome(c.TransactionID, providerCommission, adminNet);
                }
            }
            catch (Exception commEx) { _logger.Write(commEx); }

            var response = new BankTransferResponse
            {
                TransactionMasterID = c.TransactionID,
                Kind = c.Kind.ToString(),
                AccountNo = c.AccountNo,
                IFSC = c.IFSC,
                Amount = c.Amount,
                OpeningBalance = c.OpeningBalance,
                ProviderRefNo = dispatch.ProviderRefNo,
                DateTime = now.ToString("dd/MM/yyyy hh:mm tt")
            };

            string status = dispatch.Status.ToUpperInvariant();
            if (status == StatusType.Success)
            {
                await _accountCore.SuccessTransaction(c.TransactionID, dispatch.ProviderRefNo, Messages.Fund.Message.TransferSuccess);
                response.Status = StatusType.Success;
                response.Message = Messages.Fund.Message.TransferSuccess;
            }
            else if (status == StatusPending)
            {
                // Clear any hold marker: it HAS been sent now, so from here on it is an ordinary
                // pending transfer for the status-check sweep to settle, not a parked one.
                await _txnRepo.UpdateStatus(c.TransactionID, StatusPending, dispatch.ProviderRefNo,
                    string.IsNullOrWhiteSpace(dispatch.Message) ? $"{c.Kind} sent — awaiting bank confirmation" : dispatch.Message);
                response.Status = StatusPending;
                response.Message = Messages.Fund.Message.TransferPending;
            }
            else // FAILED — refund the debit + reverse commissions
            {
                await _accountCore.RefundTransaction(c.TransactionID, Messages.Fund.Message.TransferFailedRefunded, DOCTypeConstants.Recharge);
                response.Status = StatusType.Failed;
                response.Message = string.IsNullOrWhiteSpace(dispatch.Message) ? Messages.Fund.Message.TransferFailedRefunded : dispatch.Message;
            }

            var afterWallet = (await _walletRepo.GetByUserID(c.UserID)).FirstOrDefault(w => w.WalletID == c.WalletID);
            response.ClosingBalance = afterWallet?.Balance ?? c.OpeningBalance;

            return response;
        }

        // The global "Hold DMT" switch (SettingMaster row, toggled from DMT Settings). Absent row =
        // not held, so an install that has never touched the switch behaves exactly as before.
        // A read failure also means "not held": a settings hiccup must not silently park live money.
        private async Task<bool> IsDmtOnHold()
        {
            try
            {
                var setting = await _settingRepo.GetByKey(TransactionHold.DMTHoldSettingKey);
                return setting != null && setting.IsOn;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return false;
            }
        }

        // Cap a request/response payload before it goes into the log column.
        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
    }
}
