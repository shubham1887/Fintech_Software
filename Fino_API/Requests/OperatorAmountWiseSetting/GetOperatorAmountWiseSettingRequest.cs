using FluentValidation;
namespace FintechApp.Requests.OperatorAmountWiseSetting
{
    public class GetOperatorAmountWiseSettingRequest { public int OperatorAmountWiseSettingID { get; set; } public int OperatorID { get; set; } }
    public class GetOperatorAmountWiseSettingRequestValidator : AbstractValidator<GetOperatorAmountWiseSettingRequest> { public GetOperatorAmountWiseSettingRequestValidator() { RuleFor(x => x.OperatorAmountWiseSettingID).GreaterThan(0); } }
}
