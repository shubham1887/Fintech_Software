using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class ApiClub_BalanceEnquiryRequest
    {
        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _IIN;

        public string IIN
        {
            get => _IIN.TrimIfNotNull();
            set => _IIN = value.TrimIfNotNull();
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new ApiClub_BalanceEnquiryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_BalanceEnquiryRequestValidator : AbstractValidator<ApiClub_BalanceEnquiryRequest>
    {
        public ApiClub_BalanceEnquiryRequestValidator()
        {
            RuleFor(x => x.IIN)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidIIN);

            RuleFor(x => x.AadhaarNumber)
                      .Must(new Validation().IsValidAadharNo)
                      .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.ConsumerNumber)
                    .Must(new Validation().IsValidMobileNumber)
                    .WithMessage(AEPSMessage.InValidConsumerNumber);

            RuleFor(x => x.FingerPrint)
                 .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 40))
                 .WithMessage(AEPSMessage.InvalidFingerPrint);
        }
    }
}