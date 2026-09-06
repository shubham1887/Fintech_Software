using FluentValidation;
namespace FintechApp.Requests.UserServiceCharge
{
    public class CreateUserServiceChargeRequest { public int UserID { get; set; } public int ServiceID { get; set; } public decimal Charge { get; set; } public string Remark { get; set; } = string.Empty; }
    public class CreateUserServiceChargeRequestValidator : AbstractValidator<CreateUserServiceChargeRequest> { public CreateUserServiceChargeRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.ServiceID).GreaterThan(0); } }
}
