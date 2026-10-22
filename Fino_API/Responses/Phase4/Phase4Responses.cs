namespace FintechApp.Responses.Phase4
{
    public class EnqueuePostingResult
    {
        public int AcctPostingOutboxID { get; set; }
        public bool AlreadyExisted { get; set; }
    }

    /// <summary>What one pass of the outbox engine did. Nothing here is silent: a failure is a
    /// counted row with a message, and a skip (no rule, all-zero) is distinct from a failure.</summary>
    public class OutboxProcessResult
    {
        public int Attempted { get; set; }
        public int Posted { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public List<string> Messages { get; set; } = new();
    }

    public class VoucherLineRow
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; } = string.Empty;
    }

    public class VoucherRow
    {
        public int AcctVoucherID { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string PeriodKey { get; set; } = string.Empty;
        public string VoucherType { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<VoucherLineRow> Lines { get; set; } = new();
    }

    public class PostResult
    {
        public int AcctVoucherID { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string Warning { get; set; } = string.Empty;
    }

    public class TrialBalanceRow
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        /// <summary>Net, signed to the account's normal side (positive = on its normal side).</summary>
        public decimal Balance { get; set; }
    }

    public class TrialBalanceResult
    {
        public string PeriodKey { get; set; } = string.Empty;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        /// <summary>|Dr − Cr| ≤ 0.01. Always true if the engine is sound; the check exists to catch
        /// the day it stops being true.</summary>
        public bool Balanced { get; set; }
        public List<TrialBalanceRow> Rows { get; set; } = new();
    }

    /// <summary>One integrity check: the ledger's figure against the operational one it must match.</summary>
    public class IntegrityCheckResult
    {
        public string Check { get; set; } = string.Empty;
        public decimal LedgerFigure { get; set; }
        public decimal OperationalFigure { get; set; }
        public decimal Difference { get; set; }
        public bool Ok { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    public class IntegrityReport
    {
        public DateTime RanAt { get; set; }
        public bool AllOk { get; set; }
        public List<IntegrityCheckResult> Checks { get; set; } = new();
    }
}
