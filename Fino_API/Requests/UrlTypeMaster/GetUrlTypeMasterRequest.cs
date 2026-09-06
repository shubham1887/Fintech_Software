using FluentValidation;
namespace FintechApp.Requests.UrlTypeMaster { public class GetUrlTypeMasterRequest { public int UrlTypeID { get; set; } } public class GetUrlTypeMasterRequestValidator : AbstractValidator<GetUrlTypeMasterRequest> { public GetUrlTypeMasterRequestValidator() { RuleFor(x => x.UrlTypeID).GreaterThan(0); } } }
