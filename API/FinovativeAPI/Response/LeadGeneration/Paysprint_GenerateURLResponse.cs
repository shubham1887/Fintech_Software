using Project.Processing;

namespace Project.Response.LeadGeneration
{
    public class Paysprint_GenerateURLResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _referenceNumber;

        public string referenceNumber
        {
            get => _referenceNumber.TrimIfNotNull();
            set => _referenceNumber = value.TrimIfNotNull();
        }

        public int response_code { get; set; }

        private string _Transactionid;

        public string Transactionid
        {
            get
            {
                return _Transactionid.TrimIfNotNull();
            }
            set
            {
                _Transactionid = value.TrimIfNotNull();
            }
        }
    }
}