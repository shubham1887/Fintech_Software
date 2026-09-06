using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Beneficiary
{
    public class BeneficiaryVerifyOTPRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

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

        private string _BeneID;

        public string BeneID
        {
            get => _BeneID.TrimIfNotNull();
            set => _BeneID = value.TrimIfNotNull();
        }

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.TrimIfNotNull();
            set => _BeneName = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _BeneMobile;

        public string BeneMobile
        {
            get => _BeneMobile.TrimIfNotNull();
            set => _BeneMobile = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BeneficiaryVerifyOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BeneficiaryVerifyOTPRequestValidator : AbstractValidator<BeneficiaryVerifyOTPRequest>
    {
        public BeneficiaryVerifyOTPRequestValidator()
        {
            RuleFor(x => x.Mobile)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);

            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(SenderMessage.InvalidOTP);

            RuleFor(x => x.AccountNo)
            .Must(new Validation().IsValidString)
            .WithMessage(SenderMessage.InvalidBeneficiary);
        }
    }
}