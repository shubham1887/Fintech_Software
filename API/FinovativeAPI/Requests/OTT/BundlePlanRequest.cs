using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OTT
{
    public class BundlePlanRequest
    {
        private string _OperatorCode;

        public string OperatorCode
        {
            get => _OperatorCode.TrimIfNotNull();
            set => _OperatorCode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BundlePlanRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BundlePlanRequestValidator : AbstractValidator<BundlePlanRequest>
    {
        public BundlePlanRequestValidator()
        {
            RuleFor(x => x.OperatorCode)
               .Must(new Validation().IsValidString)
               .WithMessage(OTTMessage.InValidOperator);
        }
    }
}