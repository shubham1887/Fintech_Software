using FluentValidation;
namespace FintechApp.Requests.UnitMaster { public class DeleteUnitMasterRequest { public int UnitMasterID { get; set; } } public class DeleteUnitMasterRequestValidator : AbstractValidator<DeleteUnitMasterRequest> { public DeleteUnitMasterRequestValidator() { RuleFor(x => x.UnitMasterID).GreaterThan(0); } } }
