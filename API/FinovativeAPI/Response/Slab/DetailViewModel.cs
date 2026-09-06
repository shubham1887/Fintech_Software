using Project.Processing;
using Project.StaticModel;

namespace Project.Response.Slab
{
    public class DetailViewModel
    {
        private string _Details;
        public string Details { get => _Details.TrimIfNotNull(); set => _Details = value.TrimIfNotNull(); }

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

        private bool _IsROffer;

        internal bool IsROffer
        {
            get => _IsROffer;
            set => _IsROffer = value;
        }

        private int _SlabDetailID;

        public int SlabDetailID
        {
            get => _SlabDetailID;
            set => _SlabDetailID = value;
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

        private string _StrRetailerComm;

        public string StrRetailerComm
        {
            get => _StrRetailerComm;
            set => _StrRetailerComm = value;
        }

        private string _StrDistributorComm;

        public string StrDistributorComm
        {
            get => _StrDistributorComm;
            set => _StrDistributorComm = value;
        }

        private string _StrSuperDistributorComm;

        public string StrSuperDistributorComm
        {
            get => _StrSuperDistributorComm;
            set => _StrSuperDistributorComm = value;
        }

        private string _StrClientComm;

        public string StrClientComm
        {
            get => _StrClientComm;
            set => _StrClientComm = value;
        }

        private decimal _RealRetailerComm;

        public decimal RealRetailerComm
        {
            get => _RealRetailerComm;
            set => _RealRetailerComm = value;
        }

        private string _RealRetailerCommType;

        public string RealRetailerCommType
        {
            get => _RealRetailerCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RealRetailerCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RealRetailerCommissionCategory;

        public string RealRetailerCommissionCategory
        {
            get => _RealRetailerCommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _RealRetailerCommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private decimal _RealDistributorComm;

        public decimal RealDistributorComm
        {
            get => _RealDistributorComm;
            set => _RealDistributorComm = value;
        }

        private string _RealDistributorCommType;

        public string RealDistributorCommType
        {
            get => _RealDistributorCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RealDistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RealDistributorCommissionCategory;

        public string RealDistributorCommissionCategory
        {
            get => _RealDistributorCommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _RealDistributorCommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private decimal _RealSuperDistributorComm;

        public decimal RealSuperDistributorComm
        {
            get => _RealSuperDistributorComm;
            set => _RealSuperDistributorComm = value;
        }

        private string _RealSuperDistributorCommType;

        public string RealSuperDistributorCommType
        {
            get => _RealSuperDistributorCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RealSuperDistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RealSuperDistributorCommissionCategory;

        public string RealSuperDistributorCommissionCategory
        {
            get => _RealSuperDistributorCommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _RealSuperDistributorCommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private decimal _RealClientComm;

        public decimal RealClientComm
        {
            get => _RealClientComm;
            set => _RealClientComm = value;
        }

        private string _RealClientCommType;

        public string RealClientCommType
        {
            get => _RealClientCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _RealClientCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _RealClientCommissionCategory;

        public string RealClientCommissionCategory
        {
            get => _RealClientCommissionCategory.TrimIfNotNull();
            set => _RealClientCommissionCategory = value.TrimIfNotNull();
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
        public CommissionModel GeneralRetailer = new CommissionModel();

        public CommissionModel GeneralDistributer = new CommissionModel();
        public CommissionModel GeneralSuperDistributer = new CommissionModel();
        public CommissionModel GeneralClient = new CommissionModel();

        public CommissionModel RealRetailer = new CommissionModel();

        public CommissionModel RealDistributer = new CommissionModel();
        public CommissionModel RealSuperDistributer = new CommissionModel();
        public CommissionModel RealClient = new CommissionModel();

        public CommissionWrapper Retailer = new CommissionWrapper();
        public CommissionWrapper Distributer = new CommissionWrapper();
        public CommissionWrapper SuperDistributer = new CommissionWrapper();
        public CommissionWrapper Client = new CommissionWrapper();
    }

    public class CommissionWrapper
    {
        public CommissionModel General = new CommissionModel();
        public CommissionModel Real = new CommissionModel();
    }

    public class CommissionModel
    {
        private int _SlabDetailID;

        public int SlabDetailID
        {
            get => _SlabDetailID;
            set => _SlabDetailID = value;
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