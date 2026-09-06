using Project.Processing;

namespace Project.Requests.Tax
{
    public class TransactionInfo
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }
        
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        
        private int _DistributerUserID;

        public int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }
         
        private int _SuperDistributerUserID;

        public int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }


        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }
        
        private decimal _RetailerTDS;

        public decimal RetailerTDS
        {
            get => _RetailerTDS;
            set => _RetailerTDS = value;
        }
        
        private decimal _DistributerTDS;

        public decimal DistributerTDS
        {
            get => _DistributerTDS;
            set => _DistributerTDS = value;
        }

        private decimal _SuperDistributerTDS;

        public decimal SuperDistributerTDS
        {
            get => _SuperDistributerTDS;
            set => _SuperDistributerTDS = value;
        }
        
        private decimal _CashBack;

        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
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
     
    }
}


