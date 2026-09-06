using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class OperatorViewModel
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

        private string _Circle;

        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
        }
    }
}