using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashDeposit
{
    public class Paysprint_TransactionRequest
    {
        private string _RefernceNo;

        public string RefernceNo
        {
            get => _RefernceNo.TrimIfNotNull();
            set => _RefernceNo = value.TrimIfNotNull();
        }

        private string _TransactionToken;

        public string TransactionToken
        {
            get => _TransactionToken.TrimIfNotNull();
            set => _TransactionToken = value.TrimIfNotNull();
        }

        private string _MerchantTransactionID;

        public string MerchantTransactionID
        {
            get => _MerchantTransactionID.TrimIfNotNull();
            set => _MerchantTransactionID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_TransactionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_TransactionRequestValidator : AbstractValidator<Paysprint_TransactionRequest>
    {
        public Paysprint_TransactionRequestValidator()
        {
            RuleFor(x => x.OTP)
                 .Must(new Validation().IsValidString)
                 .WithMessage(TransactionMessage.InvalidOTP);
        }
    }
}