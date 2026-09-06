namespace FintechApp.Requests.TransactionProcessing
{
    // Provider push callback payload. Payin (and compatible recharge upstreams) call our callback URL
    // as a GET with these query-string keys once a recharge changes from PROCESS to SUCCESS/FAILURE:
    //   ?RequestID=..&ReferenceNo=..&Status=..&Number=..&Amount=..&Balance=..
    public class RechargeCallbackRequest
    {
        public string RequestID { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Balance { get; set; } = string.Empty;

        /// <summary>The provider is reversing a transaction it had already confirmed and has
        /// credited us back (Pay1 sends reversal_status=1). This is the only case in which an
        /// already-SUCCESS transaction may still be refunded.</summary>
        public bool IsReversal { get; set; }

        /// <summary>The provider's own message, kept on the settle / refund remark.</summary>
        public string Description { get; set; } = string.Empty;
    }

    public class TxnSelectRequest
    {
        public int UserID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class TxnSelectByIDRequest
    {
        public int ID { get; set; }
    }

    public class TxnUpdateStatusRequest
    {
        public int TransactionID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TxnID { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    public class TxnBatchIDRequest
    {
        public int TransactionID { get; set; }
    }

    // Shift a pending transaction onto a specific provider (single or batch element).
    public class TxnShiftRequest
    {
        public int TransactionID { get; set; }
        public int ProviderID { get; set; }
    }

    public class TxnProviderWiseRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
    }

    public class TxnDMTResendRequest
    {
        public int TransactionID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public int ProviderID { get; set; }
    }

    // Single-provider performance for a given date (2-hour interval avg processing time).
    public class TxnSingleProviderPerfRequest
    {
        public string Date { get; set; } = string.Empty;
        public int ProviderID { get; set; }
    }
}
