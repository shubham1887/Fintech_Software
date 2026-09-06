namespace FintechApp.Responses.UserAccounting
{
    public class UserwiseAccountingViewModel
    {
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public decimal OpeningBal { get; set; }
        public decimal Purchase { get; set; }
        public decimal Sale { get; set; }
        public decimal CommsionAmount { get; set; }
        public decimal Surcharge { get; set; }
        public decimal TotalOldAmountAdjustment { get; set; }
        public decimal TotalNewAmountAdjustment { get; set; }
        public decimal TotalCrAmt { get; set; }
        public decimal TotalDrAmt { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal ExpClosingBal { get; set; }
        public decimal Difference { get; set; }
        public int ManualUpdateCount { get; set; }
    }

    public class UserwiseAccountingWrapper
    {
        public int Count { get; set; }
        public List<UserwiseAccountingViewModel> list { get; set; } = new();
    }

    public class AmountAdjustmentViewModel
    {
        public int TransactionMasterID { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string DOC { get; set; } = string.Empty;
        public string UpdatedDOC { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal DebitAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ManualSuccessFailViewModel
    {
        public int TransactionMasterID { get; set; }
        public int UserID { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public decimal Amount { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string DOC { get; set; } = string.Empty;
        public string UpdatedDOC { get; set; } = string.Empty;
    }

    public class AmountAdjustmentWrapper
    {
        public int Count { get; set; }
        public List<AmountAdjustmentViewModel> list { get; set; } = new();
    }

    public class ManualSuccessFailWrapper
    {
        public int Count { get; set; }
        public List<ManualSuccessFailViewModel> list { get; set; } = new();
    }
}
