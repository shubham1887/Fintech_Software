using FluentValidation;
namespace FintechApp.Requests.ZoneMaster
{
    public class CreateZoneMasterRequest { public string Name { get; set; } = string.Empty; }
    public class CreateZoneMasterRequestValidator : AbstractValidator<CreateZoneMasterRequest> { public CreateZoneMasterRequestValidator() { RuleFor(x => x.Name).NotEmpty(); } }
}
