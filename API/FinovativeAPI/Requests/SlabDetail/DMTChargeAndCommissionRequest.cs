using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class DMTChargeAndCommissionRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ActiveDMTName;

        public string ActiveDMTName
        {
            get => _ActiveDMTName.TrimIfNotNull();
            set => _ActiveDMTName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new DMTChargeAndCommissionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    public class DMTChargeAndCommissionRequestValidator : AbstractValidator<DMTChargeAndCommissionRequest>
    {
        public DMTChargeAndCommissionRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.ActiveDMTName)
               .Must(new Validation().IsValidString)
               .WithMessage(ProviderMessage.InValidProvider);
        }
    }
}