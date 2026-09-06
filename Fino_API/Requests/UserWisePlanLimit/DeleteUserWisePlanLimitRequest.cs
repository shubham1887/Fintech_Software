using FluentValidation;
namespace FintechApp.Requests.UserWisePlanLimit { public class DeleteUserWisePlanLimitRequest { public int UserWisePlanLimitID { get; set; } } public class DeleteUserWisePlanLimitRequestValidator : AbstractValidator<DeleteUserWisePlanLimitRequest> { public DeleteUserWisePlanLimitRequestValidator() { RuleFor(x => x.UserWisePlanLimitID).GreaterThan(0); } } }
