using FluentValidation;
namespace FintechApp.Requests.AccountOpenSetting
{
    public class GetAccountOpenSettingRequest { public int AccountOpenSettingID { get; set; } }
    public class GetAccountOpenSettingRequestValidator : AbstractValidator<GetAccountOpenSettingRequest> { public GetAccountOpenSettingRequestValidator() { RuleFor(x => x.AccountOpenSettingID).GreaterThan(0); } }
}
