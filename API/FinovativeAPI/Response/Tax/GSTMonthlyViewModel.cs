using Project.Processing;

namespace Project.Response.Tax
{
    public class GSTMonthlyViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }

        private decimal _DistributerCommission;

        public decimal DistributerCommission
        {
            get => _DistributerCommission;
            set => _DistributerCommission = value;
        }

        private decimal _SuperDistributerCommission;

        public decimal SuperDistributerCommission
        {
            get => _SuperDistributerCommission;
            set => _SuperDistributerCommission = value;
        }

        private decimal _AdminGST;

        public decimal AdminGST
        {
            get => _AdminGST;
            set => _AdminGST = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _SuperDistributerUserID;

        public int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private int _DistributerUserID;

        public int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private int _UserTypeIDBoth;

        public int UserTypeIDBoth
        {
            get => _UserTypeIDBoth;
            set => _UserTypeIDBoth = value;
        }

        private int _UserIDForSDDS;

        public int UserIDForSDDS
        {
            get => _UserIDForSDDS;
            set => _UserIDForSDDS = value;
        }

        private decimal _RetailerGST;

        public decimal RetailerGST
        {
            get => _RetailerGST;
            set => _RetailerGST = value;
        }

        private decimal _DistributerGST;

        public decimal DistributerGST
        {
            get => _DistributerGST;
            set => _DistributerGST = value;
        }

        private decimal _SuperDistributerGST;

        public decimal SuperDistributerGST
        {
            get => _SuperDistributerGST;
            set => _SuperDistributerGST = value;
        }

        private decimal _CashBack;
        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }
        private bool _IsCCF;
        public bool IsCCF
        {
            get => _IsCCF;
            set => _IsCCF = value;
        }
    }
}