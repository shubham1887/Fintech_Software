using Project.Processing;

namespace Project.Response.CommissionHold
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private decimal _CommissionHoldAmount;

        public decimal CommissionHoldAmount
        {
            get => _CommissionHoldAmount;
            set => _CommissionHoldAmount = value;
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

        private decimal _TotalSuccessAmount;

        public decimal TotalSuccessAmount
        {
            get => _TotalSuccessAmount;
            set => _TotalSuccessAmount = value;
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }
    }
}