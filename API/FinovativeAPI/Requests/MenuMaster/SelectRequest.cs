using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MenuMaster
{
    public class SelectRequest
    {
        private int _UserTypeID;

        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }

        internal string IsValidModalState()
        {
            var validator = new UserTypeSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserTypeSelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public UserTypeSelectRequestValidator()
        {
            RuleFor(x => x.UserTypeID)
               .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUserType);
        }
    }
}