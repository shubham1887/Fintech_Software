using FluentValidation;
namespace FintechApp.Requests.AccountOpenSetting
{
    public class CreateAccountOpenSettingRequest { public string? Name { get; set; } public bool IsActiveForWebsite { get; set; } public bool IsActive { get; set; } public string? UserName { get; set; } public string MerchantID { get; set; } = string.Empty; public string? Token { get; set; } public string? CallBackURL { get; set; } public string? DisplayName { get; set; } public int Sequence { get; set; } public string? Mode { get; set; } }
    public class CreateAccountOpenSettingRequestValidator : AbstractValidator<CreateAccountOpenSettingRequest> { public CreateAccountOpenSettingRequestValidator() { RuleFor(x => x.MerchantID).NotEmpty(); } }
}
