using FluentValidation;
namespace FintechApp.Requests.UnitMaster { public class CreateUnitMasterRequest { public string Name { get; set; } = string.Empty; } public class CreateUnitMasterRequestValidator : AbstractValidator<CreateUnitMasterRequest> { public CreateUnitMasterRequestValidator() {  } } }
