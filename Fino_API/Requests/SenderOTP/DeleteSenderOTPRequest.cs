using FluentValidation;
namespace FintechApp.Requests.SenderOTP { public class DeleteSenderOTPRequest { public int SenderOTPID { get; set; } } public class DeleteSenderOTPRequestValidator : AbstractValidator<DeleteSenderOTPRequest> { public DeleteSenderOTPRequestValidator() { RuleFor(x => x.SenderOTPID).GreaterThan(0); } } }
