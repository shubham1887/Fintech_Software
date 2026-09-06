using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.DematSetting
{
    public class UpdateRequest
    {
        private int _DematSettingID;

        public int DematSettingID
        {
            get => _DematSettingID;
            set => _DematSettingID = value;
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

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
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

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _UTMSource;

        public string UTMSource
        {
            get => _UTMSource.TrimIfNotNull();
            set => _UTMSource = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
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
            RuleFor(config => config.DematSettingID)
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

            RuleFor(config => config.EmailID)
                     .Must(new Validation().IsValidString)
                     .WithMessage(DematSettingMessage.InvalidEmailID);

            RuleFor(config => config.UTMSource)
                    .Must(new Validation().IsValidString)
                    .WithMessage(DematSettingMessage.InvalidUTMSourse);
        }
    }
}