using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sequence
{
    public class SelectByUserIDWithOperatorID
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByUserIDWithOperatorIDValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByUserIDWithOperatorIDValidator : AbstractValidator<SelectByUserIDWithOperatorID>
    {
        public SelectByUserIDWithOperatorIDValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidOperatorID);
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.UserMessage.InvalidUserID);
        }
    }
}