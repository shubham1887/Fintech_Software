using FluentValidation;
namespace FintechApp.Requests.LeadGenerationSetting
{
    public class DeleteLeadGenerationSettingRequest { public int LeadGenerationSettingID { get; set; } }
    public class DeleteLeadGenerationSettingRequestValidator : AbstractValidator<DeleteLeadGenerationSettingRequest> { public DeleteLeadGenerationSettingRequestValidator() { RuleFor(x => x.LeadGenerationSettingID).GreaterThan(0); } }
}
