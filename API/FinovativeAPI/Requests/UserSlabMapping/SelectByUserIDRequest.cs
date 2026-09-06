using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserSlabMapping
{
    public class SelectByUserIDRequest
    {
        private int _UserID;

        public int UserID { get => UserID; set => _UserID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByUserIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByUserIDRequestValidator : AbstractValidator<SelectByUserIDRequest>
    {
        public SelectByUserIDRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidUser);
        }
    }
}