using FluentValidation;
namespace FintechApp.Requests.AuthorizationMaster { public class DeleteAuthorizationMasterRequest { public int AuthorizationMasterID { get; set; } } public class DeleteAuthorizationMasterRequestValidator : AbstractValidator<DeleteAuthorizationMasterRequest> { public DeleteAuthorizationMasterRequestValidator() { RuleFor(x => x.AuthorizationMasterID).GreaterThan(0); } } }
