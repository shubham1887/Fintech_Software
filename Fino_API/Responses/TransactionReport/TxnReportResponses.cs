using FintechApp.Responses.TransactionMaster;

namespace FintechApp.Responses.TransactionReport
{
    public class TxnReportWrapper
    {
        public int Count { get; set; }
        public List<TransactionResponse> list { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;

        /// <summary>
        /// The figures strip above the table — success / failed / pending money and counts, and
        /// the commission. Same shape as the analytics reports' HeaderSummary (label → figure), so
        /// the report screen renders it with no per-report code; keys are printed as given.
        ///
        /// Aggregated over everything the filter matched, not the page on screen, and NOT narrowed
        /// by the status filter: the strip is there to say how the period went, and "0 failed"
        /// because the reader is looking at the success list would be worse than no strip at all.
        /// </summary>
        public Dictionary<string, object> HeaderSummary { get; set; } = new();
    }

    public class TxnSummaryViewModel
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int PendingCount { get; set; }
        public int RefundCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal TotalCommission { get; set; }
        public double SuccessRate { get; set; }
    }

    public class UserWiseSummaryViewModel
    {
        public int UserID { get; set; }
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int PendingCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal Commission { get; set; }
        public double SuccessRate { get; set; }
    }

    public class MonthlySummaryViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal Commission { get; set; }
    }

    public class GSTSummaryViewModel
    {
        public int OperatorID { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AdminGST { get; set; }
        public decimal RetailerGST { get; set; }
        public decimal DistributerGST { get; set; }
        public decimal SuperDistributerGST { get; set; }
        public decimal TotalGST { get; set; }
        public int Count { get; set; }
    }

    public class StateWiseSummaryViewModel
    {
        public int StateID { get; set; }
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessAmount { get; set; }
        public double SuccessRate { get; set; }
    }

    public class AmountInsightViewModel
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessCount { get; set; }
    }

    /// <summary>
    /// One service's income for the period: what was sold through it and what each tier earned.
    ///
    /// The field names are the ones the Income Report screen reads. They were different here -
    /// ServiceType carried a raw ServiceID, and the commission fields were spelled
    /// RetailerCommission / DistributerCommission - so every column on that screen resolved to
    /// nothing and the report rendered as a table of empty cells. Renaming the payload rather than
    /// the screen keeps it consistent with the sale reports, which already use these names.
    /// </summary>
    public class IncomeSummaryViewModel
    {
        public int ID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        public decimal SuccessAmt { get; set; }
        public int SuccessCount { get; set; }

        /// <summary>Retailer tier. Signed: positive = commission earned, negative = surcharge paid.</summary>
        public decimal RCommission { get; set; }
        public decimal DCommission { get; set; }
        public decimal SCommission { get; set; }
        public decimal AdminCommission { get; set; }

        public decimal RetailerTDS { get; set; }
        public decimal DistributerTDS { get; set; }

        /// <summary>What the house keeps on this service, as the transactions recorded it.</summary>
        public decimal NetProfit { get; set; }

        /// <summary>Every tier's earnings less the TDS withheld from them.</summary>
        public decimal TotalIncome { get; set; }

        public int Count { get; set; }
    }
}
