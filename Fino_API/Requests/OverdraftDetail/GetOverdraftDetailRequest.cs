using FluentValidation;
namespace FintechApp.Requests.OverdraftDetail { public class GetOverdraftDetailRequest { public int OverdraftDetailID { get; set; } public int UserID { get; set; } } public class GetOverdraftDetailRequestValidator : AbstractValidator<GetOverdraftDetailRequest> { public GetOverdraftDetailRequestValidator() { RuleFor(x => x.OverdraftDetailID).GreaterThan(0); } } }
