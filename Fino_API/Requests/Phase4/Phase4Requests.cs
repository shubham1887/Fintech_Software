namespace FintechApp.Requests.Phase4
{
    /// <summary>Enqueue a business event for posting. The engine turns it into a balanced voucher
    /// from the seeded rules for its type; <see cref="Amounts"/> supplies the figures the rules
    /// name (e.g. "Amount", "Taxable", "Tax").</summary>
    public class EnqueuePostingRequest
    {
        public string EventType { get; set; } = string.Empty;

        /// <summary>Unique per real-world event, e.g. "WALLET_TOPUP:FundTxn:48213". Enqueuing the
        /// same key twice is a no-op — the idempotency control.</summary>
        public string EventKey { get; set; } = string.Empty;

        public DateTime? EventDate { get; set; }
        public string Narration { get; set; } = string.Empty;

        /// <summary>The amount fields the posting rules read, by name.</summary>
        public Dictionary<string, decimal> Amounts { get; set; } = new();
    }

    public class ManualJournalLineInput
    {
        public string AccountCode { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Narration { get; set; } = string.Empty;
    }

    /// <summary>Post a hand-written balanced journal (an adjustment, an opening balance).</summary>
    public class ManualJournalRequest
    {
        public DateTime? VoucherDate { get; set; }
        public string Narration { get; set; } = string.Empty;
        public List<ManualJournalLineInput> Lines { get; set; } = new();
        public int CreatedByUserID { get; set; }
    }

    public class ReverseVoucherRequest
    {
        public int AcctVoucherID { get; set; }
        public string Narration { get; set; } = string.Empty;
        public DateTime? ReversalDate { get; set; }
        public int CreatedByUserID { get; set; }
    }

    public class ProcessOutboxRequest
    {
        public int MaxItems { get; set; } = 200;
    }

    public class PeriodLockRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public int CreatedByUserID { get; set; }
    }

    /// <summary>Read filter for vouchers / outbox / trial balance.</summary>
    public class LedgerQueryRequest
    {
        public string PeriodKey { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>For the outbox read: Pending / Failed / Posted / Skipped, or blank for all.</summary>
        public string Status { get; set; } = string.Empty;
    }
}
