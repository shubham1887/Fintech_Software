using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Sender
{
    public class SenderRegistrationOTPRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _OTPRefID;

        public string OTPRefID
        {
            get => _OTPRefID.TrimIfNotNull();
            set => _OTPRefID = value.TrimIfNotNull();
        }

        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SenderRegistrationOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SenderRegistrationOTPRequestValidator : AbstractValidator<SenderRegistrationOTPRequest>
    {
        public SenderRegistrationOTPRequestValidator()
        {
            RuleFor(x => x.Mobile)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidMobile);
        }
    }
}