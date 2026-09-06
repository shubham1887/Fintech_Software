using FluentValidation;
namespace FintechApp.Requests.Category { public class UpdateCategoryRequest { public int CategoryID { get; set; } public string CategoryName { get; set; } = string.Empty; } public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest> { public UpdateCategoryRequestValidator() { RuleFor(x => x.CategoryID).GreaterThan(0); } } }
