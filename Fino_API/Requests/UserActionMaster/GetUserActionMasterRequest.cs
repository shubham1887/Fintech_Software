using FluentValidation;
namespace FintechApp.Requests.UserActionMaster { public class GetUserActionMasterRequest { public int UserActionMasterID { get; set; } public int UserID { get; set; } } public class GetUserActionMasterRequestValidator : AbstractValidator<GetUserActionMasterRequest> { public GetUserActionMasterRequestValidator() { RuleFor(x => x.UserActionMasterID).GreaterThan(0); } } }
