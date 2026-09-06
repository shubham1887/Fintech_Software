using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class InitRequest
    {
        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _UPIID;

        public string UPIID
        {
            get => _UPIID.TrimIfNotNull();
            set => _UPIID = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Paymode;

        public string Paymode
        {
            get => _Paymode.TrimIfNotNull();
            set => _Paymode = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _ServiceID;

        public string ServiceID
        {
            get => _ServiceID.TrimIfNotNull();
            set => _ServiceID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InitRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InitRequestValidator : AbstractValidator<InitRequest>
    {
        public InitRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidString)
                .WithMessage(PaymentGatewayMessage.InvalidAmount);

            RuleFor(x => x.WalletID)
            .Must(new Validation().IsValidInt)
            .WithMessage(PaymentGatewayMessage.InvalidWalletID);
        }
    }
}