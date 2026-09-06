using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class InsertRequest
    {
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
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
            get => _RetailerCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _RetailerCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
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
            get => _DistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _DistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private decimal _SuperDistributorComm;

        public decimal SuperDistributorComm
        {
            get => _SuperDistributorComm;
            set => _SuperDistributorComm = value;
        }

        private string _SuperDistributorCommType;

        public string SuperDistributorType
        {
            get => _SuperDistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _SuperDistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
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
            get => _RealRetailerCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _RealRetailerCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
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
            get => _RealDistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _RealDistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private decimal _RealSuperDistributorComm;

        public decimal RealSuperDistributorComm
        {
            get => _RealSuperDistributorComm;
            set => _RealSuperDistributorComm = value;
        }

        private string _RealSuperDistributorCommType;

        public string RealSuperDistributorType
        {
            get => _RealSuperDistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _RealSuperDistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private string _SlabType;

        public string SlabType
        {
            get => _SlabType.TrimIfNotNullWithDefault(StaticModel.SlabTypes.General);
            set => _SlabType = value.TrimIfNotNullWithDefault(StaticModel.SlabTypes.General);
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission; set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Fix);
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
        }

        private int _UserTypeID;
        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }
        public string RetailerCommissionCategory { get; internal set; }
        public string DistributorCommissionCategory { get; internal set; }
        public string SuperDistributorCommissionCategory { get; internal set; }
        public string RealRetailerCommissionCategory { get; internal set; }
        public string RealDistributorCommissionCategory { get; internal set; }
        public string RealSuperDistributorCommissionCategory { get; internal set; }
        public decimal ClientComm { get; internal set; }
        public string ClientCommType { get; internal set; }
        public object RealClientComm { get; internal set; }
        public string ClientCommissionCategory { get; internal set; }
        public object RealClientCommType { get; internal set; }
        public object RealClientCommissionCategory { get; internal set; }

        public CommissionDetailWrapper Retailer = new CommissionDetailWrapper();
        public CommissionDetailWrapper Distributer = new CommissionDetailWrapper();
        public CommissionDetailWrapper SuperDistributer = new CommissionDetailWrapper();
        public CommissionDetailWrapper Client = new CommissionDetailWrapper();

        internal string IsValidModalState()
        {
            var validator = new SlabDetailInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsValidFromToAmount();
            if (!success)

                return SlabDetailMessage.ValidAmount;

            return StatusType.Ok;
        }

        private bool IsValidFromToAmount()
        {
            bool success = new Validation().IsValidFromToAmount(FromAmount, ToAmount);
            return success;
        }

        public override string ToString()
        {
            return $"Commission insert: for Operator:{OperatorID} FromAmount:{FromAmount},ToAmount:{ToAmount},RetailerComm:{RetailerComm}, RetailerCommType:{RetailerCommType} " +
                $"DistributorComm:{DistributorComm} DistributorCommType{DistributorCommType}, SuperDComm:{SuperDistributorComm}, SuperDCommType:{SuperDistributorType} ";
        }
    }

    public class CommissionDetailWrapper
    {
        public CommissionDetailModel General = new CommissionDetailModel();
        public CommissionDetailModel Real = new CommissionDetailModel();
    }

    public class CommissionDetailModel
    {
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

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }

    public class SlabDetailInsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public SlabDetailInsertRequestValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabMessage.InvalidName);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);

            RuleFor(x => x.OperatorID)
              .Must(new Validation().IsValidInt)
              .WithMessage(SlabDetailMessage.InvalidOperator);

            RuleFor(x => x.FromAmount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.ToAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}