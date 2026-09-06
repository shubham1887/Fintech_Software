using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EcollectionSetting
{
    public class UpdateRequest
    {
        private int _EcollectionSettingID;

        public int EcollectionSettingID
        {
            get => _EcollectionSettingID;
            set => _EcollectionSettingID = value;
        }

        private string _PrivateKeyPath;

        public string PrivateKeyPath
        {
            get => _PrivateKeyPath.TrimIfNotNull();
            set => _PrivateKeyPath = value.TrimIfNotNull();
        }

        private string _PrivateKeyPassword;

        public string PrivateKeyPassword
        {
            get => _PrivateKeyPassword.TrimIfNotNull();
            set => _PrivateKeyPassword = value.TrimIfNotNull();
        }

        private string _PublicKeyPath;

        public string PublicKeyPath
        {
            get => _PublicKeyPath.TrimIfNotNull();
            set => _PublicKeyPath = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Prefix;

        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _IFSCCode;

        public string IFSCCode
        {
            get => _IFSCCode.TrimIfNotNull();
            set => _IFSCCode = value.TrimIfNotNull();
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
            RuleFor(x => x.EcollectionSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(EcollectionMessage.InvalidID);
            RuleFor(x => x.PrivateKeyPath)
              .Must(new Validation().IsValidString)
              .WithMessage(EcollectionMessage.InvalidPrivateKeyPath);

            RuleFor(x => x.PublicKeyPath)
           .Must(new Validation().IsValidString)
           .WithMessage(EcollectionMessage.InvalidPublicKeyPath);

            RuleFor(x => x.PrivateKeyPassword)
         .Must(new Validation().IsValidString)
         .WithMessage(EcollectionMessage.InvalidPrivateKeyPassword);

            RuleFor(x => x.Prefix)
         .Must(new Validation().IsValidString)
         .WithMessage(EcollectionMessage.InvalidPrefix);

            RuleFor(x => x.IFSCCode)
         .Must(new Validation().IsValidString)
         .WithMessage(EcollectionMessage.InvalidIFSC);
        }
    }
}