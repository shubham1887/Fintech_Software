using FluentValidation;

namespace FintechApp.Requests.CircleWiseCommission
{
    public class DeleteCircleWiseCommissionRequest { public int CircleWiseCommissionID { get; set; } }
    public class DeleteCircleWiseCommissionRequestValidator : AbstractValidator<DeleteCircleWiseCommissionRequest>
    {
        public DeleteCircleWiseCommissionRequestValidator() { RuleFor(x => x.CircleWiseCommissionID).GreaterThan(0); }
    }
}
