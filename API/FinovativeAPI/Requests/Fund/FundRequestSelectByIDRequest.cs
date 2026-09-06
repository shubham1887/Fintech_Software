using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class FundRequestSelectByIDRequest
    {
        private int _FundTransactionID;

        public int FundTransactionID { get => _FundTransactionID; set => _FundTransactionID = value; }

        internal string IsValidModalState()
        {
            var validator = new FundRequestSelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FundRequestSelectByIDRequestValidator : AbstractValidator<FundRequestSelectByIDRequest>
    {
        public FundRequestSelectByIDRequestValidator()
        {
            RuleFor(x => x.FundTransactionID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.FundMessage.InvalidID);
        }
    }
}