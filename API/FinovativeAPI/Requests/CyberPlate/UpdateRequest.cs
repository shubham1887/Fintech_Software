using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CyberPlate
{
    public class UpdateRequest
    {
        private int _CyberPlateSettingID;

        public int CyberPlateSettingID
        {
            get => _CyberPlateSettingID;
            set => _CyberPlateSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _AP;

        public string AP
        {
            get => _AP.TrimIfNotNull();
            set => _AP = value.TrimIfNotNull();
        }

        private string _OP;

        public string OP
        {
            get => _OP.TrimIfNotNull();
            set => _OP = value.TrimIfNotNull();
        }

        private string _SD;

        public string SD
        {
            get => _SD.TrimIfNotNull();
            set => _SD = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private string _TermID;

        public string TermID
        {
            get => _TermID.TrimIfNotNull();
            set => _TermID = value.TrimIfNotNull();
        }

        private string _AID;

        public string AID
        {
            get => _AID.TrimIfNotNull();
            set => _AID = value.TrimIfNotNull();
        }

        private string _SecretKeyPath;

        public string SecretKeyPath
        {
            get => _SecretKeyPath.TrimIfNotNull();
            set => _SecretKeyPath = value.TrimIfNotNull();
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
            RuleFor(x => x.CyberPlateSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CyberPlateSettingMessage.InvalidID);
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(CyberPlateSettingMessage.InvalidName);

            RuleFor(x => x.SD)
           .Must(new Validation().IsValidString)
           .WithMessage(CyberPlateSettingMessage.InvalidSD);

            RuleFor(x => x.OP)
           .Must(new Validation().IsValidString)
           .WithMessage(CyberPlateSettingMessage.InvalidOP);

            RuleFor(x => x.AP)
           .Must(new Validation().IsValidString)
           .WithMessage(CyberPlateSettingMessage.InvalidAP);

            RuleFor(x => x.AID)
           .Must(new Validation().IsValidString)
           .WithMessage(CyberPlateSettingMessage.InvalidAID);

            RuleFor(x => x.TermID)
           .Must(new Validation().IsValidString)
           .WithMessage(CyberPlateSettingMessage.InvalidTermID);

            RuleFor(x => x.SecretKeyPath)
          .Must(new Validation().IsValidString)
          .WithMessage(CyberPlateSettingMessage.InvalidSecretKeyPath);

            RuleFor(x => x.Password)
            .Must(new Validation().IsValidString)
            .WithMessage(CyberPlateSettingMessage.InvalidPassword);
        }
    }
}