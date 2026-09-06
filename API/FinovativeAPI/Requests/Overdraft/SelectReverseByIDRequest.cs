using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Overdraft
{
    public class SelectReverseByIDRequest
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID { get => _OverdraftMasterID; set => _OverdraftMasterID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectReverseByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectReverseByIDRequestValidator : AbstractValidator<SelectReverseByIDRequest>
    {
        public SelectReverseByIDRequestValidator()
        {
            RuleFor(x => x.OverdraftMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.FundMessage.InvalidID);
        }
    }
}