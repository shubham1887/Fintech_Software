using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashDeposit
{
    public class FingPay_TransactionRequest
    {
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

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
            var validator = new FingPay_TransactionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FingPay_TransactionRequestValidator : AbstractValidator<FingPay_TransactionRequest>
    {
        public FingPay_TransactionRequestValidator()
        {
            RuleFor(x => x.OTP)
                 .Must(new Validation().IsValidString)
                 .WithMessage(TransactionMessage.InvalidOTP);
        }
    }
}