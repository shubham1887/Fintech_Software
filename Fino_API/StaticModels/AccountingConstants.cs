namespace FintechApp.StaticModels
{
    // Direction of the wallet balance movement recorded on a TransactionDetails ledger row.
    public static class AmountCreditDebitType
    {
        public const string Debit = "DEBIT";
        public const string Credit = "CREDIT";
    }

    // Whether a ledger row represents a commission movement, and its direction.
    public static class CommissionCreditDebitType
    {
        public const string None = "NONE";
        public const string Credit = "CREDIT";
        public const string Debit = "DEBIT";
    }

    // Document/category type for a ledger row. Values mirror those read by LedgerService.
    public static class DOCTypeConstants
    {
        public const string Recharge = "Recharge";
        public const string FundTransfer = "FundTransfer";
        public const string FundReceive = "FundReceive";
        public const string FundReverse = "FundReverse";
        public const string CashCollection = "CASH_COLLECTION";
        public const string VirtualBalance = "VirtualBalance";
    }
}
