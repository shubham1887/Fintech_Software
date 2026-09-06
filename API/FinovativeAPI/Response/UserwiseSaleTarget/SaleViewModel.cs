using Project.Processing;

namespace Project.Response.UserwiseSaleTarget
{
    public class SaleViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Wallet;

        public string Wallet
        {
            get => _Wallet.TrimIfNotNull();
            set => _Wallet = value.TrimIfNotNull();
        }

        private decimal _TargetAmount;

        public decimal TargetAmount
        {
            get => _TargetAmount;
            set => _TargetAmount = value;
        }

        private decimal _SaleAmount;

        public decimal SaleAmount
        {
            get => _SaleAmount;
            set => _SaleAmount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _Difference;

        public decimal Difference
        {
            get => _Difference;
            set => _Difference = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private string _SaleMonthDOC;

        public string SaleMonthDOC
        {
            get => _SaleMonthDOC.TrimIfNotNull();
            set => _SaleMonthDOC = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}