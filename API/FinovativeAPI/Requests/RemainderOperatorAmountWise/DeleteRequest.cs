using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RemainderOperatorAmountWise
{
    public class DeleteRequest
    {
        private int _RemainderOperatorAmountWiseID;

        public int RemainderOperatorAmountWiseID
        {
            get => _RemainderOperatorAmountWiseID;
            set => _RemainderOperatorAmountWiseID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.RemainderOperatorAmountWiseID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InvalidID);
        }
    }
}