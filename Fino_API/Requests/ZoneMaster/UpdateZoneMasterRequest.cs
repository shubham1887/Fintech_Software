using FluentValidation;
namespace FintechApp.Requests.ZoneMaster
{
    public class UpdateZoneMasterRequest { public int ZoneMasterID { get; set; } public string Name { get; set; } = string.Empty; }
    public class UpdateZoneMasterRequestValidator : AbstractValidator<UpdateZoneMasterRequest> { public UpdateZoneMasterRequestValidator() { RuleFor(x => x.ZoneMasterID).GreaterThan(0); RuleFor(x => x.Name).NotEmpty(); } }
}
