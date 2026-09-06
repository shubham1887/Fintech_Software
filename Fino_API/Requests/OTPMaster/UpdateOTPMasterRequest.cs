using FluentValidation;

namespace FintechApp.Requests.OTPMaster
{
    public class UpdateOTPMasterRequest
    {
        public int OTPMasterID { get; set; }
        public string? OTP { get; set; }
        public bool IsVerified { get; set; }
        public string? Type { get; set; }
        public int UserID { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class UpdateOTPMasterRequestValidator : AbstractValidator<UpdateOTPMasterRequest>
    {
        public UpdateOTPMasterRequestValidator()
        {
            RuleFor(x => x.OTPMasterID).GreaterThan(0);
        }
    }
}
