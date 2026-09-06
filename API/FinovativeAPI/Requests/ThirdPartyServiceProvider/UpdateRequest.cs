using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.ThirdPartyServiceProvider
{
    public class UpdateRequest
    {
        private int _ThirdPartyServiceProviderID;
        public int ThirdPartyServiceProviderID { get => _ThirdPartyServiceProviderID; set => _ThirdPartyServiceProviderID = value; }

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

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _AppID;

        public string AppID
        {
            get => _AppID.TrimIfNotNull();
            set => _AppID = value.TrimIfNotNull();
        }

        private string _DomainName;

        public string DomainName
        {
            get => _DomainName.TrimIfNotNull();
            set => _DomainName = value.TrimIfNotNull();
        }

        private string _AllowedServices;

        public string AllowedServices
        {
            get => _AllowedServices.TrimIfNotNull();
            set => _AllowedServices = value.TrimIfNotNull();
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
            RuleFor(config => config.ThirdPartyServiceProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ThirdPartyServiceProviderMessages.InvalidSetting);

            RuleFor(config => config.Name)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ThirdPartyServiceProviderMessages.InvalidName);
            RuleFor(config => config.AppID)
                    .Must(new Validation().IsValidString)
                    .WithMessage(ThirdPartyServiceProviderMessages.InvalidAppID);
            RuleFor(config => config.Token)
                     .Must(new Validation().IsValidString)
                     .WithMessage(ThirdPartyServiceProviderMessages.InvalidToken);

            RuleFor(config => config.DomainName)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ThirdPartyServiceProviderMessages.InvalidCallBackURL);
        }
    }
}