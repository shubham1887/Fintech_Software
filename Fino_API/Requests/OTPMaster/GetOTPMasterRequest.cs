using FluentValidation;

namespace FintechApp.Requests.OTPMaster
{
    public class GetOTPMasterRequest
    {
        public int OTPMasterID { get; set; }
        public int UserID { get; set; }
    }

    public class GetOTPMasterRequestValidator : AbstractValidator<GetOTPMasterRequest>
    {
        public GetOTPMasterRequestValidator()
        {
            RuleFor(x => x.OTPMasterID).GreaterThan(0);
        }
    }
}
