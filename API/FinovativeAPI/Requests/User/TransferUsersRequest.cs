using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class TransferUsersRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _NewParentID;

        public int NewParentID
        {
            get => _NewParentID;
            set => _NewParentID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new TransferUsersRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransferUsersRequestValidator : AbstractValidator<TransferUsersRequest>
    {
        public TransferUsersRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidUser);

            RuleFor(x => x.NewParentID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidParent);
        }
    }
}