using Project.Processing;

namespace Project.Response.User
{
    public class DownlineWithBalanceResponse
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.ToTitleCase();
            set => _MobileNumber = value.ToTitleCase();
        }

        private string _Balances;

        public string Balances
        {
            get => _Balances.TrimIfNotNull();
            set => _Balances = value.TrimIfNotNull();
        }

        private float _Outstanding;

        public float Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private float _RechargeBalance;

        public float RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private float _DMTBalance;

        public float DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private float _AEPSBalance;

        public float AEPSBalance
        {
            get => _AEPSBalance;
            set => _AEPSBalance = value;
        }
    }
}