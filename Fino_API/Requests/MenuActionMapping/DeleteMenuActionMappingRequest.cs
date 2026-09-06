using FluentValidation;
namespace FintechApp.Requests.MenuActionMapping { public class DeleteMenuActionMappingRequest { public int MenuActionMappingID { get; set; } } public class DeleteMenuActionMappingRequestValidator : AbstractValidator<DeleteMenuActionMappingRequest> { public DeleteMenuActionMappingRequestValidator() { RuleFor(x => x.MenuActionMappingID).GreaterThan(0); } } }
