using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sequence
{
    public class SelectByOperatorID
    {
        private int _OperatorID;

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByOperatorIDValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByOperatorIDValidator : AbstractValidator<SelectByOperatorID>
    {
        public SelectByOperatorIDValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidOperatorID);
        }
    }
}