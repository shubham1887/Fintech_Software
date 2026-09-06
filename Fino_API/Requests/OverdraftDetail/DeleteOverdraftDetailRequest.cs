using FluentValidation;
namespace FintechApp.Requests.OverdraftDetail { public class DeleteOverdraftDetailRequest { public int OverdraftDetailID { get; set; } } public class DeleteOverdraftDetailRequestValidator : AbstractValidator<DeleteOverdraftDetailRequest> { public DeleteOverdraftDetailRequestValidator() { RuleFor(x => x.OverdraftDetailID).GreaterThan(0); } } }
