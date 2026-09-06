using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class PDRS_eKYCValidateRequest
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

        internal string IsValidModalState()
        {
            var validator = new PDRS_eKYCValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PDRS_eKYCValidateRequestValidator : AbstractValidator<PDRS_eKYCValidateRequest>
    {
        public PDRS_eKYCValidateRequestValidator()
        {
            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.eKYCMessage.InvalidOTP);

            //RuleFor(x => x.ClientID)
            //.Must(new Validation().IsValidString)
            //.WithMessage(Message.eKYCMessage.InvalidClientID);

            RuleFor(x => x.eKYCMasterID)
           .Must(new Validation().IsValidInt)
           .WithMessage(Message.eKYCMessage.InvalidID);
        }
    }
}