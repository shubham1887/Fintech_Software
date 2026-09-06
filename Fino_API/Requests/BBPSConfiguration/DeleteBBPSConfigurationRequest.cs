using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.BBPSConfiguration
{
    public class DeleteBBPSConfigurationRequest
    {
        public int BBPSConfigurationID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new DeleteBBPSConfigurationRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class DeleteBBPSConfigurationRequestValidator : AbstractValidator<DeleteBBPSConfigurationRequest>
    {
        public DeleteBBPSConfigurationRequestValidator()
        {
            RuleFor(x => x.BBPSConfigurationID)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
        }
    }
}