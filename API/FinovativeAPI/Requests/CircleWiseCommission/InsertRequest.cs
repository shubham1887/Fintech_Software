using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleWiseCommission
{
    public class InsertRequest
    {
        private int _SlabID;

        public int SlabID
        {
            get => _SlabID;
            set => _SlabID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
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
            get => _RetailerCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
            set => _RetailerCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
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
            get => _DistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
            set => _DistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
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
            get => _SuperDistributorCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
            set => _SuperDistributorCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
        }

        private decimal clientComm;

        public decimal ClientComm
        {
            get => clientComm;
            set => clientComm = value;
        }

        private string clientCommType;

        public string ClientCommType
        {
            get => clientCommType;
            set => clientCommType = value;
        }

        private string clientCommissionCategory;

        public string ClientCommissionCategory
        {
            get => clientCommissionCategory;
            set => clientCommissionCategory = value;
        }

        public CommissionDetailWrapper Retailer = new CommissionDetailWrapper();
        public CommissionDetailWrapper Distributer = new CommissionDetailWrapper();
        public CommissionDetailWrapper SuperDistributer = new CommissionDetailWrapper();

        public CommissionDetailWrapper Client = new CommissionDetailWrapper(); internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
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
            get => _CommissionType.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Flexi);
            set => _CommissionType = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Flexi);
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.SlabID)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabMessage.InvalidName);

            RuleFor(x => x.StateID)
                .Must(new Validation().IsValidInt)
                .WithMessage(SlabDetailMessage.InvalidStateID);

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