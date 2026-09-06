using FluentValidation;
namespace FintechApp.Requests.SignUp { public class DeleteSignUpRequest { public int SignUpID { get; set; } } public class DeleteSignUpRequestValidator : AbstractValidator<DeleteSignUpRequest> { public DeleteSignUpRequestValidator() { RuleFor(x => x.SignUpID).GreaterThan(0); } } }
