using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class PurchaseViewModel
    {
        private string _Month;

        public string Month
        {
            get => _Month.TrimIfNotNull();
            set => _Month = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _CompanyAddress;

        public string CompanyAddress
        {
            get => _CompanyAddress.TrimIfNotNull();
            set => _CompanyAddress = value.TrimIfNotNull();
        }

        private string _CompanyGST;

        public string CompanyGST
        {
            get => _CompanyGST.TrimIfNotNull();
            set => _CompanyGST = value.TrimIfNotNull();
        }

        private string _CompanyPincode;

        public string CompanyPincode
        {
            get => _CompanyPincode.TrimIfNotNull();
            set => _CompanyPincode = value.TrimIfNotNull();
        }

        private decimal _Credit;

        public decimal Credit
        {
            get => _Credit;
            set => _Credit = value;
        }

        private decimal _Debit;

        public decimal Debit
        {
            get => _Debit;
            set => _Debit = value;
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
    }
}