using FluentValidation;
namespace FintechApp.Requests.UserWisePlanView { public class GetUserWisePlanViewRequest { public int UserWisePlanViewID { get; set; } public int UserID { get; set; } } public class GetUserWisePlanViewRequestValidator : AbstractValidator<GetUserWisePlanViewRequest> { public GetUserWisePlanViewRequestValidator() { RuleFor(x => x.UserWisePlanViewID).GreaterThan(0); } } }
