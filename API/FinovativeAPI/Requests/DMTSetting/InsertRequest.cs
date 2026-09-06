using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.DMTSetting
{
    public class InsertRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();

            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsCCF;

        public bool IsCCF
        {
            get => _IsCCF;
            set => _IsCCF = value;
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _WalletID;

        public string WalletID
        {
            get => _WalletID.TrimIfNotNull();
            set => _WalletID = value.TrimIfNotNull();
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

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
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