using FluentValidation;
namespace FintechApp.Requests.BlackListBankMaster { public class GetBlackListBankMasterRequest { public int BlackListBankMasterID { get; set; } } public class GetBlackListBankMasterRequestValidator : AbstractValidator<GetBlackListBankMasterRequest> { public GetBlackListBankMasterRequestValidator() { RuleFor(x => x.BlackListBankMasterID).GreaterThan(0); } } }
