using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    public class TransferResponse
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

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        private string _Balance;

        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.TrimIfNotNull();
            set => _BeneName = value.TrimIfNotNull();
        }

        private string _IsRefund;

        public string IsRefund
        {
            get => _IsRefund.TrimIfNotNull();
            set => _IsRefund = value.TrimIfNotNull();
        }

        private string _SortTransactionID;

        public string SortTransactionID
        {
            get => _SortTransactionID.TrimIfNotNull();
            set => _SortTransactionID = value.TrimIfNotNull();
        }

        private string _ReferenceNumber;

        public string ReferenceNumber
        {
            get => _ReferenceNumber.TrimIfNotNull();
            set => _ReferenceNumber = value.TrimIfNotNull();
        }

        private string _PaymentMode;

        public string PaymentMode
        {
            get => _PaymentMode.TrimIfNotNull();
            set => _PaymentMode = value.TrimIfNotNull();
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }
    }
}
