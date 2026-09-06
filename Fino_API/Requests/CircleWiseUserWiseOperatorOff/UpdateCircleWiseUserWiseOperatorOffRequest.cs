using FluentValidation;
namespace FintechApp.Requests.CircleWiseUserWiseOperatorOff
{
    public class UpdateCircleWiseUserWiseOperatorOffRequest { public int CircleWiseUserWiseOperatorOffID { get; set; } public int UserID { get; set; } public int StateID { get; set; } public int OperatorID { get; set; } }
    public class UpdateCircleWiseUserWiseOperatorOffRequestValidator : AbstractValidator<UpdateCircleWiseUserWiseOperatorOffRequest> { public UpdateCircleWiseUserWiseOperatorOffRequestValidator() { RuleFor(x => x.CircleWiseUserWiseOperatorOffID).GreaterThan(0); } }
}
