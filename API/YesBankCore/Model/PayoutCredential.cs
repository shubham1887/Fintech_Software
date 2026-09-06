using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YESBankCore.Model
{
    internal class PayoutCredential
    {
        public static string MerchantId = "";
        public static string MerchantKey = "";

        public static string XIBMClientSecret = "";
        public static string XIBMClientID = "";

        public static string PrivateKeyPassword = "";
        public static string PrivateKeyFilePath = "";

        public static string Mode = "";

        public static string AuthUserName = "";
        public static string AuthPassword = "";

        internal string IsValidModalState()
        {
            var validator = new PayoutCredentialValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }
    internal class PayoutCredentialValidator : AbstractValidator<PayoutCredential>
    {
        public PayoutCredentialValidator()
        {
            RuleFor(x => PayoutCredential.MerchantId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "MerchantId"));

            RuleFor(x => PayoutCredential.MerchantKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "MerchantKey"));

            RuleFor(x => PayoutCredential.XIBMClientSecret)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "XIBMClientSecret"));

            RuleFor(x => PayoutCredential.XIBMClientID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "XIBMClientID"));

            RuleFor(x => PayoutCredential.PrivateKeyPassword)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PrivateKeyPassword"));

            RuleFor(x => PayoutCredential.PrivateKeyFilePath)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PrivateKeyFilePath"));

            RuleFor(x => PayoutCredential.AuthUserName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AuthUserName"));

            RuleFor(x => PayoutCredential.AuthPassword)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AuthPassword"));
        }
    }
}
