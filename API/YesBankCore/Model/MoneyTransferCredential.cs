using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YESBankCore.Model
{
    internal class MoneyTransferCredential
    {
        public static string PRIVATEKEY = "";
        public static string PUBLICKEY = "";
        public static string PARTNERID = "";
        public static string AGENTID = "";
        public static string Mode = "";

        internal string IsValidModalState()
        {
            var validator = new MoneyTransferCredentialValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class MoneyTransferCredentialValidator : AbstractValidator<MoneyTransferCredential>
    {
        public MoneyTransferCredentialValidator()
        {
            RuleFor(x => MoneyTransferCredential.PARTNERID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerID"));

            RuleFor(x => MoneyTransferCredential.AGENTID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AgentID"));
        }
    }
}
