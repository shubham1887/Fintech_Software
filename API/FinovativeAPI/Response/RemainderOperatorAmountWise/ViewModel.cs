using Project.Processing;

namespace Project.Response.RemainderOperatorAmountWise
{
    public class ViewModel
    {
        private int _RemainderOperatorAmountWiseID;

        public int RemainderOperatorAmountWiseID
        {
            get => _RemainderOperatorAmountWiseID;
            set => _RemainderOperatorAmountWiseID = value;
        }

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

        private int _Days;

        public int Days
        {
            get => _Days;
            set => _Days = value;
        }
    }
}