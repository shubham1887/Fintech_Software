using Project.Processing;

namespace Project.Response.Wallet
{
    public class GetBalanceViewModel
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
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

        private float _Outstanding;

        public float Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }
    }
}