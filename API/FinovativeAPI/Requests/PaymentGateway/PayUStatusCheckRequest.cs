using Project.Processing;

namespace Project.Requests.PaymentGateway
{
    public class PayUStatusCheckRequest
    {
        private string _key;

        public string key
        {
            get => _key.TrimIfNotNull();
            set => _key = value.TrimIfNotNull();
        }
        private string _command;
        public string command
        {
            get => _command.TrimIfNotNull();
            set => _command = value.TrimIfNotNull();
        }

        private string _var1;
        public string var1
        {
            get => _var1.TrimIfNotNull();
            set => _var1 = value.TrimIfNotNull();
        }
        private string _hash;
        public string hash
        {
            get => _hash.TrimIfNotNull();
            set => _hash = value.TrimIfNotNull();
        }
    }
}
