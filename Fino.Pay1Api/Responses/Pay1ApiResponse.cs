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

        /// <summary>What the biller calls the extra identifier it asked for, when it names one
        /// ("Email ID to receive receipt"). Display only.</summary>
        public string AccountLabel { get; set; } = string.Empty;

        // Complaint fields.
        public string ComplaintID { get; set; } = string.Empty;
        public int TurnaroundTimeHours { get; set; }

        /// <summary>One entry per request id when a transaction enquiry named several and Pay1
        /// answered with an array. Empty for every other operation, and for an enquiry answered
        /// with a single object — in both of those cases the fields above are the whole answer.</summary>
        public List<Pay1ApiResponse> Results { get; set; } = new();

        public bool IsSuccess => Status.Equals("success", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => Status.Equals("pending", StringComparison.OrdinalIgnoreCase);
        public bool IsFailure => Status.Equals("failure", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The entry that answers for one request id. A reply that carried a single result answers
        /// for the id that was asked about, so it is returned as-is; in a batch, the entry is
        /// matched on req_id. A batch that never mentions the id says nothing about it, and comes
        /// back as an unreadable result rather than another transaction's outcome — reading a
        /// neighbour's "success" onto this transaction would settle money against the wrong row.
        /// </summary>
        public Pay1ApiResponse For(string requestID)
        {
            if (Results.Count == 0) return this;

            string wanted = (requestID ?? string.Empty).Trim();
            var match = Results.FirstOrDefault(r =>
                r.RequestID.Equals(wanted, StringComparison.OrdinalIgnoreCase));

            return match ?? new Pay1ApiResponse
            {
                Data = Data,
                Attempted = Attempted,
                Success = false,
                Message = Messages.Pay1.Message.TransactionNotInReply,
            };
        }

        public static Pay1ApiResponse Fail(string message) => new() { Success = false, Message = message };
    }
}
