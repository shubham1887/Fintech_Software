using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    public class BeneDeactivationRequest
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

        private string _HashKey;

        public string HashKey
        {
            get => _HashKey.TrimIfNotNull();
            set => _HashKey = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _IfscCode;

        public string IfscCode
        {
            get => _IfscCode.TrimIfNotNull();
            set => _IfscCode = value.TrimIfNotNull();
        }
        //public List<string> ProductCode { get; set; }

        private string _ProductCode;

        public string ProductCode
        {
            get => _ProductCode.TrimIfNotNull();
            set => _ProductCode = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BeneDeactivationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class BeneDeactivationRequestValidator : AbstractValidator<BeneDeactivationRequest>
    {
        public BeneDeactivationRequestValidator()
        {
            RuleFor(x => x.TxnId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "txnId"));

            RuleFor(x => x.CustomerMob)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerMob"));

            RuleFor(x => x.AgentId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "agentId"));

            RuleFor(x => x.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partnerId"));

            RuleFor(x => x.Channel)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "channel"));

            RuleFor(x => x.AccountNo)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "benefAccNo"));

            RuleFor(x => x.IfscCode)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "benefIfscCode"));

            RuleFor(x => x.Status)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "status"));
        }
    }
}