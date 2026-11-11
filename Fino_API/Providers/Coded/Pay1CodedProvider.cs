using System.Globalization;
using FintechApp.Entities;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;

namespace FintechApp.Providers.Coded
{
    /// <summary>
    /// Pay1 Utility (BBPS), as a compiled integration.
    ///
    /// Pay1 signs every call with sha1(partner_id + trans_id + api_key). A ProviderDetails URL
    /// template cannot produce that hash, so this provider talks through the Fino.Pay1Api library
    /// instead of an endpoint row. Everything around it is unchanged: the route and the switching
    /// sequence still decide that Pay1 serves a biller (ProviderConfiguration.OpCode carries the
    /// Pay1 product id), every call is still written to the provider log, and the money rules
    /// still hold — a reply we never received leaves the transaction PENDING, never FAILED.
    ///
    /// Credentials come off the "API Providers" row (see <see cref="Pay1ProviderResolver"/>), so
    /// an admin adds, edits and activates Pay1 entirely from the UI.
    /// </summary>
    public class Pay1CodedProvider : CodedProviderBase
    {
        private readonly Fino.Pay1Api.Interfaces.IPay1ApiService _pay1;
        private readonly IProviderMasterRepository _providerRepo;

        public Pay1CodedProvider(
            Fino.Pay1Api.Interfaces.IPay1ApiService pay1,
            IProviderMasterRepository providerRepo,
            IProviderRequestResponseLogRepository logRepo) : base(logRepo)
        {
            _pay1 = pay1;
            _providerRepo = providerRepo;
        }

        public override bool CanHandle(ProviderMaster provider) =>
            Pay1ProviderResolver.IsPay1(provider.ProviderName);

        public override string Family => ServiceKinds.BBPS;

        // ---- transaction ---------------------------------------------------------

        public override async Task<RechargeProviderResult?> Dispatch(
            ProviderMaster provider, ProviderConfiguration route, RechargeDispatchContext context)
        {
            var request = new Fino.Pay1Api.Requests.BillPaymentRequest
            {
                Operator = route.OpCode ?? string.Empty,
                AccountNumber = context.ConsumerNumber,
                Amount = context.Amount,
                Param = context.Param1,
                Param1 = context.Param2,
                RequestID = context.ProviderRequestID,
            };

            var reply = await _pay1.BillPayment(request, Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, context.TransactionMasterID, context.ServiceID,
                context.IPAddress,
                $"Pay1 BillPayment operator={request.Operator} account={request.AccountNumber} amount={request.Amount:0.##} trans_id={request.RequestID}",
                reply.Attempted ? reply.Data : reply.Message);

            var result = new RechargeProviderResult
            {
                ProviderID = provider.ProviderID,
                ProviderRequestID = context.ProviderRequestID,
                ProviderRefNo = reply.TransactionID,
                RawResponse = Truncate(reply.Data, 4000),
                ProviderCommission = route.AdminComm > 0 ? route.AdminComm : null,
            };

            // Never sent: credentials or biller code missing. No money is at risk, so the caller
            // may try the next provider in the sequence.
            if (!reply.Attempted)
            {
                result.Status = StatusType.Failed;
                result.IsConfigured = false;
                result.Message = reply.Message;
                return result;
            }

            // Sent, but no readable answer: the payment may well have gone through - hold it.
            if (!reply.Success)
            {
                result.Status = StatusType.Pending;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.AwaitingConfirmation : reply.Message;
                return result;
            }

            if (reply.Balance > 0) result.ProviderClosingBalance = reply.Balance;

            result.Status = reply.IsSuccess ? StatusType.Success
                          : reply.IsFailure ? StatusType.Failed
                          : StatusType.Pending;   // "pending" and anything unrecognised hold the money
            result.Message = string.IsNullOrWhiteSpace(reply.Message)
                ? DefaultMessageFor(result.Status) : reply.Message;
            return result;
        }

        // ---- bill fetch ----------------------------------------------------------

        public override async Task<CodedBillFetchResult?> FetchBill(
            ProviderMaster provider, ProviderConfiguration route, BillFetchContext context)
        {
            var request = new Fino.Pay1Api.Requests.BillFetchRequest
            {
                Operator = route.OpCode ?? string.Empty,
                AccountNumber = context.ConsumerNumber,
                Param = context.Param1,
                Param1 = context.Param2,
            };

            var reply = await _pay1.BillFetch(request, Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, transactionID: 0, context.ServiceID, context.IPAddress,
                $"Pay1 BillFetch operator={request.Operator} account={request.AccountNumber}",
                reply.Attempted ? reply.Data : reply.Message);

            // Could not reach Pay1 - no money involved, let the caller try the next provider.
            if (!reply.Attempted || !reply.Success) return new CodedBillFetchResult(reply.Message, null);

            return new CodedBillFetchResult(StatusType.Ok, new BillFetchResponse
            {
                Status = reply.IsSuccess ? StatusType.Success : StatusType.Failed,
                OperatorID = context.OperatorID,
                ProviderID = provider.ProviderID,
                CustomerName = reply.CustomerName,
                BillAmount = reply.BillAmount > 0 ? reply.BillAmount : null,
                DueDate = reply.DueDate,
                BillDate = reply.BillDate,
                BillNumber = reply.BillNumber,
                Message = reply.Message,
                RawResponse = Truncate(reply.Data, 4000),
            });
        }

        // ---- status --------------------------------------------------------------

        public override async Task<ProviderStatusResult?> CheckStatus(ProviderMaster provider, ProviderStatusContext context)
        {
            var reply = await _pay1.StatusCheck(
                new Fino.Pay1Api.Requests.StatusCheckRequest { RequestIDs = context.ProviderRequestID },
                Pay1ProviderResolver.CredentialsFrom(provider));

            await WriteLog(provider.ProviderID, context.UserID, context.TransactionMasterID, context.ServiceID,
                context.IPAddress, $"Pay1 StatusCheck trans_id={context.ProviderRequestID}",
                reply.Attempted ? reply.Data : reply.Message);

            var result = new ProviderStatusResult
            {
                ProviderRefNo = string.IsNullOrWhiteSpace(reply.TransactionID) ? context.ProviderRequestID : reply.TransactionID,
                RawResponse = Truncate(reply.Data, 4000),
            };

            // An enquiry we could not complete tells us nothing - leave the transaction alone.
            if (!reply.Attempted || !reply.Success)
            {
                result.Status = StatusType.Pending;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.ProviderUnreachable : reply.Message;
                return result;
            }

            result.Status = reply.IsSuccess ? StatusType.Success
                          : reply.IsFailure ? StatusType.Failed
                          : StatusType.Pending;
            result.Message = string.IsNullOrWhiteSpace(reply.Message)
                ? DefaultMessageFor(result.Status) : reply.Message;
            return result;
        }

        // ---- balance -------------------------------------------------------------

        public override async Task<CodedBalanceResult?> CheckBalance(ProviderMaster provider)
        {
            var reply = await _pay1.Balance(Pay1ProviderResolver.CredentialsFrom(provider));

            var result = new ProviderBalanceResponse
            {
                ProviderID = provider.ProviderID,
                ProviderName = provider.ProviderName,
                ExtractedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RawResponse = Truncate(reply.Data, 4000),
            };

            if (!reply.IsSuccess)
            {
                result.Status = StatusType.Failed;
                result.Message = string.IsNullOrWhiteSpace(reply.Message)
                    ? Messages.Provider.Message.ProviderUnreachable : reply.Message;
                return new CodedBalanceResult(StatusType.Ok, result);
            }

            result.Status = StatusType.Success;
            result.RechargeBalance = reply.Balance;
            result.Message = Messages.Provider.Message.BalanceFetched;
            result.ExtractedValues["Balance"] = reply.Balance.ToString(CultureInfo.InvariantCulture);

            // Keep the dashboard figure in step with the provider, exactly as JaiJui does.
            provider.RechargeBalance = reply.Balance;
            await _providerRepo.Update(provider);

            return new CodedBalanceResult(StatusType.Ok, result);
        }
    }
}
