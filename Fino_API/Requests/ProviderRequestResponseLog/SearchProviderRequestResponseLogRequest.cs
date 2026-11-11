namespace FintechApp.Requests.ProviderRequestResponseLog
{
    /// <summary>
    /// Filter for the Provider Log screen.
    ///
    /// The screen used to pull the entire log table and filter it in the browser, which meant every
    /// visit shipped the whole request/response history across the wire and got slower every day.
    /// Filtering and paging happen here instead, so the page only ever loads what it shows.
    /// </summary>
    public class SearchProviderRequestResponseLogRequest
    {
        /// <summary>Inclusive start date (yyyy-MM-dd). Empty = no lower bound.</summary>
        public string FromDate { get; set; } = string.Empty;

        /// <summary>Inclusive end date (yyyy-MM-dd). Empty = no upper bound.</summary>
        public string ToDate { get; set; } = string.Empty;

        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public int TransactionID { get; set; }

        /// <summary>Call kind — RECHARGE / STATUS / BALANCE / DMT … Empty = all.</summary>
        public string Through { get; set; } = string.Empty;

        /// <summary>Free text matched against the request, the response and the IP.</summary>
        public string Search { get; set; } = string.Empty;

        public int SkipCount { get; set; }
        public int TakeCount { get; set; } = 25;
    }
}
