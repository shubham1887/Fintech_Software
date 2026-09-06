using Project.Processing;

namespace Project.Response.ProviderResponse
{
    public class TestResponseViewModel
    {
        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _ClosingBalance;

        public string ClosingBalance
        {
            get => _ClosingBalance.TrimIfNotNull();
            set => _ClosingBalance = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _RefID;

        public string RefID
        {
            get => _RefID.TrimIfNotNull();
            set => _RefID = value.TrimIfNotNull();
        }

        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }
    }
}