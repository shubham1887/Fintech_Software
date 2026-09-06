using FluentValidation;
namespace FintechApp.Requests.ZoneMaster
{
    public class DeleteZoneMasterRequest { public int ZoneMasterID { get; set; } }
    public class DeleteZoneMasterRequestValidator : AbstractValidator<DeleteZoneMasterRequest> { public DeleteZoneMasterRequestValidator() { RuleFor(x => x.ZoneMasterID).GreaterThan(0); } }
}
