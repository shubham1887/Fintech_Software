using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.WalletTopUp
{
    public class Paysprint_TransactionRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
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

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.TrimIfNotNull();
            set => _BeneName = value.TrimIfNotNull();
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

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
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