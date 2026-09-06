using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class LoginRequest
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

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Device = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }
        
        private string _DeviceID;

        public string DeviceID
        {
            get => _DeviceID.TrimIfNotNull();
            set => _DeviceID = value.TrimIfNotNull();
        }

        private string _CaptchaToken;

        public string CaptchaToken
        {
            get => _CaptchaToken.TrimIfNotNull();
            set => _CaptchaToken = value.TrimIfNotNull();
        }

        private string _Latitude;

        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        private string _AndroidAppVersion;

        public string AndroidAppVersion
        {
            get => _AndroidAppVersion.TrimIfNotNull();
            set => _AndroidAppVersion = value.TrimIfNotNull();
        }

        private string _iOSAppVersion;

        public string iOSAppVersion
        {
            get => _iOSAppVersion.TrimIfNotNull();
            set => _iOSAppVersion = value.TrimIfNotNull();
        }
        
        private int _Pin;

        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }

        internal string IsValidModalState()
        {
            var validator = new LoginRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage(Message.UserMessage.InvalidUserName);
            RuleFor(x => x.Password).NotEmpty().WithMessage(Message.UserMessage.InvalidPassword);
            RuleFor(x => x.FingerPrint).NotEmpty().WithMessage(Message.UserMessage.InvalidFigerPrint);
            RuleFor(x => x.CaptchaToken).NotEmpty().WithMessage(Message.UserMessage.InvalidCaptcha);
        }
    }
}