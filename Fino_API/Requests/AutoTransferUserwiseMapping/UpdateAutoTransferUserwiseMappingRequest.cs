using FluentValidation;
namespace FintechApp.Requests.AutoTransferUserwiseMapping
{
    public class UpdateAutoTransferUserwiseMappingRequest { public int AutoTransferUserwiseMappingID { get; set; } public int ParentID { get; set; } public int UserID { get; set; } public bool IsOTPVerified { get; set; } public decimal MaxOutstanding { get; set; } public decimal RechargeLowBalanceReached { get; set; } public decimal RechargeAmountIncreasedBy { get; set; } public decimal DMTLowBalanceReached { get; set; } public decimal DMTAmountIncreasedBy { get; set; } }
    public class UpdateAutoTransferUserwiseMappingRequestValidator : AbstractValidator<UpdateAutoTransferUserwiseMappingRequest> { public UpdateAutoTransferUserwiseMappingRequestValidator() { RuleFor(x => x.AutoTransferUserwiseMappingID).GreaterThan(0); } }
}
