using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Operator
{
    public class OperatorSelectByServiceIDRequest
    {
        private int serviceID;

        public int ServiceID
        {
            get => serviceID;
            set => serviceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new OperatorSelectByServiceIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorSelectByServiceIDRequestValidator : AbstractValidator<OperatorSelectByServiceIDRequest>
    {
        public OperatorSelectByServiceIDRequestValidator()
        {
            RuleFor(x => x.ServiceID)
              .Must(new Validation().IsValidpositive)
              .WithMessage(Message.ServiceMessage.Invalid);
        }
    }
}