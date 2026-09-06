using Project.Processing;

namespace Project.Response.ProviderConfig
{
    public class ProviderConfigurationSampleView
    {
        private int _ConfigurationSampleID;

        public int ConfigurationSampleID
        {
            get => _ConfigurationSampleID;
            set => _ConfigurationSampleID = value;
        }

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

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
        }
    }
}