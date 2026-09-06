using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserwiseSaleTarget
{
    public class DeleteRequest
    {
        private int _UserwiseSaleTargetID;

        public int UserwiseSaleTargetID
        {
            get => _UserwiseSaleTargetID;
            set => _UserwiseSaleTargetID = value;
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

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.UserwiseSaleTargetID)
                          .Must(new Validation().IsValidInt)
                          .WithMessage(Message.UserwiseSaleTargetMessages.Invalidtarget);
        }
    }
}