namespace Project.Response.Tax
{
    public class GSTDetailReportViewModel
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo;
            set => _GSTNo = value;
        }

        private string _InvoiceNo;

        public string InvoiceNo
        {
            get => _InvoiceNo;
            set => _InvoiceNo = value;
        }

        private string _State;

        public string State
        {
            get => _State;
            set => _State = value;
        }

        private string _InvoiceMonth;

        public string InvoiceMonth
        {
            get => _InvoiceMonth;
            set => _InvoiceMonth = value;
        }

        private string _GSTNumber;

        public string GSTNumber
        {
            get => _GSTNumber;
            set => _GSTNumber = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName;
            set => _FirmName = value;
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName;
            set => _StateName = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName;
            set => _UserName = value;
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType;
            set => _UserType = value;
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory;
            set => _ServiceCategory = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _SuccessAmt;

        public decimal SuccessAmt
        {
            get => _SuccessAmt;
            set => _SuccessAmt = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }
    }
}