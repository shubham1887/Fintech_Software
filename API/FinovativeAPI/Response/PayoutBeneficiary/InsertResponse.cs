using Project.Processing;

namespace Project.Response.PayoutBeneficiary
{
    public class InsertResponse
    {
        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private bool _IsOtpVerify;

        public bool IsOtpVerify
        {
            get => _IsOtpVerify;
            set => _IsOtpVerify = value;
        }

        private string _VendorBeneID;

        public string VendorBeneID
        {
            get => _VendorBeneID.TrimIfNotNull();
            set => _VendorBeneID = value.TrimIfNotNull();
        }
        
    }
}