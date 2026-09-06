using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Wallet
{
    public class SelectRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new WalletIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class WalletIDRequestValidator : AbstractValidator<SelectRequest>
    {
        public WalletIDRequestValidator()
        {
            RuleFor(x => x.WalletID)
              .Must(new Validation().IsValidInt)
              .WithMessage(WalletMessage.Invalid);

            RuleFor(x => x.ReceiverID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Message.FundMessage.InvalidReceiver);
        }
    }
}