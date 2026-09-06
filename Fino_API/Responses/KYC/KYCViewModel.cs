namespace FintechApp.Responses.KYC
{
    public class KYCViewModel
    {
        public int UserDocumentID { get; set; }
        public int UserID { get; set; }
        public string RequestDate { get; set; } = string.Empty;
        public string PersonName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty;
        public string ApproveDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool ISApproved { get; set; }
        public string ApprovedImageURL { get; set; } = string.Empty;
        public string SampleImageURL { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string KYCApproveName { get; set; } = string.Empty;
    }

    public class KYCWrapper
    {
        public List<KYCViewModel> list { get; set; } = new();
        public string ReportPath { get; set; } = string.Empty;
        public bool IsExcelDownload { get; set; }
        public int Count { get; set; }
    }

    public class KYCActiveDocViewModel
    {
        public int UserDocumentID { get; set; }
        public int KycDocumentMasterID { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string ApprovedImageURL { get; set; } = string.Empty;
        public string SampleImageURL { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
