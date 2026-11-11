namespace FintechApp.Requests.SenderBeneficiary
{
    public class SenderActivateRequest
    {
        public int SenderID { get; set; }
        public bool IsActive { get; set; }
    }

    public class BeneficiaryActivateRequest
    {
        public int BeneficiaryID { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Filter for the Sender / Beneficiary report. Same shape as every other report request, so the
    /// shared report screen can drive it without a special case.
    /// </summary>
    public class SelectSenderReportRequest
    {
        /// <summary>Whose senders to list. 0 = everything the caller is allowed to see.</summary>
        public int UserID { get; set; }

        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;

        /// <summary>Free text matched against the sender's name, mobile and pincode.</summary>
        public string Search { get; set; } = string.Empty;

        public int SkipCount { get; set; }
        public int TakeCount { get; set; }
    }
}
