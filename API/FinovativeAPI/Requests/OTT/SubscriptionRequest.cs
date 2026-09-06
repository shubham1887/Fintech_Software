using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.OTT
{
    public class SubscriptionRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
        }

        private int _ServiceID;

        internal int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _OTTName;

        public string OTTName
        {
            get => _OTTName.TrimIfNotNull();
            set => _OTTName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
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

        private string _EmailID;

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _SubscriptionOperatorID;

        public string SubscriptionOperatorID
        {
            get => _SubscriptionOperatorID.TrimIfNotNull();
            set => _SubscriptionOperatorID = value.TrimIfNotNull();
        }

        private string _PlanID;

        public string PlanID
        {
            get => _PlanID.TrimIfNotNull();
            set => _PlanID = value.TrimIfNotNull();
        }

        private string _PlanRefID;

        public string PlanRefID
        {
            get => _PlanRefID.TrimIfNotNull();
            set => _PlanRefID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SubscriptionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success) return StatusType.Ok;

            return failures[0].ToString();
        }
    }

    internal class SubscriptionRequestValidator : AbstractValidator<SubscriptionRequest>
    {
        public SubscriptionRequestValidator()
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