namespace FintechApp.Responses.SenderBeneficiary
{
    /// <summary>
    /// One row of the Sender / Beneficiary report: a registered DMT sender, who registered them,
    /// and how many beneficiaries they hold.
    /// </summary>
    public class SenderReportViewModel
    {
        public int SenderID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;

        /// <summary>The retailer (or other entity) the sender is registered under.</summary>
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;

        /// <summary>How many beneficiaries this sender has — the number the report exists to show.</summary>
        public int BeneficiaryCount { get; set; }

        public bool IsActive { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsKYC { get; set; }
        public DateTime DOC { get; set; }
    }

    public class SenderReportWrapper
    {
        public int Count { get; set; }
        public List<SenderReportViewModel> list { get; set; } = new();
    }
}
