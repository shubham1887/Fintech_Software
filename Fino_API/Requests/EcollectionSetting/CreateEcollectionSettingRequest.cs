using FluentValidation;
namespace FintechApp.Requests.EcollectionSetting
{
    public class CreateEcollectionSettingRequest { public string PrivateKeyPath { get; set; } = string.Empty; public string PrivateKeyPassword { get; set; } = string.Empty; public string PublicKeyPath { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public bool IsActive { get; set; } public int WalletID { get; set; } public string Prefix { get; set; } = string.Empty; public string IFSCCode { get; set; } = string.Empty; }
    public class CreateEcollectionSettingRequestValidator : AbstractValidator<CreateEcollectionSettingRequest> { public CreateEcollectionSettingRequestValidator() {  } }
}
