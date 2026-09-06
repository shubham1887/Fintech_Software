using FluentValidation;
namespace FintechApp.Requests.UserServiceCharge
{
    public class UpdateUserServiceChargeRequest { public int UserServiceChargeID { get; set; } public int UserID { get; set; } public int ServiceID { get; set; } public decimal Charge { get; set; } public string Remark { get; set; } = string.Empty; }
    public class UpdateUserServiceChargeRequestValidator : AbstractValidator<UpdateUserServiceChargeRequest> { public UpdateUserServiceChargeRequestValidator() { RuleFor(x => x.UserServiceChargeID).GreaterThan(0); } }
}
