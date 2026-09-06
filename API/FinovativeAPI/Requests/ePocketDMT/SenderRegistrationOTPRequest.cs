using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ePocketDMT
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

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        private string _ActionType;

        public string ActionType
        {
            get => _ActionType.TrimIfNotNull();
            set => _ActionType = value.TrimIfNotNull();
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

            RuleFor(x => x.SenderID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);

            RuleFor(x => x.BeneficiaryID)
             .Must(new Validation().IsValidString)
             .WithMessage(SenderMessage.InvalidBeneficiary);
        }
    }
}