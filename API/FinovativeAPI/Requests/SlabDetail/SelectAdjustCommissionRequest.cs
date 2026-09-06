using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class SelectAdjustCommissionRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectAdjustCommissionRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class SelectAdjustCommissionRequestValidator : AbstractValidator<SelectAdjustCommissionRequest>
        {
            public SelectAdjustCommissionRequestValidator()
            {
                RuleFor(x => x.OperatorID)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(Message.OperatorMessage.InValidOperator);
                RuleFor(x => x.UserTypeID)
                   .Must(new Validation().IsValidInt)
                   .WithMessage(Message.UserMessage.InvalidUserType);
            }
        }
    }
}