using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ROfferCommission
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

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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
        public decimal ClientComm { get; internal set; }
        public string ClientCommType { get; internal set; }
        public string ClientCommissionCategory { get; internal set; }

        public CommissionDetailWrapper Retailer = new CommissionDetailWrapper();
        public CommissionDetailWrapper Distributer = new CommissionDetailWrapper();
        public CommissionDetailWrapper SuperDistributer = new CommissionDetailWrapper();
        public CommissionDetailWrapper Client = new CommissionDetailWrapper();

        internal string IsValidModalState()
        {
            var validator = new ROfferCommissionInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return StatusType.Ok;
        }

        public override string ToString()
        {
            return $"ROfferCommission insert: for Operator:{OperatorID} Amount:{Amount},RetailerComm:{RetailerComm}, RetailerCommType:{RetailerCommType} " +
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

    public class ROfferCommissionInsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public ROfferCommissionInsertRequestValidator()
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

            RuleFor(x => x.Amount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}