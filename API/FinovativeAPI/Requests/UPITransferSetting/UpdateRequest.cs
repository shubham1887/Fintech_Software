using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.UPITransferSetting
{
    public class UpdateRequest
    {
        private int _UPITransferSettingID;

        public int UPITransferSettingID
        {
            get => _UPITransferSettingID;
            set => _UPITransferSettingID = value;
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

        private string _AuthCode;

        public string AuthCode
        {
            get => _AuthCode.TrimIfNotNull();
            set => _AuthCode = value.TrimIfNotNull();
        }

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
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

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;
            if (success == false)
                return failures[0].ToString();

            if (MinAmount > MaximumAmount)
                return $"MinimumAmount should less than  {MaximumAmount}";

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(config => config.UPITransferSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UPITransferSettingMessages.InvalidSetting);

            RuleFor(config => config.Name)
                      .Must(new Validation().IsValidString)
                      .WithMessage(UPITransferSettingMessages.InvalidName);
            RuleFor(config => config.UserName)
                    .Must(new Validation().IsValidString)
                    .WithMessage(UPITransferSettingMessages.InvalidUserName);
            RuleFor(config => config.Token)
                     .Must(new Validation().IsValidString)
                     .WithMessage(UPITransferSettingMessages.InvalidToken);

            RuleFor(config => config.MinAmount)
                    .Must(new Validation().IsValidDecimal)
                    .GreaterThanOrEqualTo(10)
                    .WithMessage(UPITransferSettingMessages.InvalidMinimumAmount);
        }
    }
}