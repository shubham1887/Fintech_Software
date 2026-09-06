using Project.Processing;

namespace Project.Response.PublicAPI
{
    public class GetOperatorByMobileNumberResponse
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

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _CircleName;

        public string CircleName
        {
            get => _CircleName.TrimIfNotNull();
            set => _CircleName = value.TrimIfNotNull();
        }
    }
}