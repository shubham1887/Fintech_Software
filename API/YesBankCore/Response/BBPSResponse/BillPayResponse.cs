using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class BillPayResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        
        private string _ResponseCode;

        public string ResponseCode
        {
            get => _ResponseCode.TrimIfNotNull();
            set => _ResponseCode = value.TrimIfNotNull();
        }


        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }



        private string _TransactionDateTime;

        public string TransactionDateTime
        {
            get => _TransactionDateTime.TrimIfNotNull();
            set => _TransactionDateTime = value.TrimIfNotNull();
        }

        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        private string _PrimaryNumber;
        public string PrimaryNumber
        {
            get => _PrimaryNumber.TrimIfNotNull();
            set => _PrimaryNumber = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
    }
}
