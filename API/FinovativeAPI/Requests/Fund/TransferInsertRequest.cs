using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class TransferInsertRequest
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

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
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

        private string _CashCollectionRemark;

        public string CashCollectionRemark
        {
            get => _CashCollectionRemark.TrimIfNotNull();
            set => _CashCollectionRemark = value.TrimIfNotNull();
        }

        private decimal _CashCollectionAmount;

        public decimal CashCollectionAmount
        {
            get => _CashCollectionAmount;
            set => _CashCollectionAmount = value;
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private string _ReferralCreatedUserID;

        public string ReferralCreatedUserID
        {
            get => _ReferralCreatedUserID.TrimIfNotNull();
            set => _ReferralCreatedUserID = value.TrimIfNotNull();
        }


        internal string IsValidModalState()
        {
            var validator = new TransferInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransferInsertRequestValidator : AbstractValidator<TransferInsertRequest>
    {
        public TransferInsertRequestValidator()
        {
            RuleFor(x => x.ReceiverID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidReceiver);

            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.TransactionMessage.InvalidAmount);

            RuleFor(x => x.Remark)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}