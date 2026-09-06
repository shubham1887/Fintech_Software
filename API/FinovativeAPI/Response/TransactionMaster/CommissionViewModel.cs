using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class CommissionViewModel
    {
        private string _DistributorName;

        public string DistributorName
        {
            get => _DistributorName.TrimIfNotNull();
            set => _DistributorName = value.TrimIfNotNull();
        }

        private string _SuperDistributorName;

        public string SuperDistributorName
        {
            get => _SuperDistributorName.TrimIfNotNull();
            set => _SuperDistributorName = value.TrimIfNotNull();
        }

        private decimal _DistributorCommi;

        public decimal DistributorCommission
        {
            get => _DistributorCommi;
            set => _DistributorCommi = value;
        }

        private decimal _SuperDistributorCommi;

        public decimal SuperDistributorCommission
        {
            get => _SuperDistributorCommi;
            set => _SuperDistributorCommi = value;
        }

        private int _DistributorID;

        internal int DistributorID
        {
            get => _DistributorID;
            set => _DistributorID = value;
        }

        private int _SuperDistributorID;

        internal int SuperDistributorID
        {
            get => _SuperDistributorID;
            set => _SuperDistributorID = value;
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType;
            set => _UserType = value;
        }
    }
}