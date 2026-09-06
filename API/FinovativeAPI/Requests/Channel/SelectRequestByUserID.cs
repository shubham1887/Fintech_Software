using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class SelectRequestByUserID
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestByUserIDValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestByUserIDValidator : AbstractValidator<SelectRequestByUserID>
    {
        public SelectRequestByUserIDValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabDetailMessage.InvalidUserID);
        }
    }
}