using FluentValidation;
namespace FintechApp.Requests.OperatorAmountWiseSetting
{
    public class DeleteOperatorAmountWiseSettingRequest { public int OperatorAmountWiseSettingID { get; set; } }
    public class DeleteOperatorAmountWiseSettingRequestValidator : AbstractValidator<DeleteOperatorAmountWiseSettingRequest> { public DeleteOperatorAmountWiseSettingRequestValidator() { RuleFor(x => x.OperatorAmountWiseSettingID).GreaterThan(0); } }
}
