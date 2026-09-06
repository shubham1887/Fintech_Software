using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class WorldLineStatusCheckRequest
    {
        public Merchant merchant { get; set; } = new Merchant();
        public Transaction transaction { get; set; } = new Transaction();
    }

    public class Merchant
    {
        private string _identifier;

        public string identifier
        {
            get => _identifier.TrimIfNotNull();
            set => _identifier = value.TrimIfNotNull();
        }
    }

    public class Transaction
    {
        private string _deviceIdentifier;

        public string deviceIdentifier
        {
            get => _deviceIdentifier.TrimIfNotNullWithDefault("S");
            set => _deviceIdentifier = value.TrimIfNotNullWithDefault("S");
        }

        private string _currency;

        public string currency
        {
            get => _currency.TrimIfNotNullWithDefault("INR");
            set => _currency = value.TrimIfNotNullWithDefault("INR");
        }

        private string _dateTime;

        public string dateTime
        {
            get => _dateTime.TrimIfNotNull();
            set => _dateTime = value.TrimIfNotNull();
        }

        private string _token;

        public string token
        {
            get => _token.TrimIfNotNull();
            set => _token = value.TrimIfNotNull();
        }

        private string _requestType;

        public string requestType
        {
            get => _requestType.TrimIfNotNullWithDefault("S");
            set => _requestType = value.TrimIfNotNullWithDefault("S");
        }
    }
}