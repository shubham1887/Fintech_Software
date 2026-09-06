using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class SmartPayTransctionStatusRequest
    {
        public string ID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new SmartPayTransctionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SmartPayTransctionStatusRequestValidator : AbstractValidator<SmartPayTransctionStatusRequest>
    {
        public SmartPayTransctionStatusRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}