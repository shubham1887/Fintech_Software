using FluentValidation;
namespace FintechApp.Requests.AuthorizationMaster { public class GetAuthorizationMasterRequest { public int AuthorizationMasterID { get; set; } } public class GetAuthorizationMasterRequestValidator : AbstractValidator<GetAuthorizationMasterRequest> { public GetAuthorizationMasterRequestValidator() { RuleFor(x => x.AuthorizationMasterID).GreaterThan(0); } } }
