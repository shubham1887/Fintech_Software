using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RetailerToAllFundTransfer
{
    public class TransferRequest
    {
        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }


        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }
        private int _SenderWalletID;

        public int SenderWalletID
        {
            get => _SenderWalletID;
            set => _SenderWalletID = value;
        }
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);
            RuleFor(x => x.ReceiverID)
             .Must(new Validation().IsValidInt)
             .WithMessage(FundMessage.InvalidReceiver);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.TransactionMessage.InvalidAmount);

            RuleFor(x => x.Remark)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.TransactionMessage.InvalidRemark);
        }
    }
}