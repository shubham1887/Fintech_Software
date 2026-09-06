using FluentValidation;
namespace FintechApp.Requests.AutoTransferUserwiseMapping
{
    public class CreateAutoTransferUserwiseMappingRequest { public int ParentID { get; set; } public int UserID { get; set; } public string OTP { get; set; } = string.Empty; public bool IsOTPVerified { get; set; } public decimal MaxOutstanding { get; set; } public decimal RechargeLowBalanceReached { get; set; } public decimal RechargeAmountIncreasedBy { get; set; } public decimal DMTLowBalanceReached { get; set; } public decimal DMTAmountIncreasedBy { get; set; } }
    public class CreateAutoTransferUserwiseMappingRequestValidator : AbstractValidator<CreateAutoTransferUserwiseMappingRequest> { public CreateAutoTransferUserwiseMappingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
