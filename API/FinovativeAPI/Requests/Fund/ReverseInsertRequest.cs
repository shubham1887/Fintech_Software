using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class ReverseInsertRequest
    {
        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
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

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ReverseInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ReverseInsertRequestValidator : AbstractValidator<ReverseInsertRequest>
    {
        public ReverseInsertRequestValidator()
        {
            RuleFor(x => x.SenderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidSender);

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