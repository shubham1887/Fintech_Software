using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class BillPayRequest
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


        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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
            set  => _TransactionID = value.TrimIfNotNull(); 
        }

        private string _EncryptionKey;

        public string EncryptionKey
        {
            get => _EncryptionKey.TrimIfNotNull();
            set => _EncryptionKey = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _param1;

        public string param1
        {
            get => _param1.TrimIfNotNull();
            set => _param1 = value.TrimIfNotNull();
        }

        private string _param2;

        public string param2
        {
            get => _param2.TrimIfNotNull();
            set => _param2 = value.TrimIfNotNull();
        }

        private string _param3;

        public string param3
        {
            get => _param3.TrimIfNotNull();
            set => _param3 = value.TrimIfNotNull();
        }

        private string _param4;

        public string param4
        {
            get => _param4.TrimIfNotNull();
            set => _param4 = value.TrimIfNotNull();
        }

        private string _param5;

        public string param5
        {
            get => _param5.TrimIfNotNull();
            set => _param5 = value.TrimIfNotNull();
        }
        
        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }
        
        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

    }
}
