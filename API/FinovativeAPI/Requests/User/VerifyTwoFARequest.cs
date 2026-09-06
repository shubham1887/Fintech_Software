using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class VerifyTwoFARequest
    {
        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private string _TwoFAPasscode;

        public string TwoFAPasscode
        {
            get => _TwoFAPasscode.TrimIfNotNull();
            set => _TwoFAPasscode = value.TrimIfNotNull();
        }

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Device = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }
        
        private string _DeviceID;

        public string DeviceID
        {
            get => _DeviceID.TrimIfNotNull();
            set => _DeviceID = value.TrimIfNotNull();
        }

        private int _Pin;
        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }

        internal string IsValidModalState()
        {
            var validator = new VerifyTwoFARequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class VerifyTwoFARequestValidator : AbstractValidator<VerifyTwoFARequest>
    {
        public VerifyTwoFARequestValidator()
        {
            RuleFor(x => x.UserName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidUserName);

            RuleFor(x => x.TwoFAPasscode)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.Invalid2FAPasscode);

            RuleFor(x => x.Password).NotEmpty()
                .WithMessage(Message.UserMessage.InvalidPassword);
        }
    }
}