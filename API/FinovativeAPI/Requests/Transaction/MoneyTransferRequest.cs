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
    public class MoneyTransferRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _RechargePin;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RechargePin
        {
            get => _RechargePin.TrimIfNotNull();
            set => _RechargePin = value.TrimIfNotNull();
        }

        private string _PaymentType;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PaymentType// neft and imps
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _Remark;

        [DefaultValue("")]
        public string Remark// neft and imps
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _ProviderID;

        internal int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        internal int UserID { get; set; }

        private string _BankName;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _Name;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _Mobile;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _SenderName;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SenderName
        {
            get => _SenderName.TrimIfNotNull();
            set => _SenderName = value.TrimIfNotNull();
        }

        private string _IFSC;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _BeneficiaryID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        private string _SenderID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _BankID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BankID
        {
            get => _BankID.TrimIfNotNull();
            set => _BankID = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _Latitude;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Latitude
        {
            get => _Latitude.ToTitleCase();
            set => _Latitude = value.ToTitleCase();
        }

        private string _Longitude;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Longitude
        {
            get => _Longitude.ToTitleCase();
            set => _Longitude = value.ToTitleCase();
        }

        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }

        private string _AadharNo;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AadharNo
        {
            get => _AadharNo.ToTitleCase();
            set => _AadharNo = value.ToTitleCase();
        }

        private string _PAN;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PAN
        {
            get => _PAN.ToTitleCase();
            set => _PAN = value.ToTitleCase();
        }

        private string _AuthCode;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AuthCode
        {
            get
            {
                return _AuthCode.TrimIfNotNull();
            }
            set
            {
                _AuthCode = value.TrimIfNotNull();
            }
        }

        private string _TxnMode;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TxnMode
        {
            get => _TxnMode.ToTitleCase();
            set => _TxnMode = value.ToTitleCase();
        }

        private string _AccountType;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AccountType
        {
            get => _AccountType.ToTitleCase();
            set => _AccountType = value.ToTitleCase();
        }

        public bool IsVerify { get; set; }

        internal string IsValidModalState()
        {
            var validator = new MoneyTransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class MoneyTransferRequestValidator : AbstractValidator<MoneyTransferRequest>
    {
        public MoneyTransferRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InValidAmount);

            //RuleFor(x => x.RechargePin)
            //    .Must(new Validation().IsValidString)
            //    .WithMessage(TransactionMessage.InvalidRechargePin);

            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                 .WithMessage(BeneficiaryMessage.InvalidUserID);

            //RuleFor(x => x.BankName)
            //   .Must(new Validation().IsValidString)
            //   .WithMessage(BeneficiaryMessage.InvalidBankName);

            RuleFor(x => x.AccountNo)
              .Must(new Validation().IsValidAccountNumber)
              .WithMessage(BeneficiaryMessage.InvalidAccountNo);

            RuleFor(x => x.IFSC)
              .Must(new Validation().IsValidIFSC)
              .WithMessage(BeneficiaryMessage.InvalidIFSC);

            RuleFor(x => x.BeneficiaryID)
              .Must(new Validation().IsValidString)
              .WithMessage(BeneficiaryMessage.InvalidBeneficiary);

            RuleFor(x => x.SenderID)
              .Must(new Validation().IsValidString)
              .WithMessage(BeneficiaryMessage.InvalidSender);

            RuleFor(x => x.PaymentType)
                .Must(new Validation().IsValidPaymentTypeString)
                .WithMessage(BeneficiaryMessage.InvalidPaymentType);

            RuleFor(x => x.ProviderID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(TransactionMessage.InvalidProviderID);
        }
    }
}