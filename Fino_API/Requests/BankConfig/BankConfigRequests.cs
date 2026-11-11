namespace FintechApp.Requests.BankConfig
{
    /// <summary>Create or update one bank's statement format.</summary>
    public class SaveBankStatementFormatRequest
    {
        public int BankStatementFormatID { get; set; }
        public int BankAccountID { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>CSV / XLSX / PDF.</summary>
        public string Source { get; set; } = string.Empty;

        public int HeaderRow { get; set; } = -1;
        public int SkipRows { get; set; }
        public string DateFormat { get; set; } = string.Empty;

        public string TxnDateColumn { get; set; } = string.Empty;
        public string ValueDateColumn { get; set; } = string.Empty;
        public string DescriptionColumn { get; set; } = string.Empty;
        public string ReferenceColumn { get; set; } = string.Empty;
        public string DebitColumn { get; set; } = string.Empty;
        public string CreditColumn { get; set; } = string.Empty;
        public string AmountColumn { get; set; } = string.Empty;
        public string BalanceColumn { get; set; } = string.Empty;

        public string PdfLinePattern { get; set; } = string.Empty;
        public string PdfSkipPattern { get; set; } = string.Empty;
        public bool PdfJoinWrappedLines { get; set; } = true;

        public bool IsActive { get; set; } = true;
        public string Remark { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    public class BankFormatIdRequest
    {
        public int BankStatementFormatID { get; set; }
    }

    /// <summary>Try a format against a real file and see what comes out, importing nothing.</summary>
    public class PreviewBankFormatRequest
    {
        /// <summary>A saved format to try. 0 uses the one described in this request instead.</summary>
        public int BankStatementFormatID { get; set; }

        /// <summary>The format to try when no id is given — so a format can be tested before it is saved.</summary>
        public SaveBankStatementFormatRequest? Format { get; set; }

        /// <summary>CSV text, or the file's bytes as base64 for XLSX and PDF.</summary>
        public string Content { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        /// <summary>How many rows to show. Default 15 — enough to see the shape, short enough to read.</summary>
        public int MaxRows { get; set; } = 15;
    }

    /// <summary>Create or update one "what is this line" rule.</summary>
    public class SaveBankLineCategoryRuleRequest
    {
        public int BankLineCategoryRuleID { get; set; }

        /// <summary>0 = every bank.</summary>
        public int BankAccountID { get; set; }

        /// <summary>DEBIT / CREDIT / ANY.</summary>
        public string Direction { get; set; } = string.Empty;

        /// <summary>CONTAINS / STARTS_WITH / REGEX.</summary>
        public string MatchType { get; set; } = string.Empty;

        public string Pattern { get; set; } = string.Empty;

        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }

        /// <summary>PURCHASE / USER_FUND / EXPENSE / TRANSFER / BANK_CHARGES / INTEREST / SETTLEMENT / REFUND.</summary>
        public string Category { get; set; } = string.Empty;

        public string LedgerAccountCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public string Remark { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    public class BankRuleIdRequest
    {
        public int BankLineCategoryRuleID { get; set; }
    }

    /// <summary>Re-run the rules over what is already imported.</summary>
    public class ReclassifyRequest
    {
        public int BankAccountID { get; set; }

        /// <summary>0 = every active import for that bank.</summary>
        public int BankStatementImportID { get; set; }

        public int ChangedByUserID { get; set; }
    }

    /// <summary>A person deciding what a line is, which no rule may later overwrite.</summary>
    public class SetBankLineCategoryRequest
    {
        public int BankStatementLineID { get; set; }
        public string Category { get; set; } = string.Empty;

        /// <summary>Optional: the user this line is about, when the operator recognises them.</summary>
        public int CounterpartyUserID { get; set; }

        /// <summary>Optional, for a PURCHASE line: the provider the stock was bought from. Stored
        /// as the line's counterparty name so the register and the export read "JaiJui", and so
        /// the reconciliation can look for the matching Provider Purchase entry.</summary>
        public int ProviderID { get; set; }

        /// <summary>Clear a duplicate flag the rules raised, when it has been looked at and is fine.</summary>
        public bool ClearDuplicateFlag { get; set; }

        public int ChangedByUserID { get; set; }
    }
}
