using Project.Processing;

namespace Project.Response.LapuTransactionDetail
{
    public class PurchaseViewModel
    {
        private int _LapuTransactionDetailID;

        public int LapuTransactionDetailID
        {
            get => _LapuTransactionDetailID;
            set => _LapuTransactionDetailID = value;
        }

        private int _LapuPartyMasterID;

        public int LapuPartyMasterID
        {
            get => _LapuPartyMasterID;
            set => _LapuPartyMasterID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _PartyName;

        public string PartyName
        {
            get => _PartyName.TrimIfNotNull();
            set => _PartyName = value.TrimIfNotNull();
        }

        private string _PurchaseDate;

        public string PurchaseDate
        {
            get => _PurchaseDate.TrimIfNotNull();
            set => _PurchaseDate = value.TrimIfNotNull();
        }
    }
}