namespace Fino.Pay1Api.Responses
{
    // Result of a Pay1 Utility call.
    //   Success = the call completed and the body parsed (NOT the payment outcome).
    //   Status  = Pay1's own vocabulary: success / failure / pending.
    //   Data    = the raw provider JSON, kept for the provider log and for reports.
    // Pay1 answers HTTP 200 even for a declined payment, so callers must branch on Status.
    public class Pay1ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;

        /// <summary>True once the HTTP call was actually issued. A payment that was sent but whose
        /// reply never arrived MUST be held as pending, never failed - this flag is what tells the
        /// caller the difference between "nothing left our server" and "we do not know".</summary>
        public bool Attempted { get; set; }

        /// <summary>success / failure / pending, exactly as Pay1 sent it (lower case).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Pay1 error code ("0" on success, E0xx otherwise).</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Our unique request id echoed back (partner_reqid / req_id).</summary>
        public string RequestID { get; set; } = string.Empty;

        /// <summary>Pay1's transaction reference (ref_code on payment, trans_id on enquiry).</summary>
        public string TransactionID { get; set; } = string.Empty;

        /// <summary>Partner wallet balance left at Pay1, when the reply carries it.</summary>
        public decimal Balance { get; set; }

        // Bill fetch fields (Pay1 nests these under "description").
        public string CustomerName { get; set; } = string.Empty;
        public decimal BillAmount { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string BillDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string BillPeriod { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        // Complaint fields.
        public string ComplaintID { get; set; } = string.Empty;
        public int TurnaroundTimeHours { get; set; }

        public bool IsSuccess => Status.Equals("success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => Status.Equals("pending", StringComparison.OrdinalIgnoreCase);
        public bool IsFailure => Status.Equals("failure", StringComparison.OrdinalIgnoreCase);

        public static Pay1ApiResponse Fail(string message) => new() { Success = false, Message = message };
    }
}
