using Project.Processing;

namespace Project.Response.ProviderConfig
{
    public class SampleConfigurationViewModel
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Provider;

        public string Provider
        {
            get => _Provider.TrimIfNotNull();
            set => _Provider = value.TrimIfNotNull();
        }

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
        }
    }
}