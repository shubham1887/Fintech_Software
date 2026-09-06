using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class RefundSendOTPRequest
    {
        public string ID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new RefundSendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

    }

    internal class RefundSendOTPRequestValidator : AbstractValidator<RefundSendOTPRequest>
    {
        public RefundSendOTPRequestValidator()
        {
            RuleFor(x => x.ID)
                .Must(new Validation().IsValidString)
                .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}