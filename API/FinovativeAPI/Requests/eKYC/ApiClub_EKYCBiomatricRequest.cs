using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class ApiClub_EKYCBiomatricRequest
    {
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

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ApiClub_EKYCBiomatricRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_EKYCBiomatricRequestValidator : AbstractValidator<ApiClub_EKYCBiomatricRequest>
    {
        public ApiClub_EKYCBiomatricRequestValidator()
        {
            RuleFor(x => x.TransactionID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.UserMessage.InvaildTransactionID);

            RuleFor(x => x.AadhaarNo)
            .Must(new Validation().IsValidAadharNo)
            .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.FingerPrint)
            .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 40))
            .WithMessage(AEPSMessage.InvalidFingerPrint);
        }
    }
}