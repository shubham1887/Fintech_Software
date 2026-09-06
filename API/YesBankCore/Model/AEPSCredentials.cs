using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Model
{
    internal class AEPSCredentials
    {
        public static string PRIVATEKEY = "";
        public static string PUBLICKEY = "";
        public static string PARTNERID = "";
        public static string AGENTID = "";
        public static string Mode = "";

        internal string IsValidModalState()
        {
            var validator = new AEPSCredentialsValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class AEPSCredentialsValidator : AbstractValidator<AEPSCredentials>
    {
        public AEPSCredentialsValidator()
        {
            RuleFor(x => AEPSCredentials.PARTNERID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerID"));

            RuleFor(x => AEPSCredentials.AGENTID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AgentID"));
        }
    }
}
