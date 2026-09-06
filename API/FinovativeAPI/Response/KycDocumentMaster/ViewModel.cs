using Project.Processing;

namespace Project.Response.KycDocumentMaster
{
    public class ViewModel
    {
        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private bool _IsRequired;

        public bool IsRequired
        {
            get => _IsRequired;
            set => _IsRequired = value;
        }

        private int _KycDocumentMasterID;

        public int KycDocumentMasterID
        {
            get => _KycDocumentMasterID;
            set => _KycDocumentMasterID = value;
        }

        private string _SampleImageURL;

        public string SampleImageURL
        {
            get => _SampleImageURL.TrimIfNotNull();
            set => _SampleImageURL = value.TrimIfNotNull();
        }

        private string _RegularRxpression;

        public string RegularExpression
        {
            get => _RegularRxpression.TrimIfNotNull();
            set => _RegularRxpression = value.TrimIfNotNull();
        }
    }
}