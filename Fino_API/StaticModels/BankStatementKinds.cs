namespace FintechApp.StaticModels
{
    /// <summary>What a statement file is.</summary>
    public static class BankStatementSource
    {
        public const string Csv = "CSV";
        public const string Xlsx = "XLSX";

        /// <summary>
        /// A PDF statement. Read as TEXT, line by line, and split into columns by the pattern on
        /// the bank's format — because a PDF has no columns, only ink at coordinates, and every
        /// bank lays theirs out differently.
        /// </summary>
        public const string Pdf = "PDF";

        public static readonly string[] All = { Csv, Xlsx, Pdf };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return v switch
            {
                "XLSX" or "XLS" or "EXCEL" => Xlsx,
                "PDF" => Pdf,
                _ => Csv,
            };
        }
    }

    /// <summary>
    /// What a line on the bank statement actually IS, in the operator's own words.
    ///
    /// This is the half of reconciliation the ledger cannot answer on its own: the bank says
    /// "₹50,000 debited, ref 123456" and only the business knows whether that was stock bought from
    /// a provider, an expense, or money moved between our own accounts. Matching a line to a
    /// voucher proves it happened; the category says what it was.
    /// </summary>
    public static class BankLineCategory
    {
        /// <summary>Money out to a provider — float or stock bought.</summary>
        public const string Purchase = "PURCHASE";

        /// <summary>Money in from a user — a retailer's deposit against a fund request.</summary>
        public const string UserFund = "USER_FUND";

        /// <summary>Money out that is simply spent: rent, salary, electricity, software.</summary>
        public const string Expense = "EXPENSE";

        /// <summary>Between two of our own accounts. It nets to nothing and must not be read as
        /// either income or spend.</summary>
        public const string Transfer = "TRANSFER";

        /// <summary>What the bank itself takes: charges, commission, GST on them.</summary>
        public const string BankCharges = "BANK_CHARGES";

        /// <summary>What the bank pays: interest credited.</summary>
        public const string Interest = "INTEREST";

        /// <summary>Money in from a gateway or aggregator settling a batch.</summary>
        public const string Settlement = "SETTLEMENT";

        /// <summary>Money back — a failed transfer returned, a provider refund.</summary>
        public const string Refund = "REFUND";

        /// <summary>Not classified yet. Shown as such rather than guessed at.</summary>
        public const string Uncategorised = "UNCATEGORISED";

        public static readonly string[] All =
        {
            Purchase, UserFund, Expense, Transfer, BankCharges, Interest, Settlement, Refund, Uncategorised,
        };

        /// <summary>The side of the statement a category can appear on, so a rule that would put a
        /// credit under "expense" is refused rather than quietly mis-booking money.</summary>
        public static string NaturalSide(string? category) => (category ?? string.Empty).ToUpperInvariant() switch
        {
            Purchase or Expense or BankCharges => BankLineDirection.Debit,
            UserFund or Interest or Settlement or Refund => BankLineDirection.Credit,
            _ => BankLineDirection.Any,
        };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
            return All.Contains(v) ? v : Uncategorised;
        }
    }

    /// <summary>Which side of the statement a rule applies to.</summary>
    public static class BankLineDirection
    {
        public const string Debit = "DEBIT";
        public const string Credit = "CREDIT";
        public const string Any = "ANY";

        public static readonly string[] All = { Debit, Credit, Any };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return v is Debit or Credit ? v : Any;
        }
    }

    /// <summary>How a category rule matches a statement narration.</summary>
    public static class BankLineMatchType
    {
        /// <summary>The narration contains this text, case-insensitively. The ordinary case.</summary>
        public const string Contains = "CONTAINS";

        public const string StartsWith = "STARTS_WITH";

        /// <summary>A regular expression, for a bank that encodes the counterparty in a fixed shape.</summary>
        public const string Regex = "REGEX";

        public static readonly string[] All = { Contains, StartsWith, Regex };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
            return All.Contains(v) ? v : Contains;
        }
    }
}
