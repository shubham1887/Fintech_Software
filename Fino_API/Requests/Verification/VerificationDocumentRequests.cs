namespace FintechApp.Requests.Verification
{
    public class CreateVerificationDocumentRequest
    {
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class UpdateVerificationDocumentRequest : CreateVerificationDocumentRequest
    {
        public int VerificationDocumentConfigID { get; set; }
    }

    public class DeleteVerificationDocumentRequest
    {
        public int VerificationDocumentConfigID { get; set; }
    }

    /// <summary>User submitting one document for manual verification. The file is uploaded first
    /// via Upload/Image; only the resulting path is sent here.</summary>
    public class SubmitVerificationDocumentRequest
    {
        public int VerificationDocumentConfigID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
    }
}
