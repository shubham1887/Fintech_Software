using FluentValidation;
namespace FintechApp.Requests.FailedSignUpData { public class GetFailedSignUpDataRequest { public int FailedSignUpDataID { get; set; } } public class GetFailedSignUpDataRequestValidator : AbstractValidator<GetFailedSignUpDataRequest> { public GetFailedSignUpDataRequestValidator() { RuleFor(x => x.FailedSignUpDataID).GreaterThan(0); } } }
