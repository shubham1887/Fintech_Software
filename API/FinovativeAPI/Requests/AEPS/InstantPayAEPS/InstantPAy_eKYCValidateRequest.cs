using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.InstantPayAEPS
{
    public class InstantPAy_eKYCValidateRequest
    {
        private string _otp;

        public string Otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }

        private int _eKYCMasterID;

        public int eKYCMasterID
        {
            get => _eKYCMasterID;
            set => _eKYCMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPAy_eKYCValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InstantPAy_eKYCValidateRequestValidator : AbstractValidator<InstantPAy_eKYCValidateRequest>
    {
        public InstantPAy_eKYCValidateRequestValidator()
        {
            RuleFor(x => x.Otp)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.eKYCMessage.InvalidOTP);

            RuleFor(x => x.eKYCMasterID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.eKYCMessage.InvalidID);
        }
    }
}