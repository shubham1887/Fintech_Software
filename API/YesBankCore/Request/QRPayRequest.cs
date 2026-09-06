using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YESBankCore.Request
{
    public class QRPayRequest
    {
        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("OrderNo")]
        public string OrderNo { get; set; }

        internal string IsValidModalState()
        {
            var validator = new QRPayRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            int amt = Convert.ToInt16(Convert.ToDouble(Amount));

            if (amt <= 0)
            {
                return "Invalid amount";
            }

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class QRPayRequestValidator : AbstractValidator<QRPayRequest>
    {
        public QRPayRequestValidator()
        {
            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "Amount"));

            RuleFor(x => x.OrderNo)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "MerchantId"));
        }
    }
}