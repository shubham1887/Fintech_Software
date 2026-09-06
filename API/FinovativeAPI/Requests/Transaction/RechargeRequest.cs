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
    public class RechargeRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }
        
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.TrimIfNotNull();
            set => _BillDate = value.TrimIfNotNull();
        }
        
        private string _BillerID;

        public string BillerID
        {
            get => _BillerID.TrimIfNotNull();
            set => _BillerID = value.TrimIfNotNull();
        }
        
        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Through;

        internal string Through
        {
            get => _Through.TrimIfNotNullWithDefault(PlatformCode.Web);
            set => _Through = value.TrimIfNotNullWithDefault(PlatformCode.Web);
        }

        private string _Param1;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Param1
        {
            get => _Param1.TrimIfNotNull();
            set => _Param1 = value.TrimIfNotNull();
        }

        private string _Param2;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Param2
        {
            get => _Param2.TrimIfNotNull();
            set => _Param2 = value.TrimIfNotNull();
        }

        private string _Param3;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Param3
        {
            get => _Param3.TrimIfNotNull();
            set => _Param3 = value.TrimIfNotNull();
        }

        private string _Param4;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Param4
        {
            get => _Param4.TrimIfNotNull();
            set => _Param4 = value.TrimIfNotNull();
        }

        private string _Param5;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Param5
        {
            get => _Param5.TrimIfNotNull();
            set => _Param5 = value.TrimIfNotNull();
        }

        private string _Pin;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string _PaymentType;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _IFSC;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _BBPSViewID;

        public string BBPSViewID
        {
            get => _BBPSViewID.TrimIfNotNull();
            set => _BBPSViewID = value.TrimIfNotNull();
        }

        private string _BeneficiaryName;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }

        private string _BBPS_CustomerName;

        public string BBPS_CustomerName
        {
            get => _BBPS_CustomerName.TrimIfNotNull();
            set => _BBPS_CustomerName = value.TrimIfNotNull();
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

        private string _Label;
        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private string isValidConsumnerNumber(string number)
        {
            if (number.Trim().ToLower().Contains(" "))
            {
                return "Space not allowed in number";
            }

            return StatusType.Ok;
        }

        internal string IsValidModalState()
        {
            var validator = new RechargeRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success == false)
            {
                return failures[0].ToString();
            }

            string failureMessage = isValidConsumnerNumber(ConsumerNumber);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class RechargeRequestValidator : AbstractValidator<RechargeRequest>
    {
        public RechargeRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                        .Must(new Validation().IsValidInt)
                        .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.Amount)
                      .Must(new Validation().IsValidDecimal)
                      .WithMessage(OperatorMessage.InValidAmount);

            RuleFor(x => x.ConsumerNumber)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.TransactionMessage.InvalidConsumerNumber);
        }
    }
}