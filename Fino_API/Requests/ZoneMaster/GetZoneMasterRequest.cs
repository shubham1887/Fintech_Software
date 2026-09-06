using FluentValidation;
namespace FintechApp.Requests.ZoneMaster
{
    public class GetZoneMasterRequest { public int ZoneMasterID { get; set; } }
    public class GetZoneMasterRequestValidator : AbstractValidator<GetZoneMasterRequest> { public GetZoneMasterRequestValidator() { RuleFor(x => x.ZoneMasterID).GreaterThan(0); } }
}
