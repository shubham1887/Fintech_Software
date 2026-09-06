using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class SelectLast10ForReverseTransactionRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectLast10ForReverseTransactionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectLast10ForReverseTransactionRequestValidator : AbstractValidator<SelectLast10ForReverseTransactionRequest>
    {
        public SelectLast10ForReverseTransactionRequestValidator()
        {
            RuleFor(x => x.SenderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidSender);

            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);
        }
    }
}