using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    public class RefundTransactionRequest
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

        private string _OrderId;

        public string OrderId
        {
            get => _OrderId.TrimIfNotNull();
            set => _OrderId = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        private string _SortxnId;

        public string SortxnId
        {
            get => _SortxnId.TrimIfNotNull();
            set => _SortxnId = value.TrimIfNotNull();
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

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _PartnerStateCode;

        public string PartnerStateCode
        {
            get => _PartnerStateCode.TrimIfNotNull();
            set => _PartnerStateCode = value.TrimIfNotNull();
        }

        private string _PartnerDistrictCode;

        public string PartnerDistrictCode
        {
            get => _PartnerDistrictCode.TrimIfNotNull();
            set => _PartnerDistrictCode = value.TrimIfNotNull();
        }

        private string _AgentStateCode;

        public string AgentStateCode
        {
            get => _AgentStateCode.TrimIfNotNull();
            set => _AgentStateCode = value.TrimIfNotNull();
        }

        private string _AgentDistrictCode;

        public string AgentDistrictCode
        {
            get => _AgentDistrictCode.TrimIfNotNull();
            set => _AgentDistrictCode = value.TrimIfNotNull();
        }

        private string _CustomerStateCode;

        public string CustomerStateCode
        {
            get => _CustomerStateCode.TrimIfNotNull();
            set => _CustomerStateCode = value.TrimIfNotNull();
        }

        private string _CustomerDistrictCode;

        public string CustomerDistrictCode
        {
            get => _CustomerDistrictCode.TrimIfNotNull();
            set => _CustomerDistrictCode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new RefundTransactionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class RefundTransactionRequestValidator : AbstractValidator<RefundTransactionRequest>
    {
        public RefundTransactionRequestValidator()
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

            RuleFor(x => x.OrderId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "orderId"));

            RuleFor(x => x.OTP)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "otp"));

            RuleFor(x => x.AgentStateCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AgentStateCode"));

            RuleFor(x => x.AgentDistrictCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AgentDistrictCode"));

            RuleFor(x => x.PartnerDistrictCode)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerDistrictCode"));

            RuleFor(x => x.PartnerStateCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerStateCode"));

            RuleFor(x => x.CustomerDistrictCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "CustomerDistrictCode"));

            RuleFor(x => x.CustomerStateCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "CustomerStateCode"));
        }
    }
}