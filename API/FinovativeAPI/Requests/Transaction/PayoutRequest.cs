using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Transaction
{
    public class PayoutRequest
    {
        private string _PaymentType;

        public string PaymentType// neft and imps and UPI
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.ToTitleCase();
            set => _BeneficiaryID = value.ToTitleCase();
        }

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.ToTitleCase();
            set => _BeneName = value.ToTitleCase();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        public bool IsVerify { get; set; }

        private string _APIUserRequestID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _PayoutCode;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PayoutCode
        {
            get => _PayoutCode.TrimIfNotNull();
            set => _PayoutCode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new PayoutRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PayoutRequestValidator : AbstractValidator<PayoutRequest>
    {
        public PayoutRequestValidator()
        {
            /*RuleFor(x => x.BankName)
                 .Must(new Validation().IsValidString)
                 .WithMessage(BeneficiaryMessage.InvalidBankName);*/
            //RuleFor(x => x.AccountNo)
            //             .Must(new Validation().IsValidAccountNumber)
            //             .WithMessage(BeneficiaryMessage.InvalidAccountNo);
            //RuleFor(x => x.IFSC)
            //             .Must(new Validation().IsValidIFSC)
            //             .WithMessage(BeneficiaryMessage.InvalidIFSC);
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.PaymentType)
                .Must(new Validation().IsValidString)
                .WithMessage(BeneficiaryMessage.InvalidPaymentType);
        }
    }
}