using FluentValidation;
namespace FintechApp.Requests.OperatorAmountWiseSetting
{
    public class CreateOperatorAmountWiseSettingRequest { public int OperatorID { get; set; } public decimal Amount { get; set; } public int SerialCount { get; set; } }
    public class CreateOperatorAmountWiseSettingRequestValidator : AbstractValidator<CreateOperatorAmountWiseSettingRequest> { public CreateOperatorAmountWiseSettingRequestValidator() { RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
