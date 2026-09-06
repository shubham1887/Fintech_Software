using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RemainderOperatorAmountWise
{
    public class UpdateRequest
    {
        private int _Days;

        public int Days
        {
            get => _Days;
            set => _Days = value;
        }

        private int _RemainderOperatorAmountWiseID;

        public int RemainderOperatorAmountWiseID
        {
            get => _RemainderOperatorAmountWiseID;
            set => _RemainderOperatorAmountWiseID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.RemainderOperatorAmountWiseID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidID);

            RuleFor(x => x.Days)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InvalidDaysCount);
        }
    }
}