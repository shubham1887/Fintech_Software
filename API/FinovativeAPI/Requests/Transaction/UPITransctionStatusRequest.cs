using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class UPITransctionStatusRequest
    {
        public string ID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new UPITransctionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UPITransctionStatusRequestValidator : AbstractValidator<UPITransctionStatusRequest>
    {
        public UPITransctionStatusRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}