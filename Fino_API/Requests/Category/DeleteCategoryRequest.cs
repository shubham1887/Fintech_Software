using FluentValidation;
namespace FintechApp.Requests.Category { public class DeleteCategoryRequest { public int CategoryID { get; set; } } public class DeleteCategoryRequestValidator : AbstractValidator<DeleteCategoryRequest> { public DeleteCategoryRequestValidator() { RuleFor(x => x.CategoryID).GreaterThan(0); } } }
