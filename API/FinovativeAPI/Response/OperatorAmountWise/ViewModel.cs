using Project.Processing;

namespace Project.Response.OperatorAmountWise
{
    public class ViewModel
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _SerialCount;

        public int SerialCount
        {
            get => _SerialCount;
            set => _SerialCount = value;
        }

        private int _OperatorAmountWiseSettingID;

        public int OperatorAmountWiseSettingID
        {
            get => _OperatorAmountWiseSettingID;
            set => _OperatorAmountWiseSettingID = value;
        }
    }
}