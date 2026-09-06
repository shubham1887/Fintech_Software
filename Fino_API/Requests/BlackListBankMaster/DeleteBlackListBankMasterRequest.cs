using FluentValidation;
namespace FintechApp.Requests.BlackListBankMaster { public class DeleteBlackListBankMasterRequest { public int BlackListBankMasterID { get; set; } } public class DeleteBlackListBankMasterRequestValidator : AbstractValidator<DeleteBlackListBankMasterRequest> { public DeleteBlackListBankMasterRequestValidator() { RuleFor(x => x.BlackListBankMasterID).GreaterThan(0); } } }
