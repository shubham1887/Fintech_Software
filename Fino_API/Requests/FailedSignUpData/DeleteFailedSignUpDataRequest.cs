using FluentValidation;
namespace FintechApp.Requests.FailedSignUpData { public class DeleteFailedSignUpDataRequest { public int FailedSignUpDataID { get; set; } } public class DeleteFailedSignUpDataRequestValidator : AbstractValidator<DeleteFailedSignUpDataRequest> { public DeleteFailedSignUpDataRequestValidator() { RuleFor(x => x.FailedSignUpDataID).GreaterThan(0); } } }
