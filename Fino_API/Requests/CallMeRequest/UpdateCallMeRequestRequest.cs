using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeRequest
{
    public class UpdateCallMeRequestRequest
    {
        public int CallMeRequestID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new UpdateCallMeRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class UpdateCallMeRequestValidator : AbstractValidator<UpdateCallMeRequestRequest>
    {
        public UpdateCallMeRequestValidator()
        {
            RuleFor(x => x.CallMeRequestID)
                .GreaterThan(0).WithMessage(CallMeRequestMessages.InvalidValue);
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(CallMeRequestMessages.RequiredField);
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(CallMeRequestMessages.RequiredField);
        }
    }
}