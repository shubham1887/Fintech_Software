using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderStateConfiguration
{
    public class UpdateRequest
    {
        private int _ProviderStateConfigurationID;

        public int ProviderStateConfigurationID
        {
            get => _ProviderStateConfigurationID;
            set => _ProviderStateConfigurationID = value;
        }

        private string _StateCode;

        public string StateCode
        {
            get => _StateCode.TrimIfNotNull();
            set => _StateCode = value.TrimIfNotNull();
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
            RuleFor(x => x.ProviderStateConfigurationID)
                     .Must(new Validation().IsValidInt)
                     .WithMessage(ProviderConfigMessage.InvalidConfigurationID);
        }
    }
}