using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserBankDetails
{
    public class SelectRequest
    {
        private int _UserBankDetailsID;

        public int UserBankDetailsID
        {
            get => _UserBankDetailsID;
            set => _UserBankDetailsID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.UserBankDetailsID)
           .Must(new Validation().IsValidInt)
           .WithMessage(UserBankDetailsMessage.InvalidID);
        }
    }
}