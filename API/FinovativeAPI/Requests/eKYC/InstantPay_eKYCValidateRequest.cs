using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class InstantPay_eKYCValidateRequest
    {
        private string _otp;

        public string otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }

        private string _latitude;

        public string latitude
        {
            get => _latitude.TrimIfNotNull();
            set => _latitude = value.TrimIfNotNull();
        }

        private string _longitude;

        public string longitude
        {
            get => _longitude.TrimIfNotNull();
            set => _longitude = value.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        private int _eKYCMasterID;

        public int eKYCMasterID
        {
            get => _eKYCMasterID;
            set => _eKYCMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPay_eKYCValidateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }
    internal class InstantPay_eKYCValidateRequestValidator : AbstractValidator<InstantPay_eKYCValidateRequest>
    {
        public InstantPay_eKYCValidateRequestValidator()
        {
            
             RuleFor(x => x.otp)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.eKYCMessage.InvalidOTP);
           
            RuleFor(x => x.PanNumber)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.eKYCMessage.InvalidPAN);

        }
    }
}

