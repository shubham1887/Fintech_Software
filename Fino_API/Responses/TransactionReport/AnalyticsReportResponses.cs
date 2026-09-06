namespace FintechApp.Responses.TransactionReport
{
    /// <summary>
    /// Standard envelope for every aggregate ("analytics") report. `list` is the row set the web
    /// report screen binds to, `Count` the total before paging, and `HeaderSummary` the KPI strip
    /// the screen shows above the table. Property names match the web column keys exactly.
    /// </summary>
    public class AnalyticsWrapper<T>
    {
        public int Count { get; set; }
        public List<T> list { get; set; } = new();
        public Dictionary<string, object> HeaderSummary { get; set; } = new();
    }

    /// <summary>Service-wise sale (TransactionReport/ServicewiseSale) — one row per service.</summary>
    public class ServiceSaleViewModel
    {
        public int ID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal SuccessAmt { get; set; }
        public int SuccessCount { get; set; }
        public decimal FailAmt { get; set; }
        public int FailCount { get; set; }
        public decimal PendingAmt { get; set; }
        public int PendingCount { get; set; }
        public decimal TotalAmt { get; set; }
        public int TotalCount { get; set; }
        public decimal RCommission { get; set; }
        public decimal DCommission { get; set; }
        public decimal SCommission { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal NetProfit { get; set; }
        /// <summary>Success count ÷ total count, as a display string ("94.2%").</summary>
        public string SuccessRate { get; set; } = string.Empty;
    }

    /// <summary>User-wise sale (TransactionReport/UserwiseSale) — one row per transacting user.</summary>
    public class UserSaleViewModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public decimal OpeningBal { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal Sale { get; set; }
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal Surcharge { get; set; }
        /// <summary>Funds credited to this user in the period (fund transfer / accepted request).</summary>
        public decimal Purchase { get; set; }
        /// <summary>Funds this user pushed down to their own downline in the period.</summary>
        public decimal TransferAmount { get; set; }
        public decimal ReverseAmountDr { get; set; }
        public decimal ReverseAmountCr { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
    }

    /// <summary>Operator-wise sale (TransactionReport/OperatorSale) — user × service × operator.</summary>
    public class OperatorSaleViewModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal ReatilerComm { get; set; } // legacy spelling kept for the web column key
        public string RetailerCommPercentage { get; set; } = string.Empty;
        public decimal DistributerComm { get; set; }
        public string DistributerCommPercentage { get; set; } = string.Empty;
        public decimal SuperDistributerComm { get; set; }
        public string SuperDistributerCommPercentage { get; set; } = string.Empty;
    }

    /// <summary>Top performer (TransactionReport/SelectTopPerformer) — highest sale first.</summary>
    public class TopPerformerViewModel
    {
        public int Rank { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal TotalSale { get; set; }
        public int SuccessCount { get; set; }
        public decimal TotalCommission { get; set; }
    }

    /// <summary>Operator-wise daybook row — one operator's day, split by outcome.</summary>
    public class DaybookViewModel
    {
        public int ID { get; set; }
        public int OperatorID { get; set; }
        public string Operator { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessCount { get; set; }
        public decimal SuccessAmount { get; set; }
        public int FailedCount { get; set; }
        public decimal FailedAmount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal Surcharge { get; set; }
    }

    /// <summary>TDS row — tax deducted at source, per transaction.</summary>
    public class TdsViewModel
    {
        public int ID { get; set; }
        public int TransactionMasterID { get; set; }
        public DateTime DOC { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RetailerCommission { get; set; }
        public decimal RetailerTDS { get; set; }
        public decimal DistributerTDS { get; set; }
        public decimal SuperDistributerTDS { get; set; }
        public decimal TotalTDS { get; set; }
    }

    /// <summary>Amount-bucket row (Rangewise Recharge).</summary>
    public class AmountRangeViewModel
    {
        public int ID { get; set; }
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Sum { get; set; }
        public int SuccessCount { get; set; }
        public decimal SuccessSum { get; set; }
        public int FailCount { get; set; }
        public decimal FailSum { get; set; }
        public int ProcessCount { get; set; }
        public decimal ProcessSum { get; set; }
    }

    /// <summary>Per-user DMT totals split into amount buckets (Userwise Range Report).</summary>
    public class UserDMTRangeViewModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Range1_2 { get; set; }
        public decimal Range3_1000 { get; set; }
        public decimal Range1001_2000 { get; set; }
        public decimal Range2001_3000 { get; set; }
        public decimal Range3001_4000 { get; set; }
        public decimal Range4001_5000 { get; set; }
        public decimal Range5001_10000 { get; set; }
        public decimal Range10001_15000 { get; set; }
        public decimal Range15001_20000 { get; set; }
        public decimal Range20001_25000 { get; set; }
        public decimal Range25001_50000 { get; set; }
        public decimal Range50001_1L { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>One calendar day of sale, split by service kind (Operator/User Report).</summary>
    public class DailyServiceSaleViewModel
    {
        public int ID { get; set; }
        public DateTime DOC { get; set; }
        public decimal MobileSale { get; set; }
        public decimal DTHSale { get; set; }
        public decimal PostPaid { get; set; }
        public decimal Electricity { get; set; }
        public decimal MoneyTransfer { get; set; }
        public decimal ExpressMoneyTransfer { get; set; }
        public decimal AepsSale { get; set; }
        public decimal PaymentGatewaySale { get; set; }
        public decimal TotalSale { get; set; }
    }

    /// <summary>Yesterday-vs-today sale per user, split by service kind (Userwise Operator Sale).</summary>
    public class UserServiceSellViewModel
    {
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal YesterdayMobileSale { get; set; }
        public decimal TodayMobileSale { get; set; }
        public decimal YesterdayDTHSale { get; set; }
        public decimal TodayDTHSale { get; set; }
        public decimal YesterdayPostPaid { get; set; }
        public decimal TodayPostPaid { get; set; }
        public decimal YesterdayElectricity { get; set; }
        public decimal TodayElectricity { get; set; }
        public decimal YesterdayMoneyTransfer { get; set; }
        public decimal TodayMoneyTransfer { get; set; }
        public decimal YesterdayTotal { get; set; }
        public decimal TodayTotal { get; set; }
    }

    /// <summary>Newly-added user row (Users/SelectDatewise).</summary>
    public class DatewiseUserViewModel
    {
        public int UserID { get; set; }
        public DateTime DOC { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string IsKYCApprovedStr { get; set; } = string.Empty;
        /// <summary>Per-wallet balances as "Recharge: 1,200 | DMT: 500".</summary>
        public string Balances { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string JoiningBy { get; set; } = string.Empty;
        public string RegistrationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Downline network row with per-wallet balances (Users/SelectDownlineReport).</summary>
    public class DownlineReportViewModel
    {
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public decimal RechargeBalance { get; set; }
        public decimal DMTBalance { get; set; }
        public decimal AEPSBalance { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal Outstanding { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>Merchant onboarding row (Users/SelectUserOnbordingList).</summary>
    public class UserOnboardingViewModel
    {
        public int id { get; set; }
        public DateTime DOC { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }

    /// <summary>Service-charge deduction row (ServiceChargeDeduction/Select).</summary>
    public class ServiceChargeDeductionViewModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Charge { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>
    /// One commission-slab row as seen by the user the slab is assigned to (Commission Structure).
    /// Rates are returned both raw (for maths/export) and pre-formatted (StrXComm) so the report
    /// can print "1.5 %" or "₹ 5" without knowing the commission type.
    /// </summary>
    public class CommissionStructureViewModel
    {
        public int SlabDetailID { get; set; }
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        /// <summary>Amount band this rate applies to, e.g. "₹ 10 – ₹ 1,000" or "Any amount".</summary>
        public string Details { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }

        /// <summary>The commission the LOGGED-IN user actually earns — their own tier's rate.</summary>
        public decimal MyCommission { get; set; }
        public string MyCommissionType { get; set; } = string.Empty;
        public string StrMyCommission { get; set; } = string.Empty;

        public decimal RetailerComm { get; set; }
        public decimal DistributorComm { get; set; }
        public decimal SuperDistributorComm { get; set; }
        public decimal ClientComm { get; set; }
        public string StrRetailerComm { get; set; } = string.Empty;
        public string StrDistributorComm { get; set; } = string.Empty;
        public string StrSuperDistributorComm { get; set; } = string.Empty;
        public string StrClientComm { get; set; } = string.Empty;
    }
}
