using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Model
{
    public class PayoutCredentialV1
    {
        public static string PartnerKey = "";
        public static string PrivateKey = "";

        public static string PublicKey = "";
    
        internal string IsValidModalState()
        {
            var validator = new PayoutCredentialV1Validator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }
    internal class PayoutCredentialV1Validator : AbstractValidator<PayoutCredentialV1>
    {
        public PayoutCredentialV1Validator()
        {
    
            RuleFor(x => PayoutCredentialV1.PartnerKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerKey"));

            RuleFor(x => PayoutCredentialV1.PrivateKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "privateKey"));

            RuleFor(x => PayoutCredentialV1.PublicKey)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PublicKey"));
        }
    }
}

