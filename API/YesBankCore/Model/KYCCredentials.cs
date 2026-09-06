using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Model
{
    internal class KYCCredentials
    {

        [JsonProperty("PartnerKey")]
        public static string PartnerKey = "";

        [JsonProperty("PrivateKey")]
        public static string PrivateKey = "";

        [JsonProperty("BankPublicKey")]
        public static string BankPublicKey = "";

        internal string IsValidModalState()
        {
            var validator = new KYCCredentialsValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class KYCCredentialsValidator : AbstractValidator<KYCCredentials>
    {
        public KYCCredentialsValidator()
        {
            RuleFor(x => KYCCredentials.PartnerKey)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerKey"));

            RuleFor(x => KYCCredentials.PrivateKey)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.ParameterMissing.Replace("[Param]", "PrivateKey"));

            RuleFor(x => KYCCredentials.BankPublicKey)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.ParameterMissing.Replace("[Param]", "BankPublicKey"));
        }
    }
}
