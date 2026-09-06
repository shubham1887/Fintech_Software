using FluentValidation;
using FluentValidation.Results;
using FintechApp.Messages;
using FintechApp.StaticModels;

namespace FintechApp.Requests.IPMaster
{
    public class UpdateIPMasterRequest
    {
        public int IpMasterID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new UpdateIPMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class UpdateIPMasterRequestValidator : AbstractValidator<UpdateIPMasterRequest>
    {
        private static readonly string[] AllowedFields = { "ip", "status", "userid" };

        public UpdateIPMasterRequestValidator()
        {
            RuleFor(x => x.IpMasterID)
                .GreaterThan(0).WithMessage(IPMasterMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(IPMasterMessages.RequiredField)
                .Must(f => AllowedFields.Contains(f.ToLower())).WithMessage(IPMasterMessages.InvalidFieldName);

            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(IPMasterMessages.RequiredField)
                .MaximumLength(500).WithMessage(IPMasterMessages.InvalidLength);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(IPMasterMessages.RequiredField)
                .MaximumLength(500).WithMessage(IPMasterMessages.InvalidLength);
        }
    }
}