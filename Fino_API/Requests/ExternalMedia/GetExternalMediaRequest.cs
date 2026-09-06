using FluentValidation;
namespace FintechApp.Requests.ExternalMedia
{
    public class GetExternalMediaRequest { public int ExternalMediaID { get; set; } }
    public class GetExternalMediaRequestValidator : AbstractValidator<GetExternalMediaRequest> { public GetExternalMediaRequestValidator() { RuleFor(x => x.ExternalMediaID).GreaterThan(0); } }
}
