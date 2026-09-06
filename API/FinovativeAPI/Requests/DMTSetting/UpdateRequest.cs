using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.DMTSetting
{
    public class UpdateRequest
    {
        private int _DMTSettingID;

        public int DMTSettingID
        {
            get => _DMTSettingID;
            set => _DMTSettingID = value;
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

        private bool _IsCCF;

        public bool IsCCF
        {
            get => _IsCCF;
            set => _IsCCF = value;
        }

        private bool _IsNeft;

        public bool IsNeft
        {
            get => _IsNeft;
            set => _IsNeft = value;
        }

        private bool _IsPayoutDMT;

        public bool IsPayoutDMT
        {
            get => _IsPayoutDMT;
            set => _IsPayoutDMT = value;
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

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNull();
            set => _DisplayName = value.TrimIfNotNull();
        }

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _SplitAmount;

        public decimal SplitAmount
        {
            get => _SplitAmount;
            set => _SplitAmount = value;
        }

        private decimal _SenderLimit;

        public decimal SenderLimit
        {
            get => _SenderLimit;
            set => _SenderLimit = value;
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        private bool _IsExpressDMT;
        public bool IsExpressDMT
        {
            get => _IsExpressDMT;
            set => _IsExpressDMT = value;
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
            RuleFor(config => config.DMTSettingID)
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

            RuleFor(config => config.SplitAmount)
                      .Must(new Validation().IsValidDecimal)
                      .GreaterThanOrEqualTo(5000)
                      .WithMessage(DMTSettingMessages.InvalidSplitAmount);

            RuleFor(config => config.MinAmount)
                    .Must(new Validation().IsValidDecimal)
                    .GreaterThanOrEqualTo(10)
                    .WithMessage(DMTSettingMessages.InvalidMinimumAmount);

            RuleFor(config => config.SenderLimit)
                  .Must(new Validation().IsValidDecimal)
                  .GreaterThanOrEqualTo(25000)
                  .WithMessage(DMTSettingMessages.InvalidSenderLimit);
        }
    }
}