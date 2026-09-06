using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeCategoryMaster
{
    public class UpdateCallMeCategoryMasterRequest
    {
        public int CallMeCategoryMasterID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new UpdateCallMeCategoryMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class UpdateCallMeCategoryMasterRequestValidator : AbstractValidator<UpdateCallMeCategoryMasterRequest>
    {
        public UpdateCallMeCategoryMasterRequestValidator()
        {
            RuleFor(x => x.CallMeCategoryMasterID)
                .GreaterThan(0).WithMessage(CallMeCategoryMasterMessages.InvalidValue);
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(CallMeCategoryMasterMessages.RequiredField);
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(CallMeCategoryMasterMessages.RequiredField);
        }
    }
}