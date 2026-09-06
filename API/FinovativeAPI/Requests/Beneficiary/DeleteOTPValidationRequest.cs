using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Beneficiary
{
    public class DeleteOTPValidationRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _Otpreference;

        public string Otpreference
        {
            get => _Otpreference.TrimIfNotNull();
            set => _Otpreference = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteOTPValidationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteOTPValidationRequestValidator : AbstractValidator<DeleteOTPValidationRequest>
    {
        public DeleteOTPValidationRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidBeneficiary);

            RuleFor(x => x.SenderID)
             .Must(new Validation().IsValidString)
             .WithMessage(SenderMessage.InvalidSender);

            RuleFor(x => x.OTP)
           .Must(new Validation().IsValidString)
           .WithMessage(SenderMessage.InvalidOTP);
        }
    }
}