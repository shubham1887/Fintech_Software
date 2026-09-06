using FintechApp.Responses.TransactionMaster;

namespace FintechApp.Responses.TransactionReport
{
    public class TxnReportWrapper
    {
        public int Count { get; set; }
        public List<TransactionResponse> list { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
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

    public class IncomeSummaryViewModel
    {
        public string ServiceType { get; set; } = string.Empty;
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal RetailerTDS { get; set; }
        public decimal DistributerTDS { get; set; }
        public decimal TotalIncome { get; set; }
        public int Count { get; set; }
    }
}
