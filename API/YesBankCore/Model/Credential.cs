using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YESBankCore.Model
{
    internal class Credential
    {
        public static string MerchantId = "";
        public static string MerchantKey = "";

        public static string XIBMClientSecret = "";
        public static string XIBMClientID = "";

        public static string PrivateKeyPassword = "";
        public static string PrivateKeyFilePath = "";

        public static string Mode = "";

        public static string UserName = "";
        public static string Password = "";

        internal string IsValidModalState()
        {
            var validator = new CredentialValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class CredentialValidator : AbstractValidator<Credential>
    {
        public CredentialValidator()
        {
            RuleFor(x => Credential.MerchantId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "MerchantId"));

            RuleFor(x => Credential.MerchantKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "MerchantKey"));

            RuleFor(x => Credential.XIBMClientSecret)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "XIBMClientSecret"));

            RuleFor(x => Credential.XIBMClientID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "XIBMClientID"));

            RuleFor(x => Credential.PrivateKeyPassword)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PrivateKeyPassword"));

            RuleFor(x => Credential.PrivateKeyFilePath)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PrivateKeyFilePath"));
        }
    }
}