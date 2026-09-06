using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class UserwiseGatewayViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _PaymentGateway;

        public string PaymentGateway
        {
            get => _PaymentGateway.TrimIfNotNull();
            set => _PaymentGateway = value.TrimIfNotNull();
        }

        private int _UserWisePayementGatewaySettingID;

        public int UserWisePayementGatewaySettingID
        {
            get => _UserWisePayementGatewaySettingID;
            set => _UserWisePayementGatewaySettingID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _PaymentGatewaySettingID;

        public int PaymentGatewaySettingID
        {
            get => _PaymentGatewaySettingID;
            set => _PaymentGatewaySettingID = value;
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
        }
    }
}