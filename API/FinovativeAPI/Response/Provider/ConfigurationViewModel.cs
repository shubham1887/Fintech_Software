using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Provider
{
    public class ConfigurationViewModel
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Lapu;

        public string Lapu
        {
            get => _Lapu.TrimIfNotNull();
            set => _Lapu = value.TrimIfNotNull();
        }

        private decimal _AdminCommi;

        public decimal AdminCommi
        {
            get => _AdminCommi;
            set => _AdminCommi = value;
        }

        private bool _IsAllowed;

        public bool IsAllowed
        {
            get => _IsAllowed;
            set => _IsAllowed = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Quantity;

        public string Quantity
        {
            get => _Quantity.TrimIfNotNull();
            set => _Quantity = value.TrimIfNotNull();
        }

        private int _ConfigurationID;

        public int ConfigurationID
        {
            get => _ConfigurationID;
            set => _ConfigurationID = value;
        }

        public string AdminCommType
        {
            get => _AdminCommType.TrimIfNotNull();
            set => _AdminCommType = value.TrimIfNotNull();
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }

        private int _PendingCount;

        public int PendingCount
        {
            get => _PendingCount;
            set => _PendingCount = value;
        }

        private int _PendingInMinute;

        public int PendingInMinute
        {
            get => _PendingInMinute;
            set => _PendingInMinute = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }

        private int _FailInMinute;

        public int FailInMinute
        {
            get => _FailInMinute;
            set => _FailInMinute = value;
        }

        private string _AdminCommType;

        public List<int> CircleIds = new List<int>();

        public CommissionModel Commissionmodel = new CommissionModel();
        public CommissionWrapper Commissionwrapper = new CommissionWrapper();
    }

    public class CommissionWrapper
    {
        public CommissionModel General = new CommissionModel();
    }

    public class CommissionModel
    {
        private int _ConfigurationID;

        public int ConfigurationID
        {
            get => _ConfigurationID;
            set => _ConfigurationID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        public bool IsFix
        {
            get => CommissionType == "Fix" ? true : false;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }
}