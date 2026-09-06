using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.WhiteLabelPaytmccf
{
    public class SelectRequest
    {
        private string _Mobile;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _AuthCode;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AuthCode
        {
            get
            {
                return _AuthCode.TrimIfNotNull();
            }
            set
            {
                _AuthCode = value.TrimIfNotNull();
            }
        }

        private int _ProviderID;

        internal int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }


        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }


    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.Mobile)
                .Must(new Validation().IsValidString)
                .WithMessage(TransactionMessage.InvalidMobileNumber);

            RuleFor(x => x.AuthCode)
                 .Must(new Validation().IsValidString)
                 .WithMessage(TransactionMessage.InvalidAuthCode);
        }
    }
}
