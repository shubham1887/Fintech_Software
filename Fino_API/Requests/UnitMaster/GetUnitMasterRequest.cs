using FluentValidation;
namespace FintechApp.Requests.UnitMaster { public class GetUnitMasterRequest { public int UnitMasterID { get; set; } } public class GetUnitMasterRequestValidator : AbstractValidator<GetUnitMasterRequest> { public GetUnitMasterRequestValidator() { RuleFor(x => x.UnitMasterID).GreaterThan(0); } } }
