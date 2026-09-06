using FluentValidation;
namespace FintechApp.Requests.CircleWiseUserWiseOperatorOff
{
    public class GetCircleWiseUserWiseOperatorOffRequest { public int CircleWiseUserWiseOperatorOffID { get; set; }
    public int UserID { get; set; } }
    public class GetCircleWiseUserWiseOperatorOffRequestValidator : AbstractValidator<GetCircleWiseUserWiseOperatorOffRequest> { public GetCircleWiseUserWiseOperatorOffRequestValidator() { RuleFor(x => x.CircleWiseUserWiseOperatorOffID).GreaterThan(0); } }
}
