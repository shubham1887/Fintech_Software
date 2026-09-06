using FluentValidation;
namespace FintechApp.Requests.MenuMaster
{
    public class DeleteMenuMasterRequest { public int MenuMasterID { get; set; } }
    public class DeleteMenuMasterRequestValidator : AbstractValidator<DeleteMenuMasterRequest> { public DeleteMenuMasterRequestValidator() { RuleFor(x => x.MenuMasterID).GreaterThan(0); } }
}
