using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class AEPSInitResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private bool _IsServiceActive;

        public bool IsServiceActive
        {
            get => _IsServiceActive;
            set => _IsServiceActive = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UniqueRefNo;

        public string UniqueRefNo
        {
            get => _UniqueRefNo.TrimIfNotNull();
            set => _UniqueRefNo = value.TrimIfNotNull();
        }

        private string _RequestId;

        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _UDD;

        public string UDD
        {
            get => _UDD.TrimIfNotNull();
            set => _UDD = value.TrimIfNotNull();
        }
    }
}