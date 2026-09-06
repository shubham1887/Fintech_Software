using Project.Processing;

namespace Project.Response.VirtualBalance
{
    public class ViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private int _TransactionID;

        public int TransactionID
        {
            get => _TransactionID;
            set => _TransactionID = value;
        }

        private int _VirtualBalanceID;

        public int VirtualBalanceID
        {
            get => _VirtualBalanceID;
            set => _VirtualBalanceID = value;
        }

        private string _DOCType;

        public string DOCType
        {
            get => _DOCType.TrimIfNotNull();
            set => _DOCType = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }
    }
}