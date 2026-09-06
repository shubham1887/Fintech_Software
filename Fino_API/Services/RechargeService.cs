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
    // Recharge transaction pipeline ported from old TransactionBal.Recharge. The provider-agnostic
    // part (validation, wallet debit, transaction record, settlement) is fully implemented here;
    // the actual operator dispatch is delegated to IRechargeProviderClient.
    public class RechargeService : IRechargeService
    {
        private const string StatusPending = "PENDING";

        private readonly IOperatorService _operatorService;
        private readonly IAccountProcessingService _account;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IRechargeProviderClient _provider;
        private readonly ISlabCommissionResolver _commissionResolver;
        private readonly IUserServiceAccessService _serviceAccess;
        private readonly Logger _logger;

        public RechargeService(
            IOperatorService operatorService,
            IAccountProcessingService account,
            IAccountCoreProcessingService accountCore,
            ITransactionMasterRepository txnRepo,
            IUserWalletMasterRepository walletRepo,
            IRechargeProviderClient provider,
            ISlabCommissionResolver commissionResolver,
            IUserServiceAccessService serviceAccess)
        {
            _serviceAccess = serviceAccess;
            _operatorService = operatorService;
            _account = account;
            _accountCore = accountCore;
            _txnRepo = txnRepo;
            _walletRepo = walletRepo;
            _provider = provider;
            _commissionResolver = commissionResolver;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, RechargeResponse? data)> Recharge(RechargeRequest request, int userID, string ipAddress)
        {
            try
            {
                // 1. Validate
                if (request.OperatorID <= 0) return (Messages.Common.Message.InvalidOperator, null);
                if (request.Amount <= 0) return (Messages.Common.Message.InvalidAmount, null);
                if (string.IsNullOrWhiteSpace(request.ConsumerNumber)) return (Messages.Transaction.Message.ConsumerNumberRequired, null);
                if (request.ConsumerNumber.Contains(' ')) return (Messages.Transaction.Message.InvalidConsumerNumber, null);

                // 2. Resolve operator → service
                var op = await _operatorService.GetOperatorById(request.OperatorID);
                if (op == null) return (Messages.Common.Message.InvalidOperator, null);
                if (!op.IsActive) return (Messages.Common.Message.InvalidOperator, null);
                int serviceID = op.ServiceID > 0 ? op.ServiceID : request.ServiceID;
                if (serviceID <= 0) return (Messages.Common.Message.InvalidOperator, null);

                // The service must actually be granted to this user. A service set to "Off" on their
                // user record is hidden from the catalog, and "Lock" leaves the tile visible - either
                // way the transaction is refused here, so neither can be reached by calling the API
                // directly or from a stale page.
                string access = await _serviceAccess.EnsureUsable(userID, serviceID);
                if (!access.EqualsIgnoreCase(StatusType.Ok)) return (access, null);

                int walletID = (int)WalletCode.Recharge;

                // 3. Resolve the slab commission/surcharge for this user + operator + amount.
                //    RetailerComm is SIGNED: positive = commission credited to the retailer,
                //    negative = surcharge debited on top of the amount.
                var comm = await _commissionResolver.Resolve(userID, serviceID, request.OperatorID, request.Amount);
                decimal commission = comm.RetailerComm;

                // Surcharge can be configured on ANY tier (retailer / distributor / super-distributor):
                // a negative slab value means "charge", not payout. The WHOLE surcharge is collected
                // from the retailer's wallet on top of the principal and accrues to the admin — no
                // downline earns it. Positive commissions are funded by the admin and credited
                // separately (see AccountCoreProcessingService.PostInitiationCommissions).
                decimal surchargeTotal =
                    (commission < 0 ? -commission : 0m) +
                    (comm.DistributorComm < 0 ? -comm.DistributorComm : 0m) +
                    (comm.SuperDistributorComm < 0 ? -comm.SuperDistributorComm : 0m);
                decimal retailerCredit = commission > 0 ? commission : 0m;

                // 4. Balance check: the retailer must afford principal + all surcharges; a positive
                //    retailer commission offsets it (legacy affordability semantics).
                string balanceCheck = await _account.BalanceValidationBeforeTransaction(userID, walletID, request.Amount, retailerCredit - surchargeTotal);
                if (!balanceCheck.EqualsIgnoreCase(StatusType.Ok)) return (balanceCheck, null);

                var wallet = (await _walletRepo.GetByUserID(userID)).FirstOrDefault(w => w.WalletID == walletID);
                if (wallet == null) return ("Recharge wallet not found.", null);
                decimal openingBalance = wallet.Balance;

                // The retailer pays the principal plus every tier's surcharge; positive commissions
                // are credited back separately, not netted into the debit.
                decimal debitAmount = request.Amount + surchargeTotal;

                // 5. Create the PENDING transaction record
                string providerRequestID = $"RCH{userID}{request.OperatorID}{Guid.NewGuid():N}".Substring(0, 30);
                DateTime now = DateTime.Now;
                var txn = new TransactionMaster
                {
                    UserID = userID,
                    ServiceID = serviceID,
                    OperatorID = request.OperatorID,
                    WalletID = walletID,
                    ProviderID = 0,
                    ConsumerNumber = request.ConsumerNumber,
                    Amount = request.Amount,
                    DebitAmount = debitAmount,
                    OpeningBalance = openingBalance,
                    ClosingBalance = openingBalance - debitAmount,
                    Status = StatusPending,
                    Description = "Recharge initiated",
                    ProviderRequestID = providerRequestID,
                    APIUserRequestID = request.APIUserRequestID ?? string.Empty,
                    Through = string.IsNullOrWhiteSpace(request.Through) ? "WEB" : request.Through,
                    IPAddress = ipAddress,
                    RetailerCommission = commission,
                    DistributerCommission = comm.DistributorComm,
                    SuperDistributerCommission = comm.SuperDistributorComm,
                    DistributerUserID = comm.DistributorUserID,
                    SuperDistributerUserID = comm.SuperDistributorUserID,
                    NetCommission = commission,
                    Location = string.IsNullOrWhiteSpace(request.Latitude) ? string.Empty : $"{request.Latitude},{request.Longitude}",
                    DateTime = now,
                    UpdatedateTime = now,
                    IsDeleted = false
                };
                await _txnRepo.Insert(txn);
                int txnID = txn.TransactionMasterID;
                if (txnID <= 0) return (Messages.Common.Message.ExceptionOccured, null);

                string debitDescription = $"Recharge, to: {request.ConsumerNumber} Amount: {request.Amount:0.00} Commission: {commission:0.00}";

                // 6. Debit the wallet (ledger row written by AccountProcessing)
                int debitResult = await _account.DebitBalance(new AccountPostingRequest
                {
                    UserID = userID,
                    WalletID = walletID,
                    AmountWithCommission = debitAmount,
                    Amount = request.Amount,
                    ConsumerNumber = request.ConsumerNumber,
                    CommissionCreditDebit = CommissionCreditDebitType.None,
                    DOCType = DOCTypeConstants.Recharge,
                    AssignedUserID = userID,
                    ServiceID = serviceID,
                    OperatorID = request.OperatorID,
                    TransactionDescription = debitDescription,
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

                // 7. Post the slab commissions up-front (reversed automatically on refund) into the
                //    SAME wallet the recharge was debited from. Double entry: each downline commission
                //    is credited to the downline and DEBITED from the admin who funds it; a retailer
                //    surcharge accrues to the admin. (See AccountCoreProcessingService.)
                await _accountCore.PostInitiationCommissions(txnID);

                // 8. Dispatch to the operator/aggregator (the only external piece)
                var dispatch = await _provider.SendRecharge(new RechargeDispatchContext
                {
                    TransactionMasterID = txnID,
                    OperatorID = request.OperatorID,
                    ServiceID = serviceID,
                    ConsumerNumber = request.ConsumerNumber,
                    Amount = request.Amount,
                    UserID = userID,
                    ProviderRequestID = providerRequestID,
                    IPAddress = ipAddress,
                    Param1 = request.Param1 ?? string.Empty,
                    Param2 = request.Param2 ?? string.Empty,
                    Param3 = request.Param3 ?? string.Empty
                });

                // 9. Persist what the provider actually replied (provider id, operator reference,
                //    raw response, provider balances) so reports show the real upstream outcome.
                await _txnRepo.UpdateProviderReply(txnID, dispatch.ProviderID, dispatch.ProviderRefNo,
                    dispatch.RawResponse, dispatch.ProviderOpeningBalance, dispatch.ProviderClosingBalance);

                // 10. Commission outcome: what the provider paid us minus what was paid down the
                //     chain is the admin's earning. Skipped for failures (commissions reverse).
                //     Credited to the admin wallet HERE — the moment the outcome is known and the
                //     transaction settles to PROCESS/SUCCESS — so every role (retailer + uplines at
                //     step 7, admin here) is posted at initiation, not deferred to success. A later
                //     failure reverses it via RefundTransaction.
                if (!dispatch.Status.ToUpperInvariant().StartsWith("FAIL"))
                {
                    decimal providerCommission = dispatch.ProviderCommission ?? 0m;
                    decimal adminCommission = providerCommission - (commission + comm.DistributorComm + comm.SuperDistributorComm);
                    await _txnRepo.UpdateCommissionOutcome(txnID, providerCommission, adminCommission);
                    // Credit the admin its provider-commission income (recharge). Netted against the
                    // step-7 admin funding debits this leaves the admin its profit P-(R+D+S).
                    await _accountCore.CreditAdminOutcome(txnID);
                }

                // 11. Settle based on provider outcome
                var response = new RechargeResponse
                {
                    TransactionMasterID = txnID,
                    OperatorID = request.OperatorID,
                    ConsumerNumber = request.ConsumerNumber,
                    Amount = request.Amount,
                    OpeningBalance = openingBalance,
                    ProviderRefNo = dispatch.ProviderRefNo,
                    DateTime = now.ToString("dd/MM/yyyy hh:mm tt")
                };

                string providerMessage = (dispatch.Message ?? string.Empty).Trim();
                string status = dispatch.Status.ToUpperInvariant();
                if (status == StatusType.Success)
                {
                    string remark = providerMessage.Length > 0 ? providerMessage : Messages.Transaction.Message.RechargeSuccess;
                    await _accountCore.SuccessTransaction(txnID, dispatch.ProviderRefNo, remark);
                    response.Status = StatusType.Success;
                    response.Message = remark;
                }
                else if (status == StatusPending)
                {
                    // Leave debited and PENDING; settled later via status-check or manual update.
                    if (providerMessage.Length > 0)
                        await _txnRepo.UpdateStatus(txnID, StatusPending, dispatch.ProviderRefNo, providerMessage);
                    response.Status = StatusPending;
                    response.Message = providerMessage.Length > 0 ? providerMessage : Messages.Transaction.Message.RechargePending;
                }
                else // FAILED — refund the debit + reverse commissions
                {
                    string remark = providerMessage.Length > 0 ? providerMessage : Messages.Transaction.Message.RechargeFailedRefunded;
                    await _accountCore.RefundTransaction(txnID, remark, DOCTypeConstants.Recharge);
                    response.Status = StatusType.Failed;
                    response.Message = remark;
                }

                // Reflect the latest balance
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
    }
}
