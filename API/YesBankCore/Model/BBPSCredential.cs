using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using YESBankCore.Controller;


namespace YesBankCore.Model
{
    internal class BBPSCredential
    {
        [JsonProperty("agentId")]
        public static string AgentId = "";

        [JsonProperty("partnerId")]
        public static string PartnerId = "";

        [JsonProperty("PublicKey")]
        public static string PublicKey = "";
        
        [JsonProperty("PrivateKey")]
        public static string PrivateKey = "";

        internal string IsValidModalState()
        {
            var validator = new BBPSCredentialValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class BBPSCredentialValidator : AbstractValidator<BBPSCredential>
    {
        public BBPSCredentialValidator()
        {
            RuleFor(x => BBPSCredential.AgentId)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.ParameterMissing.Replace("[Param]", "agentId"));

            RuleFor(x => BBPSCredential.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partnerId"));
        }
    }
}