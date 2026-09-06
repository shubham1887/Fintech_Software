using FluentValidation;
namespace FintechApp.Requests.VirtualBalance { public class DeleteVirtualBalanceRequest { public int VirtualBalanceID { get; set; } } public class DeleteVirtualBalanceRequestValidator : AbstractValidator<DeleteVirtualBalanceRequest> { public DeleteVirtualBalanceRequestValidator() { RuleFor(x => x.VirtualBalanceID).GreaterThan(0); } } }
