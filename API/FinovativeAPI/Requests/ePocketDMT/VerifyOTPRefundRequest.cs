using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ePocketDMT
{
    public class VerifyOTPRefundRequest
    {
        private int _MoneyTransferID;

        internal int MoneyTransferID
        {
            get => _MoneyTransferID;
            set => _MoneyTransferID = value;
        }

        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP;
            set => _OTP = value;
        }

        internal string IsValidModalState()
        {
            var validator = new VerifyOTPRefundRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class VerifyOTPRefundRequestValidator : AbstractValidator<VerifyOTPRefundRequest>
    {
        public VerifyOTPRefundRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Messages.InvalidTransactionMasterID);

            RuleFor(x => x.OTP)
             .Must(new Validation().IsValidString)
             .WithMessage(SenderMessage.InvalidOTP);
        }
    }
}