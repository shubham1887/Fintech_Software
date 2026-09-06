using FluentValidation;
namespace FintechApp.Requests.CircleWiseUserWiseOperatorOff
{
    public class CreateCircleWiseUserWiseOperatorOffRequest { public int UserID { get; set; } public int StateID { get; set; } public int OperatorID { get; set; } }
    public class CreateCircleWiseUserWiseOperatorOffRequestValidator : AbstractValidator<CreateCircleWiseUserWiseOperatorOffRequest> { public CreateCircleWiseUserWiseOperatorOffRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
