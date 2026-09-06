using Project.Processing;

namespace Project.Response.UserCreationMaster
{
    public class ViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private int _OpeningIdCount;

        public int OpeningIdCount
        {
            get => _OpeningIdCount;
            set => _OpeningIdCount = value;
        }

        private int _PurchaseCount;

        public int PurchaseCount
        {
            get => _PurchaseCount;
            set => _PurchaseCount = value;
        }

        private int _ClosingIdCount;

        public int ClosingIdCount
        {
            get => _ClosingIdCount;
            set => _ClosingIdCount = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        public int UserCreationMasterID { get; internal set; }
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        //private int _CreatedUserID;
        //public int CreatedUserID
        //{
        //    get => _CreatedUserID;
        //    set => _CreatedUserID = value;
        //}
    }
}