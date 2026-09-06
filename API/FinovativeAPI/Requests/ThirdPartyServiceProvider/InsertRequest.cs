using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.ThirdPartyServiceProvider
{
    public class InsertRequest
    {
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

        private string _DomainName;

        public string DomainName
        {
            get => _DomainName.TrimIfNotNull();
            set => _DomainName = value.TrimIfNotNull();
        }

        private string _AppID;

        public string AppID
        {
            get => _AppID.TrimIfNotNull();
            set => _AppID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(config => config.Name)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ThirdPartyServiceProviderMessages.InvalidName);

            RuleFor(config => config.Token)
                     .Must(new Validation().IsValidString)
                     .WithMessage(ThirdPartyServiceProviderMessages.InvalidToken);

            RuleFor(config => config.DomainName)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ThirdPartyServiceProviderMessages.InvalidDomainName);
        }
    }
}