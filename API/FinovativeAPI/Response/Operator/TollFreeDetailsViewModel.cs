using Project.Processing;

namespace Project.Response.Operator
{
    public class TollFreeDetailsViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _TollFreeDetails;

        public string TollFreeDetails
        {
            get { return _TollFreeDetails.TrimIfNotNull(); }
            set { _TollFreeDetails = value.TrimIfNotNull(); }
        }

        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }
    }
}