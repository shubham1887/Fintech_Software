namespace FintechApp.StaticModels
{
    /// <summary>The five account classes. An account's class fixes its normal balance, which is how
    /// a statement knows whether a positive figure sits on the debit or the credit side.</summary>
    public static class AccountType
    {
        public const string Asset = "Asset";
        public const string Liability = "Liability";
        public const string Income = "Income";
        public const string Expense = "Expense";
        public const string Equity = "Equity";

        /// <summary>Dr for assets and expenses, Cr for liabilities, income and equity.</summary>
        public static string NormalBalance(string accountType) =>
            accountType is Asset or Expense ? AccountSide.Debit : AccountSide.Credit;
    }

    public static class AccountSide
    {
        public const string Debit = "Dr";
        public const string Credit = "Cr";
    }

    /// <summary>
    /// The control accounts the integrity checks tie out against. A control account is the ledger's
    /// own figure for a balance that also lives in an operational table; the daily check proves the
    /// two still agree, which is the whole point of keeping books next to the wallets.
    /// </summary>
    public static class AccountCode
    {
        public const string Bank = "1000";
        /// <summary>Prepaid balance we hold WITH our providers — an asset. Ties to ProviderMaster stock.</summary>
        public const string ProviderStock = "1200";
        public const string GstInputCredit = "1300";
        /// <summary>What we OWE our users — the sum of their wallet balances is our liability.
        /// Ties to UserBalanceMaster.</summary>
        public const string UserWalletControl = "2000";
        public const string GstOutputPayable = "2100";
        public const string TdsPayable = "2200";
        public const string OwnerEquity = "3000";
        public const string ServiceIncome = "4000";
        public const string CommissionIncome = "4100";
        public const string ProviderPurchaseExpense = "5000";
        public const string RoundOff = "5900";
    }

    /// <summary>Where a voucher stands. Only a Posted voucher counts toward a balance; a Reversed
    /// one is cancelled by an equal-and-opposite voucher, never edited.</summary>
    public static class VoucherStatus
    {
        public const string Posted = "Posted";
        public const string Reversed = "Reversed";
    }

    /// <summary>
    /// The life of an outbox event. The point of the outbox is that none of these is ever silent:
    /// a Failed row keeps its error and its attempt count and is retried, so a posting that could
    /// not be made is a row you can see and not a number that is quietly wrong.
    /// </summary>
    public static class OutboxStatus
    {
        public const string Pending = "Pending";
        public const string Posted = "Posted";
        /// <summary>Tried and failed, with LastError set. Retryable until it succeeds or is skipped.</summary>
        public const string Failed = "Failed";
        /// <summary>Deliberately not posted (no rule for the event, or a decision to ignore it),
        /// with the reason recorded. Distinct from Failed so "nothing matched" is not read as "broke".</summary>
        public const string Skipped = "Skipped";
    }

    /// <summary>The business events that post to the ledger. Seeded posting rules key on these.</summary>
    public static class LedgerEvent
    {
        public const string WalletTopUp = "WALLET_TOPUP";
        public const string ServiceSaleGross = "SERVICE_SALE_GROSS";
        public const string CommissionEarned = "COMMISSION_EARNED";
        public const string ProviderPurchase = "PROVIDER_PURCHASE";
        public const string TaxOutput = "TAX_OUTPUT";
        public const string ManualJournal = "MANUAL_JOURNAL";
    }
}
