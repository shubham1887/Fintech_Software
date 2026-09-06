using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TransactionMaster
{
    public class UpdateTransactionStatusRequest
    {
        public int TransactionID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TxnID { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    public class UpdateTransactionStatusRequestValidator : AbstractValidator<UpdateTransactionStatusRequest>
    {
        private static readonly string[] AllowedStatuses = ["SUCCESS", "FAILED", "PENDING", "REFUND"];

        public UpdateTransactionStatusRequestValidator()
        {
            RuleFor(x => x.TransactionID)
                .GreaterThan(0).WithMessage(TransactionMasterMessages.InvalidValue);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(TransactionMasterMessages.RequiredField)
                .Must(s => AllowedStatuses.Contains(s.ToUpperInvariant()))
                .WithMessage(TransactionMasterMessages.InvalidStatus);
        }
    }
}
