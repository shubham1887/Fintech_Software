using FluentValidation;
namespace FintechApp.Requests.EcollectionSetting
{
    public class UpdateEcollectionSettingRequest { public int EcollectionSettingID { get; set; } public string PrivateKeyPath { get; set; } = string.Empty; public string PrivateKeyPassword { get; set; } = string.Empty; public string PublicKeyPath { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public bool IsActive { get; set; } public int WalletID { get; set; } public string Prefix { get; set; } = string.Empty; public string IFSCCode { get; set; } = string.Empty; }
    public class UpdateEcollectionSettingRequestValidator : AbstractValidator<UpdateEcollectionSettingRequest> { public UpdateEcollectionSettingRequestValidator() { RuleFor(x => x.EcollectionSettingID).GreaterThan(0); } }
}
