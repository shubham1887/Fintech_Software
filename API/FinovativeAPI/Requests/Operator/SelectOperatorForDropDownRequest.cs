using Project.Processing;

namespace Project.Requests.Operator
{
    public class SelectOperatorForDropDownRequest
    {
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }
    }
}