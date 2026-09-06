using Project.Processing;

namespace Project.Response.CashDeposit
{
    public class FingPay_GenerateOTPResponse
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

        private string _RefernceNo;

        public string RefernceNo
        {
            get => _RefernceNo.TrimIfNotNull();
            set => _RefernceNo = value.TrimIfNotNull();
        }

        private string _TransactionToken;

        public string TransactionToken
        {
            get => _TransactionToken.TrimIfNotNull();
            set => _TransactionToken = value.TrimIfNotNull();
        }

        private string _MerchantTransactionID;

        public string MerchantTransactionID
        {
            get => _MerchantTransactionID.TrimIfNotNull();
            set => _MerchantTransactionID = value.TrimIfNotNull();
        }
    }
}