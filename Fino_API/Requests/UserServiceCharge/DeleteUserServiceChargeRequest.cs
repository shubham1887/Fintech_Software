using FluentValidation;
namespace FintechApp.Requests.UserServiceCharge
{
    public class DeleteUserServiceChargeRequest { public int UserServiceChargeID { get; set; } }
    public class DeleteUserServiceChargeRequestValidator : AbstractValidator<DeleteUserServiceChargeRequest> { public DeleteUserServiceChargeRequestValidator() { RuleFor(x => x.UserServiceChargeID).GreaterThan(0); } }
}
