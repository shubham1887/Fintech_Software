using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class ApiClub_EKYCValidateOTPRequest
    {
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ApiClub_EKYCValidateOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_EKYCValidateOTPRequestValidator : AbstractValidator<ApiClub_EKYCValidateOTPRequest>
    {
        public ApiClub_EKYCValidateOTPRequestValidator()
        {
            RuleFor(x => x.TransactionID)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.UserMessage.InvaildTransactionID);

            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.UserMessage.InvalidOTP);
        }
    }
}