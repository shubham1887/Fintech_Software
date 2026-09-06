using FluentValidation;
namespace FintechApp.Requests.ExternalMedia
{
    public class DeleteExternalMediaRequest { public int ExternalMediaID { get; set; } }
    public class DeleteExternalMediaRequestValidator : AbstractValidator<DeleteExternalMediaRequest> { public DeleteExternalMediaRequestValidator() { RuleFor(x => x.ExternalMediaID).GreaterThan(0); } }
}
