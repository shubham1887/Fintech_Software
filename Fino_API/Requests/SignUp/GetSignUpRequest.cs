using FluentValidation;
namespace FintechApp.Requests.SignUp { public class GetSignUpRequest { public int SignUpID { get; set; } } public class GetSignUpRequestValidator : AbstractValidator<GetSignUpRequest> { public GetSignUpRequestValidator() { RuleFor(x => x.SignUpID).GreaterThan(0); } } }
