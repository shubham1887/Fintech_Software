using Project.Processing;

namespace Project.Response.RangeWiseSequence
{
    public class ViewModel
    {
        private int _RangeWiseSequenceID;

        public int RangeWiseSequenceID
        {
            get => _RangeWiseSequenceID;
            set => _RangeWiseSequenceID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _FromAmount;

        public decimal FromAmount
        {
            get => _FromAmount;
            set => _FromAmount = value;
        }

        private decimal _ToAmount;

        public decimal ToAmount
        {
            get => _ToAmount;
            set => _ToAmount = value;
        }
    }
}