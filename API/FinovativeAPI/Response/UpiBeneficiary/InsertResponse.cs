using Project.Processing;

namespace Project.Response.UpiBeneficiary
{
    public class InsertResponse
    {
        private string _UpiBeneficiaryID;

        public string UpiBeneficiaryID
        {
            get => _UpiBeneficiaryID.TrimIfNotNull();
            set => _UpiBeneficiaryID = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _VendorBeneID;

        public string VendorBeneID
        {
            get => _VendorBeneID.TrimIfNotNull();
            set => _VendorBeneID = value.TrimIfNotNull();
        }

    }
}