using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class FingPay_EKYCReSendOTPRequest
    {
        private string _EncodeFPTxnId;

        public string EncodeFPTxnId
        {
            get => _EncodeFPTxnId.TrimIfNotNull();
            set => _EncodeFPTxnId = value.TrimIfNotNull();
        }

        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        private int _PrimaryKeyId;

        public int PrimaryKeyId
        {
            get => _PrimaryKeyId;
            set => _PrimaryKeyId = value;
        }

        internal string IsValidModalState()
        {
            var validator = new FingPay_EKYCReSendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FingPay_EKYCReSendOTPRequestValidator : AbstractValidator<FingPay_EKYCReSendOTPRequest>
    {
        public FingPay_EKYCReSendOTPRequestValidator()
        {
            RuleFor(x => x.EncodeFPTxnId)
                    .Must(new Validation().IsValidPanCard)
                    .WithMessage(Message.UserMessage.InvaildEncodeFPTxnId);

            RuleFor(x => x.PrimaryKeyId)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.UserMessage.InvalidPrimaryKeyId);

            RuleFor(x => x.MobileNo)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}