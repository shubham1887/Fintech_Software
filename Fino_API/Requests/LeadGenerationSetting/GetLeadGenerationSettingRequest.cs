using FluentValidation;
namespace FintechApp.Requests.LeadGenerationSetting
{
    public class GetLeadGenerationSettingRequest { public int LeadGenerationSettingID { get; set; } }
    public class GetLeadGenerationSettingRequestValidator : AbstractValidator<GetLeadGenerationSettingRequest> { public GetLeadGenerationSettingRequestValidator() { RuleFor(x => x.LeadGenerationSettingID).GreaterThan(0); } }
}
