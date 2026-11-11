namespace FintechApp.Responses.TransactionMaster
{
    public class TransactionResponse
    {
        public int TransactionMasterID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int WalletID { get; set; }
        public int ProviderID { get; set; }
        public int DistributerUserID { get; set; }
        public int SuperDistributerUserID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProviderRequestID { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string ProviderDOCNo { get; set; } = string.Empty;
        public string ProviderRefNo { get; set; } = string.Empty;
        public decimal RetailerCommission { get; set; }
        public decimal DistributerCommission { get; set; }
        public decimal SuperDistributerCommission { get; set; }
        public decimal AdminCommission { get; set; }
        public decimal ProviderCommission { get; set; }
        public decimal RetailerTDS { get; set; }
        public decimal DistributerTDS { get; set; }
        public decimal SuperDistributerTDS { get; set; }
        public decimal AdminGST { get; set; }
        public decimal RetailerGST { get; set; }
        public decimal DistributerGST { get; set; }
        public decimal SuperDistributerGST { get; set; }
        public decimal CashBack { get; set; }
        public decimal NetCommission { get; set; }
        public decimal Charge { get; set; }
        public decimal AdminProfit { get; set; }
        // What the PROVIDER said it held before and after this transaction. Stored on every
        // dispatch but never carried into a report, so the "API Cl Bal" column on the recharge
        // and BBPS reports — the figure an operator reconciles a provider against — was blank.
        public decimal ProviderOpeningBalance { get; set; }
        public decimal ProviderClosingBalance { get; set; }
        public string DMT_SenderID { get; set; } = string.Empty;
        public string DMT_SenderName { get; set; } = string.Empty;
        public string DMT_SenderNumber { get; set; } = string.Empty;
        public string DMT_BeneficiaryID { get; set; } = string.Empty;
        public string DMT_BeneficiaryName { get; set; } = string.Empty;
        public string DMT_Bank { get; set; } = string.Empty;
        public string DMT_IFSC { get; set; } = string.Empty;
        public string DMT_BeneficaryNameByBank { get; set; } = string.Empty;
        public string DMT_PaymentType { get; set; } = string.Empty;
        public decimal DMT_OriginalAmount { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Circle { get; set; } = string.Empty;
        public string ComplaintStatus { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string ChangeStatus { get; set; } = string.Empty;
        public string PendingProviderStatus { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public string BBPS_CustomerName { get; set; } = string.Empty;
        public bool IsDispute { get; set; }
        public bool IsROffer { get; set; }
        public bool IsOnlineDMT { get; set; }
        public bool IsCCF { get; set; }
        public bool SettlementAtProviderEnd { get; set; }
        public int HitBackCount { get; set; }
        public int StateID { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }

        // Display fields the web report screens bind to (filled by report enrichment).
        public DateTime DOC { get; set; }
        public string Response { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string DistributerName { get; set; } = string.Empty;
        public string SuperDistributerName { get; set; } = string.Empty;
        /// <summary>The tier's mobile number beside its name; blank when the tier is the operator ("Admin").</summary>
        public string DistributerMobile { get; set; } = string.Empty;
        public string SuperDistributerMobile { get; set; } = string.Empty;

        // Human settlement remark (transaction Description) plus, when a complaint exists for this
        // transaction, the complaint remark appended — the report "Remark" column binds to this
        // instead of the raw provider JSON.
        public string Remark { get; set; } = string.Empty;

        // Complaint remark on its own (admin remark preferred, else customer remark), empty when
        // there is no complaint. Filled by report enrichment.
        public string ComplaintRemark { get; set; } = string.Empty;

        // The four commission tiers rolled into one readable string for the report's
        // "MDS/DS Commission" column: Retailer / Distributor / Super Distributor / Admin.
        public string CommissionInfo { get; set; } = string.Empty;

        // When the transaction was last settled/updated, and how long it took from creation to
        // that settlement. Both are display-only, filled by MapToResponse.
        public DateTime UpdatedDOC { get; set; }
        public string ProcTime { get; set; } = string.Empty;
    }
}
