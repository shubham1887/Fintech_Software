using FluentValidation;
namespace FintechApp.Requests.OperatorAmountWiseSetting
{
    public class UpdateOperatorAmountWiseSettingRequest { public int OperatorAmountWiseSettingID { get; set; } public int OperatorID { get; set; } public decimal Amount { get; set; } public int SerialCount { get; set; } }
    public class UpdateOperatorAmountWiseSettingRequestValidator : AbstractValidator<UpdateOperatorAmountWiseSettingRequest> { public UpdateOperatorAmountWiseSettingRequestValidator() { RuleFor(x => x.OperatorAmountWiseSettingID).GreaterThan(0); } }
}
