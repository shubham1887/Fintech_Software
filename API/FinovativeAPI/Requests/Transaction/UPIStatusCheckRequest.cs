using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Transaction
{
    public class UPIStatusCheckRequest
    {
        private string _APIUserRequestID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UPIStatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UPIStatusCheckRequestValidator : AbstractValidator<UPIStatusCheckRequest>
    {
        public UPIStatusCheckRequestValidator()
        {
            RuleFor(x => x.APIUserRequestID)
                .Must(new Validation().IsValidString)
                .WithMessage("Please enter valid requestID");
        }
    }
}