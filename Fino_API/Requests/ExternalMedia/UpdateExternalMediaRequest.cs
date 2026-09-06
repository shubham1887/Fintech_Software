using FluentValidation;
namespace FintechApp.Requests.ExternalMedia
{
    public class UpdateExternalMediaRequest { public int ExternalMediaID { get; set; } public string Key { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdateExternalMediaRequestValidator : AbstractValidator<UpdateExternalMediaRequest> { public UpdateExternalMediaRequestValidator() { RuleFor(x => x.ExternalMediaID).GreaterThan(0); } }
}
