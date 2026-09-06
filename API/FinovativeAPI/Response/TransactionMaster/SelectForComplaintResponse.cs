using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class SelectForComplaintResponse
    {
        private int _TransactionMasterID;
        private string _DOC;
        public int UserID { set; get; }

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

        private string _ServiceName;
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        public decimal Amount { set; get; }
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        public int TransactionMasterID { get => _TransactionMasterID; set => _TransactionMasterID = value; }
    }
}