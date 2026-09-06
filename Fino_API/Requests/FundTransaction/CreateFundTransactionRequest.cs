using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.FundTransaction
{
    public class CreateFundTransactionRequest
    {
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal CashCollectionAmount { get; set; }
        public decimal SenderOpeningBalance { get; set; }
        public decimal SenderClosingBalance { get; set; }
        public decimal ReceiverOpeningBalance { get; set; }
        public decimal ReceiverClosingBalance { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string UTRNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public int RequestToID { get; set; }
        public string CashCollectionRemark { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string ReceiptPath { get; set; } = string.Empty;
        public DateTime TransactionDOC { get; set; }
        public int TransactionDOCInt { get; set; }
        public int RequestAcceptedUserID { get; set; }
    }

    public class CreateFundTransactionRequestValidator : AbstractValidator<CreateFundTransactionRequest>
    {
        public CreateFundTransactionRequestValidator()
        {
            RuleFor(x => x.SenderID)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.ReceiverID)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.WalletID)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.OriginalAmount)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.CashCollectionAmount)
                .GreaterThanOrEqualTo(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.SenderOpeningBalance)
                .GreaterThanOrEqualTo(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.SenderClosingBalance)
                .GreaterThanOrEqualTo(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.ReceiverOpeningBalance)
                .GreaterThanOrEqualTo(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.ReceiverClosingBalance)
                .GreaterThanOrEqualTo(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.Remark)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.UTRNumber)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.AdminRemark)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.PayMode)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.Through)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.CashCollectionRemark)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.BankName)
                .MaximumLength(200).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.ReceiptPath)
                .MaximumLength(200).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.TransactionDOC)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField);
        }
    }
}
