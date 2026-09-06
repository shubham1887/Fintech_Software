using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RedirectUrl
{
    public class SelectRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name;
            set => _Name = value;
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
            RuleFor(x => x.Name)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.RedirectUrlMessage.InvalidName);
        }
    }
}