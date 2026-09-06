using FluentValidation;
namespace FintechApp.Requests.SubCategory { public class DeleteSubCategoryRequest { public int SubCategoryID { get; set; } } public class DeleteSubCategoryRequestValidator : AbstractValidator<DeleteSubCategoryRequest> { public DeleteSubCategoryRequestValidator() { RuleFor(x => x.SubCategoryID).GreaterThan(0); } } }
