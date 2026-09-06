using FluentValidation;
namespace FintechApp.Requests.UrlTypeMaster { public class UpdateUrlTypeMasterRequest { public int UrlTypeID { get; set; } public string UrlType { get; set; } = string.Empty; } public class UpdateUrlTypeMasterRequestValidator : AbstractValidator<UpdateUrlTypeMasterRequest> { public UpdateUrlTypeMasterRequestValidator() { RuleFor(x => x.UrlTypeID).GreaterThan(0); } } }
