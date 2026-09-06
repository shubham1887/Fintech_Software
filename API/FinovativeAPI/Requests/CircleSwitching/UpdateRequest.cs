using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.Response.CircleSwitching;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleSwitching
{
    public class UpdateRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        public List<OperatorCircleMaster> Circles = new List<OperatorCircleMaster>();

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ProviderMessage.InValidProvider);

            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InValidOperator);
        }
    }
}