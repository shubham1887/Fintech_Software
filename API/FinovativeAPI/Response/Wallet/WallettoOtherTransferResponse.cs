using Project.Processing;

namespace Project.Response.Wallet
{
    public class WallettoOtherTransferResponse
    {
        private string _mainWalletBal;

        public string mainWalletBal
        {
            get => _mainWalletBal.TrimIfNotNull();
            set => _mainWalletBal = value.TrimIfNotNull();
        }
        private string _AepsWalletBal;

        public string AepsWalletBal
        {
            get => _AepsWalletBal.TrimIfNotNull();
            set => _AepsWalletBal = value.TrimIfNotNull();
        }
        private string _DateTime;

        public string DateTime
        {
            get => _DateTime.TrimIfNotNull();
            set => _DateTime = value.TrimIfNotNull();
        }
        private int _ReferenceID;

        public int ReferenceID
        {
            get => _ReferenceID;
            set => _ReferenceID = value;
        }
        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}
