using FluentValidation;
namespace FintechApp.Requests.ExternalMedia
{
    public class CreateExternalMediaRequest { public string Key { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class CreateExternalMediaRequestValidator : AbstractValidator<CreateExternalMediaRequest> { public CreateExternalMediaRequestValidator() {  } }
}
