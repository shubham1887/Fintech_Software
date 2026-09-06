using Project.Processing;
using Project.StaticModel;

namespace Project.Response.CircleWiseCommission
{
    public class ViewModel
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

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _CircleWiseCommissionID;

        public int CircleWiseCommissionID
        {
            get => _CircleWiseCommissionID;
            set => _CircleWiseCommissionID = value;
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
            get => _RetailerCommType.TrimIfNotNullWithDefault(CommissionType.Flexi);
            set => _RetailerCommType = value.TrimIfNotNullWithDefault(CommissionType.Flexi);
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
            get => _DistributorCommType.TrimIfNotNullWithDefault(CommissionType.Flexi);
            set => _DistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Flexi);
        }

        private string _DistributorCommissionCategory;

        public string DistributorCommissionCategory
        {
            get => _DistributorCommissionCategory.TrimIfNotNullWithDefault(CommissionCategory.Commission);
            set => _DistributorCommissionCategory = value.TrimIfNotNullWithDefault(CommissionCategory.Commission);
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
            get => _SuperDistributorCommType.TrimIfNotNullWithDefault(CommissionType.Flexi);
            set => _SuperDistributorCommType = value.TrimIfNotNullWithDefault(CommissionType.Flexi);
        }

        private string _SuperDistributorCommissionCategory;

        public string SuperDistributorCommissionCategory
        {
            get => _SuperDistributorCommissionCategory.TrimIfNotNullWithDefault(CommissionCategory.Commission);
            set => _SuperDistributorCommissionCategory = value.TrimIfNotNullWithDefault(CommissionCategory.Commission);
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
            get => _ClientCommType.TrimIfNotNullWithDefault(CommissionType.Flexi);
            set => _ClientCommType = value.TrimIfNotNullWithDefault(CommissionType.Flexi);
        }

        private string _ClientCommissionCategory;

        public string ClientCommissionCategory
        {
            get => _ClientCommissionCategory.TrimIfNotNull();
            set => _ClientCommissionCategory = value.TrimIfNotNull();
        }

        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        private string _Details;
        public string Details { get => _Details.TrimIfNotNull(); set => _Details = value.TrimIfNotNull(); }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName;
            set => _OperatorName = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }

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
        private int _CircleWiseCommissionID;

        public int CircleWiseCommissionID
        {
            get => _CircleWiseCommissionID;
            set => _CircleWiseCommissionID = value;
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
            get => _CommissionType.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Flexi);
            set => _CommissionType = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Flexi);
        }

        public bool IsFix
        {
            get => CommissionType == "Flexi" ? true : false;
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }
}