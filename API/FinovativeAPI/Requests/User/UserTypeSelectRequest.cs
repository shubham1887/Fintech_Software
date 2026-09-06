using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class UserTypeSelectRequest
    {
        private int _UserTypeID;

        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }
        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UserTypeSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UserTypeSelectRequestValidator : AbstractValidator<UserTypeSelectRequest>
    {
        public UserTypeSelectRequestValidator()
        {
            RuleFor(x => x.UserTypeID)
               .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUserType);
        }
    }
}