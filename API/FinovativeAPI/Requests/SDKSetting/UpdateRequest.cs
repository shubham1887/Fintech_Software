using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SDKSetting
{
    public class UpdateRequest
    {
        private int _SDKSettingID;

        public int SDKSettingID
        {
            get => _SDKSettingID;
            set => _SDKSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _URL;

        public string URL
        {
            get => _URL.TrimIfNotNull();
            set => _URL = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _PublicKey;

        public string PublicKey
        {
            get => _PublicKey.TrimIfNotNull();
            set => _PublicKey = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.SDKSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(PaymentGatewayMessage.InvalidID);
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(PaymentGatewayMessage.InvalidName);

            RuleFor(x => x.PublicKey)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidPublicKey);

            RuleFor(x => x.URL)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidURL);

            RuleFor(x => x.Token)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidToken);
        }
    }
}