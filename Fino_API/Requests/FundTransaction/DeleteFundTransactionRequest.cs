using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.FundTransaction
{
    public class DeleteFundTransactionRequest
    {
        public int FundTransactionID { get; set; }
    }

    public class DeleteFundTransactionRequestValidator : AbstractValidator<DeleteFundTransactionRequest>
    {
        public DeleteFundTransactionRequestValidator()
        {
            RuleFor(x => x.FundTransactionID)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);
        }
    }
}
