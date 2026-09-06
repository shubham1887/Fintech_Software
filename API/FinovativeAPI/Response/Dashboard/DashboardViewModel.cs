namespace Project.Response.Dashboard
{
    public class DashboardViewModel
    {
        private decimal _TotalSuccessAmount;

        public decimal TotalSuccessAmount
        {
            get => _TotalSuccessAmount;
            set => _TotalSuccessAmount = value;
        }

        private decimal _TotalFailedAmount;

        public decimal TotalFailedAmount
        {
            get => _TotalFailedAmount;
            set => _TotalFailedAmount = value;
        }

        private decimal _TransferAmount;

        public decimal TransferAmount
        {
            get => _TransferAmount;
            set => _TransferAmount = value;
        }

        private int _TransferCount;

        public int TransferCount
        {
            get => _TransferCount;
            set => _TransferCount = value;
        }

        private decimal _ComplaintAmount;

        public decimal ComplaintAmount
        {
            get => _ComplaintAmount;
            set => _ComplaintAmount = value;
        }

        private int _ComplaintCount;

        public int ComplaintCount
        {
            get => _ComplaintCount;
            set => _ComplaintCount = value;
        }

        private int _KycCount;

        public int KycCount
        {
            get => _KycCount;
            set => _KycCount = value;
        }

        private decimal _TotalTarget;

        public decimal TotalTarget
        {
            get => _TotalTarget;
            set => _TotalTarget = value;
        }

        private decimal _RemainsTarget;

        public decimal RemainsTarget
        {
            get => _RemainsTarget;
            set => _RemainsTarget = value;
        }

        private decimal _PrePaidSuccess;

        public decimal PrePaidSuccess
        {
            get => _PrePaidSuccess;
            set => _PrePaidSuccess = value;
        }

        private decimal _PrePaidFail;

        public decimal PrePaidFail
        {
            get => _PrePaidFail;
            set => _PrePaidFail = value;
        }

        private decimal _PostPaidSuccess;

        public decimal PostPaidSuccess
        {
            get => _PostPaidSuccess;
            set => _PostPaidSuccess = value;
        }

        private decimal _PostPaidFail;

        public decimal PostPaidFail
        {
            get => _PostPaidFail;
            set => _PostPaidFail = value;
        }

        private decimal _DthSuccess;

        public decimal DthSuccess
        {
            get => _DthSuccess;
            set => _DthSuccess = value;
        }

        private decimal _DthFail;

        public decimal DthFail
        {
            get => _DthFail;
            set => _DthFail = value;
        }

        private decimal _DMTSuccess;

        public decimal DMTSuccess
        {
            get => _DMTSuccess;
            set => _DMTSuccess = value;
        }

        private decimal _DMTFail;

        public decimal DMTFail
        {
            get => _DMTFail;
            set => _DMTFail = value;
        }

        private decimal _AEPSSuccess;

        public decimal AEPSSuccess
        {
            get => _AEPSSuccess;
            set => _AEPSSuccess = value;
        }

        private decimal _AEPSFail;

        public decimal AEPSFail
        {
            get => _AEPSFail;
            set => _AEPSFail = value;
        }

        private decimal _PurchaseAmount;

        public decimal PurchaseAmount
        {
            get => _PurchaseAmount;
            set => _PurchaseAmount = value;
        }

        private int _PurchaseCount;

        public int PurchaseCount
        {
            get => _PurchaseCount;
            set => _PurchaseCount = value;
        }

        private decimal _TotalCommission;

        public decimal TotalCommission
        {
            get => _TotalCommission;
            set => _TotalCommission = value;
        }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }

        private decimal _TotalAdminCommission;

        public decimal TotalAdminCommission
        {
            get => _TotalAdminCommission;
            set => _TotalAdminCommission = value;
        }

        private decimal _TotalAdminSurcharge;

        public decimal TotalAdminSurcharge
        {
            get => _TotalAdminSurcharge;
            set => _TotalAdminSurcharge = value;
        }

        private decimal _AdminCommissionProfit;

        public decimal AdminCommissionProfit
        {
            get => _AdminCommissionProfit;
            set => _AdminCommissionProfit = value;
        }

        private decimal _AdminSurchargeProfit;

        public decimal AdminSurchargeProfit
        {
            get => _AdminSurchargeProfit;
            set => _AdminSurchargeProfit = value;
        }

        private decimal _RofferComm;

        public decimal RofferComm
        {
            get => _RofferComm;
            set => _RofferComm = value;
        }

        private decimal _RofferTotal;

        public decimal RofferTotal
        {
            get => _RofferTotal;
            set => _RofferTotal = value;
        }

        private string _RofferPer;

        public string RofferPer
        {
            get => _RofferPer;
            set => _RofferPer = value;
        }
    }
}