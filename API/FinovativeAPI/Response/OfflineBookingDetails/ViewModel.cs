using Project.Processing;

namespace Project.Response.OfflineBookingDetails
{
    public class ViewModel
    {
        private int _OfflineProductMasterID;

        public int OfflineProductMasterID

        {
            get => _OfflineProductMasterID;
            set => _OfflineProductMasterID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _PackageName;

        public string PackageName
        {
            get => _PackageName.TrimIfNotNull();
            set => _PackageName = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private int _OfflineBookingDetailsID;

        public int OfflineBookingDetailsID
        {
            get => _OfflineBookingDetailsID;
            set => _OfflineBookingDetailsID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}