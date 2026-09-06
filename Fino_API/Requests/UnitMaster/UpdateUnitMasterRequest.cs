using FluentValidation;
namespace FintechApp.Requests.UnitMaster { public class UpdateUnitMasterRequest { public int UnitMasterID { get; set; } public string Name { get; set; } = string.Empty; } public class UpdateUnitMasterRequestValidator : AbstractValidator<UpdateUnitMasterRequest> { public UpdateUnitMasterRequestValidator() { RuleFor(x => x.UnitMasterID).GreaterThan(0); } } }
