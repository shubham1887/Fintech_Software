using FluentValidation;
namespace FintechApp.Requests.LastChangeUserDetails { public class GetLastChangeUserDetailsRequest { public int LastChangeUserDetailsID { get; set; } } public class GetLastChangeUserDetailsRequestValidator : AbstractValidator<GetLastChangeUserDetailsRequest> { public GetLastChangeUserDetailsRequestValidator() { RuleFor(x => x.LastChangeUserDetailsID).GreaterThan(0); } } }
