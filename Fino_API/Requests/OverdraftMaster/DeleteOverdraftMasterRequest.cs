using FluentValidation;
namespace FintechApp.Requests.OverdraftMaster { public class DeleteOverdraftMasterRequest { public int OverdraftMasterID { get; set; } } public class DeleteOverdraftMasterRequestValidator : AbstractValidator<DeleteOverdraftMasterRequest> { public DeleteOverdraftMasterRequestValidator() { RuleFor(x => x.OverdraftMasterID).GreaterThan(0); } } }
