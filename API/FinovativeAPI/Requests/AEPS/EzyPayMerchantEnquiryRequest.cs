using Project.Processing;

namespace Project.Requests.AEPS
{
    public class EzyPayMerchantEnquiryRequest
    {
        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }
    }
}