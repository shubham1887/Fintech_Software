using FluentValidation;
namespace FintechApp.Requests.MenuActionMapping { public class GetMenuActionMappingRequest { public int MenuActionMappingID { get; set; } } public class GetMenuActionMappingRequestValidator : AbstractValidator<GetMenuActionMappingRequest> { public GetMenuActionMappingRequestValidator() { RuleFor(x => x.MenuActionMappingID).GreaterThan(0); } } }
