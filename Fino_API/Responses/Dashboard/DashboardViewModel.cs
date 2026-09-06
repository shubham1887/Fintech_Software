namespace FintechApp.Responses.Dashboard
{
    public class DashboardViewModel
    {
        public decimal TotalSuccessAmount { get; set; }
        public decimal TotalFailedAmount { get; set; }
        public decimal TransferAmount { get; set; }
        public int TransferCount { get; set; }
        public decimal ComplaintAmount { get; set; }
        public int ComplaintCount { get; set; }
        public int KycCount { get; set; }
        public decimal TotalTarget { get; set; }
        public decimal RemainsTarget { get; set; }
        public decimal PrePaidSuccess { get; set; }
        public decimal PrePaidFail { get; set; }
        public decimal PostPaidSuccess { get; set; }
        public decimal PostPaidFail { get; set; }
        public decimal DthSuccess { get; set; }
        public decimal DthFail { get; set; }
        public decimal DMTSuccess { get; set; }
        public decimal DMTFail { get; set; }
        public decimal AEPSSuccess { get; set; }
        public decimal AEPSFail { get; set; }
        public decimal PurchaseAmount { get; set; }
        public int PurchaseCount { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal Surcharge { get; set; }
        public decimal TotalAdminCommission { get; set; }
        public decimal TotalAdminSurcharge { get; set; }
        public decimal AdminCommissionProfit { get; set; }
        public decimal AdminSurchargeProfit { get; set; }
        public decimal RofferComm { get; set; }
        public decimal RofferTotal { get; set; }
        public string RofferPer { get; set; } = "0";

        // Admin dashboard balances (populated for the admin Select only).
        public decimal AdminWalletBalance { get; set; }       // admin's own wallets
        public decimal TotalUserWalletBalance { get; set; }   // sum of all other users' wallets
        public decimal ProviderStockTotal { get; set; }       // sum of provider recharge + DMT balances
        public decimal FundRequestPendingAmount { get; set; } // pending (PROCESS) fund requests today
        public int FundRequestPendingCount { get; set; }

        // Admin dashboard breakdowns (populated for the admin Select only).
        public List<ProviderStockItem> ProviderStocks { get; set; } = new();
        public List<UserTypeBalanceItem> UserTypeBalances { get; set; } = new();
        public List<WalletBalanceItem> AdminWallets { get; set; } = new();
        public List<ServiceSplitItem> ServiceSplit { get; set; } = new();
        public List<DayPerfItem> MonthlyPerformance { get; set; } = new();

        // Distributor / Super-Distributor dashboard: direct downline with balance + day sale.
        public List<DownlineSummaryItem> Downline { get; set; } = new();
    }

    // One direct downline user (retailer under a distributor, distributor under a super-distributor):
    // their wallet float and the day's transaction volume.
    public class DownlineSummaryItem
    {
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal SuccessAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public int SuccessCount { get; set; }
    }

    // Day-wise successful volume: selected month vs the month before (Monthly Performance chart).
    public class DayPerfItem
    {
        public int Day { get; set; }
        public decimal CurrentMonth { get; set; }
        public decimal LastMonth { get; set; }
    }

    // Per-provider purchased stock (Recharge + DMT float held with the provider).
    public class ProviderStockItem
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public decimal RechargeBalance { get; set; }
        public decimal DMTBalance { get; set; }
        public decimal Total { get; set; }
        public bool IsActive { get; set; }
    }

    // Wallet float sitting with each tier of the network (retailers, distributors, …).
    public class UserTypeBalanceItem
    {
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public decimal Balance { get; set; }
    }

    // Admin's own balance per wallet (Recharge / DMT / AEPS …).
    public class WalletBalanceItem
    {
        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    // Day volume per service, resolved from ServiceMaster (ids are admin-created, not fixed).
    public class ServiceSplitItem
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal SuccessAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }
}
