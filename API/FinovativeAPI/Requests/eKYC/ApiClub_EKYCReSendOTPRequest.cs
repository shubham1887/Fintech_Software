using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class ApiClub_EKYCReSendOTPRequest
    {
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ApiClub_EKYCReSendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_EKYCReSendOTPRequestValidator : AbstractValidator<ApiClub_EKYCReSendOTPRequest>
    {
        public ApiClub_EKYCReSendOTPRequestValidator()
        {
            RuleFor(x => x.TransactionID)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.UserMessage.InvaildTransactionID);
        }
    }
}