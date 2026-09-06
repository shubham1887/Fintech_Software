using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfig
{
    public class UpdateSampleConfigurationRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ConfigurationSampleID;

        public int ConfigurationSampleID
        {
            get => _ConfigurationSampleID;
            set => _ConfigurationSampleID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateSampleConfigurationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateSampleConfigurationRequestValidator : AbstractValidator<UpdateSampleConfigurationRequest>
    {
        public UpdateSampleConfigurationRequestValidator()
        {
            RuleFor(x => x.ProviderID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderConfigMessage.InvalidProvider);

            RuleFor(x => x.ConfigurationSampleID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderConfigMessage.InvalidSampleConfiguration);
        }
    }
}