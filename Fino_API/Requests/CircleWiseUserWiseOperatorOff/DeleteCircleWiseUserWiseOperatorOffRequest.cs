using FluentValidation;
namespace FintechApp.Requests.CircleWiseUserWiseOperatorOff
{
    public class DeleteCircleWiseUserWiseOperatorOffRequest { public int CircleWiseUserWiseOperatorOffID { get; set; } }
    public class DeleteCircleWiseUserWiseOperatorOffRequestValidator : AbstractValidator<DeleteCircleWiseUserWiseOperatorOffRequest> { public DeleteCircleWiseUserWiseOperatorOffRequestValidator() { RuleFor(x => x.CircleWiseUserWiseOperatorOffID).GreaterThan(0); } }
}
