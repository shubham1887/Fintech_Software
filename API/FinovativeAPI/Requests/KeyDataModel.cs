using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests
{
    public class KeyDataModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Platform;

        public string Platform
        {
            get => _Platform.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Platform = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _IMEI;

        public string IMEI
        {
            get => _IMEI.TrimIfNotNull();
            set => _IMEI = value.TrimIfNotNull();
        }

        private string _RefreshToken;

        public string RefreshToken
        {
            get => _RefreshToken.TrimIfNotNull();
            set => _RefreshToken = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new KeyDataModelValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            // need to check this.UserID for active /deactive

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class KeyDataModelValidator : AbstractValidator<KeyDataModel>
    {
        public KeyDataModelValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUser);

            RuleFor(x => x.Platform)
                .Must(new Validation().IsValidString)
                .WithMessage(Messages.InvalidPlatform);

            RuleFor(x => x.IMEI)
                 .Must(new Validation().IsValidString)
                .WithMessage(Messages.InvalidIMEI);
        }
    }
}