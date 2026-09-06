using Project.Processing;

namespace Project.Response.Slab
{
    public class AdjustCommissionViewModel
    {
        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _SlabDetailID;

        public int SlabDetailID
        {
            get => _SlabDetailID;
            set => _SlabDetailID = value;
        }

        private decimal _FromAmount;

        public decimal FromAmount
        {
            get => _FromAmount;
            set => _FromAmount = value;
        }

        private decimal _ToAmount;

        public decimal ToAmount
        {
            get => _ToAmount;
            set => _ToAmount = value;
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        public int ServiceID { get; internal set; }

        private string _Details;
        public string Details { get => _Details.TrimIfNotNull(); set => _Details = value.TrimIfNotNull(); }

        private decimal _RealCommission;

        public decimal RealCommission
        {
            get => _RealCommission;
            set => _RealCommission = value;
        }

        private string _RealCommissionType;

        public string RealCommissionType
        {
            get => _RealCommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _RealCommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private string _GeneralCommissionCategory;

        public string GeneralCommissionCategory
        {
            get => _GeneralCommissionCategory.TrimIfNotNull();
            set => _GeneralCommissionCategory = value.TrimIfNotNull();
        }

        private string _RealCommissionCategory;

        public string RealCommissionCategory
        {
            get => _RealCommissionCategory.TrimIfNotNull();
            set => _RealCommissionCategory = value.TrimIfNotNull();
        }

        public CommissionModel Commissionmodel = new CommissionModel();
        public CommissionWrapper Commissionwrapper = new CommissionWrapper();
    }
}