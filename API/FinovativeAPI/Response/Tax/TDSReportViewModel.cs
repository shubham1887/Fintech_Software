using Project.Processing;

namespace Project.Response.Tax
{
    public class TDSReportViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private decimal _SuccessAmt;

        public decimal SuccessAmt
        {
            get => _SuccessAmt;
            set => _SuccessAmt = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _TDS;

        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }
    }
}