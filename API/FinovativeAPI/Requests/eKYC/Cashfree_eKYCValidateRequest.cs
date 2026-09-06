using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class Cashfree_eKYCValidateRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private int _eKYCMasterID;

        public int eKYCMasterID
        {
            get => _eKYCMasterID;
            set => _eKYCMasterID = value;
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Cashfree_eKYCValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Cashfree_eKYCValidateRequestValidator : AbstractValidator<Cashfree_eKYCValidateRequest>
    {
        public Cashfree_eKYCValidateRequestValidator()
        {
            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.eKYCMessage.InvalidOTP);

            RuleFor(x => x.eKYCMasterID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.eKYCMessage.InvalidID);

            RuleFor(x => x.PanNumber)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.eKYCMessage.InvalidPAN);
        }
    }
}