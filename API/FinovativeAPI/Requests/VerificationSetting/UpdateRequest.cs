using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.VerificationSetting
{
    public class UpdateRequest
    {
        private int _VerificationSettingID;

        public int VerificationSettingID
        {
            get => _VerificationSettingID;
            set => _VerificationSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();

            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsActiveForWebsite;

        public bool IsActiveForWebsite
        {
            get => _IsActiveForWebsite;
            set => _IsActiveForWebsite = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _AuthCode;

        public string AuthCode
        {
            get => _AuthCode.TrimIfNotNull();
            set => _AuthCode = value.TrimIfNotNull();
        }

        private string _CallBackURL;

        public string CallBackURL
        {
            get => _CallBackURL.TrimIfNotNull();
            set => _CallBackURL = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNull();
            set => _DisplayName = value.TrimIfNotNull();
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private bool _IsAccountVerify;

        public bool IsAccountVerify
        {
            get => _IsAccountVerify;
            set => _IsAccountVerify = value;
        }

        private bool _IsUPIVerify;

        public bool IsUPIVerify
        {
            get => _IsUPIVerify;
            set => _IsUPIVerify = value;
        }

        private bool _IsPayoutVerify;

        public bool IsPayoutVerify
        {
            get => _IsPayoutVerify;
            set => _IsPayoutVerify = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(config => config.VerificationSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(DMTSettingMessages.InvalidSetting);

            RuleFor(config => config.Name)
                      .Must(new Validation().IsValidString)
                      .WithMessage(DMTSettingMessages.InvalidName);
            RuleFor(config => config.UserName)
                    .Must(new Validation().IsValidString)
                    .WithMessage(DMTSettingMessages.InvalidUserName);
            RuleFor(config => config.Token)
                     .Must(new Validation().IsValidString)
                     .WithMessage(DMTSettingMessages.InvalidToken);

            RuleFor(config => config.CallBackURL)
                      .Must(new Validation().IsValidString)
                      .WithMessage(DMTSettingMessages.InvalidCallBackURL);           
        }
    }
}
