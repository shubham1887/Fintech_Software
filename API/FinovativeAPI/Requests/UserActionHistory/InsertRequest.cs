using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserActionHistory
{
    public class InsertRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Action;

        public string Action
        {
            get => _Action.TrimIfNotNull();
            set => _Action = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUserID);

            RuleFor(x => x.Action)
                .Must(new Validation().IsValidString)
                .WithMessage(UserActionHistoryMessage.InvalidAction);
            RuleFor(x => x.Description)
              .Must(new Validation().IsValidString)
              .WithMessage(UserActionHistoryMessage.InvalidDescription);
        }
    }
}