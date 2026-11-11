namespace FintechApp.Responses.Fund
{
    public class FundTransferReportWrapper
    {
        public int Count { get; set; }
        public List<FundRequestViewModel> data { get; set; } = new();
        public FundHeaderSummary HeaderViewModel { get; set; } = new();

        /// <summary>
        /// The figures strip above the table, in the shape every other report uses (label →
        /// figure). <see cref="HeaderViewModel"/> above carries the same totals in the typed shape
        /// the Android build reads and is kept for it; this one is what the web report screen
        /// renders, with no per-report code and no second place to decide the wording.
        ///
        /// Without it the fund report had no strip at all: the screen looks for "HeaderSummary"
        /// and the only thing here was a differently-named object, so a report whose totals the API
        /// had always computed showed none of them.
        /// </summary>
        public Dictionary<string, object> HeaderSummary { get; set; } = new();

        public string ReportPath { get; set; } = string.Empty;
        public bool IsDownload { get; set; }
    }
}
