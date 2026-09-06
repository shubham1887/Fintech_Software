using FluentValidation;
namespace FintechApp.Requests.SmsTransactionMaster { public class GetSmsTransactionMasterRequest { public int SmsTransactionMasterID { get; set; } } public class GetSmsTransactionMasterRequestValidator : AbstractValidator<GetSmsTransactionMasterRequest> { public GetSmsTransactionMasterRequestValidator() { RuleFor(x => x.SmsTransactionMasterID).GreaterThan(0); } } }
