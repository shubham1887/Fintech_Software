using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests
{
    public class PublicAPIRequest
    {
        private string _Request;

        public string Request
        {
            get => _Request.TrimIfNotNull();
            set => _Request = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new PublicAPIRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class PublicAPIRequestValidator : AbstractValidator<PublicAPIRequest>
    {
        public PublicAPIRequestValidator()
        {
            RuleFor(x => x.Request)
              .Must(new Validation().IsValidString)
              .WithMessage(Messages.InValidRequest);
        }
    }
}