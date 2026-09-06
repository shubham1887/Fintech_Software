

using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.IPMaster
{
    public class CreateIPMasterRequest
    {
        public string IP { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Status { get; set; } = string.Empty;
        internal string IsValidModalState()
        {
            var validator = new CreateIPMasterRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }
    public class CreateIPMasterRequestValidator : AbstractValidator<CreateIPMasterRequest>
    {
        private static readonly string[] AllowedStatus = { "active", "inactive", "blocked" };

        public CreateIPMasterRequestValidator()
        {
            RuleFor(x => x.IP)
                .NotEmpty().WithMessage(IPMasterMessages.RequiredField)
                .MaximumLength(100).WithMessage(IPMasterMessages.InvalidLength)
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").WithMessage(IPMasterMessages.InvalidIPFormat);

            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(IPMasterMessages.InvalidValue);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(IPMasterMessages.RequiredField)
                .Must(s => AllowedStatus.Contains(s.ToLower())).WithMessage(IPMasterMessages.InvalidStatus);
        }
    }
}
