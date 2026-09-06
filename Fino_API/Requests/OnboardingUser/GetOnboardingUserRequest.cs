using FluentValidation;
namespace FintechApp.Requests.OnboardingUser { public class GetOnboardingUserRequest { public int OnboardingUserID { get; set; } } public class GetOnboardingUserRequestValidator : AbstractValidator<GetOnboardingUserRequest> { public GetOnboardingUserRequestValidator() { RuleFor(x => x.OnboardingUserID).GreaterThan(0); } } }
