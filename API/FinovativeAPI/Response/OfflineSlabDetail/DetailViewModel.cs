using Project.Processing;
using Project.StaticModel;

namespace Project.Response.OfflineSlabDetail
{
    public class DetailViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _OfflineSlabDetailID;

        public int OfflineSlabDetailID
        {
            get => _OfflineSlabDetailID;
            set => _OfflineSlabDetailID = value;
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
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

        private decimal _RetailerComm;

        public decimal RetailerComm
        {
            get => _RetailerComm;
            set => _RetailerComm = value;
        }

        private string _RetailerCommType;

        public string RetailerCommType
        {
            get => _RetailerCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RetailerCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RetailerCommissionCategory;

        public string RetailerCommissionCategory
        {
            get => _RetailerCommissionCategory.TrimIfNotNull();
            set => _RetailerCommissionCategory = value.TrimIfNotNull();
        }

        private decimal _DistributorComm;

        public decimal DistributorComm
        {
            get => _DistributorComm;
            set => _DistributorComm = value;
        }

        private string _DistributorCommType;

        public string DistributorCommType
        {
            get => _DistributorCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _DistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _DistributorCommissionCategory;

        public string DistributorCommissionCategory
        {
            get => _DistributorCommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _DistributorCommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private decimal _SuperDistributorComm;

        public decimal SuperDistributorComm
        {
            get => _SuperDistributorComm;
            set => _SuperDistributorComm = value;
        }

        private string _SuperDistributorCommType;

        public string SuperDistributorCommType
        {
            get => _SuperDistributorCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _SuperDistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _SuperDistributorCommissionCategory;

        public string SuperDistributorCommissionCategory
        {
            get => _SuperDistributorCommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _SuperDistributorCommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private decimal _ClientComm;

        public decimal ClientComm
        {
            get => _ClientComm;
            set => _ClientComm = value;
        }

        private string _ClientCommType;

        public string ClientCommType
        {
            get => _ClientCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _ClientCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _ClientCommissionCategory;

        public string ClientCommissionCategory
        {
            get => _ClientCommissionCategory.TrimIfNotNull();
            set => _ClientCommissionCategory = value.TrimIfNotNull();
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

        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
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

        private string _PackageName;

        public string PackageName
        {
            get => _PackageName.TrimIfNotNull();
            set => _PackageName = value.TrimIfNotNull();
        }

        private int _OfflineProductMasterID;

        public int OfflineProductMasterID
        {
            get => _OfflineProductMasterID;
            set => _OfflineProductMasterID = value;
        }

        public int ServiceID { get; internal set; }

        private string _Details;
        public string Details { get => _Details.TrimIfNotNull(); set => _Details = value.TrimIfNotNull(); }

        public CommissionModel GeneralRetailer = new CommissionModel();

        public CommissionModel GeneralDistributer = new CommissionModel();
        public CommissionModel GeneralSuperDistributer = new CommissionModel();
        public CommissionModel GeneralClient = new CommissionModel();

        public CommissionWrapper Retailer = new CommissionWrapper();
        public CommissionWrapper Distributer = new CommissionWrapper();
        public CommissionWrapper SuperDistributer = new CommissionWrapper();
        public CommissionWrapper Client = new CommissionWrapper();
    }

    public class CommissionWrapper
    {
        public CommissionModel General = new CommissionModel();
    }

    public class CommissionModel
    {
        private int _OfflineSlabDetailID;

        public int OfflineSlabDetailID
        {
            get => _OfflineSlabDetailID;
            set => _OfflineSlabDetailID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
        }

        public bool IsFix
        {
            get => CommissionType == "Fix" ? true : false;
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }
}