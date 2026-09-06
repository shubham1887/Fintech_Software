using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ROffer
{
    public class SelectPlanRequest
    {
        private int _OperatorID;

        private string _Circle;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectPlanRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class SelectPlanRequestValidator : AbstractValidator<SelectPlanRequest>
    {
        private readonly Validation validation = new Validation();

        public SelectPlanRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(validation.IsValidInt)
                .WithMessage(Message.OperatorMessage.InValidOperator);

            RuleFor(x => x.Circle)
              .Must(validation.IsValidString)
              .WithMessage(Message.OperatorMessage.InValidCircle);
        }
    }
}