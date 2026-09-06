using FluentValidation;
namespace FintechApp.Requests.Category { public class CreateCategoryRequest { public string CategoryName { get; set; } = string.Empty; } public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest> { public CreateCategoryRequestValidator() {  } } }
