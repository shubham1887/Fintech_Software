using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class FingPay_EKYCBiomatricRequest
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

        private string _AadhaarNo;

        public string AadhaarNo
        {
            get => _AadhaarNo.TrimIfNotNull();
            set => _AadhaarNo = value.TrimIfNotNull();
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private int _PrimaryKeyId;

        public int PrimaryKeyId
        {
            get => _PrimaryKeyId;
            set => _PrimaryKeyId = value;
        }

        internal string IsValidModalState()
        {
            var validator = new FingPay_EKYCBiomatricRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FingPay_EKYCBiomatricRequestValidator : AbstractValidator<FingPay_EKYCBiomatricRequest>
    {
        public FingPay_EKYCBiomatricRequestValidator()
        {
            RuleFor(x => x.EncodeFPTxnId)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.UserMessage.InvaildEncodeFPTxnId);

            RuleFor(x => x.PrimaryKeyId)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.UserMessage.InvalidPrimaryKeyId);

            RuleFor(x => x.MobileNo)
            .Must(new Validation().IsValidMobileNumber)
            .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.FingerPrint)
                    .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 20))
                    .WithMessage(AEPSMessage.InvalidFingerPrint);
        }
    }
}