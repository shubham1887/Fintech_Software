using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPWebSDK
{
    public partial class TransactionConfirmationRequest
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
        public List<TransactionConfirmationRequestParameters> Transactions { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        internal string IsValidModalState()
        {
            var validator = new TransactionConfirmationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class TransactionConfirmationRequestValidator : AbstractValidator<TransactionConfirmationRequest>
    {
        public TransactionConfirmationRequestValidator()
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

            RuleFor(x => x.RequestId)
             .Must(new Validation().IsValidString)
             .WithMessage(TransactionConfirmationMessage.InValidRequestId);
        }
    }

    public class TransactionConfirmationRequestParameters
    {
        [JsonProperty("sp_key")]
        public string SpKey { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("customer_params")]
        public List<string> CustomerParams { get; set; }
    }
}