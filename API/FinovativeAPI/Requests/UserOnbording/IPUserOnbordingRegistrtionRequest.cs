using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class IPUserOnbordingRegistrtionRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get { return _MobileNumber.TrimIfNotNull(); }
            set { _MobileNumber = value.TrimIfNotNull(); }
        }

        private string _Email;

        public string Email
        {
            get { return _Email.TrimIfNotNull(); }
            set { _Email = value.TrimIfNotNull(); }
        }

        private string _Name;

        public string Name
        {
            get { return _Name.TrimIfNotNull(); }
            set { _Name = value.TrimIfNotNull(); }
        }

        private string _Company;

        public string Company
        {
            get { return _Company.TrimIfNotNull(); }
            set { _Company = value.TrimIfNotNull(); }
        }

        private string _OTP;

        public string OTP
        {
            get { return _OTP.TrimIfNotNull(); }
            set { _OTP = value.TrimIfNotNull(); }
        }

        private string _PAN;

        public string PAN
        {
            get { return _PAN.TrimIfNotNull(); }
            set { _PAN = value.TrimIfNotNull(); }
        }

        private string _PinCode;

        public string PinCode
        {
            get { return _PinCode.TrimIfNotNull(); }
            set { _PinCode = value.TrimIfNotNull(); }
        }

        private string _Address;

        public string Address
        {
            get { return _Address.TrimIfNotNull(); }
            set { _Address = value.TrimIfNotNull(); }
        }

        internal string IsValidModalState()
        {
            var validator = new IPUserOnbordingRegistrtionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class IPUserOnbordingRegistrtionRequestValidator : AbstractValidator<IPUserOnbordingRegistrtionRequest>
    {
        public IPUserOnbordingRegistrtionRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(UserOnbordingMessage.InvalidMobile);

            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(UserOnbordingMessage.InvalidMobile);
        }
    }
}