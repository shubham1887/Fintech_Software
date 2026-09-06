using Project.Processing;
using Project.StaticModel;

namespace Project.Response.ProviderConfigurationAdminCommission
{
    public class DetailViewModel
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private int _ProviderConfigurationAdminCommissionID;

        public int ProviderConfigurationAdminCommissionID
        {
            get => _ProviderConfigurationAdminCommissionID;
            set => _ProviderConfigurationAdminCommissionID = value;
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

        private decimal _AdminComm;

        public decimal AdminComm
        {
            get => _AdminComm;
            set => _AdminComm = value;
        }

        private string _AdminCommType;

        public string AdminCommType
        {
            get => _AdminCommType.TrimIfNotNullWithDefault(CommissionType.Fix);
            set => _AdminCommType = value.TrimIfNotNullWithDefault(CommissionType.Fix);
        }

        private string _AdminCommissionCategory;

        public string AdminCommissionCategory
        {
            get => _AdminCommissionCategory.TrimIfNotNull();
            set => _AdminCommissionCategory = value.TrimIfNotNull();
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

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        public int ServiceID { get; internal set; }

        private string _Details;
        public string Details { get => _Details.TrimIfNotNull(); set => _Details = value.TrimIfNotNull(); }

        public CommissionModel GeneralAdmin = new CommissionModel();
        public CommissionWrapper Admin = new CommissionWrapper();
    }

    public class CommissionWrapper
    {
        public CommissionModel General = new CommissionModel();
    }

    public class CommissionModel
    {
        private int _ProviderConfigurationAdminCommissionID;

        public int ProviderConfigurationAdminCommissionID
        {
            get => _ProviderConfigurationAdminCommissionID;
            set => _ProviderConfigurationAdminCommissionID = value;
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