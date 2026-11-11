using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Requests;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AccountProcessing;
using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Beneficiary account verification. Penny-drops the account through the active DMT gateway
    // (JaiJui AccountVerify) to fetch the bank's account-holder name, charging a configurable fee
    // from the retailer's DMT wallet. The charge is ledgered via the same AccountProcessing path as a
    // transfer (a small PENDING TransactionMaster that settles SUCCESS on a verified name, or is
    // refunded automatically when the bank can't verify) so wallet accounting stays consistent.
    public class DMTAccountVerifyService : IDMTAccountVerifyService
    {
        private const int ServiceID_DMT = 10;
        private const string StatusPending = "PENDING";
        private const string VerifyChargeSettingKey = "DMTVerifyCharge"; // SettingMaster.Value = fee in ₹
        private const decimal DefaultVerifyCharge = 1m;                  // used when the setting is absent

        private readonly ISenderRepository _senderRepo;
        private readonly IJaiJuiApiService _jaijui;
        private readonly IJaiJuiGatewayResolver _gateway;
        private readonly IAccountProcessingService _account;
        private readonly IAccountCoreProcessingService _accountCore;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IUserWalletMasterRepository _walletRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IProviderRequestResponseLogRepository _logRepo;
        // Which wallet the DMT service debits, per the admin's Service Wallet Mapping. The
        // verification fee is charged from the same wallet as the transfer it precedes.
        private readonly IServiceWalletResolver _serviceWallet;
        private readonly Logger _logger;

        public DMTAccountVerifyService(
            ISenderRepository senderRepo,
            IJaiJuiApiService jaijui,
            IJaiJuiGatewayResolver gateway,
            IAccountProcessingService account,
            IAccountCoreProcessingService accountCore,
            ITransactionMasterRepository txnRepo,
            IUserWalletMasterRepository walletRepo,
            IOperatorRepository operatorRepo,
            ISettingMasterRepository settingRepo,
            IProviderRequestResponseLogRepository logRepo,
            IServiceWalletResolver serviceWallet)
        {
            _serviceWallet = serviceWallet;
            _senderRepo = senderRepo;
            _jaijui = jaijui;
            _gateway = gateway;
            _account = account;
            _accountCore = accountCore;
            _txnRepo = txnRepo;
            _walletRepo = walletRepo;
            _operatorRepo = operatorRepo;
            _settingRepo = settingRepo;
            _logRepo = logRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, VerifyAccountResponse? response)> VerifyAccount(
            VerifyAccountRequest request, int userId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNo)) return (Messages.Transaction.Message.AccountNumberRequired, null);
            if (string.IsNullOrWhiteSpace(request.IFSC)) return (Messages.Transaction.Message.IFSCRequired, null);

            try
            {
                // 1. Sender must exist and be OTP-verified (same guard as adding a beneficiary).
                var sender = (await _senderRepo.GetAll())
                    .FirstOrDefault(s => s.GUID == request.SenderGUID && !s.IsDeleted);
                if (sender == null) return ("Invalid sender.", null);
                if (!sender.IsOTPVerified) return ("Sender OTP not verified. Please verify the sender first.", null);

                // 2. The active DMT gateway must be a JaiJui gateway (it provides AccountVerify).
                var gateway = await _gateway.ResolveActiveGateway();
                if (gateway == null)
                    return ("Account verification is not available — activate a JaiJui gateway in DMT Settings.", null);
                var creds = gateway.Credentials;

                decimal charge = await ResolveVerifyCharge();
                // The wallet the admin mapped the DMT service to, falling back to the DMT wallet
                // this charge always came out of.
                int walletID = await _serviceWallet.Resolve(ServiceID_DMT, (int)WalletCode.DMT);

                // 3. Charge the retailer wallet up-front (ledgered), so the fee is captured even when the
                //    external call is slow/fails; a failed verification is refunded in step 6.
                int txnID = 0;
                decimal openingBalance = 0m;
                if (charge > 0m)
                {
                    string balanceCheck = await _account.BalanceValidationBeforeTransaction(userId, walletID, charge, 0m);
                    if (!balanceCheck.EqualsIgnoreCase(StatusType.Ok)) return (balanceCheck, null);

                    var wallet = (await _walletRepo.GetByUserID(userId)).FirstOrDefault(w => w.WalletID == walletID);
                    if (wallet == null) return (Messages.Wallet.Message.NotFound, null);
                    openingBalance = wallet.Balance;

                    int operatorID = await ResolveDmtOperator(request.PaymentType);
                    if (operatorID <= 0) return ("No transfer operator is configured for DMT.", null);

                    string providerRequestID = $"BVR{userId}{Guid.NewGuid():N}".Substring(0, 30);
                    DateTime now = DateTime.Now;
                    var txn = new TransactionMaster
                    {
                        UserID = userId,
                        ServiceID = ServiceID_DMT,
                        OperatorID = operatorID,
                        WalletID = walletID,
                        ProviderID = 0,
                        ConsumerNumber = request.AccountNo,
                        Amount = charge,
                        DebitAmount = charge,
                        OpeningBalance = openingBalance,
                        ClosingBalance = openingBalance - charge,
                        Status = StatusPending,
                        Description = "Beneficiary account verify",
                        ProviderRequestID = providerRequestID,
                        Through = "WEB",
                        IPAddress = ipAddress,
                        DMT_Bank = request.BankName,
                        DMT_IFSC = request.IFSC,
                        DMT_SenderNumber = sender.Mobile,
                        DMT_PaymentType = request.PaymentType,
                        DMT_OriginalAmount = charge,
                        DateTime = now,
                        UpdatedateTime = now,
                        IsDeleted = false
                    };
                    await _txnRepo.Insert(txn);
                    txnID = txn.TransactionMasterID;
                    if (txnID <= 0) return (Messages.Common.Message.ExceptionOccured, null);

                    int debitResult = await _account.DebitBalance(new AccountPostingRequest
                    {
                        UserID = userId,
                        WalletID = walletID,
                        AmountWithCommission = charge,
                        Amount = charge,
                        ConsumerNumber = request.AccountNo,
                        CommissionCreditDebit = CommissionCreditDebitType.None,
                        DOCType = DOCTypeConstants.Recharge,
                        AssignedUserID = userId,
                        ServiceID = ServiceID_DMT,
                        OperatorID = operatorID,
                        TransactionDescription = "Beneficiary account verify",
                        ReferenceID = txnID,
                        TDS = 0,
                        GST = 0,
                        DiscountAmount = 0
                    });

                    if (debitResult <= 0)
                    {
                        await _txnRepo.UpdateStatus(txnID, StatusType.Failed, string.Empty, Messages.Wallet.Message.InsufficientBalance);
                        return (Messages.Wallet.Message.InsufficientBalance, null);
                    }
                }

                // 4. Call the provider (IsVerify=true) to fetch the account-holder name.
                var verifyReqId = $"AVR{userId}{Guid.NewGuid():N}".Substring(0, 30);
                var verifyRequest = new AccountVerifyRequest
                {
                    Mobile = sender.Mobile,
                    SenderName = sender.Name,
                    BankName = request.BankName,
                    AccountNo = request.AccountNo,
                    IFSC = request.IFSC,
                    PaymentType = string.IsNullOrWhiteSpace(request.PaymentType) ? "IMPS" : request.PaymentType,
                    APIUserRequestID = verifyReqId
                };
                var resp = await _jaijui.AccountVerify(verifyRequest, creds);

                // Record the request/response for the DMT report's "Log" action (keyed to the charge txn).
                if (txnID > 0)
                {
                    try
                    {
                        await _logRepo.Insert(new ProviderRequestResponseLog
                        {
                            ProviderID = gateway.ProviderID,
                            UserID = userId,
                            TransactionID = txnID,
                            ServiceID = ServiceID_DMT,
                            IPAddress = ipAddress,
                            Through = "DMT-AccountVerify",
                            Request = Truncate(System.Text.Json.JsonSerializer.Serialize(new
                            {
                                Endpoint = "JaiJui AccountVerify",
                                verifyRequest.Mobile,
                                verifyRequest.BankName,
                                verifyRequest.AccountNo,
                                verifyRequest.IFSC,
                                verifyRequest.PaymentType,
                                verifyRequest.APIUserRequestID
                            }), 8000),
                            Response = Truncate(string.IsNullOrWhiteSpace(resp.Data)
                                ? System.Text.Json.JsonSerializer.Serialize(new { resp.Success, resp.Status, resp.Message, resp.BeneficiaryName })
                                : resp.Data, 8000),
                            DateTime = DateTime.Now,
                            UpdatedateTime = DateTime.Now
                        });
                    }
                    catch (Exception logEx) { _logger.Write(logEx); }
                }

                bool verified = resp.Success
                    && string.Equals((resp.Status ?? string.Empty).Trim(), Fino.JaiJuiApi.Messages.JaiJui.Status.Success, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(resp.BeneficiaryName);

                // 5/6. Settle the charge on the outcome.
                if (!verified)
                {
                    if (txnID > 0)
                        await _accountCore.RefundTransaction(txnID, "Account verify failed — charge refunded.", DOCTypeConstants.Recharge);
                    return (string.IsNullOrWhiteSpace(resp.Message) ? "Account could not be verified." : resp.Message, null);
                }

                if (txnID > 0)
                    await _accountCore.SuccessTransaction(txnID, verifyReqId, "Beneficiary account verified.");

                decimal closing = (await _walletRepo.GetByUserID(userId))
                    .FirstOrDefault(w => w.WalletID == walletID)?.Balance ?? (openingBalance - charge);

                return (StatusType.Ok, new VerifyAccountResponse
                {
                    Status = StatusType.Success,
                    VerifiedName = resp.BeneficiaryName,
                    BankName = string.IsNullOrWhiteSpace(resp.BankName) ? request.BankName : resp.BankName,
                    Charge = charge,
                    ClosingBalance = closing,
                    Message = "Account verified."
                });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        // Reads the configurable verification fee from SettingMaster; falls back to the default.
        private async Task<decimal> ResolveVerifyCharge()
        {
            var setting = await _settingRepo.GetByKey(VerifyChargeSettingKey);
            if (setting != null && decimal.TryParse(setting.Value, out var fee) && fee >= 0m)
                return fee;
            return DefaultVerifyCharge;
        }

        // A valid OperatorID is required (TransactionMasters.OperatorID is FK-constrained). Pick the DMT
        // money-out operator matching the payment type, else the first active operator for the service.
        private async Task<int> ResolveDmtOperator(string paymentType)
        {
            var serviceOperators = (await _operatorRepo.GetAllOperators())
                .Where(o => o.ServiceID == ServiceID_DMT)
                .ToList();

            var match = serviceOperators.FirstOrDefault(o => o.OperatorName.EqualsIgnoreCase(paymentType))
                        ?? serviceOperators.FirstOrDefault(o => o.IsActive)
                        ?? serviceOperators.FirstOrDefault();
            return match?.OperatorID ?? 0;
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
    }
}
