using Project.Processing;
using System;

namespace Project.Response.PaymentGateway
{
    public class PaymentGatewayPendingViewModel
    {
        private int _PaymentGatewayID;

        public int PaymentGatewayID
        {
            get => _PaymentGatewayID;
            set => _PaymentGatewayID = value;
        }

        private int _PaymentGatewaySettingID;

        public int PaymentGatewaySettingID
        {
            get => _PaymentGatewaySettingID;
            set => _PaymentGatewaySettingID = value;
        }

        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private DateTime _DOC;

        internal DateTime DOC
        {
            get => _DOC;
            set => _DOC = value;
        }
    }
}