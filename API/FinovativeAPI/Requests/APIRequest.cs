using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests
{
    public class APIRequest
    {
        private string _Request;
        private string _KeyData;

        public string Request
        {
            get => _Request.TrimIfNotNull();
            set => _Request = value.TrimIfNotNull();
        }

        public string KeyData
        {
            get => _KeyData.TrimIfNotNull();
            set => _KeyData = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new APIRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class APIRequestValidator : AbstractValidator<APIRequest>
    {
        public APIRequestValidator()
        {
            RuleFor(x => x.Request)
              .Must(new Validation().IsValidString)
              .WithMessage(Messages.InValidRequest);

            RuleFor(x => x.KeyData)
             .Must(new Validation().IsValidString)
             .WithMessage(Messages.InValidKeyData);
        }
    }
}