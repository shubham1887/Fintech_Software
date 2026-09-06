using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class OverdraftActiveDeactiveRequest
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID
        {
            get => _OverdraftMasterID;
            set => _OverdraftMasterID = value;
        }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new OverdraftActiveDeactiveRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OverdraftActiveDeactiveRequestValidator : AbstractValidator<OverdraftActiveDeactiveRequest>
    {
        public OverdraftActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.OverdraftMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OverdraftMessgae.Invalid);
        }
    }
}