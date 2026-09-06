using MplanCore.Controller;

namespace Project.Response.CirleWiseOperatorLimit
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
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

        private int _CircleWiseOperatorLimitID;

        public int CircleWiseOperatorLimitID
        {
            get => _CircleWiseOperatorLimitID;
            set => _CircleWiseOperatorLimitID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName;
            set => _StateName = value;
        }

        private decimal _AmountLimit;

        public decimal AmountLimit
        {
            get => _AmountLimit;
            set => _AmountLimit = value;
        }

        private decimal _RemainingLimit;

        public decimal RemainingLimit
        {
            get => _RemainingLimit;
            set => _RemainingLimit = value;
        }
    }
}