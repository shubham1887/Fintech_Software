using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfigurationAdminCommission
{
    public class InsertRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
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

        private decimal _AdminComm;

        public decimal AdminComm
        {
            get => _AdminComm;
            set => _AdminComm = value;
        }

        private string _AdminCommType;

        public string AdminCommType
        {
            get => _AdminCommType.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
            set => _AdminCommType = value.TrimIfNotNullWithDefault(StaticModel.CommissionType.Flexi);
        }

        private string _AdminCommissionCategory;

        public string AdminCommissionCategory
        {
            get => _AdminCommissionCategory.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
            set => _AdminCommissionCategory = value.TrimIfNotNullWithDefault(StaticModel.CommissionCategory.Commission);
        }

        public CommissionDetailWrapper Admin = new CommissionDetailWrapper();

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
    }

    public class CommissionDetailWrapper
    {
        public CommissionDetailModel General = new CommissionDetailModel();
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
            RuleFor(x => x.ProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ProviderMessage.InValidProvider);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);

            RuleFor(x => x.FromAmount)
               .Must(new Validation().IsValidDecimal)
               .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.ToAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}