using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class SelectByOperatorViewModel
    {
        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private decimal _ReatilerComm;

        public decimal ReatilerComm
        {
            get => _ReatilerComm;
            set => _ReatilerComm = value;
        }

        private decimal _DistributerComm;

        public decimal DistributerComm
        {
            get => _DistributerComm;
            set => _DistributerComm = value;
        }

        private decimal _SuperDistributerComm;

        public decimal SuperDistributerComm
        {
            get => _SuperDistributerComm;
            set => _SuperDistributerComm = value;
        }

        private string _RetailerCommPercentage;

        public string RetailerCommPercentage
        {
            get => _RetailerCommPercentage.TrimIfNotNull();
            set => _RetailerCommPercentage = value.TrimIfNotNull();
        }

        private string _DistributerCommPercentage;

        public string DistributerCommPercentage
        {
            get => _DistributerCommPercentage.TrimIfNotNull();
            set => _DistributerCommPercentage = value.TrimIfNotNull();
        }

        private string _SuperDistributerCommPercentage;

        public string SuperDistributerCommPercentage
        {
            get => _SuperDistributerCommPercentage.TrimIfNotNull();
            set => _SuperDistributerCommPercentage = value.TrimIfNotNull();
        }
    }
}
