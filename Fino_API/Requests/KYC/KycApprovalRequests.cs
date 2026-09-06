namespace FintechApp.Requests.Kyc
{
    /// <summary>Admin KYC queue filter. Status: Approved / Pending / Rejected / Not submitted / All.</summary>
    public class KycUserListRequest
    {
        public string Status { get; set; } = string.Empty;
        public string Search { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }

    public class KycUserDetailRequest
    {
        public int UserID { get; set; }
    }

    /// <summary>Approve or reject ONE submitted document.</summary>
    public class KycReviewDocumentRequest
    {
        public int UserDocumentID { get; set; }
        public bool IsApproved { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>
    /// Approve or reject a user's KYC as a whole. Approving marks every submitted document
    /// approved and flips IsKYCApproved; rejecting marks the outstanding ones rejected with the
    /// reason so the user knows what to re-submit.
    /// </summary>
    public class KycReviewUserRequest
    {
        public int UserID { get; set; }
        public bool IsApproved { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>
    /// The admin's KYC policy: whether KYC is enforced at all, and which services keep working
    /// while a user's KYC is still pending.
    /// </summary>
    public class KycPolicySaveRequest
    {
        public bool IsKYCEnforced { get; set; }
        public List<int> ServicesAllowedWithoutKYC { get; set; } = new();
    }
}
