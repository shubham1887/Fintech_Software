using FluentValidation;
namespace FintechApp.Requests.UserBalanceCapping
{
    public class CreateUserBalanceCappingRequest { public int UserID { get; set; } public int WalletID { get; set; } public decimal Amount { get; set; } public bool IsDelete { get; set; } }
    public class CreateUserBalanceCappingRequestValidator : AbstractValidator<CreateUserBalanceCappingRequest> { public CreateUserBalanceCappingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.WalletID).GreaterThan(0); } }
}

