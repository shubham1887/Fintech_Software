using Project.Processing;

namespace Project.Response.UserwiseSaleTarget
{
    public class ViewModel
    {
        private int _UserwiseSaleTargetID;

        public int UserwiseSaleTargetID
        {
            get => _UserwiseSaleTargetID;
            set => _UserwiseSaleTargetID = value;
        }

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

        private int _ParnetID;

        public int ParnetID
        {
            get => _ParnetID;
            set => _ParnetID = value;
        }

        private decimal _TargetAmount;

        public decimal TargetAmount
        {
            get => _TargetAmount;
            set => _TargetAmount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }
    }
}