namespace FintechApp.Responses.Verification
{
    public class VerificationDocumentResponse
    {
        public int VerificationDocumentConfigID { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        /// <summary>Only populated on the user-facing (GetRequiredDocuments) response —
        /// the user's current submission for this document, if any.</summary>
        public string Status { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        /// <summary>The reviewer's note — for a rejected document this is the reason the user has
        /// to act on, so it must reach them, not just the admin.</summary>
        public string Remark { get; set; } = string.Empty;
    }
}
