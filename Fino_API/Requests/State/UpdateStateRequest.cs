using FluentValidation;

namespace FintechApp.Requests.State
{
    public class UpdateStateRequest
    {
        public int StateID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateStateRequestValidator : AbstractValidator<UpdateStateRequest>
    {
        public UpdateStateRequestValidator()
        {
            RuleFor(x => x.StateID).GreaterThan(0);
            RuleFor(x => x.StateName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.StateCode).NotEmpty().MaximumLength(10);
        }
    }
}
