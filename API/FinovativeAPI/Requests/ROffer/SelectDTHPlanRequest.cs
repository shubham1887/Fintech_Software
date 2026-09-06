using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ROffer
{
    public class SelectDTHPlanRequest
    {
        private int _OperatorID;

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectDTHPlanRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class SelectDTHPlanRequestValidator : AbstractValidator<SelectDTHPlanRequest>
    {
        private readonly Validation validation = new Validation();

        public SelectDTHPlanRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(validation.IsValidInt)
                .WithMessage(Project.Message.OperatorMessage.InValidOperator);
        }
    }
}