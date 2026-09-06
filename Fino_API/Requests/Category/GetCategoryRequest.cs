using FluentValidation;
namespace FintechApp.Requests.Category { public class GetCategoryRequest { public int CategoryID { get; set; } } public class GetCategoryRequestValidator : AbstractValidator<GetCategoryRequest> { public GetCategoryRequestValidator() { RuleFor(x => x.CategoryID).GreaterThan(0); } } }
