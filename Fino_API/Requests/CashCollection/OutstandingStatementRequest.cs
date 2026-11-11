namespace FintechApp.Requests.CashCollection
{
    /// <summary>
    /// What the View button asks for: one user's outstanding statement, optionally narrowed to
    /// the edge it is owed on and to a date window.
    /// </summary>
    public class OutstandingStatementRequest
    {
        /// <summary>Whose statement — the user who owes the money.</summary>
        public int UserID { get; set; }

        /// <summary>
        /// Optional: only the rows owed to this upline.
        ///
        /// Outstanding is per edge of the network, so a user funded on credit by two different
        /// uplines has two debts. Left at zero an admin sees both; a non-admin viewing a downline
        /// is pinned to their own edge regardless of what they send.
        /// </summary>
        public int ParentID { get; set; }

        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;

        public int SkipCount { get; set; }

        /// <summary>Page size. Left at zero the service applies its own sane default.</summary>
        public int TakeCount { get; set; }

        public bool IsExcelDownload { get; set; }
    }
}
