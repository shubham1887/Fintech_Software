using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TransactionMaster
{
    public class CreateTransactionRequest
    {
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int WalletID { get; set; }
        public int ProviderID { get; set; }
        public int DistributerUserID { get; set; }
        public int SuperDistributerUserID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProviderRequestID { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }

    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(TransactionMasterMessages.InvalidUserID);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(TransactionMasterMessages.InvalidAmount);

            RuleFor(x => x.ConsumerNumber)
                .NotEmpty().WithMessage(TransactionMasterMessages.RequiredField)
                .MaximumLength(50).WithMessage(TransactionMasterMessages.InvalidLength);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(TransactionMasterMessages.RequiredField)
                .MaximumLength(20).WithMessage(TransactionMasterMessages.InvalidLength);

            RuleFor(x => x.ProviderRequestID)
                .NotEmpty().WithMessage(TransactionMasterMessages.RequiredField);

            RuleFor(x => x.Through)
                .NotEmpty().WithMessage(TransactionMasterMessages.RequiredField)
                .MaximumLength(20).WithMessage(TransactionMasterMessages.InvalidLength);
        }
    }
}
