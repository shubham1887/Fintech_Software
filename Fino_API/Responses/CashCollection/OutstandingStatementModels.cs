namespace FintechApp.Responses.CashCollection
{
    /// <summary>
    /// One line of a user's outstanding statement — exactly the columns the screen shows:
    /// date, who put it there, opening, credit, debit, closing, remark.
    ///
    /// Credit and Debit are separate amounts rather than one signed figure, because that is how
    /// the page is read and how it is checked: a column that should only ever hold increases is
    /// something a person can scan, and a sign that flipped is not.
    /// </summary>
    public class OutstandingStatementRow
    {
        public int CashCollectionDetailID { get; set; }

        public DateTime Date { get; set; }

        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;

        /// <summary>Who extended the credit, or who collected the payment.</summary>
        public int ParentID { get; set; }
        public string ParentName { get; set; } = string.Empty;

        public decimal Opening { get; set; }

        /// <summary>The amount, when this line INCREASED the outstanding. Zero otherwise.</summary>
        public decimal Credit { get; set; }

        /// <summary>The amount, when this line REDUCED the outstanding. Zero otherwise.</summary>
        public decimal Debit { get; set; }

        public decimal Closing { get; set; }

        public string Remark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string UTRNumber { get; set; } = string.Empty;

        /// <summary>The raw ledger type (FUND_REQUEST_CREDIT, CASH_COLLECTION, …).</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>A readable version of <see cref="Type"/> for the screen.</summary>
        public string TypeLabel { get; set; } = string.Empty;

        /// <summary>The fund request / transfer behind this line, when there is one.</summary>
        public int? FundTransactionID { get; set; }

        public int? WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;

        /// <summary>
        /// What the funded wallet went from and to. Null for a cash collection, which moves no
        /// wallet — the screen shows a dash rather than 0.00, because 0.00 reads as an empty wallet.
        /// </summary>
        public decimal? WalletOpening { get; set; }
        public decimal? WalletClosing { get; set; }
    }

    /// <summary>
    /// A page of statement rows plus the totals for the WHOLE filter, not just this page.
    ///
    /// The totals are deliberately not derived from <see cref="Rows"/>: a page of twenty lines out
    /// of four hundred would otherwise show a "total collected" that quietly means "total collected
    /// on this page", which is the kind of number someone reconciles against and then cannot
    /// explain.
    /// </summary>
    public class OutstandingStatementWrapper
    {
        public List<OutstandingStatementRow> Rows { get; set; } = new();

        /// <summary>Rows matching the filter in total — what an export loops until it has.</summary>
        public int TotalCount { get; set; }

        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }

        /// <summary>The user's outstanding right now (not as at the end of the filtered window).</summary>
        public decimal CurrentOutstanding { get; set; }

        public decimal MaxOutstanding { get; set; }

        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
