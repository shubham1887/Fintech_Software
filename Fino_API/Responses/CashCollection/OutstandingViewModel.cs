namespace FintechApp.Responses.CashCollection
{
    /// <summary>One row of the "who owes how much" list.</summary>
    public class OutstandingViewModel
    {
        public int UserID { get; set; }

        /// <summary>Firm name and mobile, pre-joined — the list is read, not filtered, on the client.</summary>
        public string UserName { get; set; } = string.Empty;

        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

        /// <summary>Who the money is owed TO — the upline that extended the credit.</summary>
        public int ParentID { get; set; }
        public string ParentName { get; set; } = string.Empty;

        public decimal Outstanding { get; set; }

        /// <summary>The credit limit. Zero means this account may not take funds on credit at all.</summary>
        public decimal MaxOutstanding { get; set; }

        /// <summary>Limit minus outstanding, floored at zero — how much more they could take.</summary>
        public decimal AvailableCredit { get; set; }

        /// <summary>When the balance last moved, so a stale debt is visible as stale.</summary>
        public DateTime? LastMovementDate { get; set; }

        // ── Ageing ───────────────────────────────────────────────────────────────────────
        //
        // The balance alone does not say whether an account is a problem: ₹50,000 taken
        // yesterday and ₹50,000 taken in July are the same number and completely different
        // conversations. The buckets below split the balance by how long each unpaid credit has
        // been outstanding, matching collections against credits oldest-first (FIFO) — the
        // convention every receivables ageing uses, and the one a retailer expects: the money
        // they pay in clears the oldest thing they took.
        //
        // The four buckets always add up to Outstanding exactly. Where the statement and the
        // balance disagree (a balance carried over from before the statement was kept, say), the
        // difference is placed in the newest bucket rather than quietly dropped, so the report
        // can never total to something other than what is owed.

        /// <summary>Unpaid credit taken in the last 30 days.</summary>
        public decimal Age0To30 { get; set; }

        /// <summary>Unpaid credit 31–60 days old.</summary>
        public decimal Age31To60 { get; set; }

        /// <summary>Unpaid credit 61–90 days old.</summary>
        public decimal Age61To90 { get; set; }

        /// <summary>Unpaid credit older than 90 days.</summary>
        public decimal Age90Plus { get; set; }

        /// <summary>The date of the oldest credit still unpaid — the age of the debt.</summary>
        public DateTime? OldestUnpaidDate { get; set; }

        /// <summary>How many days that oldest unpaid credit has been outstanding.</summary>
        public int OldestUnpaidDays { get; set; }

        /// <summary>When money was last collected against this account (a DEBIT on the statement).
        /// Null when they have never paid anything back.</summary>
        public DateTime? LastCollectionDate { get; set; }

        /// <summary>Outstanding as a percentage of the limit. 0 when there is no limit.</summary>
        public decimal UtilisationPercent { get; set; }

        /// <summary>True when the balance has gone past the limit — the one row an admin must act on.</summary>
        public bool IsOverLimit { get; set; }
    }
}
