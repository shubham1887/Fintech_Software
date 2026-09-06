using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EZYPayout
{
    public class AddRequest
    {
        private string _Contact;

        public string Contact
        {
            get => _Contact.TrimIfNotNull();
            set => _Contact = value.TrimIfNotNull();
        }

        private string _ContactName;

        public string ContactName
        {
            get => _ContactName;
            set => _ContactName = value;
        }

        internal string IsValidModalState()
        {
            var validator = new AddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddRequestValidator : AbstractValidator<AddRequest>
    {
        public AddRequestValidator()
        {
            //RuleFor(x => x.ContactName)
            //    .Must(new Validation().IsValidString)

            //RuleFor(x => x.Contact)
            // .Must(new Validation().IsValidString)
        }
    }
}