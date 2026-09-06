namespace Project.Message
{
    public class KYCMessage
    {
        public static readonly string InvalidDocumentType = "Please select valid document type";
        public static readonly string InvalidDocument = "Please select valid document";
        public static readonly string AlreadyApprove = "This type of document already approved";
        public static readonly string DuplicateDocument = "This type of document already uploaded";
        public static readonly string DuplicateDocumentNO = "Document Type is already exists";
        public static readonly string DocumentnotUpload = "Invalid document";

        public static readonly string NotFound = "Data not found";
        public static readonly string InvalidDocumentNo = "Please enter valid document number";

        public static readonly string KYC_Accept = "KYC Accept";
        public static readonly string KYC_Reject = "KYC Reject";

        public static readonly string KYCStatusUploadSuccess = "KYC status updated successfully";
        public static readonly string KYCApproveSuccess = "KYC status approved successfully";
        public static readonly string UploadSuccess = "Document uploaded successfully";
        public static readonly string UploadFail = "Document not uploaded";

        public static readonly string ApproveSuccess = "User updated successfully";
        public static readonly string IsRquiredOn = "Required on successfully";
        public static readonly string IsRquiredOff = "Required off successfully";

        public static readonly string InvalidID = "Invalid ID";
        public static readonly string RequestNotFound = "Requested data not found";

        public static readonly string InvalidFileSize = "File size must under 3MB!";
        public static readonly string InvalidFileType = "Only jpg/jpeg and png files are allowed!";
        public static readonly string KYCForAllService = "Please complete KYC first";
        public static readonly string InsertFailure = "Insert Fail";
    }
}