using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeCategoryMaster
{
    public class CreateCallMeCategoryMasterRequest
    {
        public string Name { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new CreateCallMeCategoryMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class CreateCallMeCategoryMasterRequestValidator : AbstractValidator<CreateCallMeCategoryMasterRequest>
    {
        public CreateCallMeCategoryMasterRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(CallMeCategoryMasterMessages.RequiredField)
                .MaximumLength(200).WithMessage(CallMeCategoryMasterMessages.InvalidLength);
        }
    }
}