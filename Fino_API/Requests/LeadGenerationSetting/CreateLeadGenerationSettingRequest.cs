using FluentValidation;
namespace FintechApp.Requests.LeadGenerationSetting
{
    public class CreateLeadGenerationSettingRequest { public string Name { get; set; } = string.Empty; public string MerchantID { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string Slat { get; set; } = string.Empty; public string PublicKey { get; set; } = string.Empty; public bool IsActiveWeb { get; set; } public bool IsActive { get; set; } public string Mode { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; public int Sequence { get; set; } public bool IsActiveForWebsite { get; set; } }
    public class CreateLeadGenerationSettingRequestValidator : AbstractValidator<CreateLeadGenerationSettingRequest> { public CreateLeadGenerationSettingRequestValidator() {  } }
}
