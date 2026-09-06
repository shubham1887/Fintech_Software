using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class ActivateRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private bool _IsCommissionHold;

        public bool IsCommissionHold
        {
            get => _IsCommissionHold;
            set => _IsCommissionHold = value;
        }

        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new ActivateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActivateRequestValidator : AbstractValidator<ActivateRequest>
    {
        public ActivateRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUser);
        }
    }
}