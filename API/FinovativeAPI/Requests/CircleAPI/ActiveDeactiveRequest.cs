using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleAPI
{
    public class ActiveDeactiveRequest
    {
        private int _CircleAPIMasterID;

        public int CircleAPIMasterID { get => _CircleAPIMasterID; set => _CircleAPIMasterID = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        private bool _IsActive;

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.CircleAPIMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(CircleAPIMessages.InvalidUrl);
        }
    }
}