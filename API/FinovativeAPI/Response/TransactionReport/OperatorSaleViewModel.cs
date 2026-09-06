using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class OperatorSaleViewModel
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

        private decimal _SuccessAmount;

        public decimal SuccessAmount
        {
            get => _SuccessAmount;
            set => _SuccessAmount = value;
        }

        private decimal _SuccessCount;

        public decimal SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
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