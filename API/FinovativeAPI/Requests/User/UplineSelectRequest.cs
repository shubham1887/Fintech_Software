using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class UplineSelectRequest
    {
        private int _UserTypeID;

        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }

        internal string IsValidModalState()
        {
            var validator = new UplineSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UplineSelectRequestValidator : AbstractValidator<UplineSelectRequest>
    {
        public UplineSelectRequestValidator()
        {
            RuleFor(x => x.UserTypeID)
               .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUserType);
        }
    }
}