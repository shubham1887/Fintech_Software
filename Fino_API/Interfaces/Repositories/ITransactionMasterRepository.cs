using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionMasterRepository
    {
        Task<List<TransactionMaster>> GetAll(int userId, string status, string serviceType, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
        Task<(int count, List<TransactionMaster> list)> GetFiltered(int userId, string status, List<int>? serviceIds, int serviceId, int operatorId, DateTime? fromDate, DateTime? toDate, int skip, int take);
        Task<TransactionMaster?> GetByID(int id);
        Task<TransactionMaster?> GetByProviderRequestID(string providerRequestID);

        // Look up a transaction by the API user's OWN request id. This is the idempotency key of
        // the B2B API (/api/v1): a repeated requestid replays the original transaction instead of
        // charging twice, and it is also how /api/v1/status resolves an enquiry. Scoped by user so
        // one API user can never read or replay another's transaction by guessing a request id.
        Task<TransactionMaster?> GetByApiUserRequestID(int userID, string apiUserRequestID);
        Task<TransactionMaster?> GetByProviderRequestIDLoose(string providerRequestID);
        Task<int> Insert(TransactionMaster entity);
        Task<int> UpdateStatus(int transactionMasterID, string status, string txnID, string remark);
        Task<int> UpdateProviderReply(int transactionMasterID, int providerID, string referenceNo, string rawResponse, decimal? providerOpeningBalance, decimal? providerClosingBalance);

        // Persists the commission outcome once the serving provider is known:
        // ProviderCommission (earned from the provider) and AdminCommission
        // (= ProviderCommission - retailer/distributor/super-distributor payouts).
        Task<int> UpdateCommissionOutcome(int transactionMasterID, decimal providerCommission, decimal adminCommission);
        Task<int> GetPendingCount();
        Task<int> GetPendingDMTCount();
        Task<int> GetPendingUPICount();

        // Pending recharge-family transactions eligible for an automated status re-check:
        // still PENDING/PROCESS, already dispatched to a provider (ProviderID > 0) and at least
        // minAgeSeconds old (so the provider has had time to settle). Oldest first, capped to take.
        Task<List<TransactionMaster>> GetPendingRechargeForStatusCheck(int minAgeSeconds, int take);
    }
}
