namespace FintechApp.Responses.CashCollection
{
    public class OutstandingWrapper
    {
        public List<OutstandingViewModel> data { get; set; } = new();
        public bool IsExcelDownload { get; set; }
        public string ReportPath { get; set; } = string.Empty;

        /// <summary>
        /// The sum of every row the caller may see.
        ///
        /// Computed on the server over the whole scoped set, not on the client over whatever
        /// arrived: "total credit out on the network" is the first number an operator looks at,
        /// and a total that silently means "of the rows currently rendered" is the kind that gets
        /// quoted in a meeting.
        /// </summary>
        public decimal TotalOutstanding { get; set; }

        // ── Report totals ────────────────────────────────────────────────────────────────
        // Everything below is derived over the same scoped set as TotalOutstanding, for the same
        // reason: a report footer that means "of this page" is worse than no footer.

        /// <summary>Total credit limit granted across the visible accounts.</summary>
        public decimal TotalLimit { get; set; }

        /// <summary>Total headroom left — limit minus outstanding, floored per account.</summary>
        public decimal TotalAvailable { get; set; }

        /// <summary>How many visible accounts actually carry a balance (not merely a limit).</summary>
        public int AccountsWithBalance { get; set; }

        /// <summary>How many have gone past their limit.</summary>
        public int OverLimitCount { get; set; }

        /// <summary>The ageing buckets, summed. These add up to <see cref="TotalOutstanding"/>.</summary>
        public decimal TotalAge0To30 { get; set; }
        public decimal TotalAge31To60 { get; set; }
        public decimal TotalAge61To90 { get; set; }
        public decimal TotalAge90Plus { get; set; }

        // ── The caller's own side of the ledger ──────────────────────────────────────────
        //
        // This list answers "who owes me". The other half — "what do I owe, and to whom" — is a
        // different question with a different answer, and a distributor has both at once. It is
        // returned here so the report can lead with it rather than making the reader open a
        // second screen to find their own position.

        /// <summary>What the caller owes their own upline.</summary>
        public decimal MyOutstanding { get; set; }

        /// <summary>The caller's own credit limit.</summary>
        public decimal MyLimit { get; set; }

        /// <summary>The caller's remaining headroom.</summary>
        public decimal MyAvailable { get; set; }

        /// <summary>Who the caller owes it to. Empty for admin, who has no upline.</summary>
        public string MyPayableTo { get; set; } = string.Empty;

        /// <summary>The date the report is aged against — always "now" on the server, so two
        /// readers in different timezones cannot disagree about which bucket a credit is in.</summary>
        public DateTime AsOn { get; set; }
    }
}
