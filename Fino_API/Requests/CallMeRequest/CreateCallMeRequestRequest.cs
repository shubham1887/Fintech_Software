using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.CallMeRequest
{
    public class CreateCallMeRequestRequest
    {
        public int CallMeCategoryMasterID { get; set; }
        public int UserID { get; set; }
        public int UplineUserID { get; set; }
        public string UplineRemark { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new CreateCallMeRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class CreateCallMeRequestValidator : AbstractValidator<CreateCallMeRequestRequest>
    {
        private static readonly string[] AllowedStatuses = { "pending", "resolved", "cancelled" };

        public CreateCallMeRequestValidator()
        {
            RuleFor(x => x.CallMeCategoryMasterID)
                .GreaterThan(0).WithMessage(CallMeRequestMessages.InvalidValue);
            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(CallMeRequestMessages.InvalidValue);
            RuleFor(x => x.UplineUserID)
                .GreaterThan(0).WithMessage(CallMeRequestMessages.InvalidValue);
            RuleFor(x => x.UplineRemark)
                .MaximumLength(200).WithMessage(CallMeRequestMessages.InvalidLength);
            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage(CallMeRequestMessages.InvalidLength);
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(CallMeRequestMessages.RequiredField)
                .MaximumLength(50).WithMessage(CallMeRequestMessages.InvalidLength)
                .Must(s => AllowedStatuses.Contains(s.ToLower())).WithMessage(CallMeRequestMessages.InvalidValue);
        }
    }
}