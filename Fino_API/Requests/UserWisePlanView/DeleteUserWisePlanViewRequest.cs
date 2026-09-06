using FluentValidation;
namespace FintechApp.Requests.UserWisePlanView { public class DeleteUserWisePlanViewRequest { public int UserWisePlanViewID { get; set; } } public class DeleteUserWisePlanViewRequestValidator : AbstractValidator<DeleteUserWisePlanViewRequest> { public DeleteUserWisePlanViewRequestValidator() { RuleFor(x => x.UserWisePlanViewID).GreaterThan(0); } } }
