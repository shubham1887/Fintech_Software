using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.BBPSConfiguration
{
    public class SelectByOperatorIDRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByOperatorIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByOperatorIDRequestValidator : AbstractValidator<SelectByOperatorIDRequest>
    {
        public SelectByOperatorIDRequestValidator()
        {
            RuleFor(config => config.OperatorID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(BBPSConfigurationMessages.InvalidOperatorID);
        }
    }
}