using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class InitiateResponse
    {
        private string _LogoUrl;

        public string LogoURL
        {
            get => _LogoUrl.TrimIfNotNull();
            set => _LogoUrl = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Hash;

        public string Hash
        {
            get => _Hash.TrimIfNotNull();
            set => _Hash = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Currency;

        public string Currency
        {
            get => _Currency.TrimIfNotNull();
            set => _Currency = value.TrimIfNotNull();
        }

        private string _RedirectURL;

        public string RedirectURL
        {
            get => _RedirectURL.TrimIfNotNull();
            set => _RedirectURL = value.TrimIfNotNull();
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _FailURL;

        public string FailURL
        {
            get => _FailURL.TrimIfNotNull();
            set => _FailURL = value.TrimIfNotNull();
        }

        private string _ActionURL;

        public string ActionURL
        {
            get => _ActionURL.TrimIfNotNull();
            set => _ActionURL = value.TrimIfNotNull();
        }

        private string _SuccessURL;

        public string SuccessURL
        {
            get => _SuccessURL.TrimIfNotNull();
            set => _SuccessURL = value.TrimIfNotNull();
        }

        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _ContactNumber;

        public string ContactNumber
        {
            get => _ContactNumber.TrimIfNotNull();
            set => _ContactNumber = value.TrimIfNotNull();
        }

        private string _PaymentGatewayName;

        public string PaymentGatewayName
        {
            get => _PaymentGatewayName.TrimIfNotNull();
            set => _PaymentGatewayName = value.TrimIfNotNull();
        }

        private string _ChannelID;

        public string ChannelID
        {
            get => _ChannelID.TrimIfNotNull();
            set => _ChannelID = value.TrimIfNotNull();
        }

        private string _IndustryTypeID;

        public string IndustryTypeID
        {
            get => _IndustryTypeID.TrimIfNotNull();
            set => _IndustryTypeID = value.TrimIfNotNull();
        }

        private string _Website;

        public string Website
        {
            get => _Website.TrimIfNotNull();
            set => _Website = value.TrimIfNotNull();
        }

        private string _CustID;

        public string CustID
        {
            get => _CustID.TrimIfNotNull();
            set => _CustID = value.TrimIfNotNull();
        }

        private string _TxnToken;

        public string TxnToken
        {
            get => _TxnToken.TrimIfNotNull();
            set => _TxnToken = value.TrimIfNotNull();
        }

        private string _SenderVPA;

        public string SenderVPA
        {
            get => _SenderVPA.TrimIfNotNull();
            set => _SenderVPA = value.TrimIfNotNull();
        }
        private string _payment_session_id;

        public string payment_session_id
        {
            get => _payment_session_id.TrimIfNotNull();
            set => _payment_session_id  = value.TrimIfNotNull();
        }
    }
}
