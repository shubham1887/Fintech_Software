using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Transaction
{
    public class UPITransferRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _VPA;

        public string VPA
        {
            get => _VPA.TrimIfNotNull();
            set => _VPA = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

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
            var validator = new UPITransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UPITransferRequestValidator : AbstractValidator<UPITransferRequest>
    {
        public UPITransferRequestValidator()
        {
            RuleFor(x => x.VPA)
                .Must(new Validation().IsValidString)
                .WithMessage("Please enter valid vpa");

            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage("Please enter valid name");

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage("Please enter valid amount");

            RuleFor(x => x.APIUserRequestID)
                .Must(new Validation().IsValidString)
                .WithMessage("Please enter valid requestID");
        }
    }
}