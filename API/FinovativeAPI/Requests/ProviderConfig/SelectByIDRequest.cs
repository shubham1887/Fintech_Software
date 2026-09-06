using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfig
{
    public class SelectByIDRequest
    {
        private int _ConfigurationID;
        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
        public int ConfigurationID { get => _ConfigurationID; set => _ConfigurationID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"API operator onoff with ConfigurationID:{ConfigurationID},IsActive:{IsActive}";
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.ConfigurationID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ProviderMessage.InValidConfigID);
        }
    }
}