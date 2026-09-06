using FluentValidation;
namespace FintechApp.Requests.UrlTypeMaster { public class DeleteUrlTypeMasterRequest { public int UrlTypeID { get; set; } } public class DeleteUrlTypeMasterRequestValidator : AbstractValidator<DeleteUrlTypeMasterRequest> { public DeleteUrlTypeMasterRequestValidator() { RuleFor(x => x.UrlTypeID).GreaterThan(0); } } }
