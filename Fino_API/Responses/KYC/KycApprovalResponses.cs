namespace FintechApp.Responses.Kyc
{
    /// <summary>One user in the admin KYC queue.</summary>
    public class KycUserRow
    {
        public int UserID { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;

        /// <summary>Approved / Pending / Rejected / Not submitted (see StaticModels.KycStatus).</summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>"E-KYC" when the user verified electronically, otherwise "Manual".</summary>
        public string Mode { get; set; } = string.Empty;
        /// <summary>The user completed the E-KYC / E-Verification flow — these are auto-approved.</summary>
        public bool IseKYC { get; set; }
        public bool IsKYCApproved { get; set; }
        public bool IsActive { get; set; }

        public int RequiredCount { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int InReviewCount { get; set; }

        public DateTime RegisteredOn { get; set; }
        public DateTime? LastSubmittedOn { get; set; }
        /// <summary>When the KYC was approved, and by whom (from KYCApprovedDetails).</summary>
        public DateTime? ApprovedOn { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>One configured document, merged with what this user submitted for it.</summary>
    public class KycDocumentRow
    {
        /// <summary>0 until the user submits — the row still shows, as an outstanding requirement.</summary>
        public int UserDocumentID { get; set; }
        public int VerificationDocumentConfigID { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }

        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        /// <summary>APPROVE / REJECT / PROCESS, or empty when nothing has been submitted.</summary>
        public string Status { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public DateTime? SubmittedOn { get; set; }
    }

    /// <summary>Everything the admin needs to decide one user's KYC, in one call.</summary>
    public class KycUserDetail : KycUserRow
    {
        public string PANNo { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ShopAddress { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string UniqueReferenceNo { get; set; } = string.Empty;
        public List<KycDocumentRow> Documents { get; set; } = new();
        /// <summary>E-KYC attempts on record (provider order id + when), for the audit trail.</summary>
        public List<KycEkycEntry> EkycHistory { get; set; } = new();
    }

    public class KycEkycEntry
    {
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }

    /// <summary>The KYC policy screen: the master switch plus the per-service exemptions.</summary>
    public class KycPolicyResponse
    {
        public bool IsKYCEnforced { get; set; }
        /// <summary>Key Setting "eVerification" — E-KYC mode rather than manual documents.</summary>
        public bool EVerification { get; set; }
        /// <summary>True when no active + required document is configured, so enforcement cannot
        /// bite yet (a user would have no way to become approved).</summary>
        public bool HasRequiredDocuments { get; set; }
        public List<KycPolicyService> Services { get; set; } = new();
    }

    public class KycPolicyService
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsAllowedWithoutKYC { get; set; }
    }

    /// <summary>Headline counts for the KYC queue.</summary>
    public class KycSummary
    {
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int NotSubmitted { get; set; }
        public int EkycVerified { get; set; }
    }

    public class KycUserListResponse
    {
        public KycSummary Summary { get; set; } = new();
        public List<KycUserRow> Users { get; set; } = new();
    }
}
