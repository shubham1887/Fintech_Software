using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Provider
{
    public class ProviderKeyRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ProviderKeyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ProviderKeyRequestValidator : AbstractValidator<ProviderKeyRequest>
    {
        public ProviderKeyRequestValidator()
        {
            RuleFor(x => x.DMTName)
                .Must(new Validation().IsValidString)
                 .WithMessage(DMTSettingMessages.InvalidActiveDMT);
        }
    }
}