using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class DMTTransctionStatusRequest
    {
        public string ID { get; set; }

        public bool IsAutoStatusCheck { get; set; }

        internal string IsValidModalState()
        {
            var validator = new DMTTransctionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DMTTransctionStatusRequestValidator : AbstractValidator<DMTTransctionStatusRequest>
    {
        public DMTTransctionStatusRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}