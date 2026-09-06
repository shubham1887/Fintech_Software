using Project.Processing;

namespace Project.Response.ProviderOperatorDenominationMapping
{
    public class ViewModel
    {
        private int _ProviderOperatorDenominationMappingID;

        public int ProviderOperatorDenominationMappingID
        {
            get => _ProviderOperatorDenominationMappingID;
            set => _ProviderOperatorDenominationMappingID = value;
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _Provider;

        public string Provider
        {
            get => _Provider.TrimIfNotNull();
            set => _Provider = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _DenominationMasterID;

        public int DenominationMasterID
        {
            get => _DenominationMasterID;
            set => _DenominationMasterID = value;
        }

        private string _DenominationRange;

        public string DenominationRange
        {
            get => _DenominationRange.TrimIfNotNull();
            set => _DenominationRange = value.TrimIfNotNull();
        }

        private bool _IsStop;

        public bool IsStop { get => _IsStop; set => _IsStop = value; }

        public string IsStopStr
        {
            get => (IsStop) ? "Stop" : "Allowed";
        }
    }
}