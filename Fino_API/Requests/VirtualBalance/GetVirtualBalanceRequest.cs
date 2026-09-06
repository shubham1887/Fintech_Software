using FluentValidation;
namespace FintechApp.Requests.VirtualBalance { public class GetVirtualBalanceRequest { public int VirtualBalanceID { get; set; } } public class GetVirtualBalanceRequestValidator : AbstractValidator<GetVirtualBalanceRequest> { public GetVirtualBalanceRequestValidator() { RuleFor(x => x.VirtualBalanceID).GreaterThan(0); } } }
