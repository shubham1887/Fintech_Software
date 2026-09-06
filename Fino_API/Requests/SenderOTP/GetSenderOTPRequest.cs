using FluentValidation;
namespace FintechApp.Requests.SenderOTP { public class GetSenderOTPRequest { public int SenderOTPID { get; set; } public int UserID { get; set; } } public class GetSenderOTPRequestValidator : AbstractValidator<GetSenderOTPRequest> { public GetSenderOTPRequestValidator() { RuleFor(x => x.SenderOTPID).GreaterThan(0); } } }
