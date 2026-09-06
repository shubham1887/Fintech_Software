using FluentValidation;

namespace FintechApp.Requests.OTPMaster
{
    public class DeleteOTPMasterRequest
    {
        public int OTPMasterID { get; set; }
    }

    public class DeleteOTPMasterRequestValidator : AbstractValidator<DeleteOTPMasterRequest>
    {
        public DeleteOTPMasterRequestValidator()
        {
            RuleFor(x => x.OTPMasterID).GreaterThan(0);
        }
    }
}
