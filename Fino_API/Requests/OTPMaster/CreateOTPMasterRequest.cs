using FluentValidation;

namespace FintechApp.Requests.OTPMaster
{
    public class CreateOTPMasterRequest
    {
        public string? OTP { get; set; }
        public bool IsVerified { get; set; }
        public string? Type { get; set; }
        public int UserID { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class CreateOTPMasterRequestValidator : AbstractValidator<CreateOTPMasterRequest>
    {
        public CreateOTPMasterRequestValidator()
        {
            RuleFor(x => x.UserID).GreaterThan(0);
        }
    }
}
