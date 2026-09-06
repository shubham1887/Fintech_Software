using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.FundTransaction
{
    public class GetFundTransactionRequest
    {
        public int FundTransactionID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public int WalletID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetFundTransactionRequestValidator : AbstractValidator<GetFundTransactionRequest>
    {
        public GetFundTransactionRequestValidator()
        {
            RuleFor(x => x.Status)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.Type)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage(FundTransactionMessages.InvalidValue);
        }
    }
}
