using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.UPITransfer
{
    public class UPIMoneyTransferRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _PaymentType;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PaymentType// neft and imps
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _BeneName;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BeneName
        {
            get => _BeneName.ToTitleCase();
            set => _BeneName = value.ToTitleCase();
        }

        private string _UpiBeneficiaryID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UpiBeneficiaryID
        {
            get => _UpiBeneficiaryID.TrimIfNotNull();
            set => _UpiBeneficiaryID = value.TrimIfNotNull();
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

        private string _UPIID;

        public string UPIID
        {
            get => _UPIID.TrimIfNotNull();
            set => _UPIID = value.TrimIfNotNull();
        }

        private string _Mobile;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _UPItransfercode;

        internal string UPItransfercode
        {
            get => _UPItransfercode.TrimIfNotNull();
            set => _UPItransfercode = value.TrimIfNotNull();
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

        private string _ActiveUPI;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ActiveUPI
        {
            get
            {
                return _ActiveUPI.TrimIfNotNull();
            }
            set
            {
                _ActiveUPI = value.TrimIfNotNull();
            }
        }

        public bool IsVerify { get; set; }

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

        internal string IsValidModalState()
        {
            var validator = new UPIMoneyTransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UPIMoneyTransferRequestValidator : AbstractValidator<UPIMoneyTransferRequest>
    {
        public UPIMoneyTransferRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                 .WithMessage(BeneficiaryMessage.InvalidUserID);

            RuleFor(x => x.UPIID)
              .Must(new Validation().IsValidUPIID)
              .WithMessage(TransactionMessage.InvalidUPIID);

            //RuleFor(x => x.BeneName)
            //.Must(new Validation().IsValidName)
            //   .WithMessage(TransactionMessage.InvalidUPIIDName);

            RuleFor(x => x.ProviderID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(TransactionMessage.InvalidProviderID);
        }
    }
}