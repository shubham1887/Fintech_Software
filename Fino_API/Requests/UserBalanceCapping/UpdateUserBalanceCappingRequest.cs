using FluentValidation;
namespace FintechApp.Requests.UserBalanceCapping
{
    public class UpdateUserBalanceCappingRequest { public int UserBalanceCappingID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } public decimal Amount { get; set; } public bool IsDelete { get; set; } }
    public class UpdateUserBalanceCappingRequestValidator : AbstractValidator<UpdateUserBalanceCappingRequest> { public UpdateUserBalanceCappingRequestValidator() { RuleFor(x => x.UserBalanceCappingID).GreaterThan(0); } }
}
