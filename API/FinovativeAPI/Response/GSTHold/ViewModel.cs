using Project.Processing;

namespace Project.Response.GSTHold
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _GSTHoldID;

        public int GSTHoldID
        {
            get => _GSTHoldID;
            set => _GSTHoldID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        private decimal _CGST;

        public decimal CGST
        {
            get => _CGST;
            set => _CGST = value;
        }

        private decimal _IGST;

        public decimal IGST
        {
            get => _IGST;
            set => _IGST = value;
        }

        private decimal _SGST;

        public decimal SGST
        {
            get => _SGST;
            set => _SGST = value;
        }

        private decimal _TaxableValue;

        public decimal TaxableValue
        {
            get => _TaxableValue;
            set => _TaxableValue = value;
        }

        private decimal _HoldAmount;

        public decimal HoldAmount
        {
            get => _HoldAmount;
            set => _HoldAmount = value;
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _GSTUploadedDate;

        public string GSTUploadedDate
        {
            get => _GSTUploadedDate.TrimIfNotNull();
            set => _GSTUploadedDate = value.TrimIfNotNull();
        }

        private string _GSTUploadRemark;

        public string GSTUploadRemark
        {
            get => _GSTUploadRemark.TrimIfNotNull();
            set => _GSTUploadRemark = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _LastYearMonth;

        public string LastYearMonth
        {
            get => _LastYearMonth.TrimIfNotNull();
            set => _LastYearMonth = value.TrimIfNotNull();
        }

        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private int _Year;

        public int Year
        {
            get => _Year;
            set => _Year = value;
        }
    }
}