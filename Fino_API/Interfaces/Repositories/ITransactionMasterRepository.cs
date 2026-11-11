using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    /// <summary>
    /// The text-matching half of a transaction report's filters, kept apart from the structural
    /// half (user / service / operator / status / dates) because every field here is optional and
    /// adding each as another positional parameter had already made the call unreadable.
    ///
    /// All of it is applied IN THE QUERY, before the count and before paging. That matters: these
    /// filters used to be applied - where they were applied at all - to a page that had already
    /// been cut, so narrowing a report could only ever shrink the 20 rows already on screen while
    /// the total went on reporting the unfiltered figure.
    /// </summary>
    public sealed class TxnTextFilter
    {
        /// <summary>
        /// Which column <see cref="CriteriaText"/> applies to - the report screen's
        /// "selection criteria" dropdown: TransactionID, ConsumerNumber or ProviderRefNo.
        /// (UserMobile is resolved to user ids by the caller, since it is not a column here.)
        /// </summary>
        public string Criteria { get; init; } = string.Empty;

        /// <summary>The value typed beside the criteria dropdown. Blank = no criteria filter.</summary>
        public string CriteriaText { get; init; } = string.Empty;

        /// <summary>Free text, matched across consumer number and every reference column.</summary>
        public string Search { get; init; } = string.Empty;

        /// <summary>
        /// Restrict to these users. Used for a criteria the transaction table cannot answer on its
        /// own - "Entity Mobile" is a UserMaster column, so the caller resolves it to ids first.
        /// An EMPTY (not null) list means "a filter was asked for and nothing matched", which must
        /// return nothing rather than everything.
        /// </summary>
        public List<int>? UserIDs { get; init; }

        /// <summary>
        /// Restrict to these transactions. Used for the complaint filter, which lives in another
        /// table. Empty means "nothing matched", as above.
        /// </summary>
        public List<int>? TransactionIDs { get; init; }

        /// <summary>True when nothing here narrows anything, so the caller can skip the work.</summary>
        public bool IsEmpty =>
            CriteriaText.Trim().Length == 0 && Search.Trim().Length == 0
            && UserIDs == null && TransactionIDs == null;
    }

    /// <summary>
    /// The figures strip above a filter report, over everything the filter matched rather than the
    /// page on screen. Deliberately not scoped by the report's STATUS filter — see
    /// <see cref="ITransactionMasterRepository.GetFilteredTotals"/>.
    /// </summary>
    public sealed class TxnFilterTotals
    {
        public int SuccessCount { get; set; }
        public decimal SuccessAmount { get; set; }
        public int FailedCount { get; set; }
        public decimal FailedAmount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }

        /// <summary>
        /// The caller's own commission on the settled business, signed: positive = earned,
        /// negative = a surcharge they paid. Counted on SUCCESS only, so the strip agrees with the
        /// wallet rather than promising commission on transactions that failed.
        /// </summary>
        public decimal Commission { get; set; }

        /// <summary>Every transaction the filter matched, whatever its status.</summary>
        public int TotalCount { get; set; }
    }

    public interface ITransactionMasterRepository
    {
        Task<List<TransactionMaster>> GetAll(int userId, string status, string serviceType, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
        Task<(int count, List<TransactionMaster> list)> GetFiltered(int userId, string status, List<int>? serviceIds, int serviceId, int operatorId, DateTime? fromDate, DateTime? toDate, int skip, int take, TxnTextFilter? text = null);

        /// <summary>
        /// Success / failed / pending money and counts, plus net commission, for the SAME filter
        /// the paged read uses — minus the status filter, so the strip describes the whole period
        /// rather than the slice the reader happens to be looking at. Aggregated in SQL; the report
        /// never pulls every matching row back to add them up.
        /// </summary>
        Task<TxnFilterTotals> GetFilteredTotals(int userId, List<int>? serviceIds, int serviceId, int operatorId, DateTime? fromDate, DateTime? toDate, TxnTextFilter? text = null);
        Task<TransactionMaster?> GetByID(int id);

        /// <summary>
        /// Several transactions in one query. The account statement needs the originating
        /// transaction for every row on the page (for the operator's reference and the provider
        /// that served it) and was fetching them one id at a time — two hundred round trips to
        /// render two hundred rows.
        /// </summary>
        Task<List<TransactionMaster>> GetByIDs(IEnumerable<int> ids);

        /// <summary>
        /// The total CHARGE levied on one user's transactions in a period, optionally in one
        /// wallet. For the account statement's figures strip: the charge lives on the transaction,
        /// not on the ledger posting, so it cannot be added up from the statement rows themselves,
        /// and pulling every transaction of the period back to sum one column would be worse than
        /// not showing the figure.
        /// </summary>
        Task<decimal> SumChargeForUser(int userId, DateTime? fromDate, DateTime? toDate, int walletId = 0);
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

        // Same query for any service family (RECHARGE / DMT / …) — used by the Hangfire sweeps.
        Task<List<TransactionMaster>> GetPendingForStatusCheck(string serviceKind, int minAgeSeconds, int take);

        // DMT transfers parked by the global "Hold DMT" switch (debited, never dispatched).
        Task<List<TransactionMaster>> GetHeldDMT(int take);

        // Total a remitter has already sent since fromDate, across every DMT service id. Drives
        // the regulatory per-sender limit: a transfer that would push this past the cap is refused
        // BEFORE the wallet is touched. PENDING transfers count — money that may yet settle is
        // still committed — while FAILED/REFUND ones do not, because that money came back.
        Task<decimal> GetDMTConsumedBySender(string senderMobile, DateTime fromDate);
    }
}
