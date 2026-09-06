using Project.Processing;

namespace Project.Response.KYC
{
    public class KYCViewModel
    {
        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }

        private string _DocumentNo;

        public string DocumentNo
        {
            get => _DocumentNo.TrimIfNotNull();
            set => _DocumentNo = value.TrimIfNotNull();
        }

        private string _ApprovedImageURL;

        public string ApprovedImageURL
        {
            get => _ApprovedImageURL.TrimIfNotNull();
            set => _ApprovedImageURL = value.TrimIfNotNull();
        }

        private string _SampleImageURL;

        public string SampleImageURL
        {
            get => _SampleImageURL.TrimIfNotNull();
            set => _SampleImageURL = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _UserDocumentID;

        public int UserDocumentID
        {
            get => _UserDocumentID;
            set => _UserDocumentID = value;
        }

        private int _KycDocumentMasterID;

        public int KycDocumentMasterID
        {
            get => _KycDocumentMasterID;
            set => _KycDocumentMasterID = value;
        }

        private string _Label;

        public string Label
        {
            get => _Label;
            set => _Label = value;
        }
    }
}