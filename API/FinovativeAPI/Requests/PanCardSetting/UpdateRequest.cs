using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PanCardSetting
{
    public class UpdateRequest
    {
        private int _PanCardSettingID;

        public int PanCardSettingID
        {
            get => _PanCardSettingID;
            set => _PanCardSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
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

        private string _PublicKey;

        public string PublicKey
        {
            get => _PublicKey.TrimIfNotNull();
            set => _PublicKey = value.TrimIfNotNull();
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
            RuleFor(x => x.PanCardSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(AEPSMessage.InvalidID);
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(AEPSMessage.InvalidName);

            RuleFor(x => x.MerchantID)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidMerchantID);

            RuleFor(x => x.Slat)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidSlat);

            RuleFor(x => x.Token)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidToken);
        }
    }
}