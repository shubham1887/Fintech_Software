using FluentValidation;
namespace FintechApp.Requests.VirtualAccountMaster { public class GetVirtualAccountMasterRequest { public int VirtualAccountMasterID { get; set; } } public class GetVirtualAccountMasterRequestValidator : AbstractValidator<GetVirtualAccountMasterRequest> { public GetVirtualAccountMasterRequestValidator() { RuleFor(x => x.VirtualAccountMasterID).GreaterThan(0); } } }
