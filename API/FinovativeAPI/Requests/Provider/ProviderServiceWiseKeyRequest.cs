using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Provider
{
    public class ProviderServiceWiseKeyRequest
    {
        private string _Name;

        [DefaultValue("")]
        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ProviderServiceWiseKeyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ProviderServiceWiseKeyRequestValidator : AbstractValidator<ProviderServiceWiseKeyRequest>
    {
        public ProviderServiceWiseKeyRequestValidator()
        {
            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                 .WithMessage(DMTSettingMessages.InvalidActiveDMT);
        }
    }
}