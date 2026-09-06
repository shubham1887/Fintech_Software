using FluentValidation;
namespace FintechApp.Requests.UserPlanUtilization { public class DeleteUserPlanUtilizationRequest { public int UserPlanUtilizationID { get; set; } } public class DeleteUserPlanUtilizationRequestValidator : AbstractValidator<DeleteUserPlanUtilizationRequest> { public DeleteUserPlanUtilizationRequestValidator() { RuleFor(x => x.UserPlanUtilizationID).GreaterThan(0); } } }
