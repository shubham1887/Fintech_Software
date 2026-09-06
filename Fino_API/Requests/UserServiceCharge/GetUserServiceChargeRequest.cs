using FluentValidation;
namespace FintechApp.Requests.UserServiceCharge
{
    public class GetUserServiceChargeRequest { public int UserServiceChargeID { get; set; } public int UserID { get; set; } }
    public class GetUserServiceChargeRequestValidator : AbstractValidator<GetUserServiceChargeRequest> { public GetUserServiceChargeRequestValidator() { RuleFor(x => x.UserServiceChargeID).GreaterThan(0); } }
}
