using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfig
{
    public class SelectByConfigurationIDRequest
    {
        private int _ConfigurationSampleID;

        public int ConfigurationSampleID
        {
            get => _ConfigurationSampleID;
            set => _ConfigurationSampleID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByConfigurationIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByConfigurationIDRequestValidator : AbstractValidator<SelectByConfigurationIDRequest>
    {
        public SelectByConfigurationIDRequestValidator()
        {
            RuleFor(x => x.ConfigurationSampleID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.ProviderMessage.InValidProvider);
        }
    }
}