using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class GetTransactionRefundRequest
    {
        private string _ID;

        public string ID
        {
            get => _ID.TrimIfNotNull();
            set => _ID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new GetTransactionRefundRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GetTransactionRefundRequestValidator : AbstractValidator<GetTransactionRefundRequest>
    {
        public GetTransactionRefundRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);

            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(TransactionMessage.InvalidOTP);
        }
    }
}