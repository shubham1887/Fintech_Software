using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.WalletToWalletTransfer
{
    public class AEPSToOtherRequest
    {
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

        internal string IsValidModalState()
        {
            var validator = new AEPSToOtherRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AEPSToOtherRequestValidator : AbstractValidator<AEPSToOtherRequest>
    {
        public AEPSToOtherRequestValidator()
        {
            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.TransactionMessage.InvalidAmount);

         
        }
    }
}