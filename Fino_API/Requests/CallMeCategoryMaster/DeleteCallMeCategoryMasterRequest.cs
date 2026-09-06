using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeCategoryMaster
{
    public class DeleteCallMeCategoryMasterRequest
    {
        public int CallMeCategoryMasterID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new DeleteCallMeCategoryMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class DeleteCallMeCategoryMasterRequestValidator : AbstractValidator<DeleteCallMeCategoryMasterRequest>
    {
        public DeleteCallMeCategoryMasterRequestValidator()
        {
            RuleFor(x => x.CallMeCategoryMasterID)
                .GreaterThan(0).WithMessage(CallMeCategoryMasterMessages.InvalidValue);
        }
    }
}