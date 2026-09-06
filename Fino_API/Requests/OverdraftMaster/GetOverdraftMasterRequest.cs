using FluentValidation;
namespace FintechApp.Requests.OverdraftMaster { public class GetOverdraftMasterRequest { public int OverdraftMasterID { get; set; } public int UserID { get; set; } } public class GetOverdraftMasterRequestValidator : AbstractValidator<GetOverdraftMasterRequest> { public GetOverdraftMasterRequestValidator() { RuleFor(x => x.OverdraftMasterID).GreaterThan(0); } } }
