using FluentValidation;
namespace FintechApp.Requests.TransactionDetails { public class DeleteTransactionDetailsRequest { public int TransactionDetailID { get; set; } } public class DeleteTransactionDetailsRequestValidator : AbstractValidator<DeleteTransactionDetailsRequest> { public DeleteTransactionDetailsRequestValidator() { RuleFor(x => x.TransactionDetailID).GreaterThan(0); } } }
