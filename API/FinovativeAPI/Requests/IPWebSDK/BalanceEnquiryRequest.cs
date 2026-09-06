using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPWebSDK
{
    public partial class BalanceEnquiryRequest
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("outlet_pan")]
        public string OutletPan { get; set; }

        [JsonProperty("transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        internal string IsValidModalState()
        {
            var validator = new BalanceEnquiryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class BalanceEnquiryRequestValidator : AbstractValidator<BalanceEnquiryRequest>
    {
        public BalanceEnquiryRequestValidator()
        {
            RuleFor(x => x.AppId)
                .Must(new Validation().IsValidString)
                .WithMessage(TransactionConfirmationMessage.InvalidAppId);

            RuleFor(x => x.OutletPan)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionConfirmationMessage.InValidOutletPan);

            RuleFor(x => x.SessionId)
              .Must(new Validation().IsValidString)
              .WithMessage(TransactionConfirmationMessage.InValidSessionId);
        }
    }

    public partial class Transaction
    {
        [JsonProperty("outlet_pan")]
        public string OutletPan { get; set; }
    }
}