using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYCSetting
{
    public class UpdateRequest
    {
        private int _eKYCSettingID;

        public int eKYCSettingID
        {
            get => _eKYCSettingID;
            set => _eKYCSettingID = value;
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

        private string _Slat;

        public string Slat
        {
            get => _Slat.TrimIfNotNull();
            set => _Slat = value.TrimIfNotNull();
        }

        private bool _IsActiveWeb;

        public bool IsActiveWeb
        {
            get => _IsActiveWeb;
            set => _IsActiveWeb = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _AuthKey;

        public string AuthKey
        {
            get => _AuthKey.TrimIfNotNull();
            set => _AuthKey = value.TrimIfNotNull();
        }

        private decimal _ChargeAmount;

        public decimal ChargeAmount
        {
            get => _ChargeAmount;
            set => _ChargeAmount = value;
        }

        private string _EncryptionKey;

        public string EncryptionKey
        {
            get => _EncryptionKey.TrimIfNotNull();
            set => _EncryptionKey = value.TrimIfNotNull();
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
            RuleFor(x => x.eKYCSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(PaymentGatewayMessage.InvalidID);
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(PaymentGatewayMessage.InvalidName);

            RuleFor(x => x.AuthKey)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidAuthKey);

            RuleFor(x => x.Slat)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidSlat);

            RuleFor(x => x.Token)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidToken);
        }
    }
}