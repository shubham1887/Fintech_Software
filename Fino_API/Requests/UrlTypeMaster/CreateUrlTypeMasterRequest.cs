using FluentValidation;
namespace FintechApp.Requests.UrlTypeMaster { public class CreateUrlTypeMasterRequest { public string UrlType { get; set; } = string.Empty; } public class CreateUrlTypeMasterRequestValidator : AbstractValidator<CreateUrlTypeMasterRequest> { public CreateUrlTypeMasterRequestValidator() {  } } }
