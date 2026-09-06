using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RangeWiseSequence
{
    public class SelectByIDRequest
    {
        private int _RangeWiseSequenceID;

        public int RangeWiseSequenceID
        {
            get => _RangeWiseSequenceID;
            set => _RangeWiseSequenceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.RangeWiseSequenceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.RangeWiseSequenceMessage.InvalidRange);
        }
    }
}