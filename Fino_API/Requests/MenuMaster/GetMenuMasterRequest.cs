using FluentValidation;
namespace FintechApp.Requests.MenuMaster
{
    public class GetMenuMasterRequest { public int MenuMasterID { get; set; } }
    public class GetMenuMasterRequestValidator : AbstractValidator<GetMenuMasterRequest> { public GetMenuMasterRequestValidator() { RuleFor(x => x.MenuMasterID).GreaterThan(0); } }
}
