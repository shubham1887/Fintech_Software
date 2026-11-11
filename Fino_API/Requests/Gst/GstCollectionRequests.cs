namespace FintechApp.Requests.Gst
{
    /// <summary>
    /// The GST collection report for a period. Dates rather than a month key, because a desk
    /// reconciling a filing wants the month, and one answering "what have we collected so far"
    /// wants today — and both are the same question over a different window.
    /// </summary>
    public class GstCollectionReportRequest
    {
        /// <summary>Default: the first of the current month.</summary>
        public DateTime? FromDate { get; set; }

        /// <summary>Default: today.</summary>
        public DateTime? ToDate { get; set; }

        /// <summary>0 = every service.</summary>
        public int ServiceID { get; set; }
    }
}
