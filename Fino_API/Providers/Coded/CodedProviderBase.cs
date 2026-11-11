using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;

namespace FintechApp.Providers.Coded
{
    /// <summary>
    /// The bits every coded provider needs and none should re-invent: the provider request/response
    /// log, the column clamp that goes with it, and the default wording for a status.
    ///
    /// These used to live as private helpers inside ProviderGatewayService, which is why the
    /// per-provider code had to live there too. They are here so an adapter is a self-contained
    /// class in its own file.
    /// </summary>
    public abstract class CodedProviderBase : ICodedProvider
    {
        private readonly IProviderRequestResponseLogRepository _logRepo;
        private readonly Logger _logger = new();

        protected CodedProviderBase(IProviderRequestResponseLogRepository logRepo) => _logRepo = logRepo;

        public abstract bool CanHandle(ProviderMaster provider);
        public abstract string Family { get; }

        // The four operations, as overridable virtuals rather than relying on the interface's
        // default implementations. A default interface member is only reached when NO class in
        // the hierarchy supplies the method, and the binding rules around that are subtle enough
        // that an adapter could appear to implement an operation while the gateway still called
        // the default. Declaring them here makes it plain C# overriding, which cannot misbind.
        // Null means "this provider does not do that" — the gateway carries on as usual.

        public virtual Task<RechargeProviderResult?> Dispatch(
            ProviderMaster provider, ProviderConfiguration route, RechargeDispatchContext context)
            => Task.FromResult<RechargeProviderResult?>(null);

        public virtual Task<CodedBillFetchResult?> FetchBill(
            ProviderMaster provider, ProviderConfiguration route, BillFetchContext context)
            => Task.FromResult<CodedBillFetchResult?>(null);

        public virtual Task<ProviderStatusResult?> CheckStatus(
            ProviderMaster provider, ProviderStatusContext context)
            => Task.FromResult<ProviderStatusResult?>(null);

        public virtual Task<CodedBalanceResult?> CheckBalance(ProviderMaster provider)
            => Task.FromResult<CodedBalanceResult?>(null);

        /// <summary>
        /// Record what we sent and what came back, against the transaction. Never throws:
        /// a logging failure must not take down a dispatch that may already have moved money.
        /// </summary>
        protected async Task WriteLog(int providerID, int userID, int transactionID, int serviceID,
            string ipAddress, string request, string response)
        {
            try
            {
                await _logRepo.Insert(new ProviderRequestResponseLog
                {
                    ProviderID = providerID,
                    UserID = userID > 0 ? userID : null,
                    TransactionID = transactionID,
                    ServiceID = serviceID,
                    IPAddress = ipAddress,
                    Through = "ProviderGateway",
                    Request = Truncate(request, 8000),
                    Response = Truncate(response, 8000),
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        protected static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value ?? string.Empty : value[..max];

        protected static string DefaultMessageFor(string status) => status switch
        {
            StatusType.Success => Messages.Transaction.Message.RechargeSuccess,
            StatusType.Failed => Messages.Transaction.Message.RechargeFailedRefunded,
            _ => Messages.Transaction.Message.RechargePending
        };
    }
}
