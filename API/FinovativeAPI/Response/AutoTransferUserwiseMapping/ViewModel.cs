using Project.Processing;

namespace Project.Response.AutoTransferUserwiseMapping
{
    public class ViewModel
    {
        private int _AutoTransferUserwiseMappingID;

        public int AutoTransferUserwiseMappingID
        {
            get => _AutoTransferUserwiseMappingID;
            set => _AutoTransferUserwiseMappingID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private decimal _DMTAmountIncreasedBy;

        public decimal DMTAmountIncreasedBy
        {
            get => _DMTAmountIncreasedBy;
            set => _DMTAmountIncreasedBy = value;
        }

        private decimal _DMTLowBalanceReached;

        public decimal DMTLowBalanceReached
        {
            get => _DMTLowBalanceReached;
            set => _DMTLowBalanceReached = value;
        }

        private decimal _RechargeAmountIncreasedBy;

        public decimal RechargeAmountIncreasedBy
        {
            get => _RechargeAmountIncreasedBy;
            set => _RechargeAmountIncreasedBy = value;
        }

        private decimal _RechargeLowBalanceReached;

        public decimal RechargeLowBalanceReached
        {
            get => _RechargeLowBalanceReached;
            set => _RechargeLowBalanceReached = value;
        }

        private decimal _MaxOutstanding;

        public decimal MaxOutstanding
        {
            get => _MaxOutstanding;
            set => _MaxOutstanding = value;
        }

        public bool IsOTPVerified;

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
    }
}