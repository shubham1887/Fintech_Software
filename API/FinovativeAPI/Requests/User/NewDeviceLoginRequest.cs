using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    internal class NewDeviceLoginRequest
    {
        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Device = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }
        
        private string _DeviceID;

        public string DeviceID
        {
            get => _DeviceID.TrimIfNotNull();
            set => _DeviceID = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }
        
        private int _Pin;

        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }

        internal string IsValidModalState()
        {
            var validator = new NewDeviceLoginRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class NewDeviceLoginRequestValidator : AbstractValidator<NewDeviceLoginRequest>
    {
        public NewDeviceLoginRequestValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage(Message.UserMessage.InvalidUserName);

            RuleFor(x => x.Password).NotEmpty().WithMessage(Message.UserMessage.InvalidPassword);

            RuleFor(x => x.OTP).NotEmpty().WithMessage(Message.UserMessage.InvalidOTP);

            RuleFor(x => x.FingerPrint).NotEmpty().WithMessage(Message.UserMessage.InvalidFigerPrint);
        }
    }
}