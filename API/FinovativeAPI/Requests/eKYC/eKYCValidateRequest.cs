using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class eKYCValidateRequest
    {
        private string _otp;

        public string Otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }
        private string _otpToken;

        public string otpToken
        {
            get => _otpToken.TrimIfNotNull();
            set => _otpToken = value.TrimIfNotNull();
        }
        private int _eKYCMasterID;
        public int eKYCMasterID
        {
            get => _eKYCMasterID;
            set => _eKYCMasterID = value;
        }
        private string _RequestID;
        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }
        
        internal string IsValidModalState()
        {
            var validator = new eKYCValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class eKYCValidateRequestValidator : AbstractValidator<eKYCValidateRequest>
    {
        public eKYCValidateRequestValidator()
        {
            RuleFor(x => x.Otp)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.eKYCMessage.InvalidOTP);

        }
    }
}