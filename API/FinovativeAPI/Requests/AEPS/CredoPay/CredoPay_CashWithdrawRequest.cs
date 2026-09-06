using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.CredoPay
{
    public class CredoPay_CashWithdrawRequest
    {
        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _IPAddress;
        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }
        private string _Latitude;
        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }
        private string _Longitude;
        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }
        private string _IIN;

        public string IIN
        {
            get => _IIN.TrimIfNotNull();
            set => _IIN = value.TrimIfNotNull();
        }
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }
        private string _AuthReferenceNo;

        public string AuthReferenceNo
        {
            get => _AuthReferenceNo.TrimIfNotNull();
            set => _AuthReferenceNo = value.TrimIfNotNull();
        }
        


        internal string IsValidModalState()
        {
            var validator = new CredoPay_CashWithdrawRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

    }

    internal class CredoPay_CashWithdrawRequestValidator : AbstractValidator<CredoPay_CashWithdrawRequest>
    {
        public CredoPay_CashWithdrawRequestValidator()
        {
            RuleFor(x => x.AadhaarNumber)
                      .Must(new Validation().IsValidAadharNo)
                      .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.FingerPrint)
                    .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 20))
                    .WithMessage(AEPSMessage.InvalidFingerPrint);

            RuleFor(x => x.Amount)
                    .Must(new Validation().IsValidAmoutForAEPSAndmATM)
                    .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.AuthReferenceNo)
                     .Must(new Validation().IsValidString)
                     .WithMessage(AEPSMessage.InvalidAuthReferenceNo);
            RuleFor(x => x.IIN)
                    .Must(new Validation().IsValidString)
                    .WithMessage(AEPSMessage.InvalidIIN);
            

        }
    }
}