using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeRequest
{
    public class DeleteCallMeRequestRequest
    {
        public int CallMeRequestID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new DeleteCallMeRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class DeleteCallMeRequestValidator : AbstractValidator<DeleteCallMeRequestRequest>
    {
        public DeleteCallMeRequestValidator()
        {
            RuleFor(x => x.CallMeRequestID)
                .GreaterThan(0).WithMessage(CallMeRequestMessages.InvalidValue);
        }
    }
}