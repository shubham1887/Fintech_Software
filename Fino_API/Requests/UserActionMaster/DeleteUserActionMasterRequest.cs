using FluentValidation;
namespace FintechApp.Requests.UserActionMaster { public class DeleteUserActionMasterRequest { public int UserActionMasterID { get; set; } } public class DeleteUserActionMasterRequestValidator : AbstractValidator<DeleteUserActionMasterRequest> { public DeleteUserActionMasterRequestValidator() { RuleFor(x => x.UserActionMasterID).GreaterThan(0); } } }
