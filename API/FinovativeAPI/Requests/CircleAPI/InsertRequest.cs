using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleAPI
{
    public class InsertRequest
    {
        private int _CircleAPIMasterID;

        public int CircleAPIMasterID
        {
            get => _CircleAPIMasterID;
            set => _CircleAPIMasterID = value;
        }

        private string _CircleAPIURL;

        public string CircleAPIURL
        {
            get => _CircleAPIURL.TrimIfNotNull();
            set => _CircleAPIURL = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new RequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class RequestValidator : AbstractValidator<InsertRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.CircleAPIURL)
              .Must(new Validation().IsValidString)
              .WithMessage(SMSMasterMessage.InvalidSmsUrl);
        }
    }
}