using Project.Processing;
using System;

namespace Project.Response.Channel
{
    public class ViewModel
    {
        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _UplineUserID;

        public int UplineUserID
        {
            get => _UplineUserID;
            set => _UplineUserID = value;
        }

        private int _ChannelSlabDetailID;

        public int ChannelSlabDetailID
        {
            get => _ChannelSlabDetailID;
            set => _ChannelSlabDetailID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
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

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

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

        private string _Details;

        public string Details
        {
            get => _Details.TrimIfNotNull();
            set => _Details = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public CommissionModel RealCommissionmodel = new CommissionModel();
        public CommissionModel Commissionmodel = new CommissionModel();
        public CommissionWrapper Commissionwrapper = new CommissionWrapper();

        public CommissionWrapper OwnCommissionwrapper = new CommissionWrapper();
    }

    public class CommissionWrapper
    {
        public CommissionModel General = new CommissionModel();
        public CommissionModel Real = new CommissionModel();
    }

    public class CommissionModel : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
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
            get => _CommissionCategory.TrimIfNotNullWithDefault("Commission");
            set => _CommissionCategory = value.TrimIfNotNullWithDefault("Commission");
        }
    }
}