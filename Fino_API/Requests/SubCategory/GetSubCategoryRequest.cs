using FluentValidation;
namespace FintechApp.Requests.SubCategory { public class GetSubCategoryRequest { public int SubCategoryID { get; set; } } public class GetSubCategoryRequestValidator : AbstractValidator<GetSubCategoryRequest> { public GetSubCategoryRequestValidator() { RuleFor(x => x.SubCategoryID).GreaterThan(0); } } }
