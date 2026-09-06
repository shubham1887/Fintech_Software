using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class SelectByIDResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _DateStr;

        public string DateStr
        {
            get => _DateStr.TrimIfNotNull();
            set => _DateStr = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}