using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class OTTTransctionStatusRequest
    {
        public string ID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new OTTTransctionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OTTTransctionStatusRequestValidator : AbstractValidator<OTTTransctionStatusRequest>
    {
        public OTTTransctionStatusRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}