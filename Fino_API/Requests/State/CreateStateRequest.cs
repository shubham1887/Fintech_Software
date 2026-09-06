using FluentValidation;

namespace FintechApp.Requests.State
{
    public class CreateStateRequest
    {
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateStateRequestValidator : AbstractValidator<CreateStateRequest>
    {
        public CreateStateRequestValidator()
        {
            RuleFor(x => x.StateName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.StateCode).NotEmpty().MaximumLength(10);
        }
    }
}
