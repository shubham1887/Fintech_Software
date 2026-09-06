using FluentValidation;
namespace FintechApp.Requests.ProviderResponseMaster
{
    public class UpdateProviderResponseMasterRequest { public int ProviderResponseMasterID { get; set; } public int ProviderID { get; set; } public string TextMustexits { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public bool IsReference { get; set; } public bool IsActive { get; set; } }
    public class UpdateProviderResponseMasterRequestValidator : AbstractValidator<UpdateProviderResponseMasterRequest> { public UpdateProviderResponseMasterRequestValidator() { RuleFor(x => x.ProviderResponseMasterID).GreaterThan(0); } }
}
