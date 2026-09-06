using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class SelectByIDRequest
    {
        private int _OverdraftDetailID;

        public int OverdraftDetailID { get => _OverdraftDetailID; set => _OverdraftDetailID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.OverdraftDetailID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.FundMessage.InvalidID);
        }
    }
}