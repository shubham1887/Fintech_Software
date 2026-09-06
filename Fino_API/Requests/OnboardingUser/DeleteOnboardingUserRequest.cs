using FluentValidation;
namespace FintechApp.Requests.OnboardingUser { public class DeleteOnboardingUserRequest { public int OnboardingUserID { get; set; } } public class DeleteOnboardingUserRequestValidator : AbstractValidator<DeleteOnboardingUserRequest> { public DeleteOnboardingUserRequestValidator() { RuleFor(x => x.OnboardingUserID).GreaterThan(0); } } }
