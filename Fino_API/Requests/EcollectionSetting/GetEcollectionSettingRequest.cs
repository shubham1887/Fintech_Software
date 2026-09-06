using FluentValidation;
namespace FintechApp.Requests.EcollectionSetting
{
    public class GetEcollectionSettingRequest { public int EcollectionSettingID { get; set; } }
    public class GetEcollectionSettingRequestValidator : AbstractValidator<GetEcollectionSettingRequest> { public GetEcollectionSettingRequestValidator() { RuleFor(x => x.EcollectionSettingID).GreaterThan(0); } }
}
