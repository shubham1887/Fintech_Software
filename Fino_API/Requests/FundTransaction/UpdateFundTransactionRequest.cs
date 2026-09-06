using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.FundTransaction
{
    public class UpdateFundTransactionRequest
    {
        public int FundTransactionID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateFundTransactionRequestValidator : AbstractValidator<UpdateFundTransactionRequest>
    {
        public UpdateFundTransactionRequestValidator()
        {
            RuleFor(x => x.FundTransactionID)
                .GreaterThan(0).WithMessage(FundTransactionMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(100).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(500).WithMessage(FundTransactionMessages.InvalidLength);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(FundTransactionMessages.RequiredField)
                .MaximumLength(500).WithMessage(FundTransactionMessages.InvalidLength);
        }
    }
}
