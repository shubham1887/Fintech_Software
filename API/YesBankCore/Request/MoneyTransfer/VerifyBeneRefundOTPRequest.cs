using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    public class VerifyBeneRefundOTPRequest
    {
        private string _TxnId;

        public string TxnId
        {
            get => _TxnId.TrimIfNotNull();
            set => _TxnId = value.TrimIfNotNull();
        }

        private string _CustomerMob;

        public string CustomerMob
        {
            get => _CustomerMob.TrimIfNotNull();
            set => _CustomerMob = value.TrimIfNotNull();
        }

        private string _AgentId;

        public string AgentId
        {
            get => _AgentId.TrimIfNotNull();
            set => _AgentId = value.TrimIfNotNull();
        }

        private string _PartnerId;

        public string PartnerId
        {
            get => _PartnerId.TrimIfNotNull();
            set => _PartnerId = value.TrimIfNotNull();
        }

        private string _OrgTxnId;

        public string OrgTxnId
        {
            get => _OrgTxnId.TrimIfNotNull();
            set => _OrgTxnId = value.TrimIfNotNull();
        }

        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }

        private string _HashKey;

        public string HashKey
        {
            get => _HashKey.TrimIfNotNull();
            set => _HashKey = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new VerifyBeneRefundOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class VerifyBeneRefundOTPRequestValidator : AbstractValidator<VerifyBeneRefundOTPRequest>
    {
        public VerifyBeneRefundOTPRequestValidator()
        {
            RuleFor(x => x.TxnId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "TxnId"));

            RuleFor(x => x.CustomerMob)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "CustomerMob"));

            RuleFor(x => x.AgentId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AgentId"));

            RuleFor(x => x.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerId"));

            RuleFor(x => x.OrgTxnId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "OrgTxnId"));

            RuleFor(x => x.Narration)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "Narration"));
        }
    }
}