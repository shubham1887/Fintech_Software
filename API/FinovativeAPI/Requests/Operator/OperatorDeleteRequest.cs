using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Operator
{
    public class OperatorDeleteRequest
    {
        private int _OperatorID;
        private bool _IsActive;

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new OperatorDeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorDeleteRequestValidator : AbstractValidator<OperatorDeleteRequest>
    {
        public OperatorDeleteRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUser);
        }
    }
}