using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.BBPSConfiguration
{
    public class DeleteRequest
    {
        private int _ConfigurationID;

        public int ConfigurationID { get => _ConfigurationID; set => _ConfigurationID = value; }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(config => config.ConfigurationID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(BBPSConfigurationMessages.InvalidConfiguration);
        }
    }
}