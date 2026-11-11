namespace FintechApp.StaticModels
{
    /// <summary>
    /// The ledger description of each chain-commission leg.
    ///
    /// A description is not decoration here, it is the only thing that identifies a leg after the
    /// fact. Two places read these back:
    ///   - the refund path, to decide whether a leg was ever posted. Reversing a leg that never
    ///     landed credits someone for a debit nobody took, which is how money gets created;
    ///   - LedgerIntegrityService, to pair a downline commission credit with the admin funding
    ///     debit that paid for it.
    /// Both break silently if the wording drifts, so the strings live here once and the prefixes
    /// used to find them are the same constants the builders are made of.
    ///
    /// Every string must stay byte-identical to what is already on disk — on this install there are
    /// rows going back to 11 October 2026, and changing one orphans every historical row it used to
    /// match. They all say "Recharge" because the recharge family was the first chain-funded
    /// service; DMT posts the same legs with the same wording. Renaming them is a data migration,
    /// not an edit.
    /// </summary>
    public static class ChainLegDescriptions
    {
        // ── The downline commission credit, and the admin debit that funds it ──
        public const string RetailerCommissionPrefix = "Recharge commission, txn #";
        public const string RetailerFundedPrefix = "Recharge retailer commission funded, txn #";
        public const string DistributorCommissionPrefix = "Recharge distributor commission, txn #";
        public const string DistributorFundedPrefix = "Recharge distributor commission funded, txn #";
        public const string SuperDistributorCommissionPrefix = "Recharge super-distributor commission, txn #";
        public const string SuperDistributorFundedPrefix = "Recharge super-distributor commission funded, txn #";

        // ── Single legs: no second posting to pair with ──
        public const string SurchargeReceivedPrefix = "Recharge surcharge received, txn #";
        public const string DistributorSurchargeReceivedPrefix = "Recharge distributor surcharge received, txn #";
        public const string SuperDistributorSurchargeReceivedPrefix = "Recharge super-distributor surcharge received, txn #";
        public const string ProviderCommissionIncomePrefix = "Recharge provider commission income, txn #";
        public const string LegacyAdminCommissionPrefix = "Admin commission, from txn #";

        /// <summary>
        /// The retailer's commission credit. This one carries the consumer number, so it is the only
        /// leg whose text is not just a prefix plus the transaction id.
        /// </summary>
        public static string RetailerCommission(int txnID, string consumerNumber)
            => $"{RetailerCommissionPrefix}{txnID} to: {consumerNumber}";

        public static string RetailerFunded(int txnID) => $"{RetailerFundedPrefix}{txnID}";

        public static string DistributorCommission(int txnID) => $"{DistributorCommissionPrefix}{txnID}";

        public static string DistributorFunded(int txnID) => $"{DistributorFundedPrefix}{txnID}";

        public static string SuperDistributorCommission(int txnID) => $"{SuperDistributorCommissionPrefix}{txnID}";

        public static string SuperDistributorFunded(int txnID) => $"{SuperDistributorFundedPrefix}{txnID}";

        public static string SurchargeReceived(int txnID) => $"{SurchargeReceivedPrefix}{txnID}";

        public static string DistributorSurchargeReceived(int txnID) => $"{DistributorSurchargeReceivedPrefix}{txnID}";

        public static string SuperDistributorSurchargeReceived(int txnID) => $"{SuperDistributorSurchargeReceivedPrefix}{txnID}";

        public static string ProviderCommissionIncome(int txnID) => $"{ProviderCommissionIncomePrefix}{txnID}";

        /// <summary>The legacy single net posting, used by services that are not chain-funded.</summary>
        public static string LegacyAdminCommission(int txnID, string consumerNumber)
            => $"{LegacyAdminCommissionPrefix}{txnID} to: {consumerNumber}";

        // ── The postings that take those four legs back on a refund ──
        public const string RetailerCommissionReversalPrefix = "Recharge commission reversal, txn #";
        public const string RetailerFundedRefundPrefix = "Recharge retailer commission refund, txn #";
        public const string DistributorCommissionReversalPrefix = "Recharge distributor commission reversal, txn #";
        public const string DistributorFundedRefundPrefix = "Recharge distributor commission refund, txn #";
        public const string SuperDistributorCommissionReversalPrefix = "Recharge super-distributor commission reversal, txn #";
        public const string SuperDistributorFundedRefundPrefix = "Recharge super-distributor commission refund, txn #";

        public static string RetailerCommissionReversal(int txnID) => $"{RetailerCommissionReversalPrefix}{txnID}";

        public static string RetailerFundedRefund(int txnID) => $"{RetailerFundedRefundPrefix}{txnID}";

        public static string DistributorCommissionReversal(int txnID) => $"{DistributorCommissionReversalPrefix}{txnID}";

        public static string DistributorFundedRefund(int txnID) => $"{DistributorFundedRefundPrefix}{txnID}";

        public static string SuperDistributorCommissionReversal(int txnID) => $"{SuperDistributorCommissionReversalPrefix}{txnID}";

        public static string SuperDistributorFundedRefund(int txnID) => $"{SuperDistributorFundedRefundPrefix}{txnID}";

        /// <summary>
        /// All four legs of one tier: the downline credit, the admin debit that funds it, and the
        /// two postings that take them back.
        ///
        /// On a chain-funded service these carry the same amount in opposite directions, so for any
        /// one transaction the four must sum to zero — whether it settled, failed, or had a leg taken
        /// back. LedgerIntegrityService.ChainCommissionsFunded asserts exactly that, and a non-zero
        /// sum is a leg that is missing: a commission nobody funded, or a funding reversal with no
        /// funding behind it.
        ///
        /// No prefix here is a prefix of another — each pair differs immediately after the word
        /// "commission" — which is what lets a description be classified by prefix alone.
        /// </summary>
        public static readonly (string Tier, string Credit, string Funded, string CreditUndo, string FundedUndo)[] FundedTiers =
        {
            ("retailer", RetailerCommissionPrefix, RetailerFundedPrefix,
                RetailerCommissionReversalPrefix, RetailerFundedRefundPrefix),
            ("distributor", DistributorCommissionPrefix, DistributorFundedPrefix,
                DistributorCommissionReversalPrefix, DistributorFundedRefundPrefix),
            ("super-distributor", SuperDistributorCommissionPrefix, SuperDistributorFundedPrefix,
                SuperDistributorCommissionReversalPrefix, SuperDistributorFundedRefundPrefix),
        };
    }
}
