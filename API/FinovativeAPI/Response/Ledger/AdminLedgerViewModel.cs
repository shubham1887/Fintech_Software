using Project.Processing;

namespace Project.Response.Ledger
{
    public class AdminLedgerViewModel
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

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _OpeningBalance;

        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
        }

        private decimal _CreditAmount;

        public decimal CreditAmount
        {
            get => _CreditAmount;
            set => _CreditAmount = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get { return _ClosingBalance; }
            set { _ClosingBalance = value; }
        }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }
    }
}