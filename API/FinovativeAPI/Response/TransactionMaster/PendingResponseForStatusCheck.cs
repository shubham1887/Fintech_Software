using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class PendingResponseForStatusCheck
    {
        public int TransactionMasterID { set; get; }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public decimal Amount { set; get; }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }
    }
}