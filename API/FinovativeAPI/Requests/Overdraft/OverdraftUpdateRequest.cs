using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class OverdraftUpdateRequest
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID
        {
            get => _OverdraftMasterID;
            set => _OverdraftMasterID = value;
        }

        private decimal _MaxOverdraft;

        public decimal MaxOverdraft
        {
            get => _MaxOverdraft;
            set => _MaxOverdraft = value;
        }

        internal string IsValidModalState()
        {
            var validator = new OverdraftUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OverdraftUpdateRequestValidator : AbstractValidator<OverdraftUpdateRequest>
    {
        public OverdraftUpdateRequestValidator()
        {
            RuleFor(x => x.OverdraftMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OverdraftMessgae.Invalid);
        }
    }
}