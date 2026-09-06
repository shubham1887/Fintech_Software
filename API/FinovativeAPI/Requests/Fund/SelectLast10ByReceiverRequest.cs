using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Fund
{
    public class SelectLast10ByReceiverRequest
    {
        private int _WalletID;

        public int WalletID
        {
            get => _WalletID; set => _WalletID = value;
        }

        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectLast10ByReceiverRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectLast10ByReceiverRequestValidator : AbstractValidator<SelectLast10ByReceiverRequest>
    {
        public SelectLast10ByReceiverRequestValidator()
        {
            RuleFor(x => x.ReceiverID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.FundMessage.InvalidReceiver);

            RuleFor(x => x.WalletID)
             .Must(new Validation().IsValidInt)
             .WithMessage(WalletMessage.Invalid);
        }
    }
}