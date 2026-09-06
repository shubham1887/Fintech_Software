using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class SelectByDeveloperRequest
    {
        private string _Developer;

        public string Developer
        {
            get => _Developer.TrimIfNotNull();
            set => _Developer = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByDeveloperRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByDeveloperRequestValidator : AbstractValidator<SelectByDeveloperRequest>
    {
        public SelectByDeveloperRequestValidator()
        {
            RuleFor(x => x.Developer)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.ProviderResponseMessage.InvalidDeveloper);
        }
    }
}