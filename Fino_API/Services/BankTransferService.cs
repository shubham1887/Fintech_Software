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
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IBankTransferProviderClient _provider;
        private readonly IProviderRequestResponseLogRepository _logRepo;
        private readonly ISlabCommissionResolver _commissionResolver;
        private readonly IUserServiceAccessService _serviceAccess;
        private readonly Logger _logger;

        public BankTransferService(
            IAccountProcessingService account,
            IAccountCoreProcessingService accountCore,
            ITransactionMasterRepository txnRepo,
            IUserWalletMasterRepository walletRepo,
            IOperatorRepository operatorRepo,
            IBankTransferProviderClient provider,
            IProviderRequestResponseLogRepository logRepo,
            ISlabCommissionResolver commissionResolver,
            IUserServiceAccessService serviceAccess)
        {
            _serviceAccess = serviceAccess;
            _account = account;
            _accountCore = accountCore;
            _txnRepo = txnRepo;
            _walletRepo = walletRepo;
            _operatorRepo = operatorRepo;
            _provider = provider;
            _logRepo = logRepo;
            _commissionResolver = commissionResolver;
            _logger = new Logger();
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
                int walletID = kind == TransferKind.DMT ? (int)WalletCode.DMT : (int)WalletCode.AEPS;

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
                decimal adminNet = kind == TransferKind.DMT
                    ? -(commission + comm.DistributorComm + comm.SuperDistributorComm)
                    : 0m;

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
                    NetCommission = commission,
                    AdminCommission = adminNet,
                    DMT_BeneficiaryName = request.BeneficiaryName,
                    DMT_Bank = request.BankName,
                    DMT_IFSC = request.IFSC,
                    DMT_SenderID = request.SenderID,
                    DMT_BeneficiaryID = request.BeneficiaryID,
                    DMT_PaymentType = request.PaymentType,
                    DMT_SenderNumber = request.Mobile,
                    DMT_OriginalAmount = request.Amount,
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

                // 7. Dispatch to the bank-transfer provider (the only external piece)
                var dispatch = await _provider.SendTransfer(new BankTransferContext
                {
                    Kind = kind,
                    TransactionMasterID = txnID,
                    OperatorID = operatorID,
                    ServiceID = serviceID,
                    UserID = userID,
                    Amount = request.Amount,
                    AccountNo = request.AccountNo,
                    IFSC = request.IFSC,
                    BankName = request.BankName,
                    BeneficiaryName = request.BeneficiaryName,
                    PaymentType = request.PaymentType,
                    SenderMobile = request.Mobile,
                    SenderID = request.SenderID,
                    BeneficiaryID = request.BeneficiaryID,
                    ProviderRequestID = providerRequestID,
                    IPAddress = ipAddress
                });

                // 7b. Record the provider request/response so the DMT report's "Log" action has data
                //     (mirrors what ProviderGatewayService does for recharge/BBPS). Never fatal.
                try
                {
                    await _logRepo.Insert(new ProviderRequestResponseLog
                    {
                        ProviderID = dispatch.ProviderID,
                        UserID = userID > 0 ? userID : null,
                        TransactionID = txnID,
                        ServiceID = serviceID,
                        IPAddress = ipAddress,
                        Through = kind.ToString(),
                        Request = Truncate(string.IsNullOrWhiteSpace(dispatch.RawRequest)
                            ? $"{kind} {request.Amount:0.00} → {request.AccountNo} ({request.IFSC}) {request.PaymentType} req={providerRequestID}"
                            : dispatch.RawRequest, 8000),
                        Response = Truncate(string.IsNullOrWhiteSpace(dispatch.RawResponse)
                            ? $"{dispatch.Status}: {dispatch.Message} ref={dispatch.ProviderRefNo}"
                            : dispatch.RawResponse, 8000),
                        DateTime = now,
                        UpdatedateTime = now
                    });
                }
                catch (Exception logEx) { _logger.Write(logEx); }

                // 8. Settle based on provider outcome
                var response = new BankTransferResponse
                {
                    TransactionMasterID = txnID,
                    Kind = kind.ToString(),
                    AccountNo = request.AccountNo,
                    IFSC = request.IFSC,
                    Amount = request.Amount,
                    OpeningBalance = openingBalance,
                    ProviderRefNo = dispatch.ProviderRefNo,
                    DateTime = now.ToString("dd/MM/yyyy hh:mm tt")
                };

                string status = dispatch.Status.ToUpperInvariant();
                if (status == StatusType.Success)
                {
                    await _accountCore.SuccessTransaction(txnID, dispatch.ProviderRefNo, Messages.Fund.Message.TransferSuccess);
                    response.Status = StatusType.Success;
                    response.Message = Messages.Fund.Message.TransferSuccess;
                }
                else if (status == StatusPending)
                {
                    response.Status = StatusPending;
                    response.Message = Messages.Fund.Message.TransferPending;
                }
                else // FAILED — refund the debit + reverse commissions
                {
                    await _accountCore.RefundTransaction(txnID, Messages.Fund.Message.TransferFailedRefunded, DOCTypeConstants.Recharge);
                    response.Status = StatusType.Failed;
                    response.Message = string.IsNullOrWhiteSpace(dispatch.Message) ? Messages.Fund.Message.TransferFailedRefunded : dispatch.Message;
                }

                var afterWallet = (await _walletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == walletID);
                response.ClosingBalance = afterWallet?.Balance ?? openingBalance;

                return (StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        // Cap a request/response payload before it goes into the log column.
        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
    }
}
