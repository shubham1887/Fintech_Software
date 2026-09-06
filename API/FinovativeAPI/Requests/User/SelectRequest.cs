using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class SelectRequest
    {
        private int _UserID;

        public int UserID { get => _UserID; set => _UserID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUser);
        }
    }
}